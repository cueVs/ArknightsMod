using ArknightsMod.Content.Items.Material.ReclamAlgor;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace ArknightsMod.Content.Tiles.Infrastructure
{
	public class PlanterClusterTile : ModTile
	{
		private const int TileSize = 16;
		private const int Padding = 2;
		private const int WidthInTiles = 2;
		private const int HeightInTiles = 2;
		private static readonly int StageHeight = (TileSize + Padding) * HeightInTiles;

		public override string Texture => "ArknightsMod/Content/Items/Placeable/Infrastructure/PlanterClusterItem_tile";

		public override void SetStaticDefaults() {
			Main.tileFrameImportant[Type] = true;
			Main.tileNoAttach[Type] = true;
			Main.tileLavaDeath[Type] = true;
			Main.tileCut[Type] = false;
			TileID.Sets.DisableSmartCursor[Type] = true;
			TileID.Sets.CanBeClearedDuringGeneration[Type] = true;

			AddMapEntry(new Color(110, 180, 90));

			TileObjectData.newTile.CopyFrom(TileObjectData.Style2x2);
			TileObjectData.newTile.Width = WidthInTiles;
			TileObjectData.newTile.Height = HeightInTiles;
			TileObjectData.newTile.Origin = new Point16(0, HeightInTiles - 1);
			TileObjectData.newTile.CoordinateWidth = TileSize;
			TileObjectData.newTile.CoordinatePadding = Padding;
			TileObjectData.newTile.CoordinateHeights = new[] { TileSize, TileSize };
			TileObjectData.newTile.StyleHorizontal = false;
			TileObjectData.addTile(Type);
		}

		public override bool CanExplode(int i, int j) => false;

		public override bool CanKillTile(int i, int j, ref bool blockDamaged) {
			return true;
		}

		public override void RandomUpdate(int i, int j) {
			if (!WorldGen.InWorld(i, j, 1))
				return;

			Point16 origin = GetOrigin(i, j);
			if (origin.X != i || origin.Y != j)
				return;
			Tile originTile = Framing.GetTileSafely(origin.X, origin.Y);
			int stage = GetStageFromOriginTile(originTile);
			if (stage >= 2)
				return;

			if (Main.rand.NextBool(6))	{
				SetStage(origin.X, origin.Y, stage + 1);
			}
		}

		public override void KillMultiTile(int i, int j, int frameX, int frameY) {
			int stage = frameY / StageHeight;
			if (stage < 2)
				return;

			if (Main.netMode == NetmodeID.MultiplayerClient)
				return;

			int amount = Main.rand.Next(2, 4);
			Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, WidthInTiles * 16, HeightInTiles * 16, ModContent.ItemType<RiceGrain>(), amount);
		}

		private static Point16 GetOrigin(int i, int j) {
			Tile tile = Framing.GetTileSafely(i, j);
			int left = i - (tile.TileFrameX / (TileSize + Padding)) % WidthInTiles;
			int localY = (tile.TileFrameY % StageHeight) / (TileSize + Padding);
			int top = j - localY;
			return new Point16(left, top);
		}

		private static int GetStageFromOriginTile(Tile t) {
			return t.TileFrameY / StageHeight;
		}

		private static bool IsMature(int i, int j) {
			Point16 origin = GetOrigin(i, j);
			Tile t = Framing.GetTileSafely(origin.X, origin.Y);
			return GetStageFromOriginTile(t) >= 2;
		}

		private static void SetStage(int originX, int originY, int stage) {
			int baseFrameY = stage * StageHeight;
			for (int x = 0; x < WidthInTiles; x++) {
				for (int y = 0; y < HeightInTiles; y++) {
					Tile t = Framing.GetTileSafely(originX + x, originY + y);
					if (!t.HasTile)
						continue;
					int withinObjectY = t.TileFrameY % StageHeight;
					if (withinObjectY < 0)
						withinObjectY += StageHeight;
					t.TileFrameY = (short)(baseFrameY + withinObjectY);
				}
			}

			if (Main.netMode == NetmodeID.Server)
				NetMessage.SendTileSquare(-1, originX, originY, WidthInTiles, HeightInTiles);
		}
	}
}
