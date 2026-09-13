using ArknightsMod.Content.Tiles.Infrastructure;
using Terraria;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Material
{
	/// <summary>
	/// 手性光折体，稀有度：紫
	/// </summary>
	public class ChiralRefractor : ArknightsMaterial
	{
		public override int Rarity => 3;
		public override void AddRecipes() {
			CreateRecipe()
				.AddIngredient<CoagulativeNodule>(1)
				.AddIngredient<AggregateCyclicene>(1)
				.AddIngredient<SugarPack>(1)
				.AddTile(ModContent.TileType<FactoryTile>())
				.AddCondition(Condition.DownedMechBossAny)
				.Register();
		}
	}
}
