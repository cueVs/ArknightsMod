using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ModLoader.Utilities;
using Terraria.Localization;
using Terraria.DataStructures;
using ArknightsMod.Content.Items;
using System.Runtime.CompilerServices;
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ArknightsMod.Common.VisualEffects;
using System.Collections.Generic;
using Terraria.Audio;
using ArknightsMod.Common.Damageclasses;

namespace ArknightsMod.Content.NPCs.Enemy.ThroughChapter4
{
	public class Seniorcaster : ModNPC
	{
		public override void SetStaticDefaults() {
			Main.npcFrameCount[Type] = 31;
			NPCID.Sets.NPCBestiaryDrawModifiers value = new NPCID.Sets.NPCBestiaryDrawModifiers() { // Influences how the NPC looks in the Bestiary
				Velocity = 1f // Draws the NPC in the bestiary as if its walking +1 tiles in the x direction
			};
			NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, value);
		}
		public override void SetDefaults() {
			NPC.width = 17;
			NPC.height = 50;
			NPC.damage = 12;
			NPC.defense = 20;
			NPC.lifeMax = 350;
			NPC.HitSound = SoundID.NPCHit1;
			NPC.DeathSound = SoundID.BlizzardInsideBuildingLoop;
			NPC.value = 100f;
			NPC.knockBackResist = 0.50f;
			NPC.aiStyle = -1;
			NPC.scale = 1f;
			NPC.npcSlots = 3;
		}
		private int AttackCD = 0;
		private bool attack;
		private bool walk = true;
		private int Framespeed = 6;
		private int framecounter;
		private int attackframeY;
		private float maxspeed = 1.6f;
		private int jumpCD = 0;
		private int directionchoose;
		private int lastHealthThreshold; // 记录上次触发传送的血量阈值
		private bool isTeleporting;      // 是否正在传送
		private int teleportTimer;

		public override void FindFrame(int frameHeight) {

			attackframeY = 14 * frameHeight;
			NPC.TargetClosest(true);
			if ((walk && NPC.velocity.X != 0) || attack) {
				framecounter++;
			}
			if (framecounter >= Framespeed) {
				NPC.frame.Y += frameHeight;
				framecounter = 0;
			}
			if (walk == true && (NPC.frame.Y <= attackframeY + 3 || NPC.frame.Y > (30 * frameHeight + 3))) {
				NPC.frame.Y = attackframeY + 3;
			}
			if (attack == true && (NPC.frame.Y > attackframeY + 3)) {
				NPC.frame.Y = 3;
			}

		}
		public override void AI() {

			Player p = Main.player[NPC.target];
			directionchoose = p.Center.X - NPC.Center.X >= 0 ? 1 : -1;
			float angle = (float)Math.Atan((p.Center.Y - NPC.Center.Y) / (p.Center.X - NPC.Center.X));
			if (walk == true) {
				NPC.spriteDirection = -NPC.direction;
				AttackCD++;

				// 2. 传送过程处理
				if (isTeleporting) {
					return; // 传送期间暂停其他AI
				}
				if (NPC.position.X - p.position.X < -400 || (0 < NPC.position.X - p.position.X && NPC.position.X - p.position.X < 200)) {
					if (NPC.velocity.X < maxspeed) {
						NPC.velocity.X += 0.3f;
					}
					if (NPC.velocity.X >= maxspeed) {
						NPC.velocity.X = maxspeed;
					}
				}
				if ((-200 <= NPC.position.X - p.position.X && NPC.position.X - p.position.X <= -300) || (200 >= NPC.position.X - p.position.X && NPC.position.X - p.position.X >= 300)) {
					NPC.velocity.X = 0;
				}

				if (NPC.position.X - p.position.X > 400 || (0 > NPC.position.X - p.position.X && NPC.position.X - p.position.X > -200)) {
					if (NPC.velocity.X > -maxspeed) {
						NPC.velocity.X += -0.3f;
					}
					if (NPC.velocity.X <= -maxspeed) {
						NPC.velocity.X = -maxspeed;
					}
				}

				if (Math.Abs(NPC.velocity.X) <= 0.5f && attack == false && (400 < Math.Abs(NPC.position.X - p.position.X) || 200 > Math.Abs(NPC.position.X - p.position.X))) {
					jumpCD++;
				}
				if (jumpCD >= 400) {
					jumpCD = 0;
					NPC.velocity.Y = -7.2f;
				}
				if (AttackCD >= 60 && Math.Abs(NPC.position.X - p.position.X) <= 560 && !attack && Math.Abs(NPC.position.Y - p.position.Y) <= 200) {
					walk = false;
					attack = true;
					AttackCD = 0;
				}
			}
			if (attack == true) {
				NPC.velocity.X = 0;
				AttackCD++;
				if (AttackCD == 1) {
					Projectile.NewProjectile(NPC.GetSource_FromThis(), new Vector2(p.position.X + 20 * p.velocity.X, p.position.Y + 12 * p.velocity.Y), new Vector2(0, 0).RotatedBy(angle), ModContent.ProjectileType<SCproj>(), 12, 0.8f);
				}
				if (AttackCD > 90) {
					attack = false;
					walk = true;
					AttackCD = 0;

				}
			}
		}


		public override bool? CanFallThroughPlatforms() {
			Player player = Main.player[NPC.target];
			return (player.position.Y + player.height) - (NPC.position.Y + NPC.height) > 0;
		}
		public override void OnKill() {
			SoundStyle ghostSound = SoundID.NPCDeath6 with {
				Pitch = -0.4f, // 范围[-1.0, 1.0]，-0.5表示降低八度
				Volume = 0.6f  // 可选调整音量
			};
			SoundEngine.PlaySound(ghostSound, NPC.Center);
			for (int i = 0; i < 25; i++) // 总粒子数
	{
				// 70%概率生成黑色，30%概率生成橙色
				bool isBlack = Main.rand.NextFloat() < 0.7f;

				Dust dust = Dust.NewDustPerfect(
					NPC.Center,
					isBlack ? DustID.Asphalt : DustID.FireworksRGB, // 黑色或橙色
					Main.rand.NextVector2Circular(5, 5),
					Alpha: 150,
					Scale: Main.rand.NextFloat(1.2f, 2f)
				);

				// 统一物理参数
				dust.noGravity = true;
				dust.fadeIn = 1.5f;
				dust.rotation = Main.rand.NextFloat(MathHelper.TwoPi);
			}
		}
		public override void ModifyHitByProjectile(Projectile projectile, ref NPC.HitModifiers modifiers) {
			if (SpellDamageConfig.SpellProjectiles.Contains(projectile.type)) {
				// 法术伤害无视护甲
				modifiers.ScalingArmorPenetration += 1f;
				// 0.95倍伤害减免
				modifiers.FinalDamage *= 0.5f;

				for (int i = 0; i < 3; i++) {
					Dust.NewDust(NPC.position, NPC.width, NPC.height,
						DustID.Shadowflame, 0, 0, 150, default, 0.7f);
				}
			}
		}
	}

	public class SCproj : ModProjectile
	{
		public override void SetStaticDefaults() {
			Main.projFrames[Projectile.type] = 13;
			ProjectileID.Sets.TrailingMode[Type] = 2;
			ProjectileID.Sets.TrailCacheLength[Type] = 40;

		}
		public override void SetDefaults() {
			Projectile.width = 70;
			Projectile.height = 70;
			Projectile.damage = 24;
			Projectile.penetrate = 9999;
			Projectile.scale = 2f;
			Projectile.hostile = true;
			Projectile.tileCollide = false;
		}
		

		private int flytime;
		private Vector2 axis;
		private Vector2 unaxis;
		public override void AI() {
			flytime++;
			Projectile.spriteDirection = -Projectile.direction;
			Projectile.frameCounter++;
			if (Projectile.frameCounter > 6) {
				Projectile.frame += 1;
				Projectile.frameCounter = 0;
			}
			if (flytime == 1) {
				Projectile.frame = 0;
			}
			if (flytime <= 60) {
				Projectile.hostile = false;

			}
			if (flytime > 60) {
				Projectile.hostile = true;
			}
			if (flytime > 90) {
				Projectile.timeLeft = 1;
			}

		}
		public override bool PreDraw(ref Color lightColor) {
			SpriteBatch spriteBatch = Main.spriteBatch;
			Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;

			// 动画参数
			int frameCount = 13; // 总帧数
			int frameHeight = texture.Height / frameCount; // 每帧高度
			int frameWidth = texture.Width; // 每帧宽度（整宽）

			// 当前帧的矩形区域（纵向排列）
			Rectangle frameRect = new Rectangle(
				0, // X 位置（单列动画，横向无偏移）
				Projectile.frame * frameHeight, // Y 位置（纵向偏移）
				frameWidth,
				frameHeight
			);

			// 原点设为帧的中心（推荐）
			Vector2 origin = new Vector2(frameWidth / 2, frameHeight / 2);

			// 绘制
			spriteBatch.Draw(
				texture,
				Projectile.Center - Main.screenPosition , // 位置 + 偏移
				frameRect,
				lightColor,
				Projectile.rotation, // 使用实际旋转（而不是硬编码 0）
				origin, // 修正后的原点
				Projectile.scale, // 使用 Projectile.scale 而非固定值 2
				SpriteEffects.None,
				0f
			);

			return false; // 禁用默认绘制
		}
		public override void ModifyHitPlayer(Player target, ref Player.HurtModifiers modifiers) {
			modifiers.ArmorPenetration += 9999f;
			if (Main.expertMode)
				modifiers.FinalDamage *= 0.9f; // 专家模式伤害 ×1.5
			if (Main.masterMode)
				modifiers.FinalDamage *= 0.85f;   // 大师模式伤害 ×2
		}

	}
}


