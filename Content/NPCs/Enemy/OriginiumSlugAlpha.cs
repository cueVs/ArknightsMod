using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ModLoader.Utilities;
using Terraria.DataStructures;
using ArknightsMod.Content.Items;


namespace ArknightsMod.Content.NPCs.Enemy
{
	// Party Zombie is a pretty basic clone of a vanilla NPC. To learn how to further adapt vanilla NPC behaviors, see https://github.com/tModLoader/tModLoader/wiki/Advanced-Vanilla-Code-Adaption#example-npc-npc-clone-with-modified-projectile-hoplite
	public class OriginiumSlugAlpha : ModNPC
	{
		// This is a reference property. It lets us write FirstStageTimer as if it's NPC.localAI[1], essentially giving it our own name
		public ref float Timer => ref NPC.localAI[0];

		private int status;
		private int direction;
		private float preposition;


		public override void SetStaticDefaults() {
			Main.npcFrameCount[Type] = 4;

			NPCID.Sets.NPCBestiaryDrawModifiers value = new NPCID.Sets.NPCBestiaryDrawModifiers(0) { // Influences how the NPC looks in the Bestiary
				Velocity = 1f // Draws the NPC in the bestiary as if its walking +1 tiles in the x direction
			};
			NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, value);
		}

		public override void SetDefaults() {
			NPC.width = 30;
			NPC.height = 20;
			NPC.damage = 7;
			NPC.defense = 2;
			NPC.lifeMax = 25;

			NPC.HitSound = SoundID.NPCHit1;
			NPC.DeathSound = SoundID.NPCDeath1;

			NPC.value = 3f;
			NPC.knockBackResist = 0.5f;
			NPC.aiStyle = 7; // Walks semi-randomly, jumps over holes like bunny.
		}

		public override void ModifyNPCLoot(NPCLoot npcLoot) {

			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Items.Placeable.OrirockCube>(), 8, 1, 3));

		}

		public override float SpawnChance(NPCSpawnInfo spawnInfo) {
			return SpawnCondition.OverworldDaySlime.Chance * 20f; // Spawn with 1/1st the chance of a regular slime.
			// return SpawnCondition.OverworldNightMonster.Chance * 1f; // Spawn with 1/5th the chance of a regular zombie.
		}

		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
			// We can use AddRange instead of calling Add multiple times in order to add multiple items at once
			bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[] {
				// Sets the spawning conditions of this NPC that is listed in the bestiary.
				BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Times.NightTime,
				BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Surface,


				// Sets the description of this NPC that is listed in the bestiary.
				new FlavorTextBestiaryInfoElement($"Mods.ArknightsMod.Bestiary.{Name}"),

			});
		}

		public override void FindFrame(int frameHeight) {
			NPC.spriteDirection = NPC.direction;

			// This NPC animates with a simple "go from start frame to final frame, and loop back to start frame" rule
			// In this case: 0-1-2-3-0-1-2-3
			int startFrame = 0;
			int finalFrame = 3;
			int frameSpeed = 4;

			if (NPC.velocity.Length() != 0 && NPC.position.X != preposition) {
				NPC.frameCounter += 0.5f;
				NPC.frameCounter += NPC.velocity.Length() / 10f; // Make the counter go faster with more movement speed
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

			if (NPC.ai[1] % 180 == 0) {
				NPC.ai[1] = 0;
				status = Main.rand.Next(5);

				if (status == 2 && Main.rand.NextBool(2)) {
					NPC.TargetClosest();
				}
				if (NPC.position.X == preposition) {
					direction = NPC.direction * -1;
					status = 4;
				}
				preposition = NPC.position.X;
			}
			switch (status) {
				case 0:
					NPC.direction = 1;
					NPC.velocity.X = 0.7f * NPC.direction;
					break;
				case 1:
					NPC.direction = -1;
					NPC.velocity.X = 0.8f * NPC.direction;
					break;
				case 2:
					NPC.position.X = preposition;
					break;
				case 3:
					NPC.direction = (Main.player[NPC.target].Center.X > NPC.Center.X).ToDirectionInt();
					NPC.velocity.X = 1.2f * (Main.player[NPC.target].Center.X > NPC.Center.X).ToDirectionInt();
					break;
				case 4:
					NPC.direction = direction;
					NPC.velocity.X = 0.8f * direction;
					break;
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
	}
}
