using ArknightsMod.Content.Items.Material;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;


namespace ArknightsMod.Content.NPCs.Enemy.Chapter6
{
	public class SnowSoldier : ModNPC {
		public override void SetStaticDefaults() {
			Main.npcFrameCount[Type] = 19;
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
			NPC.HitSound = SoundID.NPCHit1;
			NPC.DeathSound = SoundID.NPCDeath3;
			NPC.value = 100f;
			NPC.knockBackResist = 0.35f;
			NPC.aiStyle = -1;
			NPC.scale = 1f;
		}

		
		private int AttackCD=0;
		private bool attack;
		private bool walk=true;
		private int Framespeed = 7;
		private int framecounter;
		private int attackframeY;
		private float maxspeed = 2.0f;
		private int jumpCD = 0;
		
		public override void FindFrame(int frameHeight) {

			attackframeY = 10 * frameHeight;
			NPC.TargetClosest(true);
			NPC.frameCounter++;
			/*if(NPC.frame.Y == 0) {
				NPC.frame.Y = -8;
			}
			if (NPC.frameCounter >= Framespeed) {
				NPC.frame.Y += frameHeight;
				NPC.frameCounter = 0;
			}*/

			//26.6.12,
			//重写
			if (!attack) {
				if (NPC.velocity.X == 0) {
					NPC.frame.Y = 3 * frameHeight;
					//NPC.frame.Y = 9 * frameHeight;
				}
				else
					NPC.frame.Y = (int)((NPC.frameCounter / Framespeed) % 9) * frameHeight;

			}
			else {
				if (AttackCD == 1)
					NPC.frame.Y = attackframeY;
				else if (NPC.frameCounter % Framespeed == 0)
					NPC.frame.Y += frameHeight;
			}
		}
		public override void AI() {
			Player p = Main.player[NPC.target];
			if (walk == true) {
				NPC.spriteDirection = -NPC.direction;
				AttackCD++;
				/*
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
				if (AttackCD >= 150 && Math.Abs(NPC.position.X - p.position.X) <= 16 && !attack && Math.Abs(NPC.position.Y - p.position.Y) <= 16) {
					walk = false;
					attack = true;
					AttackCD = 0;
				}
				*/
				//26.6.11 改
				//嗯 约等于重写
				NPC.velocity.X = MathHelper.Lerp(NPC.velocity.X, NPC.direction * maxspeed, 0.1f);
				IceCleaver.LandNPCMovementLogic(NPC, NPC.width, NPC.height, 7);
				if (AttackCD >= 150 && Math.Abs(NPC.position.X - p.position.X) <= 25 && Math.Abs(NPC.position.Y - p.position.Y) <= 16) {

					NPC.velocity.X = 0;
					if (AttackCD >= 100 && !attack) {
						walk = false;
						attack = true;
						AttackCD = 0;
					}
				}
				if (Math.Abs(NPC.position.X - p.position.X) >= 40) {
					AttackCD = 114514;
				}

			}
			if (attack == true) {
				NPC.velocity.X = 0;
				AttackCD++;
				NPC.damage = 36;
				if (AttackCD > 54) {
					attack = false;
					walk = true;
					AttackCD = 0;
					NPC.damage = 18;
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
				modifiers.SourceDamage *=1.5f;

			}
			if (Main.expertMode)
				modifiers.SourceDamage *= 1.5f; // 专家模式伤害 ×1.5
			if (Main.masterMode)
				modifiers.SourceDamage *= 2f;   // 大师模式伤害 ×2
		}
		public override void ModifyNPCLoot(NPCLoot npcLoot) {

			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Device>(), 8, 1, 1));
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Polyketon>(), 8, 1, 1));

		}

	}
}
