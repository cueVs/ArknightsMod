using ArknightsMod.Content.Tiles.Infrastructure.Decorates;
using Terraria;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Placeable.Infrastructure.Decorates
{
	public class ClothesHanger2 : ArknightsInfraFurniture
	{
		public override void SetDefaults() {
			Item.DefaultToPlaceableTile(ModContent.TileType<ClothesHangerTile>(), tileStyleToPlace: 1);
			Item.value = Item.sellPrice(0, 0, 0, 30);
		}
	}
}