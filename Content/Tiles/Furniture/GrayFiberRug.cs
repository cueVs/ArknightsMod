using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace ArknightsMod.Content.Tiles.Furniture
{
	public class GrayFiberRug : ModTile
	{
		public override void SetStaticDefaults() {
			Main.tileFrameImportant[Type] = true;
			Main.tileNoAttach[Type] = true;
			Main.tileLavaDeath[Type] = true;
			TileID.Sets.DisableSmartCursor[Type] = true;

			LocalizedText name = CreateMapEntryName();
			AddMapEntry(new Color(120, 120, 120), name);

			// 贴图 34×8：宽 2 格，每格 17px；高 1 格仅 8px；无 padding
			TileObjectData.newTile.CopyFrom(TileObjectData.Style2x1);
			TileObjectData.newTile.Width = 2;
			TileObjectData.newTile.Height = 1;
			TileObjectData.newTile.Origin = new Point16(0, 0);

			TileObjectData.newTile.CoordinateWidth = 17;     // ← 关键
			TileObjectData.newTile.CoordinateHeights = new[] { 8 }; // ← 关键（贴图只有 8px 高）
			TileObjectData.newTile.CoordinatePadding = 0;

			// 让 8px 的图“贴地”，向下对齐
			TileObjectData.newTile.DrawYOffset = 16 - 8; // = 8

			// 落地
			TileObjectData.newTile.AnchorBottom = new AnchorData(
				AnchorType.SolidTile | AnchorType.SolidWithTop | AnchorType.SolidSide, 2, 0);

			TileObjectData.newTile.UsesCustomCanPlace = true;
			TileObjectData.addTile(Type);
		}
	}
}