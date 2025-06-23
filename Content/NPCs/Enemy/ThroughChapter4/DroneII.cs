using Terraria;
using Terraria.ModLoader;
using Terraria.Audio;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria.ID;
using Terraria.GameContent;
using ReLogic.Utilities;
using Terraria.DataStructures;
using ArknightsMod.Common.Damageclasses;

namespace ArknightsMod.Content.NPCs.Enemy.ThroughChapter4
{
	public class DroneII : ModNPC
	{
		// ===== 基础属性 =====
		private int targetPlayer;
		private float targetHeight;
		private float moveSpeed = 3f;
		private Vector2 lastPosition;

		// ===== 攻击系统 =====
		private int attackCooldown = 0;
		private int preAttackDirection = 1;
		private bool isInAttackPhase = false;
		private int shootTimer = 0;
		private int stuckTimer = 0;          // 卡住检测计时器
		private SlotId engineLoadSoundSlot;  // 转向音效槽
		private const float EdgeBuffer = 50f;// 屏幕边缘缓冲距离


		// ===== 翻转动画系统 =====
		private bool isFlipping = false;
		private float flipProgress = 0f;
		private int flipDirection = 1;
		private Vector2 customScale = Vector2.One;

		// ===== 动画系统 =====
		private int frame = 0;
		private int frameCounter = 0;

		// ===== 音效系统 =====
		private SlotId engineSoundSlot;
		private float enginePitch = 0f;
		private int heightAlarmTimer; // 高度超时计时器
		private const float MaxAllowedHeight = 0.6f; // 0.6个屏幕高度
		private const int MaxHeightTime = 300; // 5秒(60帧/秒 * 5)
		public override void SetStaticDefaults() {
			Main.npcFrameCount[NPC.type] = 2;
		}

		public override void SetDefaults() {
			NPC.width = 30;
			NPC.height = 20;
			NPC.lifeMax = 90;
			NPC.damage = 30;
			NPC.defense = 5;
			NPC.HitSound = SoundID.NPCHit4;
			NPC.DeathSound = SoundID.NPCDeath14;
			NPC.noGravity = true;
			NPC.noTileCollide = false;
		}

		public override void OnSpawn(IEntitySource source) {
			targetPlayer = NPC.FindClosestPlayer();
			Player player = Main.player[targetPlayer];

			// 初始高度设为玩家上方1/3屏幕处
			targetHeight = player.position.Y - Main.screenHeight / Main.rand.NextFloat(3f,4.4f);

			// 初始方向面向玩家
			NPC.direction = player.Center.X > NPC.Center.X ? 1 : -1;
			NPC.spriteDirection = NPC.direction;

			moveSpeed = Main.rand.NextFloat(2.8f, 4f);
		}

		public override void AI() {
			// 执行顺序很重要！
			CheckHeightDanger();
			if (NPC.ai[0] <= 0) {
				UpdateMovement();
			}
			UpdateAttackSystem();
			UpdateAnimation();
		}

		// ===== [1] 攻击系统 =====
		private void UpdateAttackSystem() {
			Player player = Main.player[targetPlayer];
			Vector2 velocity = Vector2.Normalize(player.Center - NPC.Center) * 14f;
			// 攻击阶段视觉控制
			if (isInAttackPhase) {
				// 强制贴图面向玩家（不改变实际移动方向）
				NPC.spriteDirection = player.Center.X > NPC.Center.X ? 1 : -1;

				if (--attackCooldown <= 0) {
					isInAttackPhase = false;
					NPC.spriteDirection = NPC.direction; // 恢复原方向
				}
			}

			// 射击冷却
			if (NPC.ai[0] > 0) {
				NPC.ai[0]--; // 冷却计时
				NPC.velocity *= 0.9f; // 减速效果
				if (NPC.ai[0] == 22) {
					Projectile.NewProjectile(
				NPC.GetSource_FromAI(),
				NPC.Center,
				velocity,
				ProjectileID.BulletDeadeye,
				11,
				1f,
				Main.myPlayer,
				0f,
				NPC.whoAmI);
				}
				if (NPC.ai[0] == 14) {
					Projectile.NewProjectile(
				NPC.GetSource_FromAI(),
				NPC.Center,
				velocity,
				ProjectileID.BulletDeadeye,
				11,
				1f,
				Main.myPlayer,
				0f,
				NPC.whoAmI);
				}
				return; // 冷却期间跳过其他行为
			}

			// 射击逻辑
			if (++shootTimer >= 300 && NPC.position.Y <= targetHeight) {
				ShootAtPlayer(player);
				shootTimer = 0;
			}
		}

		private void ShootAtPlayer(Player player) {
			// 记录攻击前方向
			preAttackDirection = NPC.direction;

			// 进入攻击阶段
			isInAttackPhase = true;
			attackCooldown = 30; // 0.5秒

			// 计算弹道
			Vector2 velocity = Vector2.Normalize(player.Center - NPC.Center) * 14f;

			// 发射海盗神射手子弹
			Projectile.NewProjectile(
				NPC.GetSource_FromAI(),
				NPC.Center,
				velocity,
				ProjectileID.BulletDeadeye,
				11,
				1f,
				Main.myPlayer,
				0f,
				NPC.whoAmI);

			// 攻击效果
			SoundEngine.PlaySound(SoundID.Item11 with { Volume = 0.7f }, NPC.Center);
			NPC.velocity *= 0.2f; // 后坐力
			NPC.ai[0] = 30; // 行为冷却
		}

		// ===== [2] 翻转系统 =====
		private void UpdateFlipSystem() {
			if (!isFlipping)
				return;

			flipProgress += 0.05f;

			// 3D翻转效果
			float angle = flipDirection * MathHelper.Pi * flipProgress;
			customScale.X = (float)Math.Abs(Math.Cos(angle));
			customScale.Y = 1f + 0.2f * (float)Math.Sin(angle);
			NPC.rotation = angle;

			// 翻转过半时改变实际方向
			if (flipProgress > 0.5f) {
				NPC.direction = flipDirection;
				NPC.spriteDirection = flipDirection;
			}

			// 动画结束
			if (flipProgress >= 1f) {
				isFlipping = false;
				customScale = Vector2.One;
				NPC.rotation = 0f;
			}
		}

		// ===== [3] 移动系统 =====
		private void UpdateMovement() {
			if (isFlipping)
				return; // 翻转期间暂停移动

			Player player = Main.player[targetPlayer];

			// 上升阶段
			if (NPC.position.Y > targetHeight) {
				NPC.velocity.X = moveSpeed * NPC.direction;
				NPC.velocity.Y = -moveSpeed * 0.6f;
				NPC.noTileCollide = true;
			}
			// 巡航阶段
			else {
				NPC.noTileCollide = true;
				NPC.velocity.X = moveSpeed * NPC.direction;
				NPC.velocity.Y = 0;

				// 边缘检测
				float screenLeft = Main.screenPosition.X + 50;
				float screenRight = Main.screenPosition.X + Main.screenWidth - 50 - NPC.width;

				bool shouldTurn = (NPC.position.X < screenLeft && NPC.velocity.X < 0) ||
								 (NPC.position.X > screenRight && NPC.velocity.X > 0) ||
								 (Vector2.Distance(lastPosition, NPC.position) < 0.5f && ++stuckTimer > 180);

				lastPosition = NPC.position;
				if (shouldTurn==true) {
					NPC.direction *= -1;
					NPC.spriteDirection *= -1;
				}
			}
			
		}

		// ===== [4] 动画系统 =====
		public override void FindFrame(int frameHeight) {
			NPC.frame.Y = frame * frameHeight;
		}

		private void UpdateAnimation() {
			// 攻击/翻转期间不更新常规动画
			if (isInAttackPhase || isFlipping)
				return;

			if (++frameCounter >= 7) {
				frameCounter = 0;
				frame ^= 1; // 切换0/1帧
			}
		}

		// ===== [5] 音效系统 =====

		private void UpdateEngineSound() {
			if (!SoundEngine.TryGetActiveSound(engineSoundSlot, out var activeSound) || !activeSound.IsPlaying) {
				engineSoundSlot = SoundEngine.PlaySound(
					SoundID.Item66 with {
						Volume = 0.4f,
						Pitch = enginePitch,
						MaxInstances = 3
					},
					NPC.Center);
			}
			else {
				activeSound.Position = NPC.Center;
				activeSound.Pitch = enginePitch;
			}
		}

		// ===== [6] 伤害系统 =====
		public override void ModifyHitByProjectile(Projectile projectile, ref NPC.HitModifiers modifiers) {
			if (SpellDamageConfig.SpellProjectiles.Contains(projectile.type)) {
				// 法术伤害无视护甲
				modifiers.ScalingArmorPenetration += 1f;
				// 0.95倍伤害减免
				modifiers.FinalDamage *= 0.95f;
			}
		}

		// ===== [7] 绘制系统 =====
		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
			Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
			Vector2 drawPos = NPC.Center - screenPos;
			SpriteEffects effects = NPC.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

			// 应用自定义变换
			Main.EntitySpriteDraw(
				texture,
				drawPos,
				NPC.frame,
				drawColor,
				NPC.rotation,
				NPC.frame.Size() * 0.5f,
				customScale,
				effects,
				0);

			return false;
		}
		private void CheckHeightDanger() {
			Player player = Main.player[targetPlayer];
			float screenHeight = Main.screenHeight;

			// 计算相对高度差（单位：像素）
			float heightDiff = NPC.position.Y - player.position.Y;

			// 超过警戒高度
			if (heightDiff < -screenHeight * MaxAllowedHeight) {
				if (++heightAlarmTimer > MaxHeightTime) {
					DespawnWithEffect();
					return;
				}
			}
			else {
				heightAlarmTimer = Math.Max(0, heightAlarmTimer - 2); // 恢复计数
			}
		}
		private void DespawnWithEffect() {
			// 粒子效果
			for (int i = 0; i < 30; i++) {
				Dust.NewDustPerfect(
					NPC.Center,
					DustID.Smoke,
					Main.rand.NextVector2Circular(3, 3),
					100, Color.Gray, 1.5f
				);
			}

			// 音效
			SoundEngine.PlaySound(SoundID.NPCDeath6 with { Volume = 0.7f }, NPC.Center);

			// 实际刷出
			NPC.active = false;
			NPC.netUpdate = true;

			// 重生逻辑（可选）
		}
		public override void OnKill() {
			// 停止所有音效

			// 爆炸效果
			SoundEngine.PlaySound(SoundID.Item14, NPC.Center);
			for (int i = 0; i < 10; i++) {
				Dust.NewDust(NPC.position, NPC.width, NPC.height,
					DustID.Smoke, Main.rand.NextFloat(-3f, 3f), Main.rand.NextFloat(-3f, 3f));
			}
		}
	}
}
