using ArknightsMod.Content.Tiles.Infrastructure.Decorates;
using Terraria;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Placeable.Infrastructure.Decorates
{
	public class CartonMiddle : ArknightsInfraFurniture
	{
		public override void SetDefaults() {
			Item.DefaultToPlaceableTile(ModContent.TileType<CartonMiddleTile>());
			Item.value = Item.sellPrice(0, 0, 0, 30);
		}
	}
}