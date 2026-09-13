using ArknightsMod.Content.Tiles.Infrastructure.Corridors;
using Terraria;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Placeable.Infrastructure.Corridors
{


	public class CorridorTileItem : ArknightsInfraBlock
	{
		public override void SetDefaults() {
			Item.DefaultToPlaceableTile(ModContent.TileType<CorridorTile>());
			Item.value = Item.sellPrice(0, 0, 0, 30);
		}
	}
}