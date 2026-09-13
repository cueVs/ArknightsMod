using ArknightsMod.Content.Tiles.Infrastructure;
using Terraria;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Material
{
	public class NucleicCrystalSinter : ArknightsMaterial
	{
		public override int Rarity => 4;
		public override void AddRecipes() {
			CreateRecipe()
				.AddIngredient<TransmutedSaltAgglomerate>(1)
				.AddIngredient<CuttingFluidSolution>(1)
				.AddIngredient<RefinedSolvent>(2)
				.AddTile(ModContent.TileType<FactoryTile>())
				.AddCondition(Condition.DownedPlantera)
				.Register();
		}
	}
}
