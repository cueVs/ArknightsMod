using Terraria;
using Terraria.ModLoader;
using ArknightsMod.Content.Tiles.Infrastructure.ControlCenter;

namespace ArknightsMod.Content.Items.Placeable.Infrastructure.ControlCenter
{
    public class RhodesScreen : ArknightsInfraFurniture
	{
		public override void SetDefaults() {
			Item.DefaultToPlaceableTile(ModContent.TileType<RhodesScreenTile>());
		}
    }
}
