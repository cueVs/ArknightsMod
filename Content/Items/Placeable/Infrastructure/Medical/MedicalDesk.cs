using ArknightsMod.Content.Tiles.Infrastructure.Medical;
using Terraria;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Placeable.Infrastructure.Medical
{
	public class MedicalDesk : ArknightsInfraFurniture
	{
		public override void SetDefaults() {
			Item.DefaultToPlaceableTile(ModContent.TileType<MedicalDeskTile>());
			Item.value = Item.sellPrice(0, 0, 10, 0);
		}
	}
}