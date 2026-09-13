using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;

namespace ArknightsMod.Content.Tiles.Infrastructure
{
	public class GlassCurtainWall : InfrastructureWall
	{
		public override void SetDefaults()
		{
			DustType = DustID.Stone;
			Main.wallLight[Type] = true;
			AddMapEntry(Color.LightSkyBlue);
		}
	}
}
