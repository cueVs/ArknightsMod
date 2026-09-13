using Terraria;

namespace ArknightsMod.Content.Items.Armor
{
	internal static class OperatorTeammateHelper
	{
		// 场上是否存在其他多人玩家，或该玩家拥有激活中的召唤物。
		public static bool HasTeammates(Player player) {
			if (HasOtherActivePlayers(player))
				return true;

			return HasActiveMinions(player);
		}

		public static bool HasOtherActivePlayers(Player player) {
			for (int i = 0; i < Main.maxPlayers; i++) {
				Player other = Main.player[i];
				if (other.active && other.whoAmI != player.whoAmI && !other.dead)
					return true;
			}

			return false;
		}

		public static bool HasActiveMinions(Player player) {
			for (int i = 0; i < Main.maxProjectiles; i++) {
				Projectile proj = Main.projectile[i];
				if (proj.active && proj.owner == player.whoAmI && (proj.minion || proj.sentry))
					return true;
			}

			return false;
		}
	}
}
