using Terraria.GameContent.Creative;
using Terraria.ModLoader;
using Terraria.ID;

namespace ArknightsMod.Content.Items.Placeable.Furniture
{
	public class AnniversaryWheel : ModItem
	{
		public override void SetStaticDefaults() {

			CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
		}

		public override void SetDefaults() {
			Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.Furniture.AnniversaryWheel>());
			Item.value = 150;
			Item.maxStack = 99;
			Item.width = 32;
			Item.height = 32;
		}

		//Please see Content/ExampleRecipes.cs for a detailed explanation of recipe creation.
		//public override void AddRecipes() {
		//	CreateRecipe()
		//		.AddIngredient(ItemID.DirtBlock, 1)
		//			  .AddTile(TileID.WorkBenches)
		//			  .Register();
		//}
	}
}
