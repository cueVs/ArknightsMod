using ArknightsMod.Content.Items.Placeable.Infrastructure.Elevators;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace ArknightsMod.Content.Tiles.Infrastructure.Elevators
{
	public class ElevatorButtonTile : ModTile
	{
		private const bool ElevatorButtonTextEnabled = false;
		public override string HighlightTexture => $"{Texture}_Highlight";
		private static readonly SoundStyle PressButtonSound = new SoundStyle("ArknightsMod/Content/Sounds/电梯_按下按钮");

		public override void SetStaticDefaults()
		{
			TileID.Sets.HasOutlines[Type] = true;
			Main.tileFrameImportant[Type] = true;
			Main.tileNoAttach[Type] = false;
			Main.tileLavaDeath[Type] = false;
			Main.tileSolid[Type] = false;
			Main.tileSolidTop[Type] = false;

			// 贴图 电梯按钮_gap1：33×50 = 横向 2×(16+1)−1、纵向 3×(16+1)−1（分片间 1px，无末尾冗余缝）
			TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
			TileObjectData.newTile.Width = 2;
			TileObjectData.newTile.Height = 3;
			TileObjectData.newTile.CoordinateWidth = 16;
			TileObjectData.newTile.CoordinateHeights = new int[] { 16, 16, 16 };
			TileObjectData.newTile.CoordinatePadding = 1;
			TileObjectData.newTile.CoordinatePaddingFix = new Point16(-1, -1);
			TileObjectData.newTile.Origin = new Point16(0, TileObjectData.newTile.Height - 1);
			TileObjectData.newTile.AnchorTop = AnchorData.Empty;
			TileObjectData.newTile.AnchorLeft = AnchorData.Empty;
			TileObjectData.newTile.AnchorRight = AnchorData.Empty;
			TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile | AnchorType.SolidWithTop | AnchorType.SolidSide, TileObjectData.newTile.Width, 0);
			TileObjectData.addTile(Type);

			AddMapEntry(new Color(120, 190, 220), Language.GetText("Mods.ArknightsMod.Tiles.ElevatorButtonTile.MapEntry"));
			RegisterItemDrop(ModContent.ItemType<ElevatorButton>());
		}

		public override bool RightClick(int i, int j)
		{
			Player player = Main.LocalPlayer;
			if (player == null || !player.active)
				return true;

			// 以按钮位置为中心搜索“附近电梯井中的电梯”，避免要求玩家必须站在井内中线。
			Vector2 queryPos = new Vector2(i * 16f + 8f, j * 16f + 8f);
			if (!TEElevator.TryFindNearbyElevatorByWorld(queryPos.X, queryPos.Y, 120, out TEElevator te, out _, out _))
			{
				if (ElevatorButtonTextEnabled) {
					Main.NewText("[Elevator] 附近未找到电梯井中的电梯。");
				}
				return true;
			}

			if (!te.MoveToNearestFloorForPlayer(player))
			{
				if (ElevatorButtonTextEnabled)
					Main.NewText("[Elevator] 未找到可用楼层。");
				return true;
			}

			// 玩家交互电梯交互按钮：立即播放“按下按钮”音效。
			SoundEngine.PlaySound(PressButtonSound, player.Center);
			int target = te.TargetFloorBottomY;
			if (ElevatorButtonTextEnabled)
				Main.NewText($"[Elevator] 已呼叫电梯到当前楼层 (bottomY={target})");
			return true;
		}
	}
}
