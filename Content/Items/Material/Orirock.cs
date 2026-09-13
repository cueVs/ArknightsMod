using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Material
{
	public class Orirock : ArknightsMaterial
	{
		public override int Rarity => 0;
		public override void SafeSetDefaults() {
			Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.OrirockCube>());
		}
		public override void AddRecipes() {

		}
	}
}