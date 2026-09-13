using ArknightsMod.Content.Items.Placeable;
using ArknightsMod.Content.Tiles.Infrastructure.Pipe;
using Terraria;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Placeable.Infrastructure.Pipe
{
	public class PipeWallInsulatedItem : ArknightsInfraBlock
	{
		public override string Texture => "ArknightsMod/Content/Items/Placeable/Infrastructure/Pipe/PipeWallItem3";

		public override void SetDefaults()
		{
			Item.DefaultToPlaceableWall(ModContent.WallType<PipeWallInsulated>());
			Item.value = Item.sellPrice(0, 0, 0, 30);
		}
	}
}
