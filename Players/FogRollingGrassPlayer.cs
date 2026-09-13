using Terraria;
using Terraria.ModLoader;

namespace ArknightsMod.Players
{
	public class FogRollingGrassPlayer : ModPlayer
	{
		public const int BaseValue = 2;
		public const int MaxValue = 16;
		private const float MetersPerStep = 200f;

		public bool HasFogRollingGrass;
		public int Value = BaseValue;

		private double checkpointPixels = -1;

		public override void ResetEffects() {
			HasFogRollingGrass = false;
		}

		public override void PostUpdate() {
			double total = Player.GetModPlayer<OdometerPlayer>().TotalPixelsMoved;

			if (!HasFogRollingGrass) {
				checkpointPixels = -1;
				return;
			}
			if (checkpointPixels < 0) {
				checkpointPixels = total;
				return;
			}

			float stepPixels = MetersPerStep * OdometerPlayer.PixelsPerMeter;
			while (total - checkpointPixels >= stepPixels && Value < MaxValue) {
				checkpointPixels += stepPixels;
				Value++;
			}
		}
	}
}
