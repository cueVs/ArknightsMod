using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;

namespace ArknightsMod.Content.Tiles.Infrastructure.Corridors
{
	public class CorridorTile : InfrastructureTile
	{
		public override void SetDefaults() {
			DustType = DustID.Stone;
			AddMapEntry(new Color(54, 53, 67));
			Main.tileMergeDirt[Type] = false;
		}
	}
}