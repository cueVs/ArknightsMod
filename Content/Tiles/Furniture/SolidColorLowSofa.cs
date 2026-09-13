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
	public class SolidColorLowSofa : ModTile
	{
		public const int NextStyleHeight = 34;

		public override void SetStaticDefaults() {
			// 基础属性（与 DareUsa 保持一致的交互/坐下集）
			Main.tileFrameImportant[Type] = true;
			Main.tileNoAttach[Type] = true;
			Main.tileLavaDeath[Type] = true;

			TileID.Sets.CanBeSatOnForNPCs[Type] = true;
			TileID.Sets.CanBeSatOnForPlayers[Type] = true;
			TileID.Sets.DisableSmartCursor[Type] = true;

			AddToArray(ref TileID.Sets.RoomNeeds.CountsAsChair); // 计作椅子

			AdjTiles = new int[] { TileID.Chairs };

			// 小地图名字/颜色
			LocalizedText name = CreateMapEntryName();
			AddMapEntry(new Color(180, 180, 195), name);

			// —— 放置数据（参考 DareUsa 的写法，用 CopyFrom + 覆盖关键字段）——
			TileObjectData.newTile.CopyFrom(TileObjectData.Style3x2); // 没有 5×2 预设，选 3×2 当模板
			TileObjectData.newTile.Width = 5;
			TileObjectData.newTile.Height = 2;
			TileObjectData.newTile.Origin = new Point16(2, 1); // 底边中点
			TileObjectData.newTile.CoordinateHeights = new[] { 16, 16 };
			TileObjectData.newTile.CoordinateWidth = 16;
			TileObjectData.newTile.CoordinatePadding = 2;       // 帧间 2px 间隔
																// 两行，不需要 CoordinatePaddingFix；如遇到贴图对不齐再加

			// 需要落地（实心/边/底皆可）
			TileObjectData.newTile.AnchorBottom = new AnchorData(
				AnchorType.SolidTile | AnchorType.SolidWithTop | AnchorType.SolidSide,
				TileObjectData.newTile.Width, 0);

			// 如果以后做左右朝向切换，可开启 Direction / Alternate
			// TileObjectData.newTile.Direction = TileObjectDirection.PlaceLeft;
			// TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile);
			// TileObjectData.newAlternate.Direction = TileObjectDirection.PlaceRight;
			// TileObjectData.addAlternate(1);

			TileObjectData.newTile.UsesCustomCanPlace = true;
			TileObjectData.addTile(Type);
		}

		// 家具一般不允许锤成斜坡/半砖
		public override bool Slope(int i, int j) => false;

		// 只在近距离内显示/触发智能交互（与示例一致）
		public override bool HasSmartInteract(int i, int j, SmartInteractScanSettings settings) =>
			settings.player.IsWithinSnappngRangeToTile(i, j, PlayerSittingHelper.ChairSittingMaxDistance);

		// 关键：坐下时提供基准“锚点”格（用帧定位多物块左上角，再 + 中点偏移）
		public override void ModifySittingTargetInfo(int i, int j, ref TileRestingInfo info) {
			Tile tile = Framing.GetTileSafely(i, j);

			// 计算该 5×2 多物块的左上角（像示例里那样用帧换回 tile 坐标）
			int originX = i - (tile.TileFrameX % (5 * 18)) / 18;
			int originY = j - (tile.TileFrameY % (2 * 18)) / 18;

			// 选择底行中间（第 3 列）作为坐下锚点

			// 面向：默认面向右（你也可以根据玩家相对位置动态设置）
			info.TargetDirection = 1;

			// 可按需微调：
			// info.directionOffset = 6;         // 玩家：6，NPC：2（示例中的写法）
			// info.visualOffset = Vector2.Zero; // 视觉偏移
		}

		public override bool RightClick(int i, int j) {
			Player player = Main.LocalPlayer;
			if (player.IsWithinSnappngRangeToTile(i, j, PlayerSittingHelper.ChairSittingMaxDistance)) {
				player.GamepadEnableGrappleCooldown();
				player.sitting.SitDown(player, i, j); // 具体坐下坐标由 ModifySittingTargetInfo 提供
			}
			return true;
		}

		public override void MouseOver(int i, int j) {
			Player player = Main.LocalPlayer;
			if (!player.IsWithinSnappngRangeToTile(i, j, PlayerSittingHelper.ChairSittingMaxDistance))
				return;

			player.noThrow = 2;
			player.cursorItemIconEnabled = true;
			player.cursorItemIconID = ModContent.ItemType<Items.Placeable.Furniture.SolidColorLowSofa>();
		}

		// 多物块破坏时只掉一份
		public override void KillMultiTile(int i, int j, int frameX, int frameY) {
			Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 5 * 16, 2 * 16,
				ModContent.ItemType<Items.Placeable.Furniture.SolidColorLowSofa>());
		}
	}
}