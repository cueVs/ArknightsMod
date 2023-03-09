using Terraria.GameContent.Creative;
using Terraria.ModLoader;
using Terraria.ID;

namespace ArknightsMod.Content.Items.Placeable.Furniture
{
	public class DareUsa : ModItem
	{
		public override void SetStaticDefaults()
		{
			// Tooltip.SetDefault("This is a modded chair.");

			CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
		}

		public override void SetDefaults()
		{
			Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.Furniture.DareUsa>());
			Item.value = 150;
			Item.maxStack = 99;
			Item.width = 64;
			Item.height = 50;
		}

		// Please see Content/ExampleRecipes.cs for a detailed explanation of recipe creation.
		//public override void AddRecipes() {
		//	CreateRecipe()
		//		.AddIngredient<OrirockCube>()
		//              .AddTile(TileID.WorkBenches)
		//              .Register();
		//}
	}
}
