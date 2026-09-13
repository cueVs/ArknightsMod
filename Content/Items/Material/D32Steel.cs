using ArknightsMod.Content.Tiles.Infrastructure;
using Terraria;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Material
{
	public class D32Steel : ArknightsMaterial
	{
		public override int Rarity => 4;
		public override void AddRecipes() {
			CreateRecipe()
				.AddIngredient<ManganeseTrihydrate>(1)
				.AddIngredient<GrindstonePentahydrate>(1)
				.AddIngredient<RMA7024>(1)
				.AddTile(ModContent.TileType<FactoryTile>())
				.AddCondition(Condition.DownedPlantera)
				.Register();
		}
	}
}
