using ArknightsMod.Content.Tiles.Infrastructure.HROffice;
using Terraria;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Placeable.Infrastructure.HROffice
{
	public class OfficeWallItem : ArknightsInfraBlock
	{
		public override void SetDefaults() {
			Item.DefaultToPlaceableWall(ModContent.WallType<OfficeWallTile>());
			Item.value = Item.sellPrice(0, 0, 0, 30);
		}
	}
}