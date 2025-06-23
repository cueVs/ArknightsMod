using ArknightsMod.Common.Damageclasses;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.WorldBuilding;

namespace ArknightsMod.Content.NPCs.Enemy.ThroughChapter4
{
	public class DoubleSword : ModNPC

	{
		private int fadeTimer;
		private bool InMove;
		private float maxspeed = 2.2f;
		private int SpellResist = 0; // 法术抗性(填明日方舟里的法抗）

		private bool Inattack;
		private int attackCD = 0;
		private int attackCDMax = 60; // 攻击冷却时间
		private int AttackDamage = 28; // 攻击伤害
		private int Attackrange = 45; // 攻击范围

		private bool InDeath = false; // 死亡状态
		private int fadeTime = 0; // 死亡淡出时间
		private int fadeOutTimer = 30; // 用于控制淡出效果的计时器
									   // 动画常量
		private int frameNumber = 30;//一共多少帧
		private int DefaultFrame = 7;//静止最后一帧是第几帧（-1）
		private int MoveStartFrame = 8;//移动开始帧
		private int MoveEndFrame = 13;//移动结束帧
		private int AttackStartFrame = 14;//攻击开始帧
		private int AttackEndFrame = 21;//攻击结束帧哦~
		private int DeathStartFrame = 22;//死亡开始帧
		private int DeathEndFrame = 29;//死亡结束帧
		private int FrameSpeed = 7;//帧速率
		private int framecounter = 0;//帧计数器

		//注：每个阶段的帧数为：结束帧-开始帧+1
		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
			bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[] {
				BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Times.NightTime,
				BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.TheDungeon,
				new FlavorTextBestiaryInfoElement(Language.GetTextValue("Mods.ArknightsMod.Bestiary.DoubleSword")),
			});
		}
		public override void SetStaticDefaults() {
			Main.npcFrameCount[NPC.type] = frameNumber;
		}
		public override void SetDefaults() {
			NPC.width = 20;
			NPC.height = 40;
			NPC.lifeMax = 150;
			NPC.damage = AttackDamage/2;
			NPC.defense = 10;
			NPC.knockBackResist = 0.5f;
			NPC.scale = 2f;
			NPC.value = 200f;
			NPC.HitSound = SoundID.NPCHit1;
			//NPC.DeathSound = SoundID.NPCDeath7;
		}
		public override void FindFrame(int frameHeight) {

			framecounter++;
			if (framecounter >= FrameSpeed) {
				NPC.frame.Y += frameHeight;
				framecounter = 0;
			}
			if (InMove & NPC.velocity.X != 0) {
				if (NPC.frame.Y < MoveStartFrame * frameHeight || NPC.frame.Y > MoveEndFrame * frameHeight) {
					NPC.frame.Y = MoveStartFrame * frameHeight;
				}
			}
			else if (Inattack) {
				if (NPC.frame.Y < AttackStartFrame * frameHeight || NPC.frame.Y > AttackEndFrame * frameHeight) {
					NPC.frame.Y = AttackStartFrame * frameHeight;
				}
			}
			else if(InDeath) {
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
		public override void AI() {
			//出场效果
			if (fadeTimer > 0) {
				fadeTimer--;
				NPC.alpha = (int)4f * fadeTimer;
				NPC.color = Color.Lerp(Color.Black, Color.White, 1f - fadeTimer / 60f);
			}
			//索敌
			NPC.TargetClosest(true);
			Player p = Main.player[NPC.target];
			NPC.spriteDirection = -NPC.direction;


			//速度系统
			if (InMove) {
				if (NPC.position.X - p.position.X > Attackrange) {
					if (NPC.velocity.X > -maxspeed) {
						NPC.velocity.X -= 0.2f;
					}
				}
				if (NPC.position.X - p.position.X < -Attackrange) {
					if (NPC.velocity.X < maxspeed) {
						NPC.velocity.X += 0.2f;
					}
				}
			}

			//AI状态系统
			attackCD++;
			if (!Main.player[NPC.target].dead) {

				if (!Inattack) {
					InMove = true;
				}
				if (InMove) {
					if ((NPC.position.X - p.position.X < Attackrange && NPC.position.X - p.position.X > -Attackrange) && attackCD > attackCDMax) {
						InMove = false;
						Inattack = true;
						attackCD = 0;
					}

				}
				if (Inattack) {
					NPC.velocity.X = 0;
					NPC.damage = 0;
					if (attackCD == 14) {
						Projectile.NewProjectile(NPC.GetSource_FromThis(), new Vector2(NPC.Center.X , NPC.Center.Y), new Vector2(0, 0), ModContent.ProjectileType<DoubleCut>(), AttackDamage/2, 0.8f);
					}
					if (attackCD == 35) {
						Projectile.NewProjectile(NPC.GetSource_FromThis(), new Vector2(NPC.Center.X, NPC.Center.Y), new Vector2(0, 0), ModContent.ProjectileType<DoubleCut>(), AttackDamage / 2, 0.8f);
					}
					if (attackCD >= FrameSpeed * (AttackEndFrame - AttackStartFrame + 1)) {
						InMove = true;
						Inattack = false;
						NPC.damage = AttackDamage / 2;// 恢复伤害
						attackCD = 0; // 重置攻击冷却
					}
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
						int DUST= Dust.NewDust(
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
					SoundStyle ghostSound = SoundID.NPCDeath7 with {
						Pitch = -0.4f, // 范围[-1.0, 1.0]，-0.5表示降低八度
						Volume = 0.4f  // 可选调整音量
					};
					SoundEngine.PlaySound(ghostSound, NPC.Center);
					NPC.life = 0;
					NPC.checkDead();// 消逝吧！
				}
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
		public override void ModifyHitByProjectile(Projectile projectile, ref NPC.HitModifiers modifiers) {
			if (SpellDamageConfig.SpellProjectiles.Contains(projectile.type)) {
				// 法术伤害无视护甲
				modifiers.ScalingArmorPenetration += 1f;
				// 法术抗性
				modifiers.FinalDamage *= 1f-(SpellResist/100);
				if (SpellResist < 20) {
					for (int i = 0; i < 3; i++) {
						Dust.NewDust(NPC.position, NPC.width, NPC.height,
							DustID.MagicMirror, 0, 0, 150, Color.LightBlue, 0.7f);
					}
				}
				if (SpellResist > 40) {
					for (int i = 0; i < 3; i++) {
						Dust.NewDust(NPC.position, NPC.width, NPC.height,
							DustID.Shadowflame, 0, 0, 150, Color.LightBlue, 0.7f);
					}
				}
			}
		}
	}
	public class DoubleCut: ModProjectile {
		public override string Texture => "ArknightsMod/Content/NPCs/Enemy/ThroughChapter4/explode";

		public override void SetDefaults() {
			Projectile.width = 100;
			Projectile.height = 100;
			Projectile.friendly = false;
			Projectile.hostile = true;
			Projectile.penetrate = 9999;
			Projectile.timeLeft = 10;
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
			target.GetModPlayer<Common.Players.ImmunePlayer>().ImmuneMultiplier = 0.6f; // 免疫倍数
		}
	}
}
