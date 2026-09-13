using ArknightsMod.Players;
using Terraria;

namespace ArknightsMod.Content.Items.Armor
{
	internal static class OperatorPassiveSkillHelper
	{
		public static bool IsPassiveSkillActive(Player player) {
			WeaponPlayer wp = player.GetModPlayer<WeaponPlayer>();
			if (!wp.SkillActive || wp.HowManySkills <= 0)
				return false;

			return wp.Skill < wp.AutoTrigger.Count && wp.AutoTrigger[wp.Skill];
		}

		public static void TryRetriggerPassiveSkill(Player player) {
			WeaponPlayer wp = player.GetModPlayer<WeaponPlayer>();
			if (wp.HowManySkills <= 0)
				return;

			for (int i = 0; i < wp.AutoTrigger.Count; i++) {
				if (!wp.AutoTrigger[i])
					continue;

				wp.Skill = i;
				wp.SkillActive = true;
				if (i < wp.SkillActiveTime.Count)
					wp.SkillTimer = (int)(wp.SkillActiveTime[i] * 60f);

				return;
			}
		}
	}
}
