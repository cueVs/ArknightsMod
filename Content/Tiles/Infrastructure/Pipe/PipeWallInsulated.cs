using ArknightsMod.Content.Items.Placeable.Infrastructure.Pipe;
using ArknightsMod.Content.Tiles.Infrastructure;
using Microsoft.Xna.Framework;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Tiles.Infrastructure.Pipe
{
	public class PipeWallInsulated : InfrastructureWall
	{
		public override string Texture => "ArknightsMod/Content/Tiles/Infrastructure/Pipe/PipeWall3";

		public override void SetDefaults()
		{
			DustType = DustID.Silver;
			AddMapEntry(new Color(140, 145, 155));
			RegisterItemDrop(ModContent.ItemType<PipeWallInsulatedItem>());
		}
	}
}
