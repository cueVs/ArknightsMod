using ArknightsMod.Content.Items.Placeable.Infrastructure;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace ArknightsMod.Content.Tiles.Infrastructure
{
	public class AutoDoorClosedTile : ModTile
	{
		public override string Texture => "ArknightsMod/Assets/Tiles/Infrastructure/AutoDoor_Close_gap2";

		public override void SetStaticDefaults() {
			Main.tileFrameImportant[Type] = true;
			Main.tileSolid[Type]          = true;
			// 不设置 tileBlockLight / tileNoSunLight：
			// 门本体不遮挡光照，避免底部黑块以及开关时的光照闪烁

			// 用 Style5x4 作为基础（≥3 行 CoordinateHeights），再覆盖尺寸
			// Style1x2 只有 2 行，强制改 Height=3 时第三格渲染异常
			TileObjectData.newTile.CopyFrom(TileObjectData.Style5x4);
			TileObjectData.newTile.Width             = 1;
			TileObjectData.newTile.Height            = 3;
			TileObjectData.newTile.Origin            = new Point16(0, 2); // 底格为锚点，点底部即可放置
			TileObjectData.newTile.CoordinateHeights = [16, 16, 16];
			TileObjectData.newTile.CoordinateWidth   = 16;
			TileObjectData.newTile.DrawYOffset       = 0; // Style5x4 默认 DrawYOffset=2，置0使门贴紧顶部实心块
			// 上方和下方均需要实心块，与原版门放置要求相同
			TileObjectData.newTile.AnchorTop    = new AnchorData(AnchorType.SolidTile, 1, 0);
			TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile, 1, 0);
			TileObjectData.addTile(Type);

			DustType = DustID.Iron;
			AddMapEntry(new Color(80, 80, 90));
		}

		public override void KillMultiTile(int i, int j, int frameX, int frameY) {
			if (frameX == 0 && frameY == 0)
				Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 16, 48, ModContent.ItemType<AutoDoorItem>());
		}

		public override void NumDust(int i, int j, bool fail, ref int num) => num = fail ? 1 : 3;
	}
}
