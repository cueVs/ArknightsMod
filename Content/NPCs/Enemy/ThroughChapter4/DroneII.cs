using ArknightsMod.Content.Items.Material;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.Utilities;

namespace ArknightsMod.Content.NPCs.Enemy.ThroughChapter4
{
	public class DroneII : ModNPC
	{
		// ===== 基础属性 =====
		private int targetPlayer;
		private float targetHeight;
		private float moveSpeed = 3.6f;     // 比 Drone 更快
		private Vector2 lastPosition;

		// ===== 攻击系统 =====
		private int attackCooldown = 0;
		private bool isInAttackPhase = false;
		private int shootTimer = 0;
		private int stuckTimer = 0;          // 卡住检测

		// ===== 动画系统 =====
		private int frame = 0;
		private int frameCounter = 0;

		// （可选）高度自毁
		private int heightAlarmTimer;                  // 高度超时计时器
		private const float MaxAllowedHeight = 0.6f;   // 0.6 屏高度
		private const int MaxHeightTime = 300;    // 5 秒

		// ===== AI 状态/出生点等（与 Drone 相同逻辑）=====
		private enum AIState { Idle, Attack }
		private AIState state = AIState.Idle;

		private Vector2 homeAnchor;
		private int turnCooldown = 0;        // 撞墙返航冷却
		private int platformDropTimer = 0;   // 下平台短暂穿透帧
		private const int PlatformDropFrames = 8;

		// Idle 漂浮（非对称上下界，便于”抬高最低点”）
		private float idlePhase = 0f;
		private const float IdlePeriodFrames = 110f; // 比 Drone 快一点点
		private const float IdleMinRaiseUp = 40f;    // 最低点上抬（更高）
		private const float IdleMaxDropDown = 12f;   // 最高点下压

		// Attack 悬停（DroneII 数值，比 Drone 飞得更高更激进）
		private const float AttackHoverAbove = 120f;  // 目标：玩家头顶上方多少像素
		private const float AttackMaxRise = 200f;     // 从出生高度最多上升多少像素

		// 索敌距离（更敏锐）
		private const float LockRange = 600f;
		private const float LoseRange = 1200f;
		private const int StuckLoseTime = 150;

		// —— 可选：固定巡航半径（如要启用，取消注释并在 UpdateMovement 里用它替换屏幕边界）——
		private const float patrolHalfWidth = 300f;

		public override void SetStaticDefaults() {
			Main.npcFrameCount[NPC.type] = 2;
		}

		public override void SetDefaults() {
			NPC.width = 30;
			NPC.height = 20;
			NPC.lifeMax = 90;    // 更耐打
			NPC.damage = 30;     // 更痛
			NPC.defense = 5;
			NPC.HitSound = SoundID.NPCHit4;
			NPC.DeathSound = SoundID.NPCDeath14;
			NPC.value = 60f;

			NPC.noGravity = true;
			NPC.noTileCollide = false; // 不穿墙；仅下平台阶段短暂 true
			NPC.aiStyle = -1;
		}

		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
			bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[] {
				BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Surface,
				new FlavorTextBestiaryInfoElement("敌方“妖怪 MKII”，在无人机基础上改进了火力模块，具备短连发能力。"),
			});
		}

		public override float SpawnChance(NPCSpawnInfo spawnInfo) {
			return SpawnCondition.OverworldDaySlime.Chance * 0.1f;
		}

		public override void ModifyNPCLoot(NPCLoot npcLoot) {
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<OrironShard>(), ModContent.GetInstance<Dropconfig>().DropDrone1, 2, 5));
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Ester>(), ModContent.GetInstance<Dropconfig>().DropDrone2 * 2, 1, 2));
		}

		public override void OnSpawn(IEntitySource source) {
			targetPlayer = NPC.FindClosestPlayer();
			Player player = Main.player[targetPlayer];

			homeAnchor = NPC.Center;
			moveSpeed = Main.rand.NextFloat(2.8f, 4f); // 比 Drone 稍快的随机范围

			NPC.direction = player.Center.X > NPC.Center.X ? 1 : -1;
			NPC.spriteDirection = NPC.direction; // 撤销翻转修改，贴图=移动方向
		}

		public override void AI() {
			if (turnCooldown > 0)
				turnCooldown--;
			if (platformDropTimer > 0)
				platformDropTimer--;

			int p = Player.FindClosest(NPC.Center, NPC.width, NPC.height);
			Player player = Main.player[p];
			float distToPlayer = Vector2.Distance(NPC.Center, player.Center);

			// —— 状态切换：与 Drone 一致 —— 
			if (!player.active || player.dead) {
				state = AIState.Idle;
			}
			else {
				if (state == AIState.Idle && distToPlayer <= LockRange)
					state = AIState.Attack;
				else if (state == AIState.Attack) {
					if (Vector2.Distance(lastPosition, NPC.position) < 0.5f)
						stuckTimer++;
					else
						stuckTimer = Math.Max(0, stuckTimer - 2);

					if (distToPlayer > LoseRange || stuckTimer > StuckLoseTime) {
						state = AIState.Idle;
						stuckTimer = 0;
					}
				}
			}

			// —— 目标高度：Idle 漂浮 / Attack 盯玩家头顶 —— 
			if (state == AIState.Idle) {
				idlePhase += MathHelper.TwoPi / IdlePeriodFrames;
				float minY = homeAnchor.Y - IdleMinRaiseUp;
				float maxY = homeAnchor.Y + IdleMaxDropDown;
				float t = 0.5f * (1f + (float)Math.Sin(idlePhase));
				targetHeight = MathHelper.Lerp(minY, maxY, t);
			}
			else {
				float wantedY = player.position.Y - AttackHoverAbove;
				float ceilY   = homeAnchor.Y - AttackMaxRise;
				targetHeight  = Math.Max(wantedY, ceilY);
			}

			// —— 移动（会下平台 / 撞墙返航 / 不穿墙）——
			UpdateMovement(targetHeight, state == AIState.Idle);

			// —— 攻击：MKII 连发（22/14 帧各补一枪）——
			if (state == AIState.Attack)
				UpdateAttackSystem(player);

			// —— 可选：高度越界自毁 —— 
			CheckHeightDanger(player);

			// —— 动画 —— 
			UpdateAnimation();

			lastPosition = NPC.position;
		}

		// ===== 移动（与 Drone 一致） =====
		private void UpdateMovement(float targetY, bool idleMode) {
			// 水平：Idle 原地；Attack 巡航
			float vx = idleMode ? 0f : moveSpeed * NPC.direction;

			// 垂直：向 targetHeight 靠拢
			float vy = 0f;
			if (NPC.position.Y > targetY)
				vy = -moveSpeed * 0.5f;
			else if (NPC.position.Y < targetY)
				vy = moveSpeed * 0.5f;

			// 会下平台：需要下降 + 脚下是平台 + 落点安全
			bool wantDescend = vy > 0f;
			if (platformDropTimer == 0 && wantDescend && IsStandingOnPlatform() && IsDropSpaceClear(12))
				platformDropTimer = PlatformDropFrames;

			NPC.noTileCollide = platformDropTimer > 0;
			if (platformDropTimer > 0 && vy < 3f)
				vy = 3f;

			NPC.velocity.X = vx;
			NPC.velocity.Y = vy;

			// 撞墙返航（仅 Attack/有水平移动时）
			if (!idleMode) {
				bool solidAhead = IsSolidAhead(NPC.direction, 8);
				if (turnCooldown == 0 && (NPC.collideX || solidAhead)) {
					NPC.direction *= -1;
					NPC.spriteDirection = NPC.direction; // 保持与方向一致
					NPC.velocity.X = 0f;
					turnCooldown = 12;
					NPC.netUpdate = true;
				}
			}

			// 垂直去抖（非下穿阶段）
			if (!NPC.noTileCollide && NPC.collideY)
				NPC.velocity.Y = 0f;

			NPC.spriteDirection = NPC.direction; // 贴图 = 移动方向

			// —— 保留 Drone 的“屏幕边缘检测”作为兜底（你也可以换成 homeAnchor±半径的固定巡航）——
			float screenLeft = Main.screenPosition.X + Main.screenWidth / 6f;
			float screenRight = Main.screenPosition.X + Main.screenWidth * 5f / 6f;

			bool edgeTurn =
				(!idleMode && NPC.position.X < screenLeft && NPC.velocity.X < 0) ||
				(!idleMode && NPC.position.X > screenRight && NPC.velocity.X > 0) ||
				(Vector2.Distance(lastPosition, NPC.position) < 0.5f && ++stuckTimer > 180);

			if (edgeTurn && !idleMode && turnCooldown == 0) {
				NPC.direction *= -1;
				NPC.spriteDirection = NPC.direction;
				NPC.velocity.X = 0f;
				turnCooldown = 12;
				NPC.netUpdate = true;
			}
		}

		// ===== 攻击（含冷却区间 22/14 帧连发） =====
		private void UpdateAttackSystem(Player player) {
			if (isInAttackPhase) {
				if (--attackCooldown <= 0)
					isInAttackPhase = false;
			}

			if (NPC.ai[0] > 0) {
				NPC.ai[0]--;
				NPC.velocity *= 0.9f;

				// 冷却区间第 22 / 14 帧各补一枪（MKII 特性）
				if (NPC.ai[0] == 22 || NPC.ai[0] == 14) {
					Vector2 v2 = Vector2.Normalize(player.Center - NPC.Center) * 14f;
					Projectile.NewProjectile(NPC.GetSource_FromAI(),
						NPC.Center, v2, ProjectileID.BulletDeadeye, 11, 1f, Main.myPlayer, 0f, NPC.whoAmI);
				}
				return;
			}

			if (++shootTimer >= 300 && NPC.position.Y <= targetHeight) {
				Vector2 v = Vector2.Normalize(player.Center - NPC.Center) * 14f;
				FirePrimary(v);
				shootTimer = 0;
			}
		}

		private void FirePrimary(Vector2 velocity) {
			isInAttackPhase = true;
			attackCooldown = 30;

			Projectile.NewProjectile(
				NPC.GetSource_FromAI(),
				NPC.Center,
				velocity,
				ProjectileID.BulletDeadeye,
				11,           // 比 Drone 高的伤害
				1f,
				Main.myPlayer, 0f, NPC.whoAmI);

			SoundEngine.PlaySound(SoundID.Item11 with { Volume = 0.7f }, NPC.Center);
			NPC.velocity *= 0.2f;
			NPC.ai[0] = 30; // 冷却区间，22/14 帧连发在 UpdateAttackSystem 里触发
		}

		// ===== 工具：前向探针/平台/下穿空间 =====
		private bool IsSolidAhead(int dir, int probe = 8) {
			float offsetX = dir == 1 ? NPC.width / 2f : -(NPC.width / 2f) - probe;
			Vector2 probePos = new Vector2(NPC.Center.X + offsetX, NPC.position.Y);

			if (Collision.SolidCollision(probePos, probe, NPC.height))
				return true;

			Point p1 = probePos.ToTileCoordinates();
			Point p2 = (probePos + new Vector2(probe, NPC.height)).ToTileCoordinates();
			for (int x = p1.X; x <= p2.X; x++)
				for (int y = p1.Y; y <= p2.Y; y++) {
					if (!WorldGen.InWorld(x, y, 1))
						continue;
					Tile t = Framing.GetTileSafely(x, y);
					if (t.HasTile && (Main.tileSolid[t.TileType] || TileID.Sets.Platforms[t.TileType]))
						return true;
				}
			return false;
		}

		private bool IsStandingOnPlatform() {
			Rectangle feet = new Rectangle((int)NPC.position.X, (int)NPC.Bottom.Y, NPC.width, 2);
			Point p1 = new Vector2(feet.Left, feet.Top).ToTileCoordinates();
			Point p2 = new Vector2(feet.Right, feet.Bottom).ToTileCoordinates();
			for (int x = p1.X; x <= p2.X; x++)
				for (int y = p1.Y; y <= p2.Y; y++) {
					if (!WorldGen.InWorld(x, y, 1))
						continue;
					Tile t = Framing.GetTileSafely(x, y);
					if (t.HasTile && TileID.Sets.Platforms[t.TileType])
						return true;
				}
			return false;
		}

		private bool IsDropSpaceClear(int dropPixels) {
			Vector2 probePos = new Vector2(NPC.position.X, NPC.position.Y + 2);
			int w = NPC.width;
			int h = Math.Max(2, dropPixels);
			if (Collision.SolidCollision(probePos, w, h))
				return false;

			Point p1 = probePos.ToTileCoordinates();
			Point p2 = (probePos + new Vector2(w, h)).ToTileCoordinates();
			for (int x = p1.X; x <= p2.X; x++)
				for (int y = p1.Y; y <= p2.Y; y++) {
					if (!WorldGen.InWorld(x, y, 1))
						continue;
					Tile t = Framing.GetTileSafely(x, y);
					if (t.HasTile && Main.tileSolid[t.TileType] && !TileID.Sets.Platforms[t.TileType])
						return false;
				}
			return true;
		}

		// ===== 动画 =====
		public override void FindFrame(int frameHeight) {
			NPC.frame.Y = frame * frameHeight;
		}

		private void UpdateAnimation() {
			if (isInAttackPhase)
				return;

			if (++frameCounter >= 7) {
				frameCounter = 0;
				frame ^= 1; // 切换0/1帧
			}
		}

		// ===== 绘制 =====
		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
			Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
			Vector2 drawPos = NPC.Center - screenPos;
			SpriteEffects effects = NPC.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

			Main.EntitySpriteDraw(
				texture,
				drawPos,
				NPC.frame,
				drawColor,
				0f,
				NPC.frame.Size() * 0.5f,
				1f,
				effects,
				0);

			return false;
		}

		// ===== 高度越界自毁（可选） =====
		private void CheckHeightDanger(Player player) {
			float screenH = Main.screenHeight;
			float heightDiff = NPC.position.Y - player.position.Y; // 负值：在玩家上方
			if (heightDiff < -screenH * MaxAllowedHeight) {
				if (++heightAlarmTimer > MaxHeightTime) {
					DespawnWithEffect();
				}
			}
			else {
				heightAlarmTimer = Math.Max(0, heightAlarmTimer - 2);
			}
		}

		private void DespawnWithEffect() {
			for (int i = 0; i < 30; i++) {
				Dust.NewDustPerfect(NPC.Center, DustID.Smoke,
					Main.rand.NextVector2Circular(3, 3), 100, Color.Gray, 1.5f);
			}
			SoundEngine.PlaySound(SoundID.NPCDeath6 with { Volume = 0.7f }, NPC.Center);
			NPC.active = false;
			NPC.netUpdate = true;
		}
	}
}
