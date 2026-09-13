using ArknightsMod.Content.Tiles.Infrastructure.ReceptionRoom;
using Terraria;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Placeable.Infrastructure.ReceptionRoom
{
	public class OfficeChair : ArknightsInfraFurniture
	{
		public override string Texture => "ArknightsMod/Content/Items/Placeable/Infrastructure/ReceptionRoom/OfficeChair_0";

		public override void SetDefaults() {
			Item.DefaultToPlaceableTile(ModContent.TileType<OfficeChairTile>());
			Item.value = Item.sellPrice(0, 0, 0, 30);
		}
	}
}
