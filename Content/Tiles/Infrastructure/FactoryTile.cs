using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Localization;
using Terraria.ID;
using Terraria.DataStructures;
using Terraria.ModLoader;
using Terraria.ObjectData;
using ArknightsMod.Content.Items.Placeable.Infrastructure;

namespace ArknightsMod.Content.Tiles.Infrastructure
{
	public class FactoryTile : ModTile
	{
        public override void SetStaticDefaults()
        {
            Main.tileFrameImportant[Type] = true;
            Main.tileObsidianKill[Type] = true;
            TileObjectData.newTile.CopyFrom(TileObjectData.Style3x2);
			TileObjectData.newTile.Origin = new Point16(1, 4);
			TileObjectData.newTile.Height = 5;
			TileObjectData.newTile.CoordinateHeights = [16, 16, 16, 16, 16];
			TileObjectData.addTile(Type);
            DustType = DustID.Iron;
			LocalizedText name = CreateMapEntryName();
			AddMapEntry(new Color(60, 58, 45)*2, name);
			RegisterItemDrop(ModContent.ItemType<FactoryItem>());
        }
        public override void NumDust(int i, int j, bool fail, ref int num)
        {
            num = (fail ? 1 : 3);
        }
    }
}
