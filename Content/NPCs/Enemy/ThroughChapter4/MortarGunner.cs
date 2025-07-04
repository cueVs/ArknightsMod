using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ModLoader.Utilities;
using Terraria.Localization;
using Terraria.DataStructures;
using ArknightsMod.Content.Items;
using Microsoft.Xna.Framework;
using System;
using Terraria.Audio;

using Microsoft.Xna.Framework.Graphics;
using ArknightsMod.Common.Damageclasses;

namespace ArknightsMod.Content.NPCs.Enemy.ThroughChapter4
{


	public class MortarGunner : ModNPC
	{
		public override void SetStaticDefaults() {
			Main.npcFrameCount[Type] = 25;

			NPCID.Sets.NPCBestiaryDrawModifiers value = new NPCID.Sets.NPCBestiaryDrawModifiers() {
				Velocity = 1f
			};
			NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, value);
		}
		public override void SetDefaults() {
			NPC.lifeMax = 160;
			NPC.defense = 15;
			NPC.width = 16;
			NPC.height = 20;
			NPC.HitSound = SoundID.NPCHit1;
			NPC.value = 60f;
			NPC.knockBackResist = 0.5f;
			NPC.aiStyle = -1;
			NPC.scale = 1f;
			NPC.npcSlots = 2;
			NPC.damage = 6;
		}
		public int Framespeed = 7;
		public bool walk = true;
		public int walktime;
		private int AttackCD = 0;
		private bool attack;
		private int framecounter;
		private int attackframeY;
		private float maxspeed = 1.1f;
		private int jumpCD = 0;
		private int directionchoose;
		private int attacktime;
		public override void FindFrame(int frameHeight) {
			NPC.TargetClosest(true);

			Player p = Main.player[NPC.target];
			NPC.frameCounter++;
			if (NPC.frameCounter >= Framespeed) {
				NPC.frame.Y += frameHeight;
				NPC.frameCounter = 0;
			}
			if (walk&&(NPC.velocity.X!=0||NPC.velocity.Y!=0)) {
				if (NPC.frame.Y >= 14 * frameHeight) {
					NPC.frame.Y = 0;
				}

			}
			if (walk && NPC.velocity.X==0 &&NPC.velocity.Y == 0) {
				NPC.frame.Y = 15 * frameHeight;
			}
			if (attack) {
				if (NPC.frame.Y >= 24 * frameHeight || NPC.frame.Y <= 15 * frameHeight) {
					NPC.frame.Y = 15 * frameHeight;
				}

			}
		}
		public override void AI() {

			Player p = Main.player[NPC.target];
			directionchoose = p.Center.X - NPC.Center.X >= 0 ? 1 : -1;
			float angle = (float)Math.Atan((p.Center.Y - NPC.Center.Y) / (p.Center.X - NPC.Center.X));
			Vector2 velocity = CalculateHighArcTrajectory(
	NPC.Center,
	p.Center,
	GetDynamicMaxHeight(Vector2.Distance(NPC.Center, p.Center))
);
			if (walk == true) {
				NPC.spriteDirection = -NPC.direction;
				AttackCD++;
				if (NPC.position.X - p.position.X < -800 || (0 < NPC.position.X - p.position.X && NPC.position.X - p.position.X < 300)) {
					if (NPC.velocity.X < maxspeed) {
						NPC.velocity.X += 0.3f;
					}
					if (NPC.velocity.X >= maxspeed) {
						NPC.velocity.X = maxspeed;
					}
				}
				if ((-300 <= NPC.position.X - p.position.X && NPC.position.X - p.position.X <= -500) || (300 >= NPC.position.X - p.position.X && NPC.position.X - p.position.X >= 500)) {
					NPC.velocity.X = 0;
				}

				if (NPC.position.X - p.position.X > 800 || (0 > NPC.position.X - p.position.X && NPC.position.X - p.position.X > -300)) {
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
				if (AttackCD >= 120 && Math.Abs(NPC.position.X - p.position.X) <= 1200 && !attack && Math.Abs(NPC.position.Y - p.position.Y) <= 400) {
					walk = false;
					attack = true;
					AttackCD = 0;
				}
			}
			if (attack == true) {
				NPC.velocity.X = 0;
				AttackCD++;
				if (AttackCD == 30) {
					Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, velocity, ModContent.ProjectileType<Motarbomb>(), 0, 0.8f);
					SoundEngine.PlaySound(SoundID.Item61, NPC.position);
				}
				if (AttackCD > 45) {
					attack = false;
					walk = true;
					AttackCD = 0;

				}
			}

		}
		private const float GRAVITY_PER_FRAME = 0.02f; // 每帧重力加速度（px/frame²）
		public static Vector2 CalculateHighArcTrajectory(Vector2 start, Vector2 target, float maxHeightPx) {
			float dx = target.X - start.X;
			float dy = start.Y - target.Y;

			// 步骤2：计算上升帧数（到达maxHeight）
			float framesUp = MathF.Sqrt(2 * maxHeightPx / GRAVITY_PER_FRAME);

			// 步骤3：计算下落帧数（maxHeight+dy）
			float framesDown = MathF.Sqrt(2 * (maxHeightPx - dy) / GRAVITY_PER_FRAME);

			// 步骤4：计算速度分量（单位：px/frame）
			float vx = dx / (framesUp + framesDown);
			float vy = -MathF.Sqrt(2 * GRAVITY_PER_FRAME * maxHeightPx);

			return new Vector2(vx, vy);
		}
		float GetDynamicMaxHeight(float distance) {
			return MathHelper.Clamp(distance * 1f, 300f, 800f);
			// 最低200px，最高800px，保持比例
		}
		public override void HitEffect(NPC.HitInfo hit) {
			// Spawn confetti when this zombie is hit.

			for (int i = 0; i < 10; i++) {
				int dustType = DustID.RedMoss;
				var dust = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, dustType);

				dust.velocity.X += Main.rand.NextFloat(-0.05f, 0.05f);
				dust.velocity.Y += Main.rand.NextFloat(-0.05f, 0.05f);

				dust.scale *= 1f + Main.rand.NextFloat(-0.03f, 0.03f);
			}
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
				modifiers.FinalDamage *= 1f;

				for (int i = 0; i < 3; i++) {
					Dust.NewDust(NPC.position, NPC.width, NPC.height,
						DustID.MagicMirror, 0, 0, 150, Color.LightBlue, 0.7f);
				}
			}
		}
	}

	public class Motarbomb : ModProjectile
	{
		private const float Gravity = 0.02f;
		private Vector2[] trailPositions = new Vector2[30];
		private Color[] trailColors = new Color[30];

		public override void SetStaticDefaults() {
			ProjectileID.Sets.TrailCacheLength[Type] = 30;
			ProjectileID.Sets.TrailingMode[Type] = 3;
		}

		public override void SetDefaults() {
			Projectile.width = 24;
			Projectile.height = 24;
			Projectile.hostile = true;
			Projectile.timeLeft = 600;
			Projectile.tileCollide = false;
			Projectile.extraUpdates = 2; // 更平滑的运动
		}

		public override void AI() {
			// 记录轨迹

			Projectile.rotation = Projectile.velocity.ToRotation();
			// 超低速重力
			Projectile.velocity.Y += Gravity;

			// 末端制导系统（最后1秒）
			Player nearestPlayer = FindNearestPlayer(1200f);
			if (Projectile.timeLeft <= 400) {
				Projectile.tileCollide = true;
			}
			if (nearestPlayer != null) {
				if (Vector2.Distance(Projectile.Center, nearestPlayer.Center) < 10f) {
					Explode();
					Projectile.Kill();
				}
			}

			// 高亮度轨迹粒子
			if (Main.rand.NextBool(2)) {
				Dust dust = Dust.NewDustPerfect(
					Projectile.Center + Main.rand.NextVector2Circular(10, 10),
					DustID.SolarFlare,
					Vector2.Zero,
					0,
					new Color(255, Main.rand.Next(150, 255),
					1.5f
				));
				dust.noGravity = true;
			}
		}
		public override bool OnTileCollide(Vector2 oldVelocity) {
			Explode();
			return true; // 销毁弹幕
		
		}
		private Player FindNearestPlayer(float maxDistance) {
			Player nearest = null;
			float closestDist = maxDistance;

			foreach (Player player in Main.player) {
				if (!player.active || player.dead)
					continue;

				float dist = Vector2.Distance(Projectile.Center, player.Center);
				if (dist < closestDist) {
					closestDist = dist;
					nearest = player;
				}
			}
			return nearest;
		}
		public override bool PreDraw(ref Color lightColor) {
			// 绘制发光轨迹
			Texture2D trailTex = ModContent.Request<Texture2D>("Terraria/Images/Extra_21").Value;
			for (int i = 0; i < trailPositions.Length - 1; i++) {
				float progress = 1f - (i / (float)trailPositions.Length);
				Vector2 scale = new Vector2(0.5f + progress * 0.8f, 0.2f + progress * 1.5f);
				Color color = trailColors[i] * (progress * 0.7f);

				Main.EntitySpriteDraw(
					trailTex,
					trailPositions[i] - Main.screenPosition,
					null,
					color,
					Projectile.velocity.ToRotation() - MathHelper.PiOver2,
					new Vector2(trailTex.Width / 2f, trailTex.Height),
					scale,
					SpriteEffects.None, 0
				);
			}

			// 弹头核心发光
			Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
			Main.EntitySpriteDraw(
				tex,
				Projectile.Center - Main.screenPosition,
				null,
				Color.White * 0.9f,
				Projectile.rotation,
				tex.Size() / 2,
				Projectile.scale * 1.5f,
				SpriteEffects.None, 0
			);

			return false;
		}
		private void Explode() {
			// 爆炸音效
			Terraria.Audio.SoundEngine.PlaySound(SoundID.Item14, Projectile.Center);

			// 爆炸视觉效果
			for (int i = 0; i < 25; i++) {
				Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height,
					DustID.Smoke, 0f, 0f, 100, default, 2f);
				dust.velocity *= 1.4f;
				dust.noGravity = true;
			}
			Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, new Vector2(0, 0), ModContent.ProjectileType<explode>(), 20, 0.8f);

		}
	}
	public class explode : ModProjectile {
		public override void SetDefaults() {
			Projectile.width = 100;
			Projectile.height = 100;
			Projectile.damage = 76;
			Projectile.penetrate = 9999;
			Projectile.tileCollide = false;
			Projectile.hostile = true;
		}
		private int flytime;
		public override void AI() {
			flytime++;
			if (flytime >= 30) {
				Projectile.timeLeft = 1;
			}
			Dust dust;
			dust = Dust.NewDustDirect(Projectile.position, 135, 135, DustID.Torch, 0, 0, 0, default, 2);
			dust.noGravity = true;
		}
	}
}

