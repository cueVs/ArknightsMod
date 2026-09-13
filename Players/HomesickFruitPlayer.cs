using Terraria;
using Terraria.ModLoader;

namespace ArknightsMod.Players
{
	// 这里只管背包里“每移动200m估价-2”的衰减；栽种后“每过一天恢复2点、封顶32”的逻辑
	// 在 HomesickFruitTileEntity 里单独处理（那部分跟玩家背包无关，是瓦片自己的状态）。
	public class HomesickFruitPlayer : ModPlayer
	{
		public const int BaseValue = 32;
		private const float MetersPerStep = 200f;

		public bool HasHomesickFruit;
		public int Value = BaseValue;

		private double checkpointPixels = -1;

		public override void ResetEffects() {
			HasHomesickFruit = false;
		}

		public override void PostUpdate() {
			double total = Player.GetModPlayer<OdometerPlayer>().TotalPixelsMoved;

			if (!HasHomesickFruit) {
				checkpointPixels = -1;
				return;
			}
			if (checkpointPixels < 0) {
				checkpointPixels = total;
				return;
			}

			float stepPixels = MetersPerStep * OdometerPlayer.PixelsPerMeter;
			while (total - checkpointPixels >= stepPixels) {
				checkpointPixels += stepPixels;
				Value = System.Math.Max(0, Value - 2);
			}
		}
	}
}
