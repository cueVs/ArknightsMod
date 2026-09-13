using ArknightsMod.Content.Items.Placeable;
using ArknightsMod.Content.Tiles.Infrastructure.Pipe;
using Terraria;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Placeable.Infrastructure.Pipe
{
	public class PipeWallCopperItem : ArknightsInfraBlock
	{
		public override string Texture => "ArknightsMod/Content/Items/Placeable/Infrastructure/Pipe/PipeWallItem2";

		public override void SetDefaults()
		{
			Item.DefaultToPlaceableWall(ModContent.WallType<PipeWallCopper>());
			Item.value = Item.sellPrice(0, 0, 0, 30);
		}
	}
}
