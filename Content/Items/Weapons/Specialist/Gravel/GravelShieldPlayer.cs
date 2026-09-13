using Terraria;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Weapons.Specialist.Gravel
{
	public class GravelShieldPlayer : ModPlayer
	{
		public int ShieldHp;
		public int ShieldMax;

		public float ShieldRatio => ShieldMax > 0 ? (float)ShieldHp / ShieldMax : 0f;

		public override void ResetEffects() {
			// ShieldMax 每帧由 HoldItem 写入，清零保证失去武器后立刻失效
			ShieldMax = 0;
		}

		public override void PostHurt(Player.HurtInfo info) {
			if (ShieldHp <= 0 || info.Damage <= 0) return;
			int absorbed = System.Math.Min(ShieldHp, info.Damage);
			ShieldHp -= absorbed;
			// 将护盾吸收的伤害退还给玩家血量
			Player.statLife = System.Math.Min(Player.statLife + absorbed, Player.statLifeMax2);
		}
	}
}
