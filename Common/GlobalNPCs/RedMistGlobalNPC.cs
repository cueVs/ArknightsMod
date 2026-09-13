using System.IO;
using ArknightsMod.Players;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace ArknightsMod.Common.GlobalNPCs
{
	// 血蕈不会额外生成 NPC。原版/任意模组每自然刷新一只敌怪时，都会独立进行一次概率判定，
	// 命中后直接给这只由当前环境选出的敌怪附着红雾效果。
	// 以 EntitySource_SpawnNPC 判断自然刷新，因此不需要维护怪物白名单，也不会漏掉其它模组的自然敌怪。
	public class RedMistGlobalNPC : GlobalNPC
	{
		public override bool InstancePerEntity => true;

		public bool IsRedMist;
		public float StatMultiplier = 1f;

		public override void OnSpawn(NPC npc, IEntitySource source) {
			if (Main.netMode == NetmodeID.MultiplayerClient || source is not EntitySource_SpawnNPC)
				return;
			if (!IsEligibleNaturalEnemy(npc))
				return;

			Player holder = FindNearestBloodMushroomHolder(npc.Center);
			if (holder == null || Main.rand.NextFloat() >= BloodMushroomPlayer.RedMistChancePerNaturalSpawn)
				return;

			ApplyRedMist(npc, 2f + holder.GetModPlayer<BloodMushroomPlayer>().ComboLevel);
		}

		private static bool IsEligibleNaturalEnemy(NPC npc) {
			return npc.active
				&& !npc.friendly
				&& !npc.townNPC
				&& !npc.CountsAsACritter
				&& !npc.dontTakeDamage
				&& npc.lifeMax > 5;
		}

		private static Player FindNearestBloodMushroomHolder(Vector2 position) {
			Player closest = null;
			float closestDistanceSquared = float.MaxValue;

			foreach (Player player in Main.ActivePlayers) {
				if (player.dead || !player.GetModPlayer<BloodMushroomPlayer>().HasBloodMushroom)
					continue;

				float distanceSquared = Vector2.DistanceSquared(position, player.Center);
				if (distanceSquared < closestDistanceSquared) {
					closest = player;
					closestDistanceSquared = distanceSquared;
				}
			}

			return closest;
		}

		private void ApplyRedMist(NPC npc, float multiplier) {
			IsRedMist = true;
			StatMultiplier = multiplier;
			npc.lifeMax = System.Math.Max(1, (int)(npc.lifeMax * multiplier));
			npc.life = npc.lifeMax;
			npc.damage = (int)(npc.damage * multiplier);
			npc.netUpdate = true;
		}

		public override void PostAI(NPC npc) {
			if (!IsRedMist)
				return;

			// 只追加本帧位移，不把放大后的速度写回 AI，避免飞行怪物逐帧指数加速。
			npc.position += npc.velocity * (StatMultiplier - 1f);

			if (Main.netMode != NetmodeID.Server && Main.rand.NextBool(3)) {
				Dust dust = Dust.NewDustDirect(npc.position, npc.width, npc.height, DustID.Blood,
					-npc.velocity.X * 0.15f, -npc.velocity.Y * 0.15f, 110, Color.Red, 1.25f);
				dust.noGravity = true;
			}
		}

		public override void DrawEffects(NPC npc, ref Color drawColor) {
			if (IsRedMist)
				drawColor = Color.Lerp(drawColor, new Color(210, 25, 35), 0.65f);
		}

		public override void OnKill(NPC npc) {
			if (!IsRedMist)
				return;

			int killer = npc.lastInteraction;
			if (killer < 0 || killer >= Main.maxPlayers)
				return;

			Player player = Main.player[killer];
			if (player.active && player.GetModPlayer<BloodMushroomPlayer>().HasBloodMushroom)
				player.GetModPlayer<BloodMushroomPlayer>().ComboLevel++;
		}

		public override void SendExtraAI(NPC npc, BitWriter bitWriter, BinaryWriter binaryWriter) {
			bitWriter.WriteBit(IsRedMist);
			if (IsRedMist)
				binaryWriter.Write(StatMultiplier);
		}

		public override void ReceiveExtraAI(NPC npc, BitReader bitReader, BinaryReader binaryReader) {
			IsRedMist = bitReader.ReadBit();
			StatMultiplier = IsRedMist ? binaryReader.ReadSingle() : 1f;
		}
	}
}
