using Terraria;
using Terraria.ID;
using Terraria.ModLoader;


namespace ArknightsMod.Content.Items.Accessories.Rogue.Rarity_l4
{
    public class GoldenGinChalice : ModItem
    {
		public override void SetStaticDefaults() {
		}

		public override void SetDefaults() {
			Item.width = 24;
			Item.height = 24;
			Item.value = Item.sellPrice(0, 16, 0, 0);
			Item.rare = ItemRarityID.Master;
			Item.accessory = true;
		}

		public override void UpdateAccessory(Player player, bool hideVisual) {
		}
	}
}