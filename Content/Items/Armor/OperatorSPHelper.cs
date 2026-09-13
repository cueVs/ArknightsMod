using System;
using ArknightsMod.Players;
using Terraria;
using Terraria.ID;

namespace ArknightsMod.Content.Items.Armor
{
	internal static class OperatorSPHelper
	{
		public static void TryGainSP(Player player, int amount) {
			if (Main.netMode == NetmodeID.MultiplayerClient || amount <= 0)
				return;

			WeaponPlayer wp = player.GetModPlayer<WeaponPlayer>();
			if (wp.HowManySkills <= 0 || wp.CurrentSkill?.CurrentLevelData == null)
				return;

			int maxSp = wp.CurrentSkill.CurrentLevelData.MaxSP;
			wp.SP = Math.Min(wp.SP + amount, maxSp);
		}
	}
}
