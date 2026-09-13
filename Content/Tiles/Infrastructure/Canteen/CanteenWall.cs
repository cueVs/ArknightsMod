using Microsoft.Xna.Framework;
using Terraria.ID;

namespace ArknightsMod.Content.Tiles.Infrastructure.Canteen
{
	public class CanteenWall : InfrastructureWall
	{
		public override void SetDefaults()
		{
			DustType = DustID.Stone;
			AddMapEntry(new Color(91, 82, 74));
		}
	}
}
