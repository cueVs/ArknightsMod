using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using ArknightsMod.Players;
namespace ArknightsMod.Content.Items.Accessories.Rogue.Rarity_l3
{
    public class FeatherBeastLiverPate : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 30;
            Item.height = 30;
            Item.accessory = true;
			Item.value = Item.sellPrice(0, 6, 0, 0);
			Item.rare = ItemRarityID.Purple;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
           player.GetModPlayer<WeaponPlayer>().SPRegenMultiplier += 1.35f;
        }
    }
}