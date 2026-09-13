using Terraria;
using Terraria.ModLoader;


namespace ArknightsMod.Content.Items.Accessories.Rogue.Rarity_l2
{
    public class CoinOperatedToy : ModItem
    {
        public override void SetStaticDefaults() {
		}

		public override void SetDefaults() {
			Item.width = 24;
			Item.height = 24;
			Item.value = Item.sellPrice(0, 3, 0, 0);
			Item.rare = 1;
			Item.accessory = true;
		}

		public override void UpdateAccessory(Player player, bool hideVisual) {

		}


	}
}