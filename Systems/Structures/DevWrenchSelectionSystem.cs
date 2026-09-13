using Terraria.DataStructures;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;
using ArknightsMod.Content.Items.Developer;

namespace ArknightsMod.Systems.Structures
{
	// 负责把「开发测试扳手」当前的框选范围画成一圈点状高亮线。
	// 世界空间绘制统一走 PostDrawTiles——这是本模组已有的惯例
	// （方块层画完之后、UI 画之前），不要挪到别的钩子里画，否则容易被
	// 其它图层盖住或者盖住别的东西。
	public class DevWrenchSelectionSystem : ModSystem
	{
		// 点状线：一段实心、一段透明，交替排列。
		private const int DashLength = 6;
		private const int GapLength = 4;
		private const int LineThickness = 2;

		public override void PostDrawTiles() {
			if (Main.dedServ || Main.gameMenu)
				return;

			Player player = Main.LocalPlayer;
			if (player?.HeldItem?.ModItem is not DevTestWrench)
				return;

			var wrenchPlayer = player.GetModPlayer<DevWrenchPlayer>();

			// 正在拖拽时画实时选框；没在拖拽但已有定格选区时，也持续画出来，
			// 方便玩家在敲 /exp 之前确认自己选的到底是哪一块。
			Point16 start, end;
			if (wrenchPlayer.IsDragging) {
				start = wrenchPlayer.DragStart;
				end = wrenchPlayer.DragCurrent;
			}
			else if (wrenchPlayer.FinalizedSelection.HasValue) {
				(start, end) = wrenchPlayer.FinalizedSelection.Value;
			}
			else {
				return;
			}

			DrawSelectionBox(start, end, wrenchPlayer.IsDragging ? Color.Cyan : Color.LimeGreen);
		}

		private static void DrawSelectionBox(Point16 startTile, Point16 endTile, Color color) {
			int minX = System.Math.Min(startTile.X, endTile.X);
			int maxX = System.Math.Max(startTile.X, endTile.X);
			int minY = System.Math.Min(startTile.Y, endTile.Y);
			int maxY = System.Math.Max(startTile.Y, endTile.Y);

			// 选框按"格子外边缘"画，+1 是因为要框住 maxX/maxY 那一格的右/下边界。
			Vector2 topLeft = new Vector2(minX * 16, minY * 16) - Main.screenPosition;
			Vector2 bottomRight = new Vector2((maxX + 1) * 16, (maxY + 1) * 16) - Main.screenPosition;

			Texture2D pixel = TextureAssets.MagicPixel.Value;

			// ⚠ PostDrawTiles 触发时 Main.spriteBatch 并不在 Begin 状态里（不同于
			// ModProjectile.PreDraw 那种运行在原版已开启的 Begin/End 区块内）。
			// 之前误照抄了"先 End() 再 Begin()"的写法，导致 Begin 从未被调用过就
			// 调用 End()，直接抛 InvalidOperationException 崩溃。这里只能自己
			// Begin，画完自己 End，进来和出去都保持"未开启"状态，不要在结束时
			// 再多 Begin 一次去"复原"——根本不需要复原，因为进来时它就没开着。
			Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
				SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone,
				null, Main.GameViewMatrix.TransformationMatrix);

			DashedLineHorizontal(pixel, topLeft.X, bottomRight.X, topLeft.Y, color);
			DashedLineHorizontal(pixel, topLeft.X, bottomRight.X, bottomRight.Y, color);
			DashedLineVertical(pixel, topLeft.Y, bottomRight.Y, topLeft.X, color);
			DashedLineVertical(pixel, topLeft.Y, bottomRight.Y, bottomRight.X, color);

			Main.spriteBatch.End();
		}

		private static void DashedLineHorizontal(Texture2D pixel, float xFrom, float xTo, float y, Color color) {
			int step = DashLength + GapLength;
			for (float x = xFrom; x < xTo; x += step) {
				float dashEnd = System.Math.Min(x + DashLength, xTo);
				DrawFilledRect(pixel, x, y - LineThickness * 0.5f, dashEnd - x, LineThickness, color);
			}
		}

		private static void DashedLineVertical(Texture2D pixel, float yFrom, float yTo, float x, Color color) {
			int step = DashLength + GapLength;
			for (float y = yFrom; y < yTo; y += step) {
				float dashEnd = System.Math.Min(y + DashLength, yTo);
				DrawFilledRect(pixel, x - LineThickness * 0.5f, y, LineThickness, dashEnd - y, color);
			}
		}

		private static void DrawFilledRect(Texture2D pixel, float x, float y, float w, float h, Color color) {
			Main.spriteBatch.Draw(pixel, new Rectangle((int)x, (int)y, (int)System.Math.Ceiling(w), (int)System.Math.Ceiling(h)), color);
		}
	}
}
