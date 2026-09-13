using ArknightsMod.Content.Items.Material;
using ArknightsMod.Content.Tiles.Infrastructure;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Placeable.Infrastructure
{
	public class FactoryPlusItem: ModItem
	{
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 100;
		}
		public override void SetDefaults() {
			Item.DefaultToPlaceableTile(ModContent.TileType<FactoryPlusTile>(), 0);
		}
		public override void AddRecipes() {
			CreateRecipe()
				.AddIngredient(ItemID.Wire, 10)
				.AddIngredient(ModContent.ItemType<FactoryItem>(), 1)
				.AddTile(TileID.WorkBenches)
				.Register();
		}
	}
}

