using Terraria;
using Terraria.ModLoader;
using Terraria.Localization;
using System.Collections.Generic;

namespace ArknightsMod.Common.Items
{
    public class KingsGlobalItem : GlobalItem
    {
        public bool isKingItem;

        public override bool InstancePerEntity => true;

        public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
        {
            if (isKingItem)
            {
                string txt = Language.GetTextValue("国王收藏品");
                tooltips.Add(new TooltipLine(Mod, "KingItem", txt));
            }
        }
    }
}