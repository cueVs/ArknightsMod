using ArknightsMod.Content.Tiles.Infrastructure.Deck;
using Terraria;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Placeable.Infrastructure.Deck
{
	public class YellowRailing : ArknightsInfraFurniture
	{
		public override void SetDefaults() {
			Item.DefaultToPlaceableTile(ModContent.TileType<YellowRailingTile>());
			Item.value = Item.sellPrice(0, 0, 10, 0);
		}
	}
}