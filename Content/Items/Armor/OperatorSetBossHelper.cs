using Terraria;

namespace ArknightsMod.Content.Items.Armor
{
	internal static class OperatorSetBossHelper
	{
		public static bool AnyBossActive() {
			for (int i = 0; i < Main.maxNPCs; i++) {
				NPC npc = Main.npc[i];
				if (npc.active && npc.boss && !npc.friendly)
					return true;
			}

			return false;
		}
	}
}
