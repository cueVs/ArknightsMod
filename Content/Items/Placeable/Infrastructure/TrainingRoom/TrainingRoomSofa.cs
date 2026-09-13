using ArknightsMod.Content.Tiles.Infrastructure.TrainingRoom;
using Terraria;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Placeable.Infrastructure.TrainingRoom
{
	public class TrainingRoomSofa : ArknightsInfraFurniture
	{
		public override void SetDefaults() {
			Item.DefaultToPlaceableTile(ModContent.TileType<TrainingRoomSofaTile>());
			Item.value = Item.sellPrice(0, 0, 10, 0);
		}
	}
}