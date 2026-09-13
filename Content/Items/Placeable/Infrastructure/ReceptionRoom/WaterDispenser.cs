using ArknightsMod.Content.Tiles.Infrastructure.ReceptionRoom;
using Terraria;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Placeable.Infrastructure.ReceptionRoom
{
	public class WaterDispenser : ArknightsInfraFurniture
	{
		public override string Texture => "ArknightsMod/Content/Items/Placeable/Infrastructure/ReceptionRoom/WaterDispenser";

		public override void SetDefaults() {
			Item.DefaultToPlaceableTile(ModContent.TileType<WaterDispenserTile>());
			Item.value = Item.sellPrice(0, 0, 0, 30);
		}
	}
}
