using ArknightsMod.Content.Tiles.Infrastructure;
using Terraria;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Material
{
	/// <summary>
	/// 重相位对映体，稀有度：金
	/// </summary>
	public class RephasicEnantiomer : ArknightsMaterial
	{
		public override int Rarity => 4;
		public override void AddRecipes() {
			CreateRecipe()
				.AddIngredient<ChiralRefractor>(2)
				.AddIngredient<CyclicenePrefab>(1)
				.AddIngredient<SolidifiedFiberBoard>(1)
				.AddTile(ModContent.TileType<FactoryTile>())
				.AddCondition(Condition.DownedPlantera)
				.Register();
		}
	}
}
