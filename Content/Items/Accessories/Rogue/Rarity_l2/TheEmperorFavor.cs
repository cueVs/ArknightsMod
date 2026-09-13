using Terraria;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Accessories.Rogue.Rarity_l2
{
    public class TheEmperorFavor : ModItem
    {

        public override void SetDefaults()
        {
            Item.width = 30;
            Item.height = 28;
			Item.value = Item.sellPrice(0, 3, 0, 0);
			Item.rare = 1; 
            Item.accessory = true;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetDamage(DamageClass.Melee) += 0.15f; 
        }
    }
}