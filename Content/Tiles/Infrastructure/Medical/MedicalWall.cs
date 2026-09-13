using Microsoft.Xna.Framework;
using Terraria.ID;

namespace ArknightsMod.Content.Tiles.Infrastructure.Medical
{
	public class MedicalWall : InfrastructureWall
	{
		public override void SetDefaults()
		{
			DustType = DustID.Stone;
			AddMapEntry(new Color(61, 80, 76));
		}
	}
}
