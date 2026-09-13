using Terraria;
using Terraria.ModLoader;
using Terraria.Localization;
using System.Collections.Generic;
using ArknightsMod.Common.Items;
namespace ArknightsMod.Common.Items
{
	public class SarkazKingGlobalItem : GlobalItem
	{
		public bool isSarkazKing;

		public override bool InstancePerEntity => true;

		public override void ModifyTooltips(Item item, List<TooltipLine> tooltips) {
			if (isSarkazKing) {
				string txt = Language.GetTextValue("魔王收藏品");
				tooltips.Add(new TooltipLine(Mod, "SarkazKingItem", txt));
			}
		}
	}
}