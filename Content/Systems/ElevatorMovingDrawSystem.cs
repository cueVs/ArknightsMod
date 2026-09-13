using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using ArknightsMod.Content.Tiles;
using ArknightsMod.Content.Tiles.Infrastructure.Elevators;
using System.Collections.Generic;

namespace ArknightsMod.Content.Systems
{
	/// <summary>
	/// 移动中的电梯在 <see cref="ModSystem.PostDrawTiles"/> 中绘制（晚于 TileDrawing 内 CustomNonSolid 的 SpecialDraw），
	/// 避免与默认瓦片/背景墙在同一批合成里竞争顺序导致下行闪烁。
	/// </summary>
	public class ElevatorMovingDrawSystem : ModSystem
	{
		// 解决问题期间的渲染日志，当前不需要输出。
		private const bool ElevatorRenderDiagLogging = false;
		private static readonly HashSet<int> _staticTileMissingDiagLoggedElevatorIds = new HashSet<int>();
		private static readonly HashSet<int> _staticGlassDrawAttemptDiagLoggedElevatorIds = new HashSet<int>();

		public override void PostDrawTiles()
		{
			if (Main.gameMenu || Main.dedServ)
				return;

			bool hasAnyElevatorTE = false;
			foreach (var kv in TileEntity.ByID)
			{
				if (kv.Value is TEElevator)
				{
					hasAnyElevatorTE = true;
					break;
				}
			}

			// 没有 TE 时也要绘制玻璃罩（否则会出现“先交互一次电梯后才出现玻璃罩”）。
			// 这里采用“扫描当前屏幕可见瓦片范围”的方式，只在客户端绘制侧生效。
			bool shouldScanTiles = !hasAnyElevatorTE;

			// 使用 Immediate 保证绘制顺序（电梯内部 -> 玻璃罩覆盖），避免 Deferred 造成的重排导致“玻璃在后面”。
			// 这里使用屏幕坐标绘制：绘制函数内部会减去 Main.screenPosition，
			// 确保不会出现“跟着玩家/相机移动”或画到屏幕外的问题。
			// 同时用 PointClamp 避免 16x16 拼接时的采样缝隙。
			Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

			try
			{
				if (hasAnyElevatorTE)
				{
					foreach (var kv in TileEntity.ByID)
					{
						if (kv.Value is not TEElevator te)
							continue;

						if (te.IsMoving)
						{
							ElevatorTile.DrawMovingElevator(Main.spriteBatch, te);
						}
						else
						{
							// 静止电梯：同样在 PostDrawTiles 里绘制玻璃罩，保证覆盖在电梯材质前面。
							(int topLeftX, int topLeftY) = ElevatorTile.GetBestTopLeftForRender(te.Position.X, te.Position.Y);
							Tile topLeftTile = Framing.GetTileSafely(topLeftX, topLeftY);
							if (topLeftTile.HasTile && topLeftTile.TileType == ModContent.TileType<ElevatorTile>())
							{
								if (_staticGlassDrawAttemptDiagLoggedElevatorIds.Add(te.ID))
								{
									if (ElevatorRenderDiagLogging)
									{
										ModContent.GetInstance<TEElevator>().Mod.Logger.Info(
											$"[ElevatorDiag] static glass draw attempt elevatorId={te.ID} topLeft=({topLeftX},{topLeftY}) tePos=({te.Position.X},{te.Position.Y})");
									}
								}
								ElevatorTile.DrawStaticGlassOverlay(Main.spriteBatch, topLeftX, topLeftY, topLeftTile, te.ID);
							}
							else
							{
								if (_staticTileMissingDiagLoggedElevatorIds.Add(te.ID))
								{
										if (ElevatorRenderDiagLogging)
										{
											ModContent.GetInstance<TEElevator>().Mod.Logger.Info(
												$"[ElevatorDiag] static glass NOT drawn elevatorId={te.ID} topLeft=({topLeftX},{topLeftY}) tePos=({te.Position.X},{te.Position.Y}) hasTile={(topLeftTile.HasTile ? 1 : 0)} tileType={topLeftTile.TileType}");
										}
								}
							}
						}
					}
				}

				if (shouldScanTiles)
				{
					int type = ModContent.TileType<ElevatorTile>();
					int minTileX = (int)(Main.screenPosition.X / 16f) - 2;
					int minTileY = (int)(Main.screenPosition.Y / 16f) - 2;
					int maxTileX = minTileX + (Main.screenWidth / 16) + 6;
					int maxTileY = minTileY + (Main.screenHeight / 16) + 6;
					if (minTileX < 0) minTileX = 0;
					if (minTileY < 0) minTileY = 0;
					if (maxTileX >= Main.maxTilesX) maxTileX = Main.maxTilesX - 1;
					if (maxTileY >= Main.maxTilesY) maxTileY = Main.maxTilesY - 1;

					HashSet<Point16> drawn = new HashSet<Point16>();
					for (int x = minTileX; x <= maxTileX; x++)
					{
						for (int y = minTileY; y <= maxTileY; y++)
						{
							Tile t = Framing.GetTileSafely(x, y);
							if (!t.HasTile || t.TileType != type)
								continue;
							// 用渲染侧的 topLeft 推导，避免 frame 异常导致错位。
							(int topLeftX, int topLeftY) = ElevatorTile.GetBestTopLeftForRender(x, y);
							Point16 key = new Point16(topLeftX, topLeftY);
							if (drawn.Contains(key))
								continue;
							drawn.Add(key);

							Tile tl = Framing.GetTileSafely(topLeftX, topLeftY);
							if (!tl.HasTile || tl.TileType != type)
								continue;
							// 没有 TE 时，用坐标组合出一个稳定 key 来驱动玻璃罩开/关动画状态。
							int pseudoId = (topLeftX & 0xFFFF) | (topLeftY << 16);
							ElevatorTile.DrawStaticGlassOverlay(Main.spriteBatch, topLeftX, topLeftY, tl, pseudoId);
						}
					}
				}
			}
			finally
			{
				Main.spriteBatch.End();
			}
		}
	}
}
