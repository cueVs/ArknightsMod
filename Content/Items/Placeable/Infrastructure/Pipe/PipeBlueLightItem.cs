using ArknightsMod.Content.Items.Placeable;
using ArknightsMod.Content.Tiles.Infrastructure.Pipe;
using Terraria;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Placeable.Infrastructure.Pipe
{
	public class PipeBlueLightItem : ArknightsInfraBlock
	{
		public override bool IsLoadingEnabled(Mod mod) => false;

		public override void SetDefaults()
		{
			Item.DefaultToPlaceableTile(ModContent.TileType<PipeBlueLightPipeTile>());
			Item.value = Item.sellPrice(0, 0, 0, 30);
		}
	}
}
