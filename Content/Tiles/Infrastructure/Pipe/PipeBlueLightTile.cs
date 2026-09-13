using ArknightsMod.Content.Items.Placeable.Infrastructure.Pipe;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Tiles.Infrastructure.Pipe
{
	public class PipeBlueLightPipeTile : InfrastructureTile
	{
		public override bool IsLoadingEnabled(Mod mod) => false;

		public override string Texture => "ArknightsMod/Content/Tiles/Infrastructure/Pipe/PipeBlueLightTile";

		public override void SetDefaults()
		{
			DustType = DustID.Stone;
			AddMapEntry(new Color(80, 140, 220));
			RegisterItemDrop(ModContent.ItemType<PipeBlueLightItem>());
			Main.tileLighted[Type] = true;
			Main.tileMergeDirt[Type] = false;
		}

		public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
		{
			r = 0.09f;
			g = 0.43f;
			b = 0.73f;
		}

		public override bool ShouldMergeWithBlackBridgeStructure() => false;
	}
}
