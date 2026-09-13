using ArknightsMod.Content.Tiles.Infrastructure.Workshop;
using Terraria;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Placeable.Infrastructure.Workshop
{
	public class WorkshopComputer : ArknightsInfraFurniture
	{
		public override void SetDefaults() {
			Item.DefaultToPlaceableTile(ModContent.TileType<WorkshopComputerTile>());
			Item.value = Item.sellPrice(0, 0, 10, 0);
		}
	}
}