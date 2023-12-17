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
namespace ArknightsMod.Content.NPCs.Enemy.ThroughChapter4
{
	// Party Zombie is a pretty basic clone of a vanilla NPC. To learn how to further adapt vanilla NPC behaviors, see https://github.com/tModLoader/tModLoader/wiki/Advanced-Vanilla-Code-Adaption#example-npc-npc-clone-with-modified-projectile-hoplite
	public class OriginiumSlug : ModNPC
	{
		private int status;
		private float preposition;
		private int direction;


		public override void SetStaticDefaults() {
			Main.npcFrameCount[Type] = 4;

			NPCID.Sets.NPCBestiaryDrawModifiers value = new NPCID.Sets.NPCBestiaryDrawModifiers() { // Influences how the NPC looks in the Bestiary
				Velocity = 1f // Draws the NPC in the bestiary as if its walking +1 tiles in the x direction
			};
			NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, value);
		}

		public override void SetDefaults() {
			NPC.width = 34;
			NPC.height = 26;
			NPC.damage = 6;
			NPC.defense = 0;
			NPC.lifeMax = 14;

			NPC.HitSound = SoundID.NPCHit1;
			NPC.DeathSound = SoundID.NPCDeath1;

			NPC.value = 3f;
			NPC.knockBackResist = 0.5f;
			NPC.aiStyle = NPCAIStyleID.Snail; // Passive Worm AI

			NPC.scale = 0.85f;

			Banner = NPC.type;
			BannerItem = ItemType<OriginiumSlugBanner>();
		}

		public override void ModifyNPCLoot(NPCLoot npcLoot) {

			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Items.Placeable.OrirockCube>(), 8, 1, 2));

		}

		public override float SpawnChance(NPCSpawnInfo spawnInfo) {
			return SpawnCondition.OverworldDaySlime.Chance * 0.8f; // Spawn with 1/1st the chance of a regular slime.
			// return SpawnCondition.OverworldNightMonster.Chance * 1f; // Spawn with 1/5th the chance of a regular zombie.
		}

		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
			// We can use AddRange instead of calling Add multiple times in order to add multiple items at once
			bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[] {
				// Sets the spawning conditions of this NPC that is listed in the bestiary.
				BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Times.NightTime,
				BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Surface,


				// Sets the description of this NPC that is listed in the bestiary.
				new FlavorTextBestiaryInfoElement(Language.GetTextValue("Mods.ArknightsMod.Bestiary.OriginiumSlug")),

			});
		}

		public override void FindFrame(int frameHeight) {
			NPC.spriteDirection = NPC.direction;

			// This NPC animates with a simple "go from start frame to final frame, and loop back to start frame" rule
			// In this case: 0-1-2-3-0-1-2-3
			int startFrame = 0;
			int finalFrame = 3;
			int frameSpeed = 6;

			if (NPC.velocity.Length() != 0 && NPC.position.X != preposition) {
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

		public override void AI() {
			if (NPC.target < 0 || NPC.target == 255 || Main.player[NPC.target].dead || !Main.player[NPC.target].active) {
				NPC.TargetClosest();
			}
			if (NPC.ai[3] % 180 == 0) {
				NPC.ai[3] = 0;
				status = Main.rand.Next(5);
				if (status == 1 || status == 3) {
					direction = (Main.player[NPC.target].Center.X > NPC.Center.X).ToDirectionInt();
					NPC.direction = direction;
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
			NPC.velocity.Y = 1.2f * NPC.directionY;
			NPC.ai[3]++;

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
	}
}
