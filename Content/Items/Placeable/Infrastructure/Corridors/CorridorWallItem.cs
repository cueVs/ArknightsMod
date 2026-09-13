using ArknightsMod.Content.Tiles.Infrastructure.Corridors;
using Terraria;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Placeable.Infrastructure.Corridors
{
	public class CorridorWallItem : ArknightsInfraBlock
	{
		public override void SetDefaults() {
			Item.DefaultToPlaceableWall(ModContent.WallType<CorridorWall>());
			Item.value = Item.sellPrice(0, 0, 0, 30);
		}
	}
}