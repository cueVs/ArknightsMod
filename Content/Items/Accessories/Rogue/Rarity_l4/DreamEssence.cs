using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using ArknightsMod.Players;
namespace ArknightsMod.Content.Items.Accessories.Rogue.Rarity_l4
{
    public class DreamEssence : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 30;
            Item.height = 30;
            Item.accessory = true;
			Item.value = Item.sellPrice(0, 16, 0, 0);
			Item.rare = ItemRarityID.Master;
		}

		public override void UpdateAccessory(Player player, bool hideVisual) {
			player.GetModPlayer<WeaponPlayer>().SPRegenMultiplier += 1.5f;
			
		}

	}
}