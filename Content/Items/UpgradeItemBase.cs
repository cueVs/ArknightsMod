using ArknightsMod.Common.UI.BattleRecord;
using ArknightsMod.Common.UI.BattleRecord.Calculators;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace ArknightsMod.Content.Items
{
	public abstract class UpgradeItemBase : ModItem
	{
		/// <summary>
		/// 物品等级
		/// </summary>
		public virtual int Level => m_Level;

		/// <summary>
		/// 等级上限
		/// </summary>
		public virtual int LevelMax => GetLevelMax(EliteStage);

		/// <summary>
		/// 物品目前经验
		/// </summary>
		public virtual int Experience { get => m_Experience; set => m_Experience = value; }

		/// <summary>
		/// 目前升级所需经验
		/// </summary>
		public virtual int UpgradeExperience => GetUpgradeExperience(Level, EliteStage);

		/// <summary>
		/// 目前精英化阶段
		/// </summary>
		public virtual int EliteStage => m_EliteStage;

		/// <summary>
		/// 允许的最大精英化阶段
		/// </summary>
		public virtual int EliteStageMax => 1;

		protected int m_Level = 1;
		protected int m_Experience;
		protected int m_EliteStage;

		private static readonly Dictionary<int, Dictionary<int, int>> UpgradeData = new Dictionary<int, Dictionary<int, int>>();

		public override void Load() {
			base.Load();
			UpgradeData.Clear();
			using (var sr = new StreamReader(Mod.GetFileStream("Assets/UpgradeData/EliteZero.txt"))) {
				UpgradeData.Add(0, new Dictionary<int, int>());
				string data = sr.ReadLine();
				while (!string.IsNullOrEmpty(data)) {
					var ds = data.Split('	', System.StringSplitOptions.RemoveEmptyEntries);
					if (int.TryParse(ds[0], out int level) && int.TryParse(ds[1], out int exp))
						UpgradeData[0].TryAdd(level, exp);
					data = sr.ReadLine();
				}
			}
			using (var sr = new StreamReader(Mod.GetFileStream("Assets/UpgradeData/EliteOne.txt"))) {
				UpgradeData.Add(1, new Dictionary<int, int>());
				string data = sr.ReadLine();
				while (!string.IsNullOrEmpty(data)) {
					var ds = data.Split('	', System.StringSplitOptions.RemoveEmptyEntries);
					if (int.TryParse(ds[0], out int level) && int.TryParse(ds[1], out int exp))
						UpgradeData[1].TryAdd(level, exp);
					data = sr.ReadLine();
				}
			}
			using (var sr = new StreamReader(Mod.GetFileStream("Assets/UpgradeData/EliteTwo.txt"))) {
				UpgradeData.Add(2, new Dictionary<int, int>());
				string data = sr.ReadLine();
				while (!string.IsNullOrEmpty(data)) {
					var ds = data.Split('	', System.StringSplitOptions.RemoveEmptyEntries);
					if (int.TryParse(ds[0], out int level) && int.TryParse(ds[1], out int exp))
						UpgradeData[2].TryAdd(level, exp);
					data = sr.ReadLine();
				}
			}
		}

		/// <summary>
		/// 检查是否能够升级
		/// </summary>
		public virtual void CheckUpgrade() {
			while (CanUpgrade()) {
				Upgrade();
			}
		}

		/// <summary>
		/// 是否可以升级
		/// </summary>
		/// <returns>可以升级时返回true，否则返回false</returns>
		public virtual bool CanUpgrade() {
			return Level < LevelMax && Experience >= UpgradeExperience;
		}

		/// <summary>
		/// 升级时
		/// </summary>
		public virtual void Upgrade() {
			m_Experience -= UpgradeExperience;
			m_Level++;
			if (m_Level == LevelMax) {
				m_Experience = 0;
			}
		}

		/// <summary>
		/// 检查是否能够升级
		/// </summary>
		public virtual void CheckElite() {
			if (CanElite()) {
				Elite();
			}
		}

		/// <summary>
		/// 是否可以升级
		/// </summary>
		/// <returns>可以升级时返回true，否则返回false</returns>
		public virtual bool CanElite() {
			return Level >= LevelMax && EliteStage < EliteStageMax;
		}

		/// <summary>
		/// 精英化时
		/// </summary>
		public virtual void Elite() {
			m_EliteStage++;
			m_Level = 0;
		}

		/// <summary>
		/// 获取物品在精英化阶段下某个等级所需的升级经验
		/// </summary>
		/// <param name="level">等级</param>
		/// <param name="eliteStage">精英化阶段</param>
		/// <returns>经验</returns>
		public virtual int GetUpgradeExperience(int level, int eliteStage) => level >= LevelMax ? 0 : UpgradeData[eliteStage][level];

		/// <summary>
		/// 获取物品精英化阶段的最大等级
		/// </summary>
		/// <param name="eliteStage">精英化阶段</param>
		/// <returns>最大等级</returns>
		public virtual int GetLevelMax(int eliteStage) => UpgradeData[eliteStage].Count + 1;

		/// <summary>
		/// 获取升满级所需的经验
		/// </summary>
		/// <returns>升满级所需经验</returns>
		public virtual int GetFullLevelExperience() {
			int output = UpgradeExperience - Experience;
			for (int i = Level + 1; i < LevelMax; i++) {
				output += GetUpgradeExperience(i, EliteStage);
			}
			return output;
		}

		public virtual void DrawUpgradePreview(SpriteBatch spriteBatch, Rectangle rectangle, BattleRecordCalculator battleRecordCalculator, ExperienceCalculator experienceCalculator) {
		}

		public override ModItem Clone(Item newEntity) {
			var output = base.Clone(newEntity);
			if (output is UpgradeItemBase upgradeItem) {
				upgradeItem.m_Experience = Experience;
				upgradeItem.m_Level = Level;
				upgradeItem.m_EliteStage = EliteStage;
			}
			return output;
		}

		public override void SaveData(TagCompound tag) {
			base.SaveData(tag);
			tag.Add("Experience", Experience);
			tag.Add("Level", Level);
			tag.Add("EliteStage", EliteStage);
		}

		public override void LoadData(TagCompound tag) {
			base.LoadData(tag);
			if (tag.TryGet("Experience", out int experience)) {
				m_Experience = experience;
			}
			if (tag.TryGet("Level", out int level)) {
				m_Level = level;
			}
			if (tag.TryGet("EliteStage", out int eliteStage)) {
				m_EliteStage = eliteStage;
			}
		}
	}
}