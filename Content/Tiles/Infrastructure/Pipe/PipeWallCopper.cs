using ArknightsMod.Content.Items.Placeable.Infrastructure.Pipe;
using ArknightsMod.Content.Tiles.Infrastructure;
using Microsoft.Xna.Framework;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Tiles.Infrastructure.Pipe
{
	public class PipeWallCopper : InfrastructureWall
	{
		public override string Texture => "ArknightsMod/Content/Tiles/Infrastructure/Pipe/PipeWall2";

		public override void SetDefaults()
		{
			DustType = DustID.Copper;
			AddMapEntry(new Color(151, 107, 75));
			RegisterItemDrop(ModContent.ItemType<PipeWallCopperItem>());
		}
	}
}
