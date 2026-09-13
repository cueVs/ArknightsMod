using ArknightsMod.Content.Tiles.Infrastructure.Decorates;
using Terraria;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Placeable.Infrastructure.Decorates
{
	public class ClothesHanger : ArknightsInfraFurniture
	{
		public override void SetDefaults() {
			Item.DefaultToPlaceableTile(ModContent.TileType<ClothesHangerTile>());
			Item.value = Item.sellPrice(0, 0, 0, 30);
		}
	}
}