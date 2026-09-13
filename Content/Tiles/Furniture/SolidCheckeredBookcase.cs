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
	public class SolidCheckeredBookcase : ModTile
	{
		public override void SetStaticDefaults() {
			Main.tileFrameImportant[Type] = true;
			Main.tileNoAttach[Type] = true;
			Main.tileLavaDeath[Type] = true;
			TileID.Sets.DisableSmartCursor[Type] = true;

			AddToArray(ref TileID.Sets.RoomNeeds.CountsAsTable); // 或者不计

			LocalizedText name = CreateMapEntryName();
			AddMapEntry(new Color(200, 200, 210), name);

			TileObjectData.newTile.CopyFrom(TileObjectData.Style3x4); // 取 3×4 模板再覆盖
			TileObjectData.newTile.Width = 6;
			TileObjectData.newTile.Height = 4;
			TileObjectData.newTile.Origin = new Point16(2, 3);
			TileObjectData.newTile.CoordinateHeights = new[] { 16, 16, 16, 16 };
			TileObjectData.newTile.CoordinatePadding = 2;
			TileObjectData.newTile.CoordinatePaddingFix = new Point16(-2, -2); // 106×70 修正
			TileObjectData.newTile.AnchorBottom = new AnchorData(
				AnchorType.SolidTile | AnchorType.SolidWithTop | AnchorType.SolidSide, 6, 0);
			TileObjectData.addTile(Type);
		}
	}
}