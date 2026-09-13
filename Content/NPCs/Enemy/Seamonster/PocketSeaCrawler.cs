using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.Utilities;
using Microsoft.Xna.Framework;

using Microsoft.Xna.Framework.Graphics;
using ArknightsMod.Content.Items.Material;
using System;
using Terraria.Audio;
using ArknightsMod.Common.VisualEffects;



namespace ArknightsMod.Content.NPCs.Enemy.Seamonster
{
	public class PocketSeaCrawler : ModNPC
	{
		public override void FindFrame(int frameHeight) {
			NPC.spriteDirection = NPC.direction;
			Player p = Main.player[NPC.target];
			int Startframe = 1;
			int Endframe = 4;
			int Framespeed = 5;
			if (NPC.ai[3] < 300 || NPC.frame.Y > 5 * frameHeight) {
				NPC.frameCounter++;
			}
			
			
			if (NPC.frameCounter > Framespeed) {
				NPC.frame.Y += frameHeight;
				NPC.frameCounter = 0;
			}
			if (NPC.frame.Y >= 4 * frameHeight) {
				NPC.frame.Y = frameHeight;
			}
		}
		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
			bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[] {
				BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Ocean,
				new FlavorTextBestiaryInfoElement("掺杂了杂质的恐鱼，身体组织的密度超过了部分合金。被它刺伤的患者会觉得自己被点着了。"),
			});
		}
		public override void SetDefaults() {
			NPC.width = 60;
			NPC.height = 60;
			NPC.damage = 25;
			NPC.defense = 0;
			NPC.lifeMax = 1000;
			NPC.HitSound = SoundID.NPCHit1;
			NPC.DeathSound = SoundID.NPCDeath1;
			NPC.value = 4800;
			NPC.knockBackResist = 0.1f;
			NPC.aiStyle = 0; // Fighter AI, important to choose the aiStyle that matches the NPCID that we want to mimic. // Use vanilla zombie's type when executing AI code. (This also means it will try to despawn during daytime)
			AnimationType = -1; // Use vanilla zombie's type when executing animation code. Important to also match Main.npcFrameCount[NPC.type] in SetStaticDefaults.
								// Makes kills of this NPC go towards dropping the banner it's associated with.
								//new int[1] { ModContent.GetInstance<ExampleSurfaceBiome>().Type }; // Associates this NPC with the ExampleSurfaceBiome in Bestiary
			NPC.npcSlots = 3;
			Main.npcFrameCount[Type] = 5;
			NPC.friendly = false;
			NPC.noGravity = false;
			if (Main.expertMode) {
				NPC.lifeMax = (int)(NPC.lifeMax * 0.8);
				NPC.damage = (int)(NPC.damage * 0.8);
			}

		}
		public override void AI() {

			NPC.ai[3]++;

			NPC.TargetClosest(true);
			Player p = Main.player[NPC.target];

			if (NPC.ai[3] <= 300) {
				if (NPC.position.X - p.position.X > 30) {
					NPC.velocity.X = -0.6f;



					

				}
				if (NPC.position.X - p.position.X <= -30) {
					NPC.velocity.X = 0.6f;


					

				}
				if (NPC.collideX) {
					NPC.velocity.Y = -0.6f;
				}

			}
			if (NPC.ai[3] < 400 && NPC.ai[3] > 300) {

				NPC.velocity.X = 0;

			}
			if (NPC.ai[3] >= 400) {
				NPC.ai[3] = 0;
			}

			int direction = (Main.player[NPC.target].Center.X > NPC.Center.X).ToDirectionInt();
			NPC.direction = direction;
		}
		public override float SpawnChance(NPCSpawnInfo spawnInfo) {
			return SeabornSpawnHelper.PocketSeaCrawlerChance(spawnInfo, 0.6f);
		}
		public override void OnHitPlayer(Player target, Player.HurtInfo hurtInfo) {
			
			NPC.ai[3] = 200;

		}
		public override void ModifyNPCLoot(NPCLoot npcLoot) {
			LeadingConditionRule notExpertRule = new LeadingConditionRule(new Conditions.NotExpert());
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<CorruptedRecord>(), 1, 2, 5));
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<CoagulatingGel>(), 3, 1, 3));

		}
		public override void HitEffect(NPC.HitInfo hit) {
			for (int i = 0; i < 10; i++) {
				int dustType = DustID.BlueMoss;
				var dust = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, dustType);
				dust.velocity.X += Main.rand.NextFloat(-0.05f, 0.05f);
				dust.velocity.Y += Main.rand.NextFloat(-0.05f, 0.05f);
				dust.scale *= 1f + Main.rand.NextFloat(-0.03f, 0.03f);
			}
		}
		public override void OnKill() {
			int Gore1 = Mod.Find<ModGore>("PSCrawler1").Type;
			var entitySource = NPC.GetSource_Death();
			for (int i = 0; i < 3; i++) {
				Gore.NewGore(entitySource, NPC.position, new Vector2(Main.rand.Next(-5, 4), Main.rand.Next(-5, 4)), Mod.Find<ModGore>("PSCrawler2").Type);
				Gore.NewGore(entitySource, NPC.position, new Vector2(Main.rand.Next(-5, 4), Main.rand.Next(-5, 4)), Mod.Find<ModGore>("PSCrawler3").Type);
			}
			
			Gore.NewGore(entitySource, NPC.position, new Vector2(Main.rand.Next(-5, 4), Main.rand.Next(-5, 4)), Mod.Find<ModGore>("PSCrawler4").Type);
		}
		public override bool? CanFallThroughPlatforms() {
			Player player = Main.player[NPC.target];
			return (player.position.Y + player.height) - (NPC.position.Y + NPC.height) > 0;
		}
	}
}


