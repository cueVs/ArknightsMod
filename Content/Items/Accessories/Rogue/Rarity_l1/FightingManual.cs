using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Accessories.Rogue.Rarity_l1
{
    public class FightingManual : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 30;
            Item.height = 30;
			Item.value = Item.sellPrice(0, 2, 0, 0);
			Item.rare = ItemRarityID.Green; 
            Item.accessory = true;
        }
        public override void UpdateAccessory(Player player, bool hideVisual)
        {
			
			player.GetDamage(DamageClass.Melee) += 0.07f;
        }
    }
}