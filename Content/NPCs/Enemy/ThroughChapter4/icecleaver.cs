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
using System.Reflection.Metadata;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace ArknightsMod.Content.NPCs.Enemy.ThroughChapter4
{
	public class icecleaver:ModNPC
	{
		public override void SetStaticDefaults() {
			Main.npcFrameCount[Type] = 21;
			NPCID.Sets.NPCBestiaryDrawModifiers value = new NPCID.Sets.NPCBestiaryDrawModifiers() { // Influences how the NPC looks in the Bestiary
				Velocity = 1f // Draws the NPC in the bestiary as if its walking +1 tiles in the x direction
			};
			NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, value);
		}
		public override void SetDefaults() {
			NPC.width = 35;
			NPC.height = 60;
			NPC.damage = 38;
			NPC.defense = 50;
			NPC.lifeMax = 1600;
			NPC.HitSound = SoundID.NPCHit1;
			NPC.DeathSound = SoundID.NPCDeath3;
			NPC.value = 100f;
			NPC.knockBackResist = 0.20f;
			NPC.aiStyle = -1;
			NPC.scale = 1f;
		}
		public override void FindFrame(int frameHeight) {

			attackframeY = 10 * frameHeight;
			NPC.TargetClosest(true);
			framecounter++;
			if (framecounter >= Framespeed) {
				NPC.frame.Y += frameHeight;
				framecounter = 0;
			}
			if (walk == true && (NPC.frame.Y <= attackframeY || NPC.frame.Y > (20 * frameHeight))) {
				NPC.frame.Y = attackframeY;
			}
			if (attack == true && (NPC.frame.Y > attackframeY)) {
				NPC.frame.Y = 0;
			}

		}
		
		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
			Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
			NPC.spriteDirection = -NPC.direction;
			// 动态计算原点（水平居中，底部对齐碰撞箱）
			Vector2 origin1 = new Vector2(NPC.frame.Width *2/ 3f, NPC.frame.Height - 55);
			Vector2 origin2 = new Vector2(NPC.frame.Width / 3f, NPC.frame.Height - 55);

			if (NPC.spriteDirection > 0) {
				spriteBatch.Draw(
				texture,
				NPC.Center - screenPos + new Vector2(0, 4f), // 整体下移4像素
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
				NPC.Center - screenPos + new Vector2(0, 4f), // 整体下移4像素
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

		private int AttackCD = 0;
		private bool attack;
		private bool walk = true;
		private int Framespeed = 7;
		private int framecounter;
		private int attackframeY;
		private float maxspeed = 1.2f;
		private int jumpCD = 0;

		
		public override void AI() {
			Player p = Main.player[NPC.target];
			if (walk == true) {
				NPC.spriteDirection = -NPC.direction;
				AttackCD++;
				if (NPC.position.X - p.position.X < -5) {
					if (NPC.velocity.X < maxspeed) {
						NPC.velocity.X += 0.4f;
					}
					if (NPC.velocity.X >= maxspeed) {
						NPC.velocity.X = maxspeed;
					}
				}

				if (NPC.position.X - p.position.X > 5) {
					if (NPC.velocity.X > -maxspeed) {
						NPC.velocity.X += -0.4f;
					}
					if (NPC.velocity.X <= -maxspeed) {
						NPC.velocity.X = -maxspeed;
					}
				}

				if (Math.Abs(NPC.velocity.X) <= 0.5f) {
					jumpCD++;
				}
				if (jumpCD >= 180) {
					jumpCD = 0;
					NPC.velocity.Y = -7.2f;
				}
				if (AttackCD >= 200 && Math.Abs(NPC.position.X - p.position.X) <= 100 && !attack && Math.Abs(NPC.position.Y - p.position.Y) <= 100) {
					walk = false;
					attack = true;
					AttackCD = 0;
				}
			}
			if (attack == true) {
				NPC.velocity.X = 0;
				AttackCD++;
				NPC.damage = 76;
				if (AttackCD == 25) {
					if (NPC.spriteDirection < 0) {
						Projectile.NewProjectile(NPC.GetSource_FromThis(), new Vector2(NPC.Center.X + 60, NPC.Center.Y + 40), new Vector2(0, 0), ModContent.ProjectileType<Icebreak>(), 38, 0.8f);

					}
					if (NPC.spriteDirection > 0) {
						Projectile.NewProjectile(NPC.GetSource_FromThis(), new Vector2(NPC.Center.X - 60, NPC.Center.Y + 40), new Vector2(0, 0), ModContent.ProjectileType<Icebreak>(), 38, 0.8f);

					}
				}
				
				if (AttackCD > 70) {
					attack = false;
					walk = true;
					AttackCD = 0;
					NPC.damage = 38;
				}
			}
		}

		public override bool? CanFallThroughPlatforms() {
			Player player = Main.player[NPC.target];
			return (player.position.Y + player.height) - (NPC.position.Y + NPC.height) > 0;
		}
		public override void ModifyHitPlayer(Player target, ref Player.HurtModifiers modifiers) {
			Player p = Main.player[NPC.target];
			if (p.frozen == true) {
				modifiers.SourceDamage *= 3;

			}
			if (Main.expertMode)
				modifiers.SourceDamage *= 1.5f; // 专家模式伤害 ×1.5
			if (Main.masterMode)
				modifiers.SourceDamage *= 2f;   // 大师模式伤害 ×2
		}
		public override void ModifyNPCLoot(NPCLoot npcLoot) {

			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Items.Material.Device>(), 8, 1, 1));
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Items.Material.Polyketon>(), 8, 1, 1));

		}
	}
	public class Icebreak : ModProjectile
	{
		public override void SetDefaults() {
			Projectile.width = 180;
			Projectile.height = 180;
			Projectile.damage = 76;
			Projectile.penetrate = 9999;
			Projectile.tileCollide = false;
			Projectile.hostile = true;
		}
		private int flytime;
		public override void AI() { 
			flytime++;
			if (flytime >= 50) {
				Projectile.timeLeft = 1;
			}
			Dust dust;
			dust = Dust.NewDustDirect(Projectile.position,135, 135, DustID.Ice, 0, 0, 0, default, 1);
			dust.noGravity = true;
		}
		public override void ModifyHitPlayer(Player target, ref Player.HurtModifiers modifiers) {
			if (target.frozen == true) {
				modifiers.SourceDamage *= 3;
			}
			if (Main.expertMode)
				modifiers.SourceDamage *= 0.75f; // 专家模式伤害 ×1.5
			if (Main.masterMode)
				modifiers.SourceDamage *= 0.7f;   // 大师模式伤害 ×2
		}
	}
}
