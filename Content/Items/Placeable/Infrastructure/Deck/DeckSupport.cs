using ArknightsMod.Content.Tiles.Infrastructure.Deck;
using Terraria;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Placeable.Infrastructure.Deck
{
	public class DeckSupport : ArknightsInfraBlock
	{
		public override void SetDefaults() {
			Item.DefaultToPlaceableTile(ModContent.TileType<DeckSupportTile>());
			Item.value = Item.sellPrice(0, 0, 0, 20);
		}
	}
}