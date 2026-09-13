using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace ArknightsMod.Players
{
	public class FrostCrystalTreePlayer : ModPlayer
	{
		public const int BaseValue = 6;
		private const int SmallKillGain = 1;
		private const int BossKillGain = 10;
		private const int HurtLoss = 5;

		public bool HasFrostCrystalTree;
		public int Value = BaseValue;
		public bool Broken;

		public override void ResetEffects() {
			HasFrostCrystalTree = false;
		}

		public void RegisterKill(bool isBoss) {
			if (Broken) return;
			Value += isBoss ? BossKillGain : SmallKillGain;
		}

		private void RegisterHurt() {
			if (Broken) return;
			Value -= HurtLoss;
			if (Value <= 0) Break();
		}

		private void Break() {
			if (Broken) return;
			Value = System.Math.Max(0, Value / 2);
			Broken = true;
		}

		public override void PostHurt(Player.HurtInfo info) {
			RegisterHurt();
		}

		public override void Kill(double damage, int hitDirection, bool pvp, PlayerDeathReason damageSource) {
			Break();
		}
	}
}
