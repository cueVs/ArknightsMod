using Microsoft.Xna.Framework;
using Terraria.ID;

namespace ArknightsMod.Content.Tiles.Infrastructure.Medical
{
	public class MedicalBlockTile : InfrastructureTile
	{
		public override void SetDefaults()
		{
			DustType = DustID.Stone;
			AddMapEntry(new Color(141, 166, 160));
		}
	}
}
