using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Material
{
	public class RMA7012 : ArknightsMaterial
	{
		public override int Rarity => 2;
		public override void SafeSetStaticDefaults() {
			ItemID.Sets.SortingPriorityMaterials[Item.type] = 58;
		}
		public override void SafeSetDefaults() {
			Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.RMA7012>());
		}
	}
}