using ArknightsMod.Content.Tiles.Infrastructure.Deck;
using Terraria;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Placeable.Infrastructure.Deck
{
	public class OrangeContainer : ArknightsInfraFurniture
	{
		public override void SetDefaults() {
			Item.DefaultToPlaceableTile(ModContent.TileType<OrangeContainerTile>());
			Item.value = Item.sellPrice(0, 0, 10, 0);
		}
	}
}