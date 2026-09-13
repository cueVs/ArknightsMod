using System.Collections.Generic;
using ArknightsMod.Content.Tiles.Infrastructure;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Systems
{
	// 自动门系统：玩家靠近时打开，离开时关闭，切换时播放原版门音效。
	// 仅在服务端（或单机）执行，避免客户端重复操作。
	public class AutoDoorSystem : ModSystem
	{
		// 触发开门的水平感应距离（像素）：约 3 格
		private const float OpenDistance  = 48f;
		// 关门的水平感应距离（留出迟滞防止在边界来回抖动）
		private const float CloseDistance = 64f;
		// 同一扇门两次状态切换之间的最小间隔帧数（防止渲染闪烁）
		private const int ToggleCooldown = 8;

		// key = (tileX, topTileY)，value = 上一次切换的 GameUpdateCount
		private static readonly Dictionary<(int, int), ulong> _lastToggle = new();

		public override void PostUpdateWorld() {
			if (Main.netMode == NetmodeID.MultiplayerClient)
				return;

			int closedType = ModContent.TileType<AutoDoorClosedTile>();
			int openType   = ModContent.TileType<AutoDoorOpenTile>();

			for (int p = 0; p < Main.maxPlayers; p++) {
				Player player = Main.player[p];
				if (!player.active || player.dead)
					continue;

				int px    = (int)(player.Center.X / 16);
				int py    = (int)(player.Center.Y / 16);
				int range = 5;

				for (int dy = -range; dy <= range; dy++) {
					for (int dx = -range; dx <= range; dx++) {
						int tx = px + dx;
						int ty = py + dy;
						if (tx < 0 || ty < 0 || tx >= Main.maxTilesX || ty >= Main.maxTilesY)
							continue;

						Tile tile = Main.tile[tx, ty];
						if (!tile.HasTile || tile.TileFrameY != 0)
							continue;

						if (tile.TileType == closedType) {
							float hDist = System.Math.Abs(player.Center.X - (tx * 16 + 8));
							if (hDist <= OpenDistance)
								TrySetDoorState(tx, ty, openType, opening: true);
						}
						else if (tile.TileType == openType) {
							if (!AnyPlayerNear(tx, ty, CloseDistance))
								TrySetDoorState(tx, ty, closedType, opening: false);
						}
					}
				}
			}
		}

		private static void TrySetDoorState(int x, int topY, int newType, bool opening) {
			var key = (x, topY);
			if (_lastToggle.TryGetValue(key, out ulong last) &&
				Main.GameUpdateCount - last < ToggleCooldown)
				return; // 冷却中，跳过本次切换

			_lastToggle[key] = Main.GameUpdateCount;
			SetDoorState(x, topY, newType, opening);
		}

		private static bool AnyPlayerNear(int tx, int ty, float distPx) {
			float doorCenterX = tx * 16 + 8f;
			for (int p = 0; p < Main.maxPlayers; p++) {
				Player pl = Main.player[p];
				if (!pl.active || pl.dead)
					continue;
				if (System.Math.Abs(pl.Center.X - doorCenterX) <= distPx)
					return true;
			}
			return false;
		}

		private static void SetDoorState(int x, int topY, int newType, bool opening) {
			for (int dy = 0; dy < 3; dy++) {
				int ty = topY + dy;
				if (ty >= Main.maxTilesY)
					break;
				Main.tile[x, ty].TileType = (ushort)newType;
			}
			// 不调用 WorldGen.SquareTileFrame：会触发周边帧重算，产生一帧撕裂。

			if (Main.netMode != NetmodeID.Server) {
				Vector2 soundPos = new Vector2(x * 16 + 8, topY * 16 + 24);
				SoundEngine.PlaySound(opening ? SoundID.DoorOpen : SoundID.DoorClosed, soundPos);
			}

			if (Main.netMode == NetmodeID.Server)
				NetMessage.SendTileSquare(-1, x, topY, 1, 3);
		}
	}
}
