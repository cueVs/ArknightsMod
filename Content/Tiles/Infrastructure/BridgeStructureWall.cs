using Microsoft.Xna.Framework;
using Terraria.ID;

namespace ArknightsMod.Content.Tiles.Infrastructure
{
	public class BridgeStructureWall : InfrastructureWall
	{
		public override void SetDefaults()
		{
			DustType = DustID.Stone;
			AddMapEntry(Color.Gray);
		}
	}
}
