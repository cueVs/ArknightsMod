using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent;
using Terraria.GameContent.ObjectInteractions;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace ArknightsMod.Content.Tiles.Furniture
{
	public class PorcelainBed : ModTile
	{
		public override void SetStaticDefaults() {
			Main.tileFrameImportant[Type] = true;
			Main.tileNoAttach[Type] = true;
			Main.tileLavaDeath[Type] = true;

			TileID.Sets.CanBeSleptIn[Type] = true;
			TileID.Sets.DisableSmartCursor[Type] = true;

			LocalizedText name = CreateMapEntryName();
			AddMapEntry(new Color(210, 210, 220), name);

			// 你的图是 70×34（比 72×36 少 2px），保持 18 栅格并用 PaddingFix 去缝
			TileObjectData.newTile.CopyFrom(TileObjectData.Style4x2);
			TileObjectData.newTile.Width = 4;
			TileObjectData.newTile.Height = 2;
			TileObjectData.newTile.Origin = new Point16(2, 1);

			TileObjectData.newTile.CoordinateWidth = 16;
			TileObjectData.newTile.CoordinateHeights = new[] { 16, 16 };
			TileObjectData.newTile.CoordinatePadding = 2;
			TileObjectData.newTile.CoordinatePaddingFix = new Point16(-2, -2); // ← 去中缝

			// ✅ 允许放在平台（沿用原版）
			TileObjectData.newTile.AnchorBottom = new AnchorData(
				AnchorType.SolidTile | AnchorType.SolidWithTop | AnchorType.SolidSide, 4, 0);

			TileObjectData.newTile.UsesCustomCanPlace = true;
			TileObjectData.addTile(Type);
		}

		public override bool HasSmartInteract(int i, int j, SmartInteractScanSettings settings) =>
			settings.player.IsWithinSnappngRangeToTile(i, j, PlayerSleepingHelper.BedSleepingMaxDistance);

		public override void ModifySleepingTargetInfo(int i, int j, ref TileRestingInfo info) {
			Tile tile = Framing.GetTileSafely(i, j);

			// 求 4×2 左上角（18px/格）
			int left = i - (tile.TileFrameX % (4 * 18)) / 18;
			int top = j - (tile.TileFrameY % (2 * 18)) / 18;

			// 与原版接近：底排中偏右
			info.AnchorTilePosition = new Point(left + 2, top + 1);

			// 根据点击相对位置定朝向（原版体验）
			info.TargetDirection = (i < left + 2) ? -1 : 1;

			// ☆ 平台补偿：锚点正下方如果是平台，略微把睡觉位置上提，避免被平台边沿“弹开”
			// 注意：不同 tML 版本成员名可能大小写不同，若没有 VisualOffset 字段，直接删掉这一行即可。
			int ax = info.AnchorTilePosition.X;
			int ay = info.AnchorTilePosition.Y;
			Tile below = Framing.GetTileSafely(ax, ay + 1);
			if (below.HasTile && TileID.Sets.Platforms[below.TileType]) {
				// 向上提 2px；根据你贴图厚度可调成 1~3
				info.VisualOffset = new Vector2(0f, -2f);
			}
		}

		public override bool RightClick(int i, int j) {
			Player player = Main.LocalPlayer;
			if (!player.IsWithinSnappngRangeToTile(i, j, PlayerSleepingHelper.BedSleepingMaxDistance))
				return true;

			player.GamepadEnableGrappleCooldown();

			// 改成直接启动睡觉逻辑（旧版写法）
			player.sleeping.StartSleeping(player, i, j);

			return true;
		}

		public override void KillMultiTile(int i, int j, int frameX, int frameY) {
			Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 4 * 16, 2 * 16,
				ModContent.ItemType<Items.Placeable.Furniture.PorcelainBed>());
		}
	}
}
