using ArknightsMod.Content.Items.Armor.Vanguard.Texas;
using ArknightsMod.Players;
using Terraria;

namespace ArknightsMod.Content.Items.Armor
{
	internal static class OperatorSetSkillEffectHelper
	{
		public static float GetSkillDamageMultiplier(Player player) {
			WeaponPlayer wp = player.GetModPlayer<WeaponPlayer>();
			if (!wp.SkillActive)
				return 1f;

			if (player.GetModPlayer<TexasSetPlayer>().TexasSetActive)
				return 1.3f;

			return 1f;
		}

		public static int ScaleControlDuration(Player player, int baseTicks) {
			WeaponPlayer wp = player.GetModPlayer<WeaponPlayer>();
			if (!wp.SkillActive || !player.GetModPlayer<TexasSetPlayer>().TexasSetActive)
				return baseTicks;

			return (int)(baseTicks * 1.3f);
		}
	}
}
