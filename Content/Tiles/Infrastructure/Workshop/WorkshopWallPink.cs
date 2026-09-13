using ArknightsMod.Content.Items.Placeable.Infrastructure.Workshop;
using ArknightsMod.Content.Tiles.Infrastructure;
using Microsoft.Xna.Framework;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Tiles.Infrastructure.Workshop
{
	public class WorkshopWallPink : InfrastructureWall
	{
		public override string Texture => "ArknightsMod/Content/Items/Placeable/Infrastructure/Workshop/加工站墙壁1_Output";

		public override void SetDefaults()
		{
			DustType = DustID.Stone;
			AddMapEntry(new Color(220, 126, 158));
			RegisterItemDrop(ModContent.ItemType<WorkshopWallPinkItem>());
		}
	}
}
