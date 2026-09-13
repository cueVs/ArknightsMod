using Terraria;
using Terraria.ModLoader;

namespace ArknightsMod.Players
{
	public class WaveSprayPlayer : ModPlayer
	{
		public const int BaseValue = 3;
		public const int MinValue = 0;
		public const int MaxValue = 66;
		private const float MetersPerStep = 500f;

		public bool HasWaveSpray;
		public int Value = BaseValue;

		private double checkpointPixels = -1;

		public override void ResetEffects() {
			HasWaveSpray = false;
		}

		public override void PostUpdate() {
			double total = Player.GetModPlayer<OdometerPlayer>().TotalPixelsMoved;

			if (!HasWaveSpray) {
				checkpointPixels = -1;
				return;
			}
			if (checkpointPixels < 0) {
				checkpointPixels = total;
				return;
			}

			if (!Main.raining) {
				checkpointPixels = total; // 非雨雪天气不计入位移，估价保持不变
				return;
			}

			float stepPixels = MetersPerStep * OdometerPlayer.PixelsPerMeter;
			while (total - checkpointPixels >= stepPixels) {
				checkpointPixels += stepPixels;
				int delta = Main.rand.NextBool() ? 5 : -4;
				Value = System.Math.Clamp(Value + delta, MinValue, MaxValue);
			}
		}
	}
}
