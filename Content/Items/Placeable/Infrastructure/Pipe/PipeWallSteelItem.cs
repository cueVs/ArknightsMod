using ArknightsMod.Content.Items.Placeable;
using ArknightsMod.Content.Tiles.Infrastructure.Pipe;
using Terraria;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Placeable.Infrastructure.Pipe
{
	public class PipeWallSteelItem : ArknightsInfraBlock
	{
		public override string Texture => "ArknightsMod/Content/Items/Placeable/Infrastructure/Pipe/PipeWallItem1";

		public override void SetDefaults()
		{
			Item.DefaultToPlaceableWall(ModContent.WallType<PipeWallSteel>());
			Item.value = Item.sellPrice(0, 0, 0, 30);
		}
	}
}
