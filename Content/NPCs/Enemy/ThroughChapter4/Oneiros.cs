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
using System.Threading;
using System.Reflection.Metadata;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace ArknightsMod.Content.NPCs.Enemy.ThroughChapter4
{
	public class Oneiros : ModNPC
	{
		public override void SetStaticDefaults() {
			Main.npcFrameCount[Type] = 9;
			NPCID.Sets.NPCBestiaryDrawModifiers value = new NPCID.Sets.NPCBestiaryDrawModifiers() { // Influences how the NPC looks in the Bestiary
				Velocity = 1f // Draws the NPC in the bestiary as if its walking +1 tiles in the x direction
			};
			NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, value);
		}
		public override void SetDefaults() {
			NPC.width = 20;
			NPC.height = 40;
			NPC.damage = 18;
			NPC.defense = 10;
			NPC.lifeMax = 340;
			NPC.HitSound = SoundID.NPCHit4;
			NPC.DeathSound = SoundID.NPCDeath14;
			NPC.value = 100f;
			NPC.knockBackResist = 0.35f;
			NPC.aiStyle = -1;
			NPC.scale = 2f;
			NPC.noGravity = true;
		}


		private int AttackCD = 0;
		private bool attack = false;
		private bool walk = true;
		private int Framespeed = 7;
		private int framecounter;
		private int attackframeY;
		private float maxspeed = 2.0f;
		private int jumpCD = 0;
		private int walktimer;
		private Vector2 Targetpos1;
		private Vector2 Targetpos2;
		private Vector2 Vplus;
		private int directionchoose;
		private int Holdtime;
		private bool preattack;

		public override void FindFrame(int frameHeight) {

			attackframeY = 4 * frameHeight;
			NPC.TargetClosest(true);
			NPC.frameCounter++;
			if (NPC.frame.Y == 0) {
				NPC.frame.Y = 0;
			}
			if (NPC.frameCounter >= Framespeed) {
				NPC.frame.Y += frameHeight;
				NPC.frameCounter = 0;
			}
			if (walk == true && NPC.frame.Y >= attackframeY) {
				NPC.frame.Y = 0;
			}
			if (attack == true && (NPC.frame.Y < (attackframeY) || NPC.frame.Y > (8 * frameHeight))) {
				NPC.frame.Y = attackframeY+frameHeight;
			}

		}
		public override void AI() {
			Player p = Main.player[NPC.target];
			directionchoose = p.Center.X - NPC.Center.X >= 0 ? 1 : -1;
			float angle = (float)Math.Atan((p.Center.Y - NPC.Center.Y) / (p.Center.X - NPC.Center.X));
			if (walk == true) {
				walktimer++;
				//Dust dust;
				//dust = Dust.NewDustDirect(Targetpos1, 22, 22, DustID.IceTorch, 0, 0, 0, default, 4);
				//dust.noGravity = true;
			}
			NPC.TargetClosest(true);
			if (p.position.X - NPC.position.X > 0) {
				Targetpos1.X = (p.position.X - Math.Min(((720-walktimer) * Main.screenWidth / 1080), Main.screenWidth / 3));
				Targetpos1.Y = (p.position.Y -100- ((NPC.life / NPC.lifeMax) * 0.6f + 0.4f) * Math.Max((720 - walktimer), 0) * Main.screenHeight / 1440);
			}
			if (p.position.X - NPC.position.X <= 0) {
				Targetpos1.X = (p.position.X + Math.Min(((720 - walktimer) * Main.screenWidth / 1080), Main.screenWidth / 3));
				Targetpos1.Y = (p.position.Y -100 - ((NPC.life / NPC.lifeMax) * 0.6f + 0.4f) * Math.Max((720 - walktimer), 0) * Main.screenHeight / 1440);

			}
			if (p.position.X - NPC.position.X > 0) {
				Targetpos2 = new Vector2(p.position.X -  (int)(Main.screenWidth*0.5), p.position.Y-Main.screenHeight/3);

			}
			if (p.position.X - NPC.position.X <= 0) {
				Targetpos2 = new Vector2(p.position.X + (int)(Main.screenWidth * 0.5), p.position.Y - Main.screenHeight / 3);

			}
			if (walk) {
				Vplus = new Vector2(Math.Max(-0.2f,Math.Min((Targetpos1.X - NPC.position.X)/750,0.2f)) , Math.Max(-0.2f, Math.Min((Targetpos1.Y - NPC.position.Y)/520, 0.2f)));
				if (Math.Abs(NPC.velocity.X) <= 1.75f) {
					NPC.velocity.X += Math.Max(-0.2f, Math.Min((Targetpos1.X - NPC.position.X) / 750, 0.2f));
				}
				if (NPC.velocity.X >= 1.75f) {
					NPC.velocity.X = 1.75f;
				}
				if (NPC.velocity.X <= -1.75f) {
					NPC.velocity.X = -1.75f;
				}

				if (Math.Abs(NPC.velocity.Y) <= 1.75f) {
					NPC.velocity.Y += Math.Max(-0.2f, Math.Min((Targetpos1.Y - NPC.position.Y) / 520, 0.2f));
				}
				if (NPC.velocity.Y >= 1.75f) {
					NPC.velocity.Y = 1.75f;
				}
				if (NPC.velocity.Y <= -1.75f) {
					NPC.velocity.Y = -1.75f;
				}
			}
			if (attack) {
				if (Holdtime <= 20) {
					NPC.velocity.X = 0;
					NPC.velocity.Y = 0;
				}
				Vplus = new Vector2(Math.Max(-0.3f, Math.Min((Targetpos2.X - NPC.position.X) / 750, 0.3f)), Math.Max(-0.3f, Math.Min((Targetpos2.Y - NPC.position.Y) / 520, 0.3f)));
				if (Math.Abs(NPC.velocity.X) < 2.8f) {
					NPC.velocity.X += Math.Max(-0.3f, Math.Min((Targetpos2.X - NPC.position.X) / 750, 0.3f));
				}
				if (NPC.velocity.X >= 2.8f) {
					NPC.velocity.X = 2.8f;
				}
				if (NPC.velocity.X <= -2.8f) {
					NPC.velocity.X = -2.8f;
				}
				if (Math.Abs(NPC.velocity.Y) < 2.8f) {
					NPC.velocity.Y += Math.Max(-0.3f, Math.Min((Targetpos2.Y - NPC.position.Y) / 520, 0.3f));
				}
				if (NPC.velocity.Y >= 2.8f) {
					NPC.velocity.Y = 2.8f;
				}
				if (NPC.velocity.Y <= -2.8f) {
					NPC.velocity.Y = -2.8f;
				}
			}
			if (walk) {
				AttackCD++;
				if (AttackCD >= 200 && Math.Abs(NPC.position.X - p.position.X)<208 && Math.Abs(NPC.position.Y - p.position.Y) < 208) {
					preattack = true;
					AttackCD = 0;
				}
				if (preattack == true) {
					Holdtime++;
					NPC.velocity = Vector2.Zero;
					if (Holdtime >= 90) {
						Projectile.NewProjectile(NPC.GetSource_FromThis(), new Vector2(NPC.position.X+20, NPC.position.Y + 60), new Vector2(directionchoose * 0.8f, 0).RotatedBy(angle), ModContent.ProjectileType<Oneirosbomb>(), 0, 0.8f);
						attack = true;
						walk = false;
						preattack = false;
						Holdtime = 0;
					}
				}
			}
			if (attack) {
				Holdtime++;
				if (Math.Abs(NPC.position.X - Targetpos2.X) < 400) {
					walk = true;
					attack = false;
					walktimer = 0;
					AttackCD = 0;
					Holdtime = 0;
				}
			}
		}
	}
	public class Oneirosbomb : ModProjectile {
		// 配置参数
		private const float HoverTime = 0.4f;     // 停留时间(秒)
		private const float LaunchSpeed = 15f;    // 冲刺速度
		private const int ExplosionRadius = 120;  // 爆炸半径

		private Vector2? targetPosition;          // 锁定时的目标位置
		private float hoverTimer;                 // 停留计时器
		private bool hasLaunched;                // 是否已发射


		public override void SetDefaults() {
			Projectile.width = 22;
			Projectile.height = 22;
			Projectile.friendly = false;
			Projectile.hostile = true;
			Projectile.penetrate = 1;
			Projectile.timeLeft = 600;
			Projectile.tileCollide = true;       // 现在会与物块碰撞
			Projectile.aiStyle = -1;
			Projectile.scale = 1.2f;
		}

		public override void AI() {
			// 粒子效果 - 停留阶段
			if (Main.rand.NextBool(8)) {
				Dust.NewDust(Projectile.position, Projectile.width, Projectile.height,
					DustID.IceTorch, 0f, 0f, 100, default, 0.8f);
			}

			// 停留阶段
			if (!hasLaunched) {
				hoverTimer += 1f / 60f; // 每帧增加1/60秒

				// 寻找最近的玩家
				Player target = FindNearestPlayer(800f);

				// 停留时间结束，锁定目标位置
				if (hoverTimer >= HoverTime && target != null) {
					LockTargetPosition(target.Center);
					hasLaunched = true;
				}

				// 停留时的上下浮动效果
				Projectile.position.Y += (float)Math.Sin(Main.GameUpdateCount * 0.1f) * 0.5f;
			}
			// 冲刺阶段
			else if (targetPosition.HasValue) {
				// 直线冲向锁定位置
				Vector2 direction = targetPosition.Value - Projectile.Center;
				if (direction != Vector2.Zero) {
					direction.Normalize();
				}
				Projectile.velocity = direction * LaunchSpeed;

				// 旋转朝向移动方向（朝下贴图版本）
				Projectile.rotation = Projectile.velocity.ToRotation() - MathHelper.PiOver2;

				// 如果接近目标位置，爆炸
				if (Vector2.Distance(Projectile.Center, targetPosition.Value) < 10f) {
					Explode();
					Projectile.Kill();
				}
			}
		}

		private Player FindNearestPlayer(float maxDistance) {
			Player closestPlayer = null;
			float closestDistance = maxDistance;

			foreach (Player player in Main.player) {
				if (player.active && !player.dead) {
					float distance = Vector2.Distance(player.Center, Projectile.Center);
					if (distance < closestDistance) {
						closestDistance = distance;
						closestPlayer = player;
					}
				}
			}
			return closestPlayer;
		}

		private void LockTargetPosition(Vector2 position) {
			targetPosition = position;
			Terraria.Audio.SoundEngine.PlaySound(SoundID.Item8, Projectile.Center); // 锁定音效
		}

		public override void OnHitPlayer(Player target, Player.HurtInfo hurtInfo) {
			// 爆炸逻辑
			Explode();
			Projectile.Kill();
		}

		public override bool OnTileCollide(Vector2 oldVelocity) {
			Explode();
			return true; // 销毁弹幕
		}

		[Obsolete]
		public override void Kill(int timeLeft) {
			// 确保爆炸只执行一次
			if (Projectile.active) {
				Explode();
			}
		}

		private void Explode() {
			// 爆炸音效
			Terraria.Audio.SoundEngine.PlaySound(SoundID.Item14, Projectile.Center);

			// 爆炸视觉效果
			for (int i = 0; i < 25; i++) {
				Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height,
					DustID.Electric, 0f, 0f, 100, default, 2f);
				dust.velocity *= 1.4f;
				dust.noGravity = true;
			}
			Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, new Vector2(0, 0), ModContent.ProjectileType<Iceexplode>(), 25, 0.8f);
			// 爆炸伤害
			//foreach (Player player in Main.player) {
			//if (player.active && !player.dead &&
			//Vector2.Distance(player.Center, Projectile.Center) <= ExplosionRadius) {
			//int damage = (int)(Projectile.damage * 0.8f);
			//player.Hurt(Terraria.DataStructures.PlayerDeathReason.ByProjectile(player.whoAmI, Projectile.whoAmI),
			//damage, player.direction, false, false);
			//}
			//}
		}
	}
	public class Iceexplode : ModProjectile {
		public override void SetStaticDefaults() {
			Main.projFrames[Projectile.type] = 5;
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
			if (flytime <= 12) {
				Projectile.hostile = false;

			}
			if (flytime > 10) {
				Projectile.hostile = true;
			}
			if (flytime > 29) {
				Projectile.timeLeft = 1;
			}

		}
		public override bool PreDraw(ref Color lightColor) {
			SpriteBatch spriteBatch = Main.spriteBatch;
			Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;

			// 动画参数
			int frameCount = 5; // 总帧数
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
				Projectile.Center - Main.screenPosition, // 位置 + 偏移
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
		public override void OnHitPlayer(Player target, Player.HurtInfo info) {
			Projectile.playerImmune[target.whoAmI] = 10;

			if (target.HasBuff(BuffID.Chilled)) {
				target.AddBuff(BuffID.Frozen, 200);
			}
			else {
				target.AddBuff(46, 200);
			}
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

