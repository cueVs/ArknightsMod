using Terraria;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Accessories.Rogue.Rarity_l2
{
    public class AsmallroundShieldOfDifferEntiron : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 30;
            Item.height = 30;
			Item.value = Item.sellPrice(0, 3, 0, 0);
			Item.rare = 1; // 橙色稀有度
            Item.accessory = true;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            // 计算基础防御（排除所有加成）
            player.statDefense += (int)(player.statDefense * 0.15f);
        }
    }
}