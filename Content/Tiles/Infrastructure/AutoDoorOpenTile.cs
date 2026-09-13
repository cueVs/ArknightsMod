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
	public class AutoDoorOpenTile : ModTile
	{
		public override string Texture => "ArknightsMod/Assets/Tiles/Infrastructure/AutoDoor_Open_gap2";

		public override void SetStaticDefaults() {
			Main.tileFrameImportant[Type] = true;
			Main.tileSolid[Type]          = false;
			Main.tileBlockLight[Type]     = false;
			Main.tileNoSunLight[Type]     = false;
			Main.tileCut[Type]            = false;
			Main.tileNoAttach[Type]       = true;

			TileObjectData.newTile.CopyFrom(TileObjectData.Style5x4);
			TileObjectData.newTile.Width             = 1;
			TileObjectData.newTile.Height            = 3;
			TileObjectData.newTile.Origin            = new Point16(0, 0);
			TileObjectData.newTile.CoordinateHeights = [16, 16, 16];
			TileObjectData.newTile.CoordinateWidth   = 16;
			TileObjectData.newTile.DrawYOffset       = 0;
			// 开门状态由系统自动切换，无需独立锚点限制
			TileObjectData.newTile.AnchorTop    = new AnchorData(AnchorType.None, 0, 0);
			TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.None, 0, 0);
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
