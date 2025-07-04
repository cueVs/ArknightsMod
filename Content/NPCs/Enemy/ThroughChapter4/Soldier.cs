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

namespace ArknightsMod.Content.NPCs.Enemy.ThroughChapter4
{
	// Party Zombie is a pretty basic clone of a vanilla NPC. To learn how to further adapt vanilla NPC behaviors, see https://github.com/tModLoader/tModLoader/wiki/Advanced-Vanilla-Code-Adaption#example-npc-npc-clone-with-modified-projectile-hoplite
	public class Soldier : ModNPC
	{
		public override void SetStaticDefaults() {
			Main.npcFrameCount[Type] = 28;
		}


		public override void SetDefaults() {
			NPC.width = 9;
			NPC.height = 20;
			NPC.damage = 16;
			NPC.defense = 5;
			NPC.lifeMax = 105;
			NPC.HitSound = SoundID.NPCHit1;
			NPC.value = 60f;
			NPC.knockBackResist = 0.5f;
			NPC.aiStyle = -1; // Fighter AI, important to choose the aiStyle that matches the NPCID that we want to mimic
			NPC.scale = 2f;

		}
		private int atimer;
		private int rest;
		private int rushtime;
		private int attackCD;
		private int jumpCD;
		private int walktime;
		private int attacktime;
		private bool rush;
		private bool walk;
		private bool attack;
		private float maxspeed1 = 1.1f;
		private float maxspeed2 = 4.5f;
		private int Framespeed = 9;
		public override void FindFrame(int frameHeight) {
			NPC.TargetClosest(true);

			Player p = Main.player[NPC.target];
			NPC.frameCounter++;
			if (NPC.frameCounter >= Framespeed) {
				NPC.frame.Y += frameHeight;
				NPC.frameCounter = 0;
			}
			if (walk) {
				if (walktime == 1) {
					NPC.frame.Y = 0;
				}
				if (NPC.frame.Y >= 13 * frameHeight) {
					NPC.frame.Y = 0;
				}

			}
			if (attack) {
				if (attacktime == 1) {
					NPC.frame.Y = 22 * frameHeight;
				}
				if (NPC.frame.Y >= 27 * frameHeight || NPC.frame.Y <= 21 * frameHeight) {
					NPC.frame.Y = 22 * frameHeight;
				}

			}
			if (rush) {
				if (rushtime == 1) {
					NPC.frame.Y = 14 * frameHeight;
				}
				if (NPC.frame.Y >= 21 * frameHeight || NPC.frame.Y <= 13 * frameHeight) {
					NPC.frame.Y = 14 * frameHeight;
				}

			}

		}
		public override void AI() {
			NPC.TargetClosest(true);
			Player p = Main.player[NPC.target];


			if (Math.Abs(NPC.position.X - p.position.X) <= 200 && attack == false && !rush) {
				rush = true;
				walk = false;
				rushtime = 0;

			}
			if (attackCD > 0 && Math.Abs(NPC.position.X - p.position.X) <= 16 && !attack && Math.Abs(NPC.position.Y - p.position.Y) <= 16) {
				rush = false;
				attack = true;
				walk = false;

				attacktime = 0;

			}
			if (Math.Abs(NPC.position.X - p.position.X) >= 200 && attack == false && walk == false) {
				walk = true;
				rush = false;
				walktime = 0;
			}

			if (walk) {
				NPC.spriteDirection = -NPC.direction;
				walktime++;
				attackCD++;
				if (NPC.position.X - p.position.X < -50) {
					if (NPC.velocity.X < maxspeed1) {
						NPC.velocity.X += 0.2f;
					}
					if (NPC.velocity.X >= maxspeed1) {
						NPC.velocity.X = maxspeed1;
					}
				}

				if (NPC.position.X - p.position.X > 5) {
					if (NPC.velocity.X > -maxspeed1) {
						NPC.velocity.X += -0.2f;
					}
					if (NPC.velocity.X <= -maxspeed1) {
						NPC.velocity.X = -maxspeed1;
					}
				}

				if (Math.Abs(NPC.velocity.X) <= 0.5f) {
					jumpCD++;
				}
				if (jumpCD >= 60) {
					jumpCD = 0;
					NPC.velocity.Y = -6;
				}
			}
			if (rush) {
				NPC.spriteDirection = -NPC.direction;
				rushtime++;
				attackCD++;
				if (NPC.position.X - p.position.X < -50 && NPC.velocity.X < maxspeed2) {
					NPC.velocity.X += 0.6f;
				}
				if (NPC.position.X - p.position.X > 50 && NPC.velocity.X > -maxspeed2) {
					NPC.velocity.X += -0.6f;
				}

				if (Math.Abs(NPC.velocity.X) <= 1.8f) {
					jumpCD++;
				}
				if (jumpCD >= 60) {
					jumpCD = 0;
					NPC.velocity.Y = -9;
				}
			}
			if (attack) {
				NPC.velocity.X = 0;
				attacktime++;
				if (attacktime > 54) {
					attack = false;
					rush = true;
					attackCD = 0;
				}
			}
		}
		public override bool? CanFallThroughPlatforms() {
			Player player = Main.player[NPC.target];
			return (player.position.Y + player.height) - (NPC.position.Y + NPC.height) > 0;
		}
		public override void ModifyNPCLoot(NPCLoot npcLoot) {

			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Items.Material.Device>(), 8, 1, 1));
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Items.Material.Polyketon>(), 8, 1, 1));

		}

		public override float SpawnChance(NPCSpawnInfo spawnInfo) {
			return SpawnCondition.OverworldNightMonster.Chance * 1f; // Spawn with 1/5th the chance of a regular zombie.
		}

		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
			// We can use AddRange instead of calling Add multiple times in order to add multiple items at once
			bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[] {
				// Sets the spawning conditions of this NPC that is listed in the bestiary.
				BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Times.NightTime,
				BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Surface,


				// Sets the description of this NPC that is listed in the bestiary.
				new FlavorTextBestiaryInfoElement(Language.GetTextValue("Mods.ArknightsMod.Bestiary.Soldier")),

			});
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
			if (projectile.CountsAsClass(DamageClass.Magic)) {
				// 法术伤害无视护甲
				modifiers.ScalingArmorPenetration += 1f;

				for (int i = 0; i < 3; i++) {
					Dust.NewDust(NPC.position, NPC.width, NPC.height,
						DustID.MagicMirror, 0, 0, 150, Color.LightBlue, 0.7f);
				}
			}
		}
	}
}
