using ArknightsMod.Content.Items.Material;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Content.NPCs.Critters
{
	// 不是瓦片，是一只会飘的小生物：地表/天空低概率刷新，跟着风向漂，能被捕虫网抓到，
	// 抓到之后掉落 Content.Items.Material.FogRollingGrass。
	public class FogRollingGrass : ModNPC
	{
		public override void SetStaticDefaults() {
			Main.npcFrameCount[Type] = 1;
			NPCID.Sets.CountsAsCritter[Type] = true;
		}

		public override void SetDefaults() {
			NPC.width = 28;
			NPC.height = 28;
			NPC.aiStyle = -1;
			NPC.damage = 0;
			NPC.defense = 0;
			NPC.lifeMax = 5;
			NPC.HitSound = SoundID.NPCHit1;
			NPC.DeathSound = SoundID.NPCDeath2;
			NPC.value = 0f;
			NPC.knockBackResist = 1f;
			NPC.noGravity = true;
			NPC.friendly = false;
			NPC.catchItem = (short)ModContent.ItemType<Items.Material.FogRollingGrass>();
		}

		public override float SpawnChance(NPCSpawnInfo spawnInfo) {
			if (!spawnInfo.PlayerInTown && (spawnInfo.Player.ZoneOverworldHeight || spawnInfo.Player.ZoneSkyHeight))
				return 0.02f;
			return 0f;
		}

		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
			bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[] {
				BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Surface,
				new FlavorTextBestiaryInfoElement("在迷雾裹挟下，离大地与根系越来越远。"),
			});
		}

		private float wobble;

		public override void AI() {
			wobble += 0.05f;

			float windPush = Main.windSpeedCurrent * 1.2f;
			NPC.velocity.X = MathHelper.Lerp(NPC.velocity.X, windPush, 0.02f);
			NPC.velocity.Y += (float)System.Math.Sin(wobble) * 0.01f;
			NPC.velocity.Y = MathHelper.Clamp(NPC.velocity.Y, -0.6f, 0.6f);

			NPC.rotation = NPC.velocity.X * 0.05f;

			if (Main.rand.NextBool(6)) {
				Dust dust = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, DustID.Smoke, 0f, 0f, 200, default, 0.6f);
				dust.velocity *= 0.2f;
				dust.noGravity = true;
			}
		}
	}
}
