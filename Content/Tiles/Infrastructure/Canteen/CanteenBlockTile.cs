using Microsoft.Xna.Framework;
using Terraria.ID;

namespace ArknightsMod.Content.Tiles.Infrastructure.Canteen
{
	public class CanteenBlockTile : InfrastructureTile
	{
		public override void SetDefaults()
		{
			DustType = DustID.Stone;
			AddMapEntry(new Color(156, 142, 104));
		}
	}
}
