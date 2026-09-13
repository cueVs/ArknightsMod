using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Accessories.Rogue.Rarity_l3
{
    public class FatalBoltsDivineSpeed : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 30;
            Item.height = 30;
			Item.value = Item.sellPrice(0, 6, 0, 0);
			Item.rare = ItemRarityID.Purple; // 青柠色(专家模式)稀有度
            Item.accessory = true;
        }
        public override void UpdateAccessory(Player player, bool hideVisual)
        {
			// 提升50%最大生命值
			player.GetAttackSpeed(DamageClass.Ranged) += 0.7f;
        }
    }
}