using ArknightsMod.Content.Tiles.Infrastructure.Deck;
using Terraria;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Placeable.Infrastructure.Deck
{
	public class SmallServer : ArknightsInfraFurniture
	{
		public override void SetDefaults() {
			Item.DefaultToPlaceableTile(ModContent.TileType<SmallServerTile>());
			Item.value = Item.sellPrice(0, 0, 10, 0);
		}
	}
}