using ArknightsMod.Content.Items.Placeable.Infrastructure.Elevators;
using ArknightsMod.Content.Systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.ObjectInteractions;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace ArknightsMod.Content.Tiles.Infrastructure.Elevators
{
	public class ElevatorTile : ModTile
	{
		private static string GlassTexturePath = $"{ModContent.GetInstance<ElevatorTile>().Texture}_Glass";
		// 调试：用红色半透明矩形覆盖玻璃罩区域，快速验证坐标/绘制是否命中电梯上层。
		private const bool GlassDebugDrawRect = false;
		// 解决问题用的日志，当前需求不需要输出。
		private const bool GlassDiagLogging = false;

		private const int Width = 4;
		private const int Height = 7;
		private const int HeightPx = Height * 16;
		// 玩家进入这个距离范围：玻璃罩目标状态=开启（visible=100%）；离开：目标状态=关闭（visible=0%）。
		// 上/下拉仅作为视觉过渡（通过 Lerp 逼近目标），不会做“按距离半开半关”。
		private const float GlassToggleDistancePx = 96f;
		private const float GlassToggleHysteresisPx = 16f;
		private const float GlassAnimSpeedPxPerTick = 10f;
	// 视觉偏移：玻璃罩需要整体下移 2.5 格
		private const float GlassOffsetDownTiles = 2.0f;
	private const float GlassOffsetDownPx = GlassOffsetDownTiles * 16f;
		// 透明度：约 25%
	private const byte GlassOverlayAlpha = 64;
		/// <summary>
		/// 玻璃罩 RGB 在环境光基础上的缩放（&lt;1 减轻“白雾”感，仍随 Lighting 变化）。
		/// </summary>
		private const float GlassLightingRgbScale = 0.82f;
		/// <summary>
		/// 轻微冷色偏移（加到 B 通道），让叠层略像玻璃而非中性灰雾。
		/// </summary>
		private const int GlassCoolBlueBias = 6;

		/// <summary>
		/// 按电梯占用格子的光照着色玻璃（与本体瓦片一致走 Lighting），再叠固定透明度。
		/// </summary>
		private static Color GetGlassDrawColor(int tileX, int tileY)
		{
			Color lit = Lighting.GetColor(tileX, tileY);
			int r = (int)(lit.R * GlassLightingRgbScale);
			int g = (int)(lit.G * GlassLightingRgbScale);
			int b = (int)(lit.B * GlassLightingRgbScale + GlassCoolBlueBias);
			return new Color(
				(byte)MathHelper.Clamp(r, 0, 255),
				(byte)MathHelper.Clamp(g, 0, 255),
				(byte)MathHelper.Clamp(b, 0, 255),
				GlassOverlayAlpha);
		}

		// 记录每个电梯的“当前隐藏像素”（只在客户端绘制侧使用）。
		private static readonly Dictionary<int, float> _glassHiddenPxByElevatorId = new Dictionary<int, float>();
		private static readonly Dictionary<int, bool> _glassTargetOpenByElevatorId = new Dictionary<int, bool>();
		private static readonly HashSet<int> _glassDiagLoggedElevatorIds = new HashSet<int>();
		private static readonly HashSet<int> _glassMovingDrawDiagLoggedElevatorIds = new HashSet<int>();
		private static readonly HashSet<int> _glassStaticDrawDiagLoggedElevatorIds = new HashSet<int>();
		private static readonly HashSet<int> _glassHiddenReturnDiagLoggedElevatorIds = new HashSet<int>();
		private const int RequiredSideTileType = 350;
		private static ulong _lastRenderDiagTick;

		public override void SetStaticDefaults()
		{
			Main.tileFrameImportant[Type] = true;
			Main.tileNoAttach[Type] = true;
			Main.tileLavaDeath[Type] = false;
			Main.tileSolidTop[Type] = true;
			Main.tileSolid[Type] = false;

			TileObjectData.newTile.UsesCustomCanPlace = false;
			TileObjectData.newTile.Width = Width;
			TileObjectData.newTile.Height = Height;
			TileObjectData.newTile.Origin = new Point16(1, Height - 1);
			TileObjectData.newTile.CoordinateWidth = 16;
			TileObjectData.newTile.CoordinatePadding = 1;
			TileObjectData.newTile.DrawYOffset = 0;

			int[] heights = new int[Height];
			for (int k = 0; k < Height; k++)
				heights[k] = 16;
			TileObjectData.newTile.CoordinateHeights = heights;

			// 普通放置：底部需要实心方块支撑
			TileObjectData.newTile.AnchorTop = AnchorData.Empty;
			TileObjectData.newTile.AnchorBottom = AnchorData.Empty;
			TileObjectData.newTile.AnchorLeft = AnchorData.Empty;
			TileObjectData.newTile.AnchorRight = AnchorData.Empty;
			TileObjectData.newTile.AnchorWall = false;

			TileObjectData.newTile.HookPostPlaceMyPlayer = new PlacementHook(ModContent.GetInstance<TEElevator>().Hook_AfterPlacement, -1, 0, true);

			// 增加多个放置吸附点，允许从底部 4 格任意一格进行放置（类似门的多 Origin 放置体验）
			TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile);
			TileObjectData.newAlternate.Origin = new Point16(0, Height - 1);
			TileObjectData.addAlternate(0);

			TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile);
			TileObjectData.newAlternate.Origin = new Point16(2, Height - 1);
			TileObjectData.addAlternate(0);

			TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile);
			TileObjectData.newAlternate.Origin = new Point16(3, Height - 1);
			TileObjectData.addAlternate(0);

			TileObjectData.addTile(Type);

			AddMapEntry(new Color(180, 180, 220), Language.GetText("Mods.ArknightsMod.Tiles.ElevatorTile.MapEntry"));
			RegisterItemDrop(ModContent.ItemType<Elevator>());
		}

		public override void KillMultiTile(int i, int j, int frameX, int frameY)
		{
			if (TEElevator.IsSuppressingKillMultiTile)
				return;
			DebugLog($"[Elevator] KillMultiTile at=({i},{j}) frame=({frameX},{frameY})");
			ModContent.GetInstance<TEElevator>().Kill(i, j);
		}

		public override bool HasSmartInteract(int i, int j, SmartInteractScanSettings settings)
		{
			return true;
		}

		public override bool PreDraw(int i, int j, SpriteBatch spriteBatch)
		{
			if (TryGetMovingElevatorByCell(i, j, out _, out _, out _))
			{
				// 移动中由 ElevatorMovingDrawSystem.PostDrawTiles 统一绘制（晚于整层瓦片合成），避免与默认绘制在同一管线内闪烁。
				return false;
			}

			Tile tile = Framing.GetTileSafely(i, j);
			if (tile.HasTile && tile.TileType == Type && TryGetNearbyMovingDownElevator(i, j, out TEElevator downTe))
			{
				// 关键日志：当格子属于电梯 tile，但在 PreDraw 阶段没命中 moving TE，这通常会触发默认绘制造成闪烁。
				DiagnosticLog(
					$"[ElevatorRenderDiag] PreDrawMiss cell=({i},{j}) tePos=({downTe.Position.X},{downTe.Position.Y}) dir={downTe.MoveDir} carry={downTe.VisualPixelOffset:F2}",
					chat: true,
					throttleTicks: 45);
			}
			return true;
		}

		public override void PostDraw(int i, int j, SpriteBatch spriteBatch)
		{
			// 玻璃罩统一在 ElevatorMovingDrawSystem.PostDrawTiles 中绘制，确保在电梯前景遮罩。
		}

		/// <summary>
		/// 移动中电梯的完整绘制（由 <see cref="ElevatorMovingDrawSystem"/> 在每帧 PostDrawTiles 调用一次）。
		/// </summary>
		public static void DrawMovingElevator(SpriteBatch spriteBatch, TEElevator ete)
		{
			if (ete == null || !ete.IsMoving)
				return;

			int topLeftX = ete.Position.X;
			int topLeftY = ete.Position.Y;
			float yOffset = ete.VisualPixelOffset;
			if (ete.MoveDir < 0)
			{
				DiagnosticLog(
					$"[ElevatorRenderDiag] PostDrawTilesMoving topLeft=({topLeftX},{topLeftY}) yOffset={yOffset:F2} base=({ete.RenderBaseFrameX},{ete.RenderBaseFrameY})",
					chat: false,
					throttleTicks: 30);
			}

			Texture2D texture = TextureAssets.Tile[ModContent.TileType<ElevatorTile>()].Value;
			TileObjectData data = GetElevatorTileObjectData(topLeftX, topLeftY);
			if (data == null)
				return;

			// 在 ElevatorMovingDrawSystem.PostDrawTiles 中使用 GameViewMatrix 绘制世界坐标，
			// 不应再额外叠加 offScreenRange，否则会导致该绘制分支相对原本瓦片/背景发生偏移。
			// 在 ElevatorMovingDrawSystem 的 spriteBatch.Begin 已使用 Main.GameViewMatrix.TransformationMatrix，
			// 这里不要再额外叠加 offScreenRange，否则会产生固定的右下偏移。
			Vector2 zero = Vector2.Zero;
			// PostDrawTiles 使用 GameViewMatrix.TransformationMatrix 时，绘制坐标应使用屏幕坐标：
			// worldPos - Main.screenPosition (+ offScreenRange when drawing to render target).
			// 对齐到整像素，避免因 yOffset 小数导致 16x16 拼接出现细缝。
			Vector2 baseScreenPos = new Vector2(topLeftX * 16, (float)MathF.Round(topLeftY * 16f + yOffset))
				- Main.screenPosition + zero;
			int baseFrameX = ete.RenderBaseFrameX;
			int baseFrameY = ete.RenderBaseFrameY;

			int coordW = data.CoordinateWidth;
			int coordPadX = data.CoordinatePadding;
			int coordPadY = data.CoordinatePadding;

			for (int x = 0; x < Width; x++)
			{
				int fx = baseFrameX + x * (coordW + coordPadX);
				for (int y = 0; y < Height; y++)
				{
					int fy = baseFrameY;
					for (int yi = 0; yi < y; yi++)
						fy += data.CoordinateHeights[yi] + coordPadY;

					int ch = data.CoordinateHeights[y];
					Rectangle src = new Rectangle(fx, fy, coordW, ch);
					Vector2 pos = baseScreenPos + new Vector2(x * 16, y * 16);
					pos.X = (float)MathF.Round(pos.X);
					pos.Y = (float)MathF.Round(pos.Y);
					spriteBatch.Draw(texture, pos, src, Lighting.GetColor(topLeftX + x, topLeftY + y));
				}
			}

			DrawMovingGlassOverlay(spriteBatch, ete, topLeftX, topLeftY, yOffset, data, baseFrameX, baseFrameY, coordW, coordPadX, coordPadY, baseScreenPos);
		}

		public static void DrawStaticGlassOverlay(SpriteBatch spriteBatch, int topLeftX, int topLeftY, Tile topLeftTile, int elevatorId)
		{
			float hiddenPx = GetGlassHiddenPixels(topLeftX, topLeftY, 0f, elevatorId);
			if (hiddenPx >= HeightPx - 0.01f)
				return;

			Texture2D glassTexture = ModContent.Request<Texture2D>(GlassTexturePath, AssetRequestMode.ImmediateLoad).Value;
			TileObjectData data = GetElevatorTileObjectData(topLeftX, topLeftY);
			if (data == null)
				return;

			// 静止时电梯内部由 vanilla 使用 tile frame 绘制；
			// 玻璃罩由我们手动绘制，所以 src 的起点必须跟随 tile frame，避免固定偏移。
			int baseFrameX = topLeftTile.TileFrameX;
			int baseFrameY = topLeftTile.TileFrameY;


			// 在 ElevatorMovingDrawSystem 的 spriteBatch.Begin 已使用 Main.GameViewMatrix.TransformationMatrix，
			// 这里不要再额外叠加 offScreenRange，否则会产生固定的右下偏移。
			Vector2 zero = Vector2.Zero;
			Vector2 baseScreenPos = new Vector2(topLeftX * 16, topLeftY * 16) - Main.screenPosition + zero;
			baseScreenPos += new Vector2(0f, GlassOffsetDownPx);

			// 玻璃罩是独立贴图：贴图源网格带 1px 间隔，需要在 src 取样时考虑 padding；
			// 否则会导致右下缝隙未闭合与“黑色超出材质”（越界采样/错位采样）。

			// hiddenPx 表示从“电梯顶部向下”隐藏了多少像素。
			// 为避免出现 16x16 分段裁切造成的缝隙，这里对裁切边界做整数化，
			// 并以“底部锚定”的方式在每个 16px 分段内裁切（srcY+posY 同步）。
			int hiddenBottomPxInt = (int)MathF.Round(MathHelper.Clamp(hiddenPx, 0f, HeightPx));
			int visibleBottomPxInt = HeightPx - hiddenBottomPxInt;

			const int glassCellW = 16;
			const int glassCellH = 16;
			const int glassGapY = 1;
			// 你的 67x101 玻璃贴图的“内容”并不是从第一格 y=0 就开始，
			// 第一格里上方基本透明，若从 y=0 取样会导致玻璃看起来整体偏到电梯右下。
			// 因此这里跳过贴图第 0 行单元，从贴图第 1 行单元开始取样。
			const int glassSrcRowOffset = 1;
			int glassGapX = 0;
			if (Width > 1)
				glassGapX = (glassTexture.Width - glassCellW * Width) / (Width - 1);
			glassGapX = Math.Max(0, glassGapX);
			int glassRowCount = (glassTexture.Height + glassGapY) / (glassCellH + glassGapY);
			int drawRows = Math.Min(Height, Math.Max(0, glassRowCount - glassSrcRowOffset));
			if (GlassDiagLogging && _glassStaticDrawDiagLoggedElevatorIds.Add(elevatorId))
			{
				Vector2 baseScreenPosApprox = baseScreenPos;
				int firstFx = baseFrameX + 0 * (glassCellW + glassGapX);
				int firstFy = baseFrameY + glassSrcRowOffset * (glassCellH + glassGapY);
				int firstSrcY = 0;
				ModContent.GetInstance<TEElevator>().Mod.Logger.Info(
					$"[GlassTexDiag] static elevatorId={elevatorId} topLeft=({topLeftX},{topLeftY}) tex=({glassTexture.Width}x{glassTexture.Height}) firstSrc=(x={firstFx},y={firstFy + firstSrcY},w=16,h=16) baseScreenPos={baseScreenPos} baseScreenPos≈{baseScreenPosApprox}");
				ModContent.GetInstance<TEElevator>().Mod.Logger.Info(
					$"[GlassDrawDiag] static elevatorId={elevatorId} topLeft=({topLeftX},{topLeftY}) hiddenPx={hiddenPx:F2} hiddenBottomPxInt={hiddenBottomPxInt} drawToScreen={Main.drawToScreen} offScreenRange={Main.offScreenRange} baseScreenPos≈{baseScreenPosApprox} screenPos=({Main.screenPosition.X:F0},{Main.screenPosition.Y:F0})");
			}
			if (GlassDebugDrawRect)
			{
				Rectangle rect = new Rectangle((int)baseScreenPos.X, (int)baseScreenPos.Y, Width * 16, drawRows * 16);
				spriteBatch.Draw(TextureAssets.MagicPixel.Value, rect, new Color(255, 60, 60, 80));
			}

			for (int x = 0; x < Width; x++)
			{
				// src 网格带 1px 间隔：宽方向 16 + gapX，行方向 16 + gapY
				int fx = baseFrameX + x * (glassCellW + glassGapX);
				for (int y = 0; y < drawRows; y++)
				{
					int fy = baseFrameY + (y + glassSrcRowOffset) * (glassCellH + glassGapY);

					int segTopPx = y * 16;
					if (segTopPx >= visibleBottomPxInt)
						continue;

					int visibleHeightPx = Math.Min(16, visibleBottomPxInt - segTopPx);
					if (visibleHeightPx <= 0)
						continue;

					Rectangle src = new Rectangle(fx, fy, glassCellW, visibleHeightPx);
					Vector2 pos = baseScreenPos + new Vector2(x * 16, y * 16);
					pos.X = (float)MathF.Round(pos.X);
					pos.Y = (float)MathF.Round(pos.Y);
					Color glassColor = GetGlassDrawColor(topLeftX + x, topLeftY + y);
					spriteBatch.Draw(glassTexture, pos, src, glassColor);
				}
			}
		}

		private static void DrawMovingGlassOverlay(SpriteBatch spriteBatch, TEElevator ete, int topLeftX, int topLeftY, float yOffset, TileObjectData data, int baseFrameX, int baseFrameY, int coordW, int coordPadX, int coordPadY, Vector2 baseScreenPos)
		{
			float hiddenPx = GetGlassHiddenPixels(topLeftX, topLeftY, yOffset, ete.ID);
			if (hiddenPx >= HeightPx - 0.01f)
			{
				if (GlassDiagLogging && _glassHiddenReturnDiagLoggedElevatorIds.Add(ete.ID))
				{
					ModContent.GetInstance<TEElevator>().Mod.Logger.Info(
						$"[GlassDrawDiag] moving elevatorId={ete.ID} hiddenPx={hiddenPx:F2} => early return");
				}
				return;
			}

			Texture2D glassTexture = ModContent.Request<Texture2D>(GlassTexturePath, AssetRequestMode.ImmediateLoad).Value;
			int hiddenBottomPxInt = (int)MathF.Round(MathHelper.Clamp(hiddenPx, 0f, HeightPx));
			int visibleBottomPxInt = HeightPx - hiddenBottomPxInt;
			Vector2 baseScreenPosAdjusted = baseScreenPos + new Vector2(0f, GlassOffsetDownPx);

			// 你的 67x101 玻璃贴图的“内容”并不是从第一格 y=0 开始，
			// debug/取样两处要保持一致：跳过贴图第 0 行单元，从第 1 行开始取样。
			const int glassCellW = 16;
			const int glassCellH = 16;
			const int glassGapY = 1;
			const int glassSrcRowOffset = 1;

			if (GlassDiagLogging && _glassMovingDrawDiagLoggedElevatorIds.Add(ete.ID))
			{
				Vector2 baseScreenPosApprox = baseScreenPos;
				int debugVisibleHeight = Math.Min(16, Math.Max(0, visibleBottomPxInt));
				// first cell 在 src 中要跳过一个贴图行单元（与 static 保持一致）
				int debugFirstSrcY = baseFrameY + glassSrcRowOffset * (glassCellH + glassGapY);
				ModContent.GetInstance<TEElevator>().Mod.Logger.Info(
					$"[GlassTexDiag] moving elevatorId={ete.ID} topLeft=({topLeftX},{topLeftY}) tex=({glassTexture.Width}x{glassTexture.Height}) firstSrc=(x={baseFrameX},y={debugFirstSrcY},w=16,h={debugVisibleHeight}) baseScreenPos={baseScreenPos} baseScreenPos≈{baseScreenPosApprox}");
				ModContent.GetInstance<TEElevator>().Mod.Logger.Info(
					$"[GlassDrawDiag] moving elevatorId={ete.ID} topLeft=({topLeftX},{topLeftY}) yOffset={yOffset:F2} hiddenPx={hiddenPx:F2} hiddenBottomPxInt={hiddenBottomPxInt} drawToScreen={Main.drawToScreen} offScreenRange={Main.offScreenRange} baseScreenPos≈{baseScreenPosApprox} screenPos=({Main.screenPosition.X:F0},{Main.screenPosition.Y:F0}) visibleH={debugVisibleHeight}");
			}

			int glassGapX = 0;
			if (Width > 1)
				glassGapX = (glassTexture.Width - glassCellW * Width) / (Width - 1);
			glassGapX = Math.Max(0, glassGapX);
			int glassRowCount = (glassTexture.Height + glassGapY) / (glassCellH + glassGapY);
			int drawRows = Math.Min(Height, Math.Max(0, glassRowCount - glassSrcRowOffset));

			if (GlassDebugDrawRect)
			{
				Rectangle rect = new Rectangle((int)baseScreenPosAdjusted.X, (int)baseScreenPosAdjusted.Y, Width * 16, drawRows * 16);
				spriteBatch.Draw(TextureAssets.MagicPixel.Value, rect, new Color(255, 60, 60, 80));
			}

			for (int x = 0; x < Width; x++)
			{
				// src 网格带 1px 间隔：宽方向 16 + gapX，行方向 16 + gapY
				int fx = baseFrameX + x * (glassCellW + glassGapX);
				for (int y = 0; y < drawRows; y++)
				{
					int fy = baseFrameY + (y + glassSrcRowOffset) * (glassCellH + glassGapY);

					int segTopPx = y * 16;
					if (segTopPx >= visibleBottomPxInt)
						continue;

					int visibleHeightPx = Math.Min(16, visibleBottomPxInt - segTopPx);
					if (visibleHeightPx <= 0)
						continue;

					Rectangle src = new Rectangle(fx, fy, glassCellW, visibleHeightPx);
					Vector2 pos = baseScreenPosAdjusted + new Vector2(x * 16, y * 16);
					pos.X = (float)MathF.Round(pos.X);
					pos.Y = (float)MathF.Round(pos.Y);
					Color glassColor = GetGlassDrawColor(topLeftX + x, topLeftY + y);
					spriteBatch.Draw(glassTexture, pos, src, glassColor);
				}
			}
		}

		private static float GetGlassHiddenPixels(int topLeftX, int topLeftY, float yOffset, int elevatorId)
		{
			Player player = Main.LocalPlayer;
			if (player == null)
				return 0f;

			float elevatorCenterX = (topLeftX + Width * 0.5f) * 16f;
			float elevatorCenterY = (topLeftY + Height * 0.5f) * 16f + yOffset;
			float distPx = Vector2.Distance(player.Center, new Vector2(elevatorCenterX, elevatorCenterY));

			// targetOpen=true => 完全显示；false => 完全隐藏
			bool prevTargetOpen = _glassTargetOpenByElevatorId.TryGetValue(elevatorId, out bool openedPrev) ? openedPrev : true;
			bool targetOpen = prevTargetOpen;

			if (distPx <= GlassToggleDistancePx - GlassToggleHysteresisPx)
				targetOpen = false;
			else if (distPx >= GlassToggleDistancePx + GlassToggleHysteresisPx)
				targetOpen = true;

			float desiredHiddenPx = targetOpen ? 0f : HeightPx;

			float curHiddenPx = _glassHiddenPxByElevatorId.TryGetValue(elevatorId, out float openedHidden) ? openedHidden : desiredHiddenPx;
			float step = GlassAnimSpeedPxPerTick;
			if (curHiddenPx < desiredHiddenPx)
				curHiddenPx = MathF.Min(curHiddenPx + step, desiredHiddenPx);
			else if (curHiddenPx > desiredHiddenPx)
				curHiddenPx = MathF.Max(curHiddenPx - step, desiredHiddenPx);

			_glassTargetOpenByElevatorId[elevatorId] = targetOpen;
			_glassHiddenPxByElevatorId[elevatorId] = curHiddenPx;

			if (GlassDiagLogging && _glassDiagLoggedElevatorIds.Add(elevatorId))
			{
				ModContent.GetInstance<TEElevator>().Mod.Logger.Info(
					$"[GlassDiag] elevatorId={elevatorId} topLeft=({topLeftX},{topLeftY}) yOffset={yOffset:F2} distPx={distPx:F1} targetOpen={(targetOpen ? 1 : 0)} hiddenPx={curHiddenPx:F1}");
			}

			return curHiddenPx;
		}

		private static bool TryGetMovingElevatorByCell(int i, int j, out int topLeftX, out int topLeftY, out TEElevator movingTe)
		{
			foreach (var kv in TEElevator.ByID)
			{
				if (kv.Value is not TEElevator te || !te.IsMoving)
					continue;
				int x0 = te.Position.X;
				int y0 = te.Position.Y;
				if (i < x0 || i >= x0 + Width || j < y0 || j >= y0 + Height)
					continue;
				topLeftX = x0;
				topLeftY = y0;
				movingTe = te;
				return true;
			}
			topLeftX = -1;
			topLeftY = -1;
			movingTe = null;
			return false;
		}

		private static bool TryGetNearbyMovingDownElevator(int i, int j, out TEElevator movingTe)
		{
			foreach (var kv in TEElevator.ByID)
			{
				if (kv.Value is not TEElevator te || !te.IsMoving || te.MoveDir >= 0)
					continue;
				int x0 = te.Position.X - 1;
				int y0 = te.Position.Y - 1;
				if (i < x0 || i >= x0 + Width + 2 || j < y0 || j >= y0 + Height + 2)
					continue;
				movingTe = te;
				return true;
			}
			movingTe = null;
			return false;
		}

		private static void DiagnosticLog(string msg, bool chat, int throttleTicks)
		{
			try
			{
				ModContent.GetInstance<TEElevator>().Mod.Logger.Info(msg);
			}
			catch
			{
			}
		}

		private static TileObjectData GetElevatorTileObjectData(int topLeftX, int topLeftY)
		{
			for (int sx = 0; sx < Width; sx++)
			{
				for (int sy = 0; sy < Height; sy++)
				{
					Tile t = Framing.GetTileSafely(topLeftX + sx, topLeftY + sy);
					if (t.HasTile && t.TileType == ModContent.TileType<ElevatorTile>())
					{
						TileObjectData d = TileObjectData.GetTileData(t);
						if (d != null)
							return d;
					}
				}
			}
			return TileObjectData.GetTileData(ModContent.TileType<ElevatorTile>(), 0);
		}

		public override bool RightClick(int i, int j)
		{
			// 优先用 footprint 反查已存在的 TE，使用其权威坐标打开 UI；
			// 这样可避免 topLeft 反推与 TE 注册坐标不一致导致“检测不到电梯”。
			if (TEElevator.TryFindElevatorContainingTile(i, j, out TEElevator existing))
			{
				ModContent.GetInstance<ElevatorUISystem>().ShowSettingsWindow(existing.Position.X, existing.Position.Y);
				return true;
			}

			// 没有 TE（例如旧世界放置的电梯）：反推 topLeft，单机/主机补建一个 TE。
			(int topLeftX, int topLeftY) = FindBestTopLeft(i, j);
			if (Main.netMode != NetmodeID.MultiplayerClient)
			{
				int id = ModContent.GetInstance<TEElevator>().Find(topLeftX, topLeftY);
				if (id < 0)
				{
					DebugLog($"[Elevator] TE missing on RightClick. AutoPlace at=({topLeftX},{topLeftY})");
					id = ModContent.GetInstance<TEElevator>().Place(topLeftX, topLeftY);
				}
				if (id >= 0 && TileEntity.ByID.TryGetValue(id, out TileEntity entity) && entity is TEElevator placed)
				{
					placed.ScanFloors();
					ModContent.GetInstance<ElevatorUISystem>().ShowSettingsWindow(placed.Position.X, placed.Position.Y);
					return true;
				}
			}

			// 兜底：直接用反推坐标打开（多人客户端等待服务端同步 TE）。
			ModContent.GetInstance<ElevatorUISystem>().ShowSettingsWindow(topLeftX, topLeftY);
			return true;
		}

		private static (int topLeftX, int topLeftY) FindBestTopLeft(int i, int j)
		{
			Tile t0 = Framing.GetTileSafely(i, j);
			if (!t0.HasTile || t0.TileType != ModContent.TileType<ElevatorTile>())
			{
				Point16 p = TileObjectData.TopLeft(i, j);
				return (p.X, p.Y);
			}

			TileObjectData data = TileObjectData.GetTileData(t0);
			int stepX = (data?.CoordinateWidth ?? 16) + (data?.CoordinatePadding ?? 2);
			int stepY = ((data != null && data.CoordinateHeights != null && data.CoordinateHeights.Length > 0) ? data.CoordinateHeights[0] : 16) + (data?.CoordinatePadding ?? 2);
			if (stepX <= 0)
				stepX = 18;
			if (stepY <= 0)
				stepY = 18;

			// 1) 先扫描附近真正的 topLeft（与 TileObjectData.IsTopLeft 一致；不能要求 FrameX/Y==0，否则多 style 贴图会找错）
			for (int x = i - (Width - 1); x <= i; x++)
			{
				for (int y = j - (Height - 1); y <= j; y++)
				{
					if (x < 0 || x >= Main.maxTilesX || y < 0 || y >= Main.maxTilesY)
						continue;
					Tile t = Framing.GetTileSafely(x, y);
					if (!t.HasTile || t.TileType != t0.TileType)
						continue;
					if (TileObjectData.IsTopLeft(t))
						return (x, y);
				}
			}

			// 2) 退化：用 frame 反推 topLeft（frame 有可能不合法，但比 TileObjectData.TopLeft 更贴近真实）
			int localX = t0.TileFrameX / stepX;
			int localY = t0.TileFrameY / stepY;
			if (localX < 0 || localX >= Width || localY < 0 || localY >= Height)
			{
				Point16 p = TileObjectData.TopLeft(i, j);
				return (p.X, p.Y);
			}
			return (i - localX, j - localY);
		}

		public static (int topLeftX, int topLeftY) GetBestTopLeftForRender(int i, int j)
		{
			return FindBestTopLeft(i, j);
		}

		private static void DebugLog(string msg)
		{
			try
			{
				ModContent.GetInstance<TEElevator>().Mod.Logger.Info(msg);
			}
			catch
			{
			}
		}

		private static int CheckSidesHook(int x, int y, int type, int style, int direction, int alternate)
		{
			// x,y 是左上角(因为 processedCoordinates=true)
			// 只检查左右，不检查上下：
			// 左侧外一列与右侧外一列在本电梯 7 格高度范围内各至少存在 1 个指定 tile 即可放置（允许悬空放置）
			int leftX = x - 1;
			int rightX = x + Width;

			if (leftX < 0 || rightX >= Main.maxTilesX)
				return 0;
			if (y < 0 || y + Height - 1 >= Main.maxTilesY)
				return 0;
			// 约束 1：电梯底部两侧必须是镀层（作为“井的边界”），防止偏移到井外仍然能放
			int bottomY = y + Height - 1;
			Tile bottomLeft = Framing.GetTileSafely(leftX, bottomY);
			Tile bottomRight = Framing.GetTileSafely(rightX, bottomY);
			if (!bottomLeft.HasTile || bottomLeft.TileType != RequiredSideTileType)
				return 0;
			if (!bottomRight.HasTile || bottomRight.TileType != RequiredSideTileType)
				return 0;

			// 约束 2：在电梯高度范围内左右两侧各至少出现一次镀层（允许门洞/楼层大空隙）
			bool hasLeft = false;
			bool hasRight = false;
			for (int dy = 0; dy < Height; dy++)
			{
				Tile left = Framing.GetTileSafely(leftX, y + dy);
				Tile right = Framing.GetTileSafely(rightX, y + dy);

				if (!hasLeft && left.HasTile && left.TileType == RequiredSideTileType)
					hasLeft = true;
				if (!hasRight && right.HasTile && right.TileType == RequiredSideTileType)
					hasRight = true;

				if (hasLeft && hasRight)
					break;
			}
			if (!hasLeft || !hasRight)
				return 0;

			return 1;
		}
	}
}
