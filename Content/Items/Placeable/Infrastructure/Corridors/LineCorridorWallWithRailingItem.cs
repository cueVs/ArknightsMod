using ArknightsMod.Content.Tiles.Infrastructure.Corridors;
using Terraria;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Placeable.Infrastructure.Corridors
{
	public class LineCorridorWallWithRailingItem : ArknightsInfraBlock
	{
		public override void SetDefaults() {
			Item.DefaultToPlaceableWall(ModContent.WallType<LineCorridorWallWithRailing>());
			Item.value = Item.sellPrice(0, 0, 0, 30);
		}
	}
}