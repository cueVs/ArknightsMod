using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Accessories.Rogue.Rarity_l3
{
    public class VieuxVanguardsBlade : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 30;
            Item.height = 30;
			Item.value = Item.sellPrice(0, 6, 0, 0);
			Item.rare = ItemRarityID.Purple; 
            Item.accessory = true;
        }
        public override void UpdateAccessory(Player player, bool hideVisual)
        {
			
			player.GetDamage(DamageClass.Melee) += 0.35f;
        }
    }
}