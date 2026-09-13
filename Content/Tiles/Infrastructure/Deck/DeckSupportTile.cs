using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;

namespace ArknightsMod.Content.Tiles.Infrastructure.Deck
{
	public class DeckSupportTile : InfrastructureTile
	{
		public override void SetDefaults() {
			DustType = DustID.Tin;
			AddMapEntry(new Color(145, 114, 80));
			Main.tileMergeDirt[Type] = false;
		}
	}
}