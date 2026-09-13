using System.Collections.Generic;
using ArknightsMod.Content.Items.Armor;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Armor.Guard.Utage
{
	internal static class UtageSetTooltipHelper
	{
		public static void AppendDynamicSetEffect(Mod mod, List<TooltipLine> tooltips) {
			if (Main.netMode == NetmodeID.Server)
				return;

			Player player = Main.LocalPlayer;
			if (!player.TryGetModPlayer(out UtageSetPlayer utage) || !utage.UtageSetActive)
				return;

			float percent = utage.GetMeleeAttackSpeedBonusPercent();
			string text = Language.GetTextValue("Mods.ArknightsMod.ArmorSets.Utage.SetEffectActive", percent.ToString("0.#"));
			OperatorOutfitTooltipLayout.ReplaceWrappedLines(mod, tooltips, "SetEffect", text);
		}
	}
}
