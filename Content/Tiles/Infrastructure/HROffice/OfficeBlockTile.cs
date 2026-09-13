using ArknightsMod.Content.Items.Placeable.Infrastructure.HROffice;
using Microsoft.Xna.Framework;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Tiles.Infrastructure.HROffice
{
	public class OfficeBlockTile : InfrastructureTile
	{
		public override void SetDefaults()
		{
			DustType = DustID.Stone;
			AddMapEntry(new Color(166, 178, 180));
			RegisterItemDrop(ModContent.ItemType<OfficeBlockItem>());
		}
	}
}
