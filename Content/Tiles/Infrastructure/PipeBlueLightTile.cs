using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;

namespace ArknightsMod.Content.Tiles.Infrastructure
{
	public class PipeBlueLightTile : InfrastructureTile
	{
		public override bool ShouldMergeWithBlackBridgeStructure() => true;
		public override void SetDefaults() {
			DustType = DustID.Stone;
			AddMapEntry(new Color(15, 82, 142));
			Main.tileLighted[Type] = true;
		}
		public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b) {
			float strength = 1f;
			r = 0.15f * strength;
			g = 0.82f * strength;
			b = 1.42f * strength;
		}
	}
}