using ArknightsMod.Content.Items.Placeable;
using ArknightsMod.Content.Tiles.Infrastructure.Workshop;
using Terraria;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Placeable.Infrastructure.Workshop
{
	public class WorkshopWallYellowItem : ArknightsInfraBlock
	{
		public override string Texture => "ArknightsMod/Content/Items/Placeable/Infrastructure/Workshop/加工站墙壁_0";

		public override void SetDefaults()
		{
			Item.DefaultToPlaceableWall(ModContent.WallType<WorkshopWallYellow>());
			Item.value = Item.sellPrice(0, 0, 0, 30);
		}
	}
}
