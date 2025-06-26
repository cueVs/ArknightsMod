using ArknightsMod.Common.Damageclasses;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Content.NPCs.Enemy.Evolution
{
	public class Evolution: ModNPC {
		//转阶段参数
		private bool Stage1 = true;
		private bool Stage2 = false;
		private bool Stage3 = false;
		private int Stage1Time = 0;
		private int Stage2Time = 0;
		private int Stage3Time = 0;
		private int Stage1MaxTime = 60; // 一分钟进化
		private int Stage2MaxTime = 60; // 一分钟进化
		private int Stage2ImmuneTime = 10; // 10秒免疫伤害
		private int Stage3ImmuneTime = 10; // 10秒免疫伤害
		//数值参数
		private int defense = 50;
		private int SpellResist = 50; // 法术抗性(填明日方舟里的法抗）
		private int Health = 5000;
		private int AttackDamage1 = 70;//接触伤害
		private int AttackDamage2 = 60;//射弹伤害
		private int AttackDamage3 = 80;//真实伤害
		//动画参数
		private int frameNumber = 70; //一共多少帧
		private int Stage1Frame = 10; //第一阶段最后一帧(-1)
		private int Change12Frame = 19; //一转二阶段最后一帧(-1)
		private int Stage2Frame = 29; //第二阶段最后一帧(-1)
		private int Change23Frame = 46; //二转三阶段最后一帧(-1)
		private int Stage3Frame = 56;//三阶段最后一帧(-1)
		private int frameSpeed = 7; //帧速率
		private int framecounter = 0; //帧计数器
		private int fadeTimer = 60;
		private int LeftShield = 0;
		private int RightShield = 0;
		private int AllShield = 0;

		public override void SetStaticDefaults() {
			Main.npcFrameCount[Type] = frameNumber;
			NPCID.Sets.NPCBestiaryDrawModifiers value = new NPCID.Sets.NPCBestiaryDrawModifiers() { // Influences how the NPC looks in the Bestiary
				Velocity = 1f // Draws the NPC in the bestiary as if its walking +1 tiles in the x direction
			};
			NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, value);
		}
		public override void SetDefaults() {
			NPC.lifeMax = Health;
			NPC.defense = defense;
			NPC.damage = AttackDamage1;
			NPC.scale = 2f;
			NPC.boss = true;
			NPC.knockBackResist = 0;
			Music = MusicLoader.GetMusicSlot("ArknightsMod/Music/Evolution");
			if (Main.expertMode || Main.masterMode) {
				NPC.lifeMax = (int)(NPC.lifeMax * 0.75);
				NPC.damage = (int)(NPC.damage * 0.75);
			}
			if (Stage1 || Stage2) {

				NPC.width = 55;
				NPC.height = 100;
			}
			if (Stage3) {
				NPC.width = 75;
				NPC.height = 100;
			}
			
		}
		public override void FindFrame(int frameHeight) {
			framecounter++;

			if (Stage1) {
				if (framecounter >= frameSpeed) {
					NPC.frame.Y += frameHeight;
					framecounter = 0;
				}
				if (NPC.frame.Y >= Stage1Frame * frameHeight) {
					NPC.frame.Y = 0;
				}
			}
			else if (Stage2) {
				if (framecounter >= frameSpeed) {
					NPC.frame.Y += frameHeight;
					framecounter = 0;
				}
				if (NPC.frame.Y <= Stage1Frame * frameHeight) {
					NPC.frame.Y = (Stage1Frame + 1) * frameHeight;
				}
				if (NPC.frame.Y >= Stage2Frame * frameHeight) {
					NPC.frame.Y = (Change12Frame + 1) * frameHeight;
				}
			}
			else if (Stage3) {
				if (framecounter >= frameSpeed) {
					NPC.frame.Y += frameHeight;
					framecounter = 0;
				}
				if (NPC.frame.Y <= Stage2Frame * frameHeight) {
					NPC.frame.Y = (Stage2Frame + 1) * frameHeight;
				}
				if (NPC.frame.Y >= Stage3Frame * frameHeight) {
					NPC.frame.Y = (Change23Frame + 1) * frameHeight;
				}
			}
		}
		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
			Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
			Texture2D Shield =ModContent.Request<Texture2D>("ArknightsMod/Common/VisualEffects/EvolutionShield").Value;
			Texture2D AllShieldText = ModContent.Request<Texture2D>("ArknightsMod/Common/VisualEffects/Evolutionshield3").Value;
			NPC.spriteDirection = NPC.direction;
			if (fadeTimer > 0) {
				fadeTimer--;
			}
			Color drawcolor = Color.Lerp(new Color(0,0,0,65), new Color(255,255,255,255), 1f - fadeTimer / 60f);
			Color LeftShieldcolor = Color.Lerp(new Color(255, 120, 0, 128), new Color(255, 120, 0, 10), 1f - LeftShield / 30f);
			Color RightShieldcolor = Color.Lerp(new Color(255, 120, 0, 200), new Color(255, 120, 0, 70), 1f - RightShield / 30f);
			Color AllShieldColor = Color.Lerp(new Color(255, 255, 255, 200), new Color(0, 0,0, 0), 1f - AllShield / 60f);
			// 动态计算原点（水平居中，底部对齐碰撞箱）
			Vector2 origin1 = new Vector2(110, 110);
			Vector2 origin2 = new Vector2(85, 110);

			if (NPC.spriteDirection > 0) {
				spriteBatch.Draw(
				texture,
				NPC.Center - screenPos + new Vector2(0, 4f), // 整体下移4像素
				NPC.frame,
				drawcolor,
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
				NPC.Center - screenPos + new Vector2(0, 4f), // 整体下移4像素
				NPC.frame,
				drawcolor,
				NPC.rotation,
				origin2,
				NPC.scale,
				NPC.spriteDirection == -1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally,
				0f
				);

			}
			if (LeftShield > 0) {
				spriteBatch.Draw(
				Shield,
				NPC.Center - screenPos + new Vector2(-130, -130f), 
				new Rectangle(0,0,Shield.Width, Shield.Height),
				LeftShieldcolor,
				0,
				new Vector2(0, 0),
				1f,
				0f,
				0f
				);
			}
			if (RightShield > 0) {
				spriteBatch.Draw(
				Shield,
				NPC.Center - screenPos + new Vector2(-130, -130f),
				new Rectangle(0, 0, Shield.Width, Shield.Height),
				LeftShieldcolor,
				0,
				new Vector2(0, 0),
				1f,
				SpriteEffects.FlipHorizontally,
				0
				);
			}
			if (AllShield > 0) {
					spriteBatch.Draw(
					AllShieldText,
					NPC.Center - screenPos + new Vector2(-130, -130f),
					new Rectangle(0, 0, AllShieldText.Width, AllShieldText.Height),
					AllShieldColor,
					0,
					new Vector2(0, 0),
					1f,
					0,
					0
					);
			}
			return false;
		}
		
		public override void OnSpawn(IEntitySource source) {
			fadeTimer = 60; // 持续60帧
			NPC.color = Color.Black; // 初始为纯黑
			NPC.alpha = 240;
		}
		public override void AI() {
			//动画相关
			if (LeftShield >= 0) {
				LeftShield--;
			}
			if (RightShield >= 0) {
				RightShield--;
			}
			if (AllShield >= 0) {
				AllShield--;
			}
			//控制转阶段
			if (Stage1) {
				Stage1Time++;
				if (NPC.life <= NPC.lifeMax * 0.6f || Stage1Time > Stage1MaxTime * 60) {
					Stage1 = false;
					Stage2 = true;
				}

			}
			if (Stage2) {
				Stage2Time++;
				if (NPC.life <= NPC.lifeMax * 0.2f || Stage2Time > Stage2MaxTime * 60) {
					Stage2 = false;
					Stage3 = true;
				}
			}
			if (Stage3) {
				NPC.width = 170;
				NPC.height = 200;
			}
			NPC.TargetClosest(true);

		}
		public override void ModifyHitByProjectile(Projectile projectile, ref NPC.HitModifiers modifiers) {
			if (SpellDamageConfig.SpellProjectiles.Contains(projectile.type)) {
				// 法术伤害无视护甲
				modifiers.ScalingArmorPenetration += 1f;
				// 法术抗性
				modifiers.FinalDamage *= 1f - (SpellResist / 100);
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
			if (Stage1 & (projectile.velocity.X >= NPC.velocity.X || projectile.position.X <= NPC.position.X)) {
				modifiers.FinalDamage *= 0.2f;
				RightShield = 30;
			}
			if (Stage2 & (projectile.velocity.X <= NPC.velocity.X || projectile.position.X >= NPC.position.X)) {
				modifiers.FinalDamage *= 0.2f;
				LeftShield = 30;
			}
			if (Stage3) {
				modifiers.FinalDamage *= 0.01f;
				AllShield = 60;
			}

		}
		public override void ModifyHitByItem(Player player, Item item, ref NPC.HitModifiers modifiers) {
			if (Stage1 & player.position.X <= NPC.position.X) {
				modifiers.FinalDamage *= 0.2f;
				RightShield = 30;
			}
			if (Stage2 & player.position.X >= NPC.position.X) {
				modifiers.FinalDamage *= 0.2f;
				LeftShield = 30;
			}
			if (Stage3) {
				modifiers.FinalDamage *= 0.01f;
				AllShield= 60;
			}
		}
	}
		

}

