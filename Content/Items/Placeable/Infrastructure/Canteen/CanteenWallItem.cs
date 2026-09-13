using ArknightsMod.Content.Tiles.Infrastructure.Canteen;
using Terraria;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Placeable.Infrastructure.Canteen
{
	public class CanteenWallItem : ArknightsInfraBlock
	{
		public override void SetDefaults() {
			Item.DefaultToPlaceableWall(ModContent.WallType<CanteenWall>());
			Item.value = Item.sellPrice(0, 0, 0, 30);
		}
	}
}