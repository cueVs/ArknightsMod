using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.Audio;
using Terraria.DataStructures;

namespace ArknightsMod.Content.NPCs.Enemy.ThroughChapter4
{
	public class ShieldGuard : ModNPC {
		// 动画常量
		private const int TotalFrames = 23;
		private const int FrameRate = 7;
		private const int DefaultFrame = 0;
		private const int AttackStartFrame = 1; // 第2帧(0-based)
		private const int AttackEndFrame = 8;   // 第9帧
		private const int ShieldExtendFrame = 3; // 第4帧(0-based)开始盾牌
		private const int WalkStartFrame = 10;    // 第10帧(0-based)

		// 战斗常量
		private const int AttackCooldown = 180; // 3秒
		private const int AttackDamage = 60;
		private const int NormalDamage = 30;
		private const float StrongKnockback = 15f;
		private const float WeakKnockback = 5f;

		// 移动常量
		private const float MaxSpeed = 1f;
		private const float Acceleration = 0.02f;

		// 碰撞箱
		private const int DefaultWidth = 50;
		private const int DefaultHeight = 66;
		private const int ShieldExtension = 20; // 盾牌延伸距离

		// 状态变量
		private int attackTimer = 0;
		private bool isAttacking = false;
		private bool isWalking = false;
		private int frameCounter = 0;
		private int currentFrame = 0;

		public override void SetStaticDefaults() {
			Main.npcFrameCount[NPC.type] = TotalFrames;
		}

		public override void SetDefaults() {
			NPC.width = DefaultWidth;
			NPC.height = DefaultHeight;
			NPC.damage = NormalDamage;
			NPC.defense = 80;
			NPC.lifeMax = 300;
			NPC.HitSound = SoundID.NPCHit4;
			NPC.DeathSound = SoundID.NPCDeath6;
			NPC.value = 1000f;
			NPC.knockBackResist = 0f;
			NPC.aiStyle = -1;
			NPC.noGravity = false;
			NPC.noTileCollide = false;
			
		}

		public override void AI() {
			// 目标锁定
			Player target = Main.player[NPC.target];
			NPC.TargetClosest();
			target = Main.player[NPC.target];
			if (target == null)
				return;


			// 攻击冷却
			if (attackTimer > 0)
				attackTimer--;

			// 移动逻辑
			if (!isAttacking) {
				float direction = Math.Sign(target.Center.X - NPC.Center.X);

				// 加速逻辑
				if (Math.Abs(NPC.velocity.X) <= MaxSpeed) {
					NPC.velocity.X += direction * Acceleration;
				}
				else {
					NPC.velocity.X = Math.Sign(NPC.velocity.X) * MaxSpeed;
				}

				// 行走状态检测
				if (Math.Abs(NPC.velocity.X) > 0.1f) {
					if (!isWalking) {
						isWalking = true;
						currentFrame = WalkStartFrame - 1;
						frameCounter = 0;
					}
				}
				else {
					isWalking = false;
					currentFrame = DefaultFrame;
				}

				// 攻击条件检测
				if (attackTimer <= 0 &&
					Math.Abs(target.Center.X - NPC.Center.X+target.velocity.X*10) < 60 &&
					Math.Abs(target.Center.Y - NPC.Center.Y) < 40) {
					StartAttack();
				}
			}
			else {
				// 攻击期间完全停止移动
				NPC.velocity.X = 0;

				// 在攻击第4帧生成盾牌弹幕
				if (currentFrame == ShieldExtendFrame && frameCounter == 0) {
					if (Main.netMode != NetmodeID.MultiplayerClient) {
						Projectile.NewProjectile(
							NPC.GetSource_FromAI(),
							NPC.Center,
							Vector2.Zero,
							ModContent.ProjectileType<ShieldGuardShield>(),
							AttackDamage/2,
							0f,
							Main.myPlayer,
							NPC.whoAmI,
							NPC.direction
						);
					}
				}

				// 攻击结束检测
				if (attackTimer <= AttackCooldown - (AttackEndFrame - AttackStartFrame + 1) * FrameRate) {
					EndAttack();
				}
			}

			// 更新朝向
			if (NPC.velocity.X != 0) {
				NPC.direction = NPC.spriteDirection = Math.Sign(NPC.velocity.X);
			}
		}
		private void StartAttack() {
			isAttacking = true;
			isWalking = false;
			attackTimer = AttackCooldown;
			currentFrame = AttackStartFrame;
			frameCounter = 0;
			SoundEngine.PlaySound(SoundID.Item1 with { Volume = 0.8f }, NPC.Center);
		}

		private void EndAttack() {
			isAttacking = false;
			isWalking = true;
			currentFrame = isWalking ? WalkStartFrame - 1 : DefaultFrame;
			frameCounter = 0;
		}

		public override void FindFrame(int frameHeight) {
			NPC.frame.Y = currentFrame * frameHeight;

			frameCounter++;
			if (frameCounter >= FrameRate) {
				frameCounter = 0;

				if (isAttacking) {
					currentFrame++;
					if (currentFrame > AttackEndFrame) {
						currentFrame = AttackEndFrame;
					}
				}
				else if (isWalking) {
					currentFrame++;
					if (currentFrame >= TotalFrames-3) {
						currentFrame = WalkStartFrame+4;
					}
				}
				else {
					currentFrame = DefaultFrame;
				}
			}
		}

		public override void OnHitPlayer(Player target, Player.HurtInfo hurtInfo) {
			if (isAttacking && currentFrame >= ShieldExtendFrame) {
				hurtInfo.Damage = AttackDamage;
				hurtInfo.Knockback = target.noKnockback ? StrongKnockback : WeakKnockback;
				float knockback= target.noKnockback ? StrongKnockback : WeakKnockback;
				target.velocity.X = NPC.direction*knockback;
				SoundEngine.PlaySound(SoundID.DD2_ExplosiveTrapExplode with { Pitch = -0.5f, Volume = 0.7f }, target.Center);
				// 火花特效
				for (int i = 0; i < 10; i++) {
					Dust.NewDustPerfect(
						target.Center,
						DustID.Torch,
						new Vector2(Main.rand.NextFloat(-3f, 3f), Main.rand.NextFloat(-2f, 0f)),
						100, default, 1.5f).noGravity = true;
				}
			}
			else {
				hurtInfo.Damage = NormalDamage;
				hurtInfo.Knockback = 5f;
			}
		}

	}

	public class ShieldGuardShield : ModProjectile
	{
		public override string Texture => "ArknightsMod/Content/NPCs/Enemy/ThroughChapter4/explode";

		private NPC Owner => Main.npc[(int)Projectile.ai[0]];
		private int Direction => (int)Projectile.ai[1];

		public override void SetDefaults() {
			Projectile.width = 50;
			Projectile.height = 66;
			Projectile.hostile = true;
			Projectile.friendly = false;
			Projectile.penetrate = -1;
			Projectile.tileCollide = false;
			Projectile.timeLeft = 30;
			Projectile.alpha = 255;
			Projectile.localNPCHitCooldown = 10;
			Projectile.usesLocalNPCImmunity = false;
			Projectile.usesIDStaticNPCImmunity = false;
			Projectile.ownerHitCheck = true;
			
		}

		public override void AI() {
			if (!Owner.active || Owner.type != ModContent.NPCType<NPCs.Enemy.ThroughChapter4.ShieldGuard>()) {
				Projectile.Kill();
				return;
			}

			Projectile.Center = Owner.Center + new Vector2(Direction * (50 / 2 + 10), 0);
		}

		public override void OnHitPlayer(Player target, Player.HurtInfo info) {
			Vector2 knockback = new Vector2(
		Direction * 8f, // 水平方向强制8速度
		-6f              // 垂直速度
	);

			// 直接设置玩家速度（最强制的击飞方式）
			target.velocity = knockback;
			SoundEngine.PlaySound(SoundID.DD2_ExplosiveTrapExplode with { Pitch = -0.5f, Volume = 0.7f }, target.Center);
		}

		public override bool ShouldUpdatePosition() => false;
	}
}




