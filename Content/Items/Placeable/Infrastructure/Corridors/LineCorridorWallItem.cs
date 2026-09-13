using ArknightsMod.Content.Tiles.Infrastructure.Corridors;
using Terraria;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Placeable.Infrastructure.Corridors
{
	public class LineCorridorWallItem : ArknightsInfraBlock
	{
		public override void SetDefaults() {
			Item.DefaultToPlaceableWall(ModContent.WallType<LineCorridorWall>());
			Item.value = Item.sellPrice(0, 0, 0, 30);
		}
	}
}