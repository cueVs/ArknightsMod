using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.Utilities;

namespace ArknightsMod.Content.NPCs.Enemy.Seamonster
{
	internal static class SeabornSpawnHelper
	{
		private const float RainMultiplier = 1.5f;

		public static float OceanSeabornChance(NPCSpawnInfo spawnInfo, float scale)
		{
			if (!NPC.downedBoss1)
				return 0f;
			float chance = SpawnCondition.OceanMonster.Chance * scale;
			if (chance <= 0f)
				return 0f;
			if (Main.raining)
				chance *= RainMultiplier;
			return chance;
		}

		public static float PocketSeaCrawlerChance(NPCSpawnInfo spawnInfo, float scale)
		{
			if (!NPC.downedBoss3)
				return 0f;
			return OceanSeabornChance(spawnInfo, scale);
		}
	}
}
