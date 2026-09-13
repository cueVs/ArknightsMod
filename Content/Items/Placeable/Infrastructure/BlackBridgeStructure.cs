using ArknightsMod.Content.Tiles.Infrastructure;
using Terraria;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Placeable.Infrastructure
{
	public class BlackBridgeStructure : ArknightsInfraBlock
	{
		public override void SetDefaults() {
			Item.DefaultToPlaceableTile(ModContent.TileType<BlackBridgeStructureTile>());
			Item.value = Item.sellPrice(0, 0, 0, 30);
		}
	}
}