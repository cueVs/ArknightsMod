using ArknightsMod.Content.Tiles.Infrastructure;
using Terraria;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Material
{
	/// <summary>
	/// 环烃预制件，稀有度：紫
	/// </summary>
	public class CyclicenePrefab : ArknightsMaterial
	{
		public override int Rarity => 3;
		public override void AddRecipes() {
			CreateRecipe()
				.AddIngredient<AggregateCyclicene>(1)
				.AddIngredient<FuscousFiber>(1)
				.AddIngredient<TransmutedSalt>(1)
				.AddTile(ModContent.TileType<FactoryTile>())
				.AddCondition(Condition.DownedMechBossAny)
				.Register();
		}
	}
}
