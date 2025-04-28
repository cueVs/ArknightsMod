using ArknightsMod.Content.Items;

namespace ArknightsMod.Common.UI.BattleRecord.Calculators
{
	public sealed class ExperienceCalculator
	{
		public UpgradeItemBase UpgradeItem;
		public int ExperienceBudget;

		public int UpgradeLevelNeedExperience =>
			UpgradeItem == null ? 0 :
				UpgradeItem.GetUpgradeExperience(UpgradeItem.Level, UpgradeItem.EliteStage) - UpgradeItem.Experience;

		public int UpgradeMaxLevelNeedExperience {
			get {
				if (UpgradeItem == null)
					return 0;
				int exp = -UpgradeItem.Experience;
				int level = UpgradeItem.Level;
				while (level < UpgradeItem.LevelMax) {
					exp += UpgradeItem.GetUpgradeExperience(level, UpgradeItem.EliteStage);
					level++;
				}
				return exp;
			}
		}

		public int UpgradeLevelPreview {
			get {
				if (UpgradeItem == null)
					return 0;
				int level = 0;
				int exp = ExperienceBudget + UpgradeItem.Experience;
				while (exp > 0) {
					int lv = UpgradeItem.Level + level;
					if (lv >= UpgradeItem.LevelMax)
						break;
					exp -= UpgradeItem.GetUpgradeExperience(lv, UpgradeItem.EliteStage);
					if (exp > 0)
						level++;
				}
				return level;
			}
		}

		public int UpgradeLevelPreviewExperience =>
			UpgradeItem == null ? 0 :
			UpgradeItem.GetUpgradeExperience(UpgradeItem.Level + UpgradeLevelPreview, UpgradeItem.EliteStage);

		public int ExperienceBudgetPreview {
			get {
				if (UpgradeItem == null)
					return 0;
				int level = 0;
				int exp = ExperienceBudget + UpgradeItem.Experience;
				while (exp > 0) {
					int lv = UpgradeItem.Level + level;
					int e = UpgradeItem.GetUpgradeExperience(lv, UpgradeItem.EliteStage);
					if (lv >= UpgradeItem.LevelMax)
						return e;
					if (exp >= e) {
						exp -= e;
						level++;
					}
					else
						break;
				}
				return exp;
			}
		}

		public int GetUpgradeToLevelExperience(int level) {
			if (UpgradeItem == null || level <= UpgradeItem.Level)
				return 0;
			int levelCache = UpgradeItem.Level;
			int exp = 0;
			while (levelCache < level) {
				exp += UpgradeItem.GetUpgradeExperience(levelCache, UpgradeItem.EliteStage);
				levelCache++;
				if (levelCache >= UpgradeItem.LevelMax)
					break;
			}
			exp -= UpgradeItem.Experience;
			return exp;
		}

		public int GetUpgradeLevelForExperience(int exp) {
			if (UpgradeItem == null)
				return 0;
			int level = 0;
			exp += UpgradeItem.Experience;
			while (exp > 0) {
				int lv = UpgradeItem.Level + level;
				exp -= UpgradeItem.GetUpgradeExperience(lv, UpgradeItem.EliteStage);
				if (exp > 0)
					level++;
				if (lv >= UpgradeItem.LevelMax - 1)
					break;
			}
			return level;
		}
	}
}