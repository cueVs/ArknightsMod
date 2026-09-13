using ArknightsMod.Content.Items.Weapons.Supporter.Pramanix;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Common.GlobalNPCs
{
	// 三技能诱导：敌人朝玩家附近可达地面聚拢，保持安全距离且无法碰撞伤害玩家
	public class PramanixSkill3LureGlobalNPC : GlobalNPC
	{
		public override bool InstancePerEntity => true;

		public int LureOwner = -1;
		public int LureTimer;

		public bool IsLured => LureTimer > 0 && LureOwner >= 0;

		public void BeginLure(int owner, int durationTicks) {
			LureOwner = owner;
			LureTimer = durationTicks;
		}

		public void ClearLure() {
			LureOwner = -1;
			LureTimer = 0;
		}

		public override void PostAI(NPC npc) {
			if (!IsLured)
				return;

			if (LureTimer <= 0) {
				ClearLure();
				return;
			}

			LureTimer--;

			if (LureOwner < 0 || LureOwner >= Main.maxPlayers) {
				ClearLure();
				return;
			}

			Player player = Main.player[LureOwner];
			if (!player.active || player.dead) {
				ClearLure();
				return;
			}

			npc.target = LureOwner;
			npc.direction = player.Center.X >= npc.Center.X ? 1 : -1;

			Vector2 destination = GetReachableOrbitPoint(player, npc);
			Vector2 toDest = destination - npc.Center;
			float dist = toDest.Length();

			if (dist < 6f) {
				npc.velocity.X *= 0.82f;
				return;
			}

			float speed = SaintBellPlayer.Skill3LureMoveSpeed;
			if (npc.Distance(player.Center) < SaintBellPlayer.Skill3LureMinDistance)
				speed *= 0.35f;

			Vector2 step = toDest.SafeNormalize(Vector2.Zero) * speed;
			if (step.Length() > dist)
				step = toDest;

			npc.velocity.X = MathHelper.Lerp(npc.velocity.X, step.X, 0.42f);
			if (!npc.noGravity)
				npc.velocity.Y = MathHelper.Lerp(npc.velocity.Y, step.Y, 0.28f);
			else
				npc.velocity.Y = MathHelper.Lerp(npc.velocity.Y, step.Y * 0.55f, 0.22f);
		}

		public override bool CanHitPlayer(NPC npc, Player target, ref int cooldownSlot) {
			if (IsLured && target.whoAmI == LureOwner)
				return false;
			return base.CanHitPlayer(npc, target, ref cooldownSlot);
		}

		private static Vector2 GetReachableOrbitPoint(Player player, NPC npc) {
			Vector2 offset = npc.Center - player.Center;
			if (offset.LengthSquared() < 64f)
				offset = new Vector2(npc.direction == 0 ? 1f : npc.direction, -0.25f);

			Vector2 dir = offset.SafeNormalize(Vector2.UnitX);
			Vector2 orbit = player.Center + dir * SaintBellPlayer.Skill3LureOrbitRadius;
			Vector2 grounded = FindGroundBelow(orbit, 28);
			return grounded;
		}

		private static Vector2 FindGroundBelow(Vector2 worldPos, int maxTilesDown) {
			int tileX = (int)(worldPos.X / 16f);
			int startY = (int)(worldPos.Y / 16f);

			for (int y = 0; y <= maxTilesDown; y++) {
				int tileY = startY + y;
				if (!WorldGen.InWorld(tileX, tileY, 10))
					break;

				Tile tile = Main.tile[tileX, tileY];
				if (tile == null)
					continue;
				if (tile.HasUnactuatedTile && Main.tileSolid[tile.TileType] && !TileID.Sets.Platforms[tile.TileType])
					return new Vector2(tileX * 16f + 8f, tileY * 16f - npcFootOffset);
			}

			return worldPos;
		}

		private const float npcFootOffset = 12f;
	}
}
