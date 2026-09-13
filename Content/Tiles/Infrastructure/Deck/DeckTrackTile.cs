using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;

namespace ArknightsMod.Content.Tiles.Infrastructure.Deck
{
	public class DeckTrackTile : InfrastructureTile
	{
		public override void SetDefaults() {
			DustType = DustID.Stone;
			AddMapEntry(Color.DarkGray);
			Main.tileMergeDirt[Type] = false;
		}
	}
}