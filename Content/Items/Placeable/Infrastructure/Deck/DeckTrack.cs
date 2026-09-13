using ArknightsMod.Content.Tiles.Infrastructure.Deck;
using Terraria;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Placeable.Infrastructure.Deck
{
	public class DeckTrack : ArknightsInfraBlock
	{
		public override void SetDefaults() {
			Item.DefaultToPlaceableTile(ModContent.TileType<DeckTrackTile>());
			Item.value = Item.sellPrice(0, 0, 0, 20);
		}
	}
}