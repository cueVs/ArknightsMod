using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ModLoader.Utilities;
using Terraria.DataStructures;
using ArknightsMod.Content.Items.Placeable.Banners;
using Terraria.Localization;
using Terraria.UI;
using static Terraria.ModLoader.ModContent;
using ArknightsMod.Content.Projectiles;
using System;
using Microsoft.Xna.Framework;


namespace ArknightsMod.Content.NPCs.Enemy.GT
{
	// Party Zombie is a pretty basic clone of a vanilla NPC. To learn how to further adapt vanilla NPC behaviors, see https://github.com/tModLoader/tModLoader/wiki/Advanced-Vanilla-Code-Adaption#example-npc-npc-clone-with-modified-projectile-hoplite
	public class AcidOgSlug : ModNPC
	{
		private int status;
		private int Frame_State;

		public override bool IsLoadingEnabled(Mod mod)
		{
			return ModContent.GetInstance<MonsterConfig>().EnableAcidOgSlug;
		}

		private enum ActionState {
			Walk,
			Attack
		}


		public override void SetStaticDefaults() {
			Main.npcFrameCount[Type] = 15;

			NPCID.Sets.NPCBestiaryDrawModifiers value = new NPCID.Sets.NPCBestiaryDrawModifiers() { // Influences how the NPC looks in the Bestiary
				Velocity = 1f // Draws the NPC in the bestiary as if its walking +1 tiles in the x direction
			};
			NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, value);
		}

		public override void SetDefaults() {
			NPC.width = 40;
			NPC.height = 34;
			NPC.damage = 12;
			NPC.defense = 8;
			NPC.lifeMax = 40;

			NPC.HitSound = SoundID.NPCHit1;
			NPC.DeathSound = SoundID.NPCDeath1;

			NPC.value = 30f;
			NPC.knockBackResist = 0.5f;
			NPC.aiStyle = NPCAIStyleID.Snail; // Passive Worm AI

			NPC.scale = 0.85f;

			Banner = NPC.type;
			BannerItem = ItemType<AcidOgSlugBanner>();
		}

		public override void ModifyNPCLoot(NPCLoot npcLoot) {

			npcLoot.Add(ItemDropRule.Common(ItemType<Items.Material.Oriron>(), ModContent.GetInstance<Dropconfig>().DropAcidOgSlug, 1, 2));

		}

		public override float SpawnChance(NPCSpawnInfo spawnInfo) {
			return SpawnCondition.Underground.Chance * 0.4f;
			// return SpawnCondition.OverworldNightMonster.Chance * 1f; // Spawn with 1/5th the chance of a regular zombie.
		}

		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
			// We can use AddRange instead of calling Add multiple times in order to add multiple items at once
			bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[] {
				// Sets the spawning conditions of this NPC that is listed in the bestiary.
				BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Underground,


				// Sets the description of this NPC that is listed in the bestiary.
				new FlavorTextBestiaryInfoElement(Language.GetTextValue("Mods.ArknightsMod.Bestiary.AcidOgSlug")),

			});
		}

		public override void FindFrame(int frameHeight) {
			NPC.spriteDirection = NPC.direction;


			if(Frame_State == (int)ActionState.Walk) {
				int startFrame = 0;
				int finalFrame = 4;
				int frameSpeed = 6;

				if (NPC.velocity.Length() != 0) {
					NPC.frameCounter += 0.6f;
					NPC.frameCounter += NPC.velocity.Length() / 4f; // Make the counter go faster with more movement speed
				}

				if (NPC.frameCounter > frameSpeed) {
					NPC.frameCounter = 0;
					if (NPC.velocity.Length() != 0 && status != 2) {
						NPC.frame.Y += frameHeight;
					}

					if (NPC.frame.Y > finalFrame * frameHeight) {
						NPC.frame.Y = startFrame * frameHeight;
					}
				}
			}
			else if(Frame_State == (int)ActionState.Attack) {
				int startFrame = 5;
				int finalFrame = 14;
				int frameSpeed = 5;
				if(NPC.frame.Y < startFrame * frameHeight) {
					NPC.frame.Y = startFrame * frameHeight;
				}

				NPC.frameCounter += 1f;

				if (NPC.frameCounter > frameSpeed) {
					NPC.frameCounter = 0;
					NPC.frame.Y += frameHeight;

					if (NPC.frame.Y > finalFrame * frameHeight) {
						NPC.frame.Y = 0 * frameHeight;
						Frame_State = (int)ActionState.Walk;
					}
				}
			}
		}

		public override void AI() {
			if (NPC.target < 0 || NPC.target == 255 || Main.player[NPC.target].dead || !Main.player[NPC.target].active) {
				NPC.TargetClosest();
			}
			if (NPC.HasValidTarget && Main.netMode != NetmodeID.MultiplayerClient) {
				if (Math.Abs(Main.player[NPC.target].Center.X - NPC.Center.X) > 280 || Math.Abs(Main.player[NPC.target].Center.Y - NPC.Center.Y) > 180) {
					RandomWalk();
				}
				else {
					if (Frame_State == (int)ActionState.Walk) {
						Approach();
					}
					else if (Frame_State == (int)ActionState.Attack) {
						Attack();
					}
					else {
						Frame_State = (int)ActionState.Walk;
					}
				}
			}

			base.AI();
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

		private void RandomWalk() {
			Frame_State = (int)ActionState.Walk;
			if (NPC.ai[3] % 180 == 0) {
				NPC.ai[3] = 0;
				status = Main.rand.Next(5);
				if (status == 1 || status == 3) {
					NPC.direction = (Main.player[NPC.target].Center.X > NPC.Center.X).ToDirectionInt();
				}
				if (status == 4) {
					NPC.direction *= -1;
				}
			}
			switch (status) {
				case 0:
					NPC.velocity.X = 1f * NPC.direction;
					break;
				case 1:
					NPC.velocity.X = 0.9f * NPC.direction;
					break;
				case 2:
					NPC.velocity.X *= 0;
					break;
				case 3:
					NPC.velocity.X = 1.3f * NPC.direction;
					break;
				case 4:
					NPC.velocity.X = 0.7f * NPC.direction;
					break;
			}
			if (NPC.collideX) {
				NPC.velocity.Y = 1.2f * NPC.directionY;
			}
			NPC.ai[3]++;
		}

		private void Attack() {
			NPC.direction = (Main.player[NPC.target].Center.X > NPC.Center.X).ToDirectionInt();
			NPC.velocity.X *= 0;
			NPC.velocity.Y *= 0;
			NPC.ai[3]++;
			if (NPC.ai[3] == 35) {
				Vector2 position = NPC.Center;

				float addHight;
				if((NPC.Center.Y - Main.player[NPC.target].Center.Y) > 20) {
					addHight = 10f;
				}
				else {
					addHight = 0f;
				}

				float x = Main.player[NPC.target].Center.X - NPC.Center.X;
				float y = Main.player[NPC.target].Center.Y - NPC.Center.Y - 20f - addHight;

				float theta = new Vector2(x, y).ToRotation();
				float addSpeed = Math.Abs(Main.player[NPC.target].Center.X - NPC.Center.X) / 60; // Change speed according to distance

				Projectile.NewProjectile(NPC.GetSource_FromAI(), position, (8f + addSpeed) * new Vector2((float)Math.Cos(theta), (float)Math.Sin(theta)), ProjectileType<AcidOgSlugProjectile>(), 10, 0f, Main.myPlayer);
			}
		}

		private void Approach() {
			if(Math.Abs(Main.player[NPC.target].Center.X - NPC.Center.X) > 2) {
				NPC.direction = (Main.player[NPC.target].Center.X > NPC.Center.X).ToDirectionInt();
				NPC.velocity.X = 1f * NPC.direction;
			}
			else {
				NPC.velocity.X *= 0;
			}
			if (NPC.collideX) {
				NPC.velocity.Y = 1.2f * NPC.directionY;
			}
			NPC.ai[3]++;

			if (NPC.ai[3] % 100 == 0 && NPC.collideY) {
				Frame_State = (int)ActionState.Attack;
				NPC.ai[3] = 0;
			}
		}
	}
}
