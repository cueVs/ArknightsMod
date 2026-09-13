using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace ArknightsMod.Players
{
	// 共享的“已走过的距离”里程表，供雾滚草/浪花/恋家果这类按移动距离结算估价的自然物复用，
	// 避免每个物品各自重复写一遍位置差分累加逻辑。约定 1m = 1 格 = 16px。
	public class OdometerPlayer : ModPlayer
	{
		public const float PixelsPerMeter = 16f;

		public double TotalPixelsMoved { get; private set; }

		private Vector2 lastPosition;
		private bool initialized;

		public override void PostUpdate() {
			if (!initialized) {
				lastPosition = Player.position;
				initialized = true;
				return;
			}

			TotalPixelsMoved += Vector2.Distance(Player.position, lastPosition);
			lastPosition = Player.position;
		}
	}
}
