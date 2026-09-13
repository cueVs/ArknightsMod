
using ArknightsMod.Systems.Gameplay.Damage;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace ArknightsMod.Content.NPCs.Enemy.TillChapter7
{
	public class InsaneZombieL : ModNPC

	{
		private float diffX;
		private float diffY;
		private float distance;
		private int fadeTimer;
		private bool InMove = true;
		private float maxspeed =5f;
		private int SpellResist = 30; // 法术抗性(填明日方舟里的法抗）
		private float acceleration = 0.2f;
		private float maxSpeed = 3f;
		private const int VerticalTolerance = 30;
		private bool Inattack;
		private int jumpCD;
		private int attackCD = 0;
		private int attackCDMax = 20; // 攻击冷却时间
		private int AttackDamage = 131; // 攻击伤害
		private int Attackrange = 50; // 攻击范围
		private bool isJumping = false;
		private float jumpAttackInertia = 0f;
		private bool InDeath = false; // 死亡状态
		private int fadeTime = 0; // 死亡淡出时间
		private int fadeOutTimer = 15; // 用于控制淡出效果的计时器
									   // 动画常量
		private int frameNumber = 24;//一共多少帧
		private int DefaultFrame = 0;//静止最后一帧是第几帧（-1）
		private int MoveStartFrame = 1;//移动开始帧
		private int MoveEndFrame = 6;//移动结束帧
		private int AttackStartFrame = 7;//攻击开始帧
		private int AttackEndFrame = 18;//攻击结束帧哦~
		private int DeathStartFrame = 19;//死亡开始帧
		private int DeathEndFrame = 24;//死亡结束帧
		private int FrameSpeed = 7;//帧速率
		private int framecounter = 0;//帧计数器
		private const int MinHeight = 45;
		private const int MaxHeight = 150;
		private const float JumpRange = 150;
		private const float JumpPower = 5f;
		private const float JumpHeight = 10f;
		private const float MinJumpPower = 7f;
		private const float MaxJumpPower = 10f;
		private float blooding;
		private float blooding2;

		//注：每个阶段的帧数为：结束帧-开始帧+1
		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
			Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
			NPC.spriteDirection = -NPC.direction;
			// 动态计算原点（水平居中，底部对齐碰撞箱）
			Vector2 origin1 = new Vector2(NPC.frame.Width / 2f, NPC.frame.Height - 19);
			Vector2 origin2 = new Vector2(NPC.frame.Width / 2f, NPC.frame.Height - 19);

			if (NPC.spriteDirection > 0) {
				spriteBatch.Draw(
				texture,
				NPC.Center - screenPos + new Vector2(0, 10f), // 整体下移4像素
				NPC.frame,
				drawColor,
				NPC.rotation,
				origin1,
				NPC.scale,
				NPC.spriteDirection == -1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally,
				0f
				);

			}
			if (NPC.spriteDirection < 0) {
				spriteBatch.Draw(
				texture,
				NPC.Center - screenPos + new Vector2(0, 10f), // 整体下移4像素
				NPC.frame,
				drawColor,
				NPC.rotation,
				origin2,
				NPC.scale,
				NPC.spriteDirection == -1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally,
				0f
				);

			}
			return false;
		}
		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
			bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[] {
				BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Times.NightTime,
				BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.TheDungeon,
				new FlavorTextBestiaryInfoElement(Language.GetTextValue("彻底失去理智的敌方士兵，会逐渐持续失去生命。感染已经深入骨髓，比一般的狂暴宿主更具进攻欲望。难以想象这样的生物会存在于战场之上。")),
			});
		}
		public override void SetStaticDefaults() {
			Main.npcFrameCount[NPC.type] = frameNumber;
		}
		public override void SetDefaults() {
			NPC.width = 25;
			NPC.height = 50;
			NPC.lifeMax = 2250;
			NPC.damage = AttackDamage / 2;
			NPC.defense = 23;
			NPC.knockBackResist = 0.2f;
			NPC.scale = 1f;
			NPC.value = 10000f;
			NPC.HitSound = SoundID.NPCHit1;
			//NPC.DeathSound = SoundID.NPCDeath7;

			var genreNPC = NPC.GetGlobalNPC<DamageCategoryNPC>();
			genreNPC.artsResistance = SpellResist / 100f;
		}
		public override void FindFrame(int frameHeight) {

			framecounter++;
			if (framecounter >= FrameSpeed) {
				NPC.frame.Y += frameHeight;
				framecounter = 0;
			}
			if (InMove & NPC.velocity.X != 0) {
				if (NPC.frame.Y < MoveStartFrame * frameHeight  || NPC.frame.Y > MoveEndFrame * frameHeight) {
					NPC.frame.Y = MoveStartFrame * frameHeight;
				}
			}
			else if (Inattack) {
				if (NPC.frame.Y < AttackStartFrame * frameHeight || NPC.frame.Y > AttackEndFrame * frameHeight) {
					NPC.frame.Y = AttackStartFrame * frameHeight;
				}
			}
			else if (InDeath) {
				if (NPC.frame.Y < DeathStartFrame * frameHeight) {
					NPC.frame.Y = DeathStartFrame * frameHeight;
				}
			}
			else {
				if (NPC.frame.Y < 0 * frameHeight || NPC.frame.Y > DefaultFrame * frameHeight) {
					NPC.frame.Y = 0 * frameHeight;
				}
			}
		}

		public override void OnSpawn(IEntitySource source) {
			fadeTimer = 60; // 持续60帧
			NPC.color = Color.Black; // 初始为纯黑
			NPC.alpha = 240;
		}
		public override bool? CanFallThroughPlatforms() {
			Player player = Main.player[NPC.target];
			return (player.position.Y + player.height) - (NPC.position.Y + NPC.height) > 30;
		}
		public override void AI() {

			var genreNPC = NPC.GetGlobalNPC<DamageCategoryNPC>();
			genreNPC.artsResistance = SpellResist / 100f;
			//出场效果
			if (fadeTimer > 0) {
				fadeTimer--;
				NPC.alpha = (int)4f * fadeTimer;
				NPC.color = Color.Lerp(Color.Black, Color.White, 1f - fadeTimer / 60f);
			}
			//索敌
			NPC.TargetClosest();
			Player Player = Main.player[NPC.target];
			diffX = Player.Center.X - NPC.Center.X;
			diffY = Player.Center.Y - NPC.Center.Y;
			distance = (float)Math.Sqrt(Math.Pow(diffX / 16, 2) + Math.Pow(diffY / 16, 2));


			//速度系统
			if (InMove) {
				blooding++;
				if (InMove == true&&blooding >= 5 && NPC.life >= 6) {
					NPC.life -= 5;
					blooding = 0;
				}
				jumpCD++;
				if (InMove) {
					if (NPC.position.X - Player.position.X > Attackrange) {
						if (NPC.velocity.X > -maxspeed) {
							NPC.velocity.X -= 0.3f;
						}
					}
					if (NPC.position.X - Player.position.X < -Attackrange) {
						if (NPC.velocity.X < maxspeed) {
							NPC.velocity.X += 0.3f;
						}
					}
				}
				if (NPC.collideX == true && jumpCD >= 60) {
					NPC.velocity.Y = -10f;
					jumpCD = 0;
				}
				float distance = Vector2.Distance(NPC.Center, Player.Center);
				Vector2 direction = (Player.Center - NPC.Center).SafeNormalize(Vector2.UnitX);
				if (
					distance <= JumpRange &&NPC.collideY && jumpCD >= 60 && Main.player[NPC.target].Center.Y < (NPC.Center.Y - MinHeight) && (NPC.Center.Y - MaxHeight) <= Main.player[NPC.target].Center.Y)
					{
					NPC.velocity.X = direction.X * JumpPower;
					float yDistance = NPC.Center.Y - Player.Center.Y;
					float yPower = yDistance * 0.09f;
					yPower = Math.Min(yPower, MaxJumpPower);
					yPower = Math.Max(yPower, MinJumpPower);
					NPC.velocity.Y = -yPower;
					jumpCD = 0;
				    }
			}

			//AI状态系统
			attackCD++;
			if (!Main.player[NPC.target].dead) {

				if (!Inattack) {
					InMove = true;
				}
				if (InMove) {
					if ((NPC.position.X - Player.position.X < Attackrange && NPC.position.X - Player.position.X > -Attackrange) && attackCD > attackCDMax&& Math.Abs(NPC.position.Y - Player.position.Y) < VerticalTolerance) {
						InMove = false;
						Inattack = true;
						attackCD = 0;
					}
				}
				if (Inattack) {
					isJumping = !NPC.collideY;
					if (isJumping) {
						if (attackCD == 0) { 
							jumpAttackInertia = NPC.velocity.X * 0.5f;
						}
						NPC.velocity.X = jumpAttackInertia; 
					}
					else {
						NPC.velocity.X = 0;
					}
					NPC.damage = 0;
				}
					if (attackCD == 28 && Inattack == true) {
					Player target = Main.player[NPC.target];
					if (target == null || target.dead)
						return;
					float xOffset = target.Center.X < NPC.Center.X ? -10f : 10f;
					Vector2 projectilePos = new Vector2(NPC.Center.X + xOffset, NPC.Center.Y);

					// 生成射弹（位置随玩家方向动态偏移）
					Projectile.NewProjectile(
						NPC.GetSource_FromThis(),
						projectilePos,
						new Vector2(0, 0),
						ModContent.ProjectileType<InsaneZombieLHit>(),
						AttackDamage / 2,
						0.8f
					);
				}
					if (attackCD >= FrameSpeed * (AttackEndFrame - AttackStartFrame + 1)) {
						InMove = true;
						Inattack = false;
						NPC.damage = AttackDamage / 2;// 恢复伤害
						attackCD = 0; // 重置攻击冷却
					}
					blooding2++;
					if (Inattack == true&& blooding2 >= 5 && NPC.life >= 6) {
						NPC.life -= 5;
						blooding2 = 0;
					}
			}
			if (InDeath) {
				fadeTime++;

				InMove = false;
				Inattack = false;
				NPC.velocity.X = 0;
				int frameHeight = NPC.frame.Height;
				NPC.dontTakeDamage = true;// 要死了，无敌了
				NPC.alpha = (int)(255 * fadeTime) / fadeOutTimer; // 逐渐变透明
				NPC.color = Color.Lerp(Color.Black, Color.White, 1f - (fadeTime / fadeOutTimer));
				for (int i = 0; i < 1; i++) // 总粒子数
				{
					// 70%概率生成黑色，30%概率生成橙色
					bool isBlack = Main.rand.NextFloat() < 0.7f;
					if (fadeTime % 3 == 0) {
						int DUST = Dust.NewDust(
						NPC.position,
						NPC.width, NPC.height,
						isBlack ? DustID.Asphalt : DustID.OrangeStainedGlass, // 黑色或橙色
						Main.rand.NextFloat(2f, 3f),
						Main.rand.NextFloat(-2f, -3f),
						100,
						default,
						Main.rand.NextFloat(0.4f, 0.6f)
					);
						Dust dust = Main.dust[DUST];
						dust.noGravity = true;
						dust.fadeIn = 0.8f;
						dust.rotation = Main.rand.NextFloat(MathHelper.Pi);
						dust.noLight = true; // 不发光


					}
				}
				if (NPC.frame.Y >= DeathEndFrame * frameHeight) {
					SoundStyle ghostSound = SoundID.NPCDeath2 with {
						Pitch = -0.1f, // 范围[-1.0, 1.0]，-0.5表示降低八度
						Volume = 0.4f  // 可选调整音量
					};
					SoundEngine.PlaySound(ghostSound, NPC.Center);
					NPC.life = 0;
					NPC.checkDead();// 消逝吧！
				}
			}
			if (Main.player[NPC.target].dead) {
				InMove = false;
				Inattack = false;
				NPC.velocity.X = 0;
			}
		}
		public override bool CheckDead() {
			if (InDeath & NPC.frame.Y >= DeathEndFrame * NPC.frame.Height) {
				return true;
			}
			else {
				NPC.life = 1;
				fadeTime = 0;
				NPC.dontTakeDamage = true;
				InDeath = true;
				return false;
			}
		}
	}
	public class InsaneZombieLHit : ModProjectile
	{
		public override string Texture => "ArknightsMod/Content/NPCs/Enemy/ThroughChapter4/explode";

		public override void SetDefaults() {
			Projectile.width = 60;
			Projectile.height = 50;
			Projectile.friendly = false;
			Projectile.hostile = true;
			Projectile.penetrate = 9999;
			Projectile.timeLeft = 3;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
			Projectile.DamageType = DamageClass.Melee;
			Projectile.scale = 1f;
			Projectile.localNPCHitCooldown = 10;
			Projectile.usesLocalNPCImmunity = false;
			Projectile.usesIDStaticNPCImmunity = false;

		}
		public override void OnHitPlayer(Player target, Player.HurtInfo info) {
			target.AddBuff(BuffID.Bleeding, 120); // 添加流血效果，持续30
			target.immuneTime = 0;
		}
		public override void ModifyHitPlayer(Player target, ref Player.HurtModifiers modifiers) {
			target.GetModPlayer<global::ArknightsMod.Players.ImmunePlayer>().ImmuneMultiplier = 0.6f; // 免疫倍数
		}
	}
}
