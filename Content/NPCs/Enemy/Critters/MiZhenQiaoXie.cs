using ArknightsMod.Content.Items;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Content.NPCs.Enemy.Critters
{
	// 迷珍巧械：不主动攻击，玩家靠近或打中它都会朝反方向逃跑。
	// 贴图分三张：本体 + 前轮 + 后轮，PreDraw 里手动叠图，
	// 默认（未翻转）朝向下前轮贴在本体左下角、后轮贴在右下角；转向逃跑时随本体一起镜像翻转。
	public class MiZhenQiaoXie : ModNPC
	{
		private const float DetectRange = 500f;
		private const float MaxSpeed = 7f; // 明显快于玩家步行速度，逃跑才看得出来是在逃跑
		private const float Acceleration = 0.4f;
		private const float DashBurstSpeed = MaxSpeed * 0.7f; // 刚发现玩家/刚被打中那一下的瞬间冲刺速度
		private const float HopSpeed = 5f;
		private const int HitFleeDuration = 180; // 被打中后至少持续逃跑 3 秒，即使攻击者不在检测范围内
		private const float WheelOutwardOffset = 6f; // 轮子在贴边基础上再向外挪出的像素量
		private const float BodyLiftOffset = 4f; // 本体相对轮子再抬高的像素量
		private const float WheelRotationPerVelocity = 0.15f; // 轮子转速和水平速度的换算系数

		private int fleeTimer;
		private int fleeDirection; // -1 向左逃，1 向右逃
		private bool wasFleeing;
		private float wheelRotation;

		private static Asset<Texture2D> _frontWheel;
		private static Asset<Texture2D> _rearWheel;

		public override void SetStaticDefaults() {
			NPCID.Sets.NPCBestiaryDrawModifiers drawModifiers = new() { Velocity = 1f };
			NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, drawModifiers);
		}

		public override void SetDefaults() {
			NPC.width = 30;
			NPC.height = 38;
			NPC.damage = 35;
			NPC.defense = 27;
			NPC.lifeMax = 400;
			NPC.HitSound = SoundID.NPCHit1;
			NPC.DeathSound = SoundID.NPCDeath1;
			NPC.value = 500f;
			NPC.knockBackResist = 0.5f;
			NPC.aiStyle = NPCAIStyleID.Passive;
			NPC.noGravity = false;
			NPC.noTileCollide = false;

			_frontWheel ??= ModContent.Request<Texture2D>("ArknightsMod/Content/NPCs/Enemy/Critters/MiZhenQiaoXie_FrontWheel");
			_rearWheel ??= ModContent.Request<Texture2D>("ArknightsMod/Content/NPCs/Enemy/Critters/MiZhenQiaoXie_RearWheel");
		}

		public override float SpawnChance(NPCSpawnInfo spawnInfo) {
			// 生成环境：地表 / 地下 / 洞穴均可，不挑昼夜、不挑善恶邪恶地皮。
			if (!spawnInfo.Player.ZoneOverworldHeight && !spawnInfo.Player.ZoneDirtLayerHeight && !spawnInfo.Player.ZoneRockLayerHeight)
				return 0f;

			// 概率参考同类型"稀有小怪"写法，对标宝箱怪那种稀少、可遇不可求的量级——
			// 没有从原版代码里抠出宝箱怪的精确数值，这里是按经验估的稀有档，
			// 如果实际测试觉得太密/太稀，直接改这个数就行。
			return 0.02f;
		}

		public override void ModifyNPCLoot(NPCLoot npcLoot) {
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<OriginiumIngot>(), 1, 1, 2));
		}

		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
			bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[] {
				BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Surface,
				BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Underground,
				BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Caverns,
				new FlavorTextBestiaryInfoElement(Terraria.Localization.Language.GetTextValue("Mods.ArknightsMod.Bestiary.MiZhenQiaoXie")),
			});
		}

		public override void AI() {
			if (fleeTimer > 0)
				fleeTimer--;

			Player nearest = FindNearestPlayerInRange(DetectRange);
			if (nearest != null) {
				fleeDirection = NPC.Center.X >= nearest.Center.X ? 1 : -1;
				// 玩家还在探测范围内时持续刷新，保证不会因为单帧判定间隙掉头
				fleeTimer = System.Math.Max(fleeTimer, 30);
			}

			bool fleeing = fleeTimer > 0;
			if (fleeing) {
				if (fleeDirection == 0)
					fleeDirection = NPC.direction != 0 ? NPC.direction : 1;

				NPC.direction = fleeDirection;

				// 刚从"没在逃"切换到"开始逃"的那一帧：直接给一个冲刺速度+小跳，
				// 而不是从 0 慢慢加速——原来纯靠 Acceleration 爬速度，跟玩家步行速度太接近，
				// 看起来不像"发现你就跑"，更像"凑巧同向走"。
				if (!wasFleeing) {
					NPC.velocity.X = fleeDirection * DashBurstSpeed;
					if (NPC.velocity.Y == 0f)
						NPC.velocity.Y = -HopSpeed * 0.8f;
				}

				NPC.velocity.X = MathHelper.Clamp(NPC.velocity.X + fleeDirection * Acceleration, -MaxSpeed, MaxSpeed);

				// 撞墙时小跳一下，越过一两格台阶
				if (NPC.collideX && NPC.velocity.Y == 0f)
					NPC.velocity.Y = -HopSpeed;
			}
			else {
				NPC.velocity.X *= 0.9f;
				if (System.Math.Abs(NPC.velocity.X) < 0.05f)
					NPC.velocity.X = 0f;
			}

			wasFleeing = fleeing;
			NPC.spriteDirection = NPC.direction;

			// 轮子转速跟着水平移动速度走，方向由速度符号决定——不受 spriteDirection 翻转影响，
			// 翻转时 SpriteEffects.FlipHorizontally 会自动把视觉效果处理对，不需要额外反向。
			wheelRotation += NPC.velocity.X * WheelRotationPerVelocity;
			wheelRotation = MathHelper.WrapAngle(wheelRotation);
		}

		private Player FindNearestPlayerInRange(float range) {
			Player nearest = null;
			float nearestDistSq = range * range;
			foreach (Player player in Main.ActivePlayers) {
				float distSq = Vector2.DistanceSquared(player.Center, NPC.Center);
				if (distSq < nearestDistSq) {
					nearestDistSq = distSq;
					nearest = player;
				}
			}
			return nearest;
		}

		public override void OnHitByItem(Player player, Item item, NPC.HitInfo hit, int damageDone) => TriggerFleeFromHit(hit.HitDirection);

		public override void OnHitByProjectile(Projectile projectile, NPC.HitInfo hit, int damageDone) => TriggerFleeFromHit(hit.HitDirection);

		private void TriggerFleeFromHit(int hitDirection) {
			if (hitDirection != 0)
				fleeDirection = hitDirection;
			fleeTimer = HitFleeDuration;
		}

		public override void HitEffect(NPC.HitInfo hit) {
			for (int i = 0; i < 6; i++) {
				int dust = Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Iron, hit.HitDirection, -1f, 0, default, 1f);
				Main.dust[dust].noGravity = true;
			}
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
			Texture2D bodyTex = TextureAssets.Npc[Type].Value;
			Texture2D frontWheelTex = _frontWheel.Value;
			Texture2D rearWheelTex = _rearWheel.Value;

			bool flipped = NPC.spriteDirection == -1;
			SpriteEffects fx = flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
			float mirror = flipped ? -1f : 1f;

			// groundAnchor 是轮子的基准位置（不受本体抬升影响），bodyScreenPos 在这基础上再往上抬一点，
			// 让本体看起来"架"在轮子上面，而不是整体一起挪动。
			Vector2 groundAnchor = NPC.Center - screenPos + new Vector2(0f, NPC.gfxOffY);
			Vector2 bodyScreenPos = groundAnchor - new Vector2(0f, BodyLiftOffset);
			var bodyOrigin = new Vector2(bodyTex.Width / 2f, bodyTex.Height / 2f);
			spriteBatch.Draw(bodyTex, bodyScreenPos, null, drawColor, 0f, bodyOrigin, NPC.scale, fx, 0f);

			float halfW = bodyTex.Width / 2f * NPC.scale;
			float halfH = bodyTex.Height / 2f * NPC.scale;

			// 默认（未翻转）：前轮贴左下角、后轮贴右下角，再各自向外挪 WheelOutwardOffset 一点；
			// 翻转时随镜像一起换到另一侧。基准用 groundAnchor，不用抬高后的 bodyScreenPos。
			Vector2 frontWheelPos = groundAnchor + new Vector2(
				mirror * (-halfW - WheelOutwardOffset + frontWheelTex.Width * 0.5f * NPC.scale),
				halfH - frontWheelTex.Height * 0.4f * NPC.scale);
			Vector2 rearWheelPos = groundAnchor + new Vector2(
				mirror * (halfW + WheelOutwardOffset - rearWheelTex.Width * 0.5f * NPC.scale),
				halfH - rearWheelTex.Height * 0.4f * NPC.scale);

			var frontOrigin = new Vector2(frontWheelTex.Width / 2f, frontWheelTex.Height / 2f);
			var rearOrigin = new Vector2(rearWheelTex.Width / 2f, rearWheelTex.Height / 2f);

			// 轮子跟着 wheelRotation 转动，本体不转——只有轮子在滚。
			spriteBatch.Draw(frontWheelTex, frontWheelPos, null, drawColor, wheelRotation, frontOrigin, NPC.scale, fx, 0f);
			spriteBatch.Draw(rearWheelTex, rearWheelPos, null, drawColor, wheelRotation, rearOrigin, NPC.scale, fx, 0f);

			return false;
		}
	}
}
