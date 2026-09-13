using ArknightsMod.Content.Items.Material;
using ArknightsMod.Content.Tiles.Infrastructure;
using Terraria;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Placeable
{
	public abstract class ArknightsInfraFurniture : ModItem
	{
		public sealed override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 100;
		}

		public override void AddRecipes() {
			Recipe recipe = CreateRecipe()
				.AddIngredient<CarbonBrick>()
				.AddTile<FactoryTile>()
				.Register();
		}
	}
}