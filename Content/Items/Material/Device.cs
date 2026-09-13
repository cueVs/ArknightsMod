using ArknightsMod.Content.Tiles.Infrastructure;
using Terraria;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Material
{
	public class Device : ArknightsMaterial
	{
		public override int Rarity => 1;
		public override void AddRecipes() {
			CreateRecipe()
				.AddIngredient<DamagedDevice>(3)
				.AddTile(ModContent.TileType<FactoryTile>())
				.AddCondition(Condition.DownedEowOrBoc)
				.Register();
		}
	}
}
