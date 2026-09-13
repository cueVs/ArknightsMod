using ArknightsMod.Content.Tiles.Infrastructure;
using Terraria;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Material
{
	public class OriginiumShard : ModItem
	{
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 100; 
		}

		public override void SetDefaults() {
			Item.width = 32; 
			Item.height = 32; 

			Item.maxStack = 9999;
			Item.value = Item.sellPrice(0, 0, 2, 50); 
		}
		public override void AddRecipes() {
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient<OrirockCube>(2);
			recipe.AddTile(ModContent.TileType<FactoryTile>());
			recipe.Register();
			recipe = CreateRecipe();
			recipe.AddIngredient<Device>();
			recipe.AddTile(ModContent.TileType<FactoryTile>());
			recipe.Register();
		}
	}
}
