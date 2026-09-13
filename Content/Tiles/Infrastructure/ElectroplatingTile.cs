using Microsoft.Xna.Framework;
using Terraria.ID;

namespace ArknightsMod.Content.Tiles.Infrastructure
{
	public class ElectroplatingTile : InfrastructureTile
	{
		public override void SetDefaults() {
			DustType = DustID.Stone;
			AddMapEntry(Color.Gray);
		}
	}
}