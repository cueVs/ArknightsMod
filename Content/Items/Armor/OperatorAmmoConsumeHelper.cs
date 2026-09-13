using ArknightsMod.Content.Items.Armor.Specialist.ExusiaiAlter;
using ArknightsMod.Players;
using Terraria;

namespace ArknightsMod.Content.Items.Armor
{
	internal static class OperatorAmmoConsumeHelper
	{
		public static void NotifyAmmoConsumed(Player consumer) {
			if (consumer == null || !consumer.active || consumer.dead)
				return;

			for (int i = 0; i < Main.maxPlayers; i++) {
				Player player = Main.player[i];
				if (!player.active || player.dead)
					continue;

				ExusiaiAlterSetPlayer alter = player.GetModPlayer<ExusiaiAlterSetPlayer>();
				if (alter.ExusiaiAlterSetActive)
					alter.OnAllyAmmoConsumed(consumer);
			}
		}

		public static bool HasAmmoSkill(Player player) {
			WeaponPlayer wp = player.GetModPlayer<WeaponPlayer>();
			if (wp.HowManySkills <= 0)
				return false;

			return wp.StockMax[wp.Skill] > 0;
		}
	}
}
