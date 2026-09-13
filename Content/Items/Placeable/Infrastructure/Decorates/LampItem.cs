using ArknightsMod.Content.Tiles.Infrastructure.Decorates;
using Terraria;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Placeable.Infrastructure.Decorates
{
	public class LampItem : ArknightsInfraFurniture
	{
		public override void SetDefaults() {
			Item.DefaultToPlaceableTile(ModContent.TileType<LampTile>());
			Item.value = Item.sellPrice(0, 0, 0, 30);
		}
	}
}