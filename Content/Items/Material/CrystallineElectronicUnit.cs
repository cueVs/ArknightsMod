using ArknightsMod.Content.Tiles.Infrastructure;
using Terraria;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Material
{
	public class CrystallineElectronicUnit : ArknightsMaterial
	{
		public override int Rarity => 4;
		public override void AddRecipes() {
			CreateRecipe()
				.AddIngredient<CrystallineCircuit>(1)
				.AddIngredient<PolymerizedGel>(2)
				.AddIngredient<IncandescentAlloyBlock>(1)
				.AddTile(ModContent.TileType<FactoryTile>())
				.AddCondition(Condition.DownedPlantera)
				.Register();
		}
	}
}
