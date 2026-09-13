using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Accessories.Rogue.Rarity_l3
{
    public class KnightlyCodexRenewed : ModItem
    {
		public override void SetStaticDefaults() {
		}

		public override void SetDefaults() {
			Item.width = 24;
			Item.height = 24;
			Item.value = Item.sellPrice(0, 6, 0, 0);
			Item.rare = ItemRarityID.Cyan;
			Item.accessory = true;
		}

		public override void UpdateAccessory(Player player, bool hideVisual) {

		}


	}
}