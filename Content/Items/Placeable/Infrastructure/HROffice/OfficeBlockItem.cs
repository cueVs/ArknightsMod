using ArknightsMod.Content.Tiles.Infrastructure.HROffice;
using Terraria;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Placeable.Infrastructure.HROffice
{
	public class OfficeBlockItem : ArknightsInfraBlock
	{
		public override void SetDefaults() {
			Item.DefaultToPlaceableTile(ModContent.TileType<OfficeBlockTile>());
			Item.value = Item.sellPrice(0, 0, 0, 30);
		}
	}
}