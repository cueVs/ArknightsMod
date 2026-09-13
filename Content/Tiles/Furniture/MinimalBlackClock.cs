using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace ArknightsMod.Content.Tiles.Furniture
{
	public class MinimalBlackClock : ModTile
	{
		public override void SetStaticDefaults() {
			Main.tileFrameImportant[Type] = true;
			Main.tileNoAttach[Type] = true;
			Main.tileLavaDeath[Type] = true;
			TileID.Sets.DisableSmartCursor[Type] = true;

			LocalizedText name = CreateMapEntryName();
			AddMapEntry(new Color(50, 50, 55), name);

			TileObjectData.newTile.CopyFrom(TileObjectData.Style2x2);
			TileObjectData.newTile.Width = 2;
			TileObjectData.newTile.Height = 2;
			TileObjectData.newTile.Origin = new Point16(1, 1); // 便于挂墙
			TileObjectData.newTile.CoordinateHeights = new[] { 16, 16 };
			TileObjectData.newTile.CoordinatePadding = 2;
			TileObjectData.newTile.CoordinatePaddingFix = new Point16(-2, -2);
			TileObjectData.newTile.AnchorWall = true; // 挂墙
			TileObjectData.addTile(Type);
		}
	}
}