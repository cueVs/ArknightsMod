using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Systems.Structures
{
	// 便携建筑放置后的"方块飞入落位"出场特效——移植自另一个自有项目
	// PocketStrata 的 PocketWorldTileTransitionSystem（子世界结构
	// 出现时的效果），只搬了"出现"（opening）这一半，没有搬"消失"那一半，
	// 因为便携建筑只会凭空出现，不存在"结构消失"的场景，源系统里配套的
	// fromCells/exit 飞出动画整个用不上，故意没有照搬，不是漏做。
	//
	// 效果：方块贴着地面从下往上、按行错开延迟依次"飞"入最终位置，
	// 配合镜头轻微震动 + 地面扬尘。落地前真实方块暂时隐形（IsTileInvisible/
	// IsWallInvisible），避免真身和飞行中的"虚影"同时可见。
	//
	// 联机：这个类本身只做本机视觉效果，不摸任何会联网同步的状态，
	// 调用方（AkStructureDeploySystem）负责保证：
	// 真正的方块数据只由服务器/单人权威写入 + NetMessage.SendTileSquare
	// 同步，而"要不要播这个特效"这件事则通过单独广播给所有客户端
	// （包括放置者自己）来触发，确保所有人本地各自播放同一份、时间对得上的
	// 动画，而不是只有放置者能看到。
	public sealed class PortableStructureEntryEffect : ModSystem
	{
		private const float RowStaggerSeconds = 0.13f;
		private const float EnterLeadSeconds = 0.14f;
		private const float EnterAnimSeconds = 0.68f;
		private const float EnterStartBelowPixels = 76f;
		private const float EnterHorizontalSpreadPixels = 36f;
		private const float MaxRadialDelaySeconds = 0.22f;
		private const float FadeOutStart = 0.88f;

		private sealed class FlyCell
		{
			public int TileX;
			public int TileY;
			public StructureCell Cell;
			public float Delay;
			public float Hash;
			public bool Started;
			public bool Finished;
			public bool HiddenTile;
			public bool HiddenWall;
		}

		private readonly List<FlyCell> _cells = new();

		private Rectangle _area;
		private float _elapsed;
		private bool _active;
		private Vector2 _structureCenterWorld;
		private float _structureGroundWorldY;
		private int _lastRiseDustFrame = -1;

		public static float CameraShake { get; private set; }

		private static PortableStructureEntryEffect Instance;

		public override void Load() => Instance = this;

		public override void Unload() {
			ClearEffect();
			Instance = null;
		}

		public override void ModifyScreenPosition() {
			if (CameraShake <= 0.05f)
				return;

			Main.screenPosition += new Vector2(
				Main.rand.NextFloat(-CameraShake, CameraShake),
				Main.rand.NextFloat(-CameraShake, CameraShake));
		}

		public override void PostUpdateEverything() {
			if (CameraShake > 0.05f)
				CameraShake *= _active ? 0.9f : 0.75f;

			if (!_active || Main.dedServ)
				return;

			_elapsed += 1f / 60f;
			UpdateCells();
			SpawnGroundDust();

			if (!HasPendingWork())
				FinishEffect();
		}

		public override void PostDrawTiles() {
			if (!_active || Main.dedServ || Main.gameMenu || _cells.Count == 0)
				return;

			// PostDrawTiles 触发时 spriteBatch 不在 Begin 状态（这个坑之前在扳手那边
			// 踩过一次），这里只自己 Begin/End，不要照抄"先 End 再 Begin"那种写法。
			Main.spriteBatch.Begin(
				SpriteSortMode.Immediate,
				BlendState.AlphaBlend,
				SamplerState.PointClamp,
				DepthStencilState.None,
				Main.Rasterizer,
				null,
				Main.GameViewMatrix.TransformationMatrix);

			foreach (FlyCell fc in _cells) {
				if (!fc.Started || fc.Finished)
					continue;

				float localTime = _elapsed - fc.Delay;
				float enterProgress = MathHelper.Clamp((localTime - EnterLeadSeconds) / EnterAnimSeconds, 0f, 1f);
				if (localTime <= EnterLeadSeconds)
					continue;

				Vector2 targetCenter = new(fc.TileX * 16f + 8f, fc.TileY * 16f + 8f);
				float eased = EaseOut(enterProgress);
				float scale = MathHelper.Lerp(0.72f, 1f, eased);
				float alpha = MathHelper.Lerp(0f, 1f, EaseIn(MathHelper.Clamp(enterProgress * 1.08f, 0f, 1f)));

				if (enterProgress > FadeOutStart) {
					float t = (enterProgress - FadeOutStart) / (1f - FadeOutStart);
					alpha *= MathHelper.Clamp(1f - t * t, 0f, 1f);
				}

				if (alpha <= 0.01f)
					continue;

				float rotation = (fc.Hash - 0.5f) * 0.55f * (1f - eased);
				Vector2 worldPos = ComputeEnterWorldPos(fc, targetCenter, eased);

				PortableStructureFlyingTileDrawHelper.DrawCell(
					Main.spriteBatch, fc.Cell, worldPos, scale, rotation, alpha, fc.TileX, fc.TileY);
			}

			Main.spriteBatch.End();
		}

		// 开始播放一次"结构出现"动画。area 必须是结构真正落地
		// 之后所占的世界 tile 矩形——调用之前请先完成真正的方块写入
		// （StructureData.PlaceAt），这个方法只管好看，不碰任何
		// 方块数据本身。仅在单人/联机客户端本地生效，专用服务器直接忽略。
		public static void TryBeginEntryEffect(Rectangle area, StructureData structure) {
			if (Main.dedServ || Main.gameMenu)
				return;
			if (Main.netMode != NetmodeID.SinglePlayer && Main.netMode != NetmodeID.MultiplayerClient)
				return;
			if (area.Width <= 0 || area.Height <= 0 || structure == null)
				return;

			Instance ??= ModContent.GetInstance<PortableStructureEntryEffect>();
			if (Instance._active)
				Instance.ClearEffect();

			Instance._area = area;
			Instance._elapsed = 0f;
			Instance._lastRiseDustFrame = -1;
			Instance._cells.Clear();
			CameraShake = 0f;

			int minRow = area.Height, maxRow = -1;
			for (int dy = 0; dy < area.Height && dy < structure.Height; dy++) {
				for (int dx = 0; dx < area.Width && dx < structure.Width; dx++) {
					StructureCell cell = structure[dx, dy];
					if (!cell.HasTile && !cell.HasWall)
						continue;
					if (dy < minRow) minRow = dy;
					if (dy > maxRow) maxRow = dy;
				}
			}
			if (maxRow < 0) {
				minRow = 0;
				maxRow = area.Height - 1;
			}

			Instance._structureGroundWorldY = (area.Y + maxRow + 1) * 16f - 2f;
			Instance._structureCenterWorld = new Vector2(
				(area.X + area.Width * 0.5f) * 16f,
				(area.Y + (minRow + maxRow + 1f) * 0.5f) * 16f);

			for (int dy = 0; dy < area.Height && dy < structure.Height; dy++) {
				for (int dx = 0; dx < area.Width && dx < structure.Width; dx++) {
					StructureCell cell = structure[dx, dy];
					if (!cell.HasTile && !cell.HasWall)
						continue;

					int tx = area.X + dx;
					int ty = area.Y + dy;
					float hash = Hash01(tx, ty);

					float rowFromBottom = maxRow - dy;
					float radial = Vector2.Distance(
						new Vector2((area.X + area.Width * 0.5f) * 16f, ty * 16f),
						Instance._structureCenterWorld) * 0.00035f * MaxRadialDelaySeconds;
					float delay = Math.Max(0f, rowFromBottom) * RowStaggerSeconds + radial + hash * 0.04f;

					Instance._cells.Add(new FlyCell {
						TileX = tx,
						TileY = ty,
						Cell = cell,
						Delay = delay,
						Hash = hash,
					});
				}
			}

			if (Instance._cells.Count == 0)
				return;

			// 关键：这里要把所有格子的真实方块「立即」整体隐藏，而不是等每行自己的
			// 延迟到了才隐藏。之前的 bug 就出在这——结构在调用本方法之前已经被
			// TryDeployLocally 真实落地、下一帧就会正常可见；如果隐藏动作跟着每行的
			// Delay 延后触发，落地和"藏起来开始飞入"之间会有一段空窗，真实方块会先
			// 原样露一下，然后才被藏起来重新飞入，看起来就是"先出现一部分、又被盖掉
			// 重新来一遍"。整体提前到 Begin 的这一刻统一隐藏，就不会再有这个空窗——
			// 从特效开始的那一帧起，所有格子都是"虚影"，直到各自延迟到了才揭晓成真身。
			foreach (FlyCell fc in Instance._cells)
				Instance.HideCell(fc);

			Instance._active = true;
			CameraShake = 1.5f;
		}

		private void UpdateCells() {
			foreach (FlyCell fc in _cells) {
				if (fc.Finished)
					continue;

				if (!fc.Started && _elapsed >= fc.Delay)
					fc.Started = true;

				if (fc.Started && _elapsed - fc.Delay >= EnterLeadSeconds + EnterAnimSeconds)
					FinishCell(fc);
			}
		}

		// 只负责「藏起真身」，不再负责 fc.Started——现在所有格子在特效一开始就统一
		// 调用这个方法整体隐藏一次（见 TryBeginEntryEffect 末尾），不是等各自延迟到了
		// 才隐藏，见上面那段注释解释的 bug 原因。
		private void HideCell(FlyCell fc) {
			if (fc.TileX < 0 || fc.TileY < 0 || fc.TileX >= Main.maxTilesX || fc.TileY >= Main.maxTilesY)
				return;

			Tile t = Main.tile[fc.TileX, fc.TileY];
			if (t.HasTile && !t.IsTileInvisible) {
				t.IsTileInvisible = true;
				fc.HiddenTile = true;
			}
			if (t.WallType != WallID.None && !t.IsWallInvisible) {
				t.IsWallInvisible = true;
				fc.HiddenWall = true;
			}
		}

		private void FinishCell(FlyCell fc) {
			if (fc.Finished)
				return;

			RestoreCellVisual(fc);
			fc.Finished = true;
		}

		private void RestoreCellVisual(FlyCell fc) {
			if (fc.TileX < 0 || fc.TileY < 0 || fc.TileX >= Main.maxTilesX || fc.TileY >= Main.maxTilesY)
				return;

			Tile t = Main.tile[fc.TileX, fc.TileY];
			if (fc.HiddenTile)
				t.IsTileInvisible = false;
			if (fc.HiddenWall)
				t.IsWallInvisible = false;

			fc.HiddenTile = false;
			fc.HiddenWall = false;
		}

		private bool HasPendingWork() {
			foreach (FlyCell fc in _cells) {
				if (!fc.Finished)
					return true;
			}
			return false;
		}

		private void FinishEffect() {
			foreach (FlyCell fc in _cells)
				RestoreCellVisual(fc);

			ClearEffect();
		}

		private void ClearEffect() {
			foreach (FlyCell fc in _cells)
				RestoreCellVisual(fc);

			_active = false;
			_cells.Clear();
			_area = Rectangle.Empty;
			_elapsed = 0f;
			_lastRiseDustFrame = -1;
			_structureGroundWorldY = 0f;
			CameraShake = 0f;
		}

		private void SpawnGroundDust() {
			if (_area.Width <= 0)
				return;

			int frame = (int)(_elapsed * 60f);
			if (frame == _lastRiseDustFrame)
				return;
			_lastRiseDustFrame = frame;

			float groundY = _structureGroundWorldY;
			float left = _area.X * 16f;
			float width = _area.Width * 16f;

			for (int k = 0; k < 12; k++) {
				Vector2 pos = new(
					left + Main.rand.NextFloat(0f, width),
					groundY + Main.rand.NextFloat(-3f, 2f));

				int d = Dust.NewDust(pos, 10, 3, DustID.Smoke,
					Main.rand.NextFloat(-0.4f, 0.4f), Main.rand.NextFloat(-0.08f, 0.08f),
					110, new Color(190, 170, 140), Main.rand.NextFloat(1.1f, 1.9f));

				if (d >= Main.maxDust)
					continue;

				Main.dust[d].noGravity = true;
				Main.dust[d].velocity *= 0.2f;
			}

			if (frame % 14 == 0 && HasPendingWork())
				CameraShake = Math.Min(CameraShake + 1.2f, 7f);
		}

		private Vector2 ComputeEnterWorldPos(FlyCell fc, Vector2 targetCenter, float eased) {
			float below = EnterStartBelowPixels + fc.Hash * 28f;
			float side = (fc.Hash - 0.5f) * EnterHorizontalSpreadPixels * (1f - eased * 0.65f);
			Vector2 start = targetCenter + new Vector2(side, below);
			return Vector2.Lerp(start, targetCenter, eased);
		}

		private static float Hash01(int x, int y) {
			uint h = (uint)(x * 73856093 ^ y * 19349663);
			h ^= h >> 16;
			h *= 2246822507u;
			return (h & 0xFFFF) / 65535f;
		}

		private static float EaseIn(float t) => t * t;

		private static float EaseOut(float t) {
			float inv = 1f - t;
			return 1f - inv * inv;
		}
	}
}
