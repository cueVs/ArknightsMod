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


	public class MortarGunner : ModNPC
	{
		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
			// We can use AddRange instead of calling Add multiple times in order to add multiple items at once
			bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[] {
				// Sets the spawning conditions of this NPC that is listed in the bestiary.
				BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Surface,
				BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Times.NightTime,

				// Sets the description of this NPC that is listed in the bestiary.
				new FlavorTextBestiaryInfoElement(Language.GetTextValue("Mods.ArknightsMod.Bestiary.MortarGunner")),

			});
		}
		public override void SetStaticDefaults() {
			NPC.lifeMax = 330;
			NPC.defense = 5;
			NPC.width = 16;
			NPC.height = 20;
			NPC.HitSound = SoundID.NPCHit1;
			NPC.DeathSound = SoundID.NPCDeath2;
			NPC.value = 60f;
			NPC.knockBackResist = 0.5f;
			NPC.aiStyle = -1; 
			NPC.scale = 2f;
		}
		public int Framespeed = 7;
		public bool walk;
		public int walktime;
		private int AttackCD = 0;
		private bool attack;
		private int framecounter;
		private int attackframeY;
		private float maxspeed = 1.6f;
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
		}
	}
}
