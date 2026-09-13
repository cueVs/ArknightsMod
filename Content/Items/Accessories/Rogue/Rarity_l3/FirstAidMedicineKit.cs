using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Accessories.Rogue.Rarity_l3
{
    public class FirstAidMedicineKit : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 30;
            Item.height = 30;
			Item.value = Item.sellPrice(0, 6, 0, 0);
			Item.rare = ItemRarityID.LightPurple; // 青柠色(专家模式)稀有度
            Item.accessory = true;
        }
        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            // 提升50%最大生命值
            player.statLifeMax2 += (int)(player.statLifeMax2 * 0.35f);
        }
    }
}