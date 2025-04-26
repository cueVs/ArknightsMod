using Hjson;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.ModLoader;

namespace ArknightsMod.Common.Items
{
	public static class SkillDataLoader
	{
		private const string _path = "Common/Items/SkillDatas.hjson";
		private const string ChargeType = nameof(SkillData.ChargeType);
		private const string AutoTrigger = nameof(SkillData.AutoTrigger);
		private const string AutoUpdateActive = nameof(SkillData.AutoUpdateActive);
		private const string SummonSkill = nameof(SkillData.SummonSkill);
		private const string LevelData = nameof(SkillData.LevelData);
		private const string InitSP = nameof(SkillLevelData.InitSP);
		private const string MaxSp = nameof(SkillLevelData.MaxSP);
		private const string ActiveTime = nameof(SkillLevelData.ActiveTime);
		private const string MaxStack = nameof(SkillLevelData.MaxStack);
		private readonly static Dictionary<string, SkillData[]> weaponSkills = [];

		public static void LoadData(Mod mod) {
			weaponSkills.Clear();
			var logger = mod.Logger;
			using var stream = mod.GetFileStream(_path);
			using var reader = new StreamReader(stream);
			string hjsonContent = reader.ReadToEnd();
			string hjsonString;
			try {
				hjsonString = HjsonValue.Parse(hjsonContent).ToString();
			}
			catch (Exception e) {
				throw new Exception($"The skill data file is malformed and failed to load: ", e);
			}

			var hjsonObject = JObject.Parse(hjsonString);

			foreach (var weaponEntry in hjsonObject.Properties()) {
				string name = weaponEntry.Name;
				var datas = weaponSkills[name] = new SkillData[3];
				int index = 0;
				foreach (var skillEntry in ((JObject)weaponEntry.Value).Properties()) {
					string skillName = skillEntry.Name;
					if (index >= 3) {
						logger.Warn($"{name} has over index skill {skillName}, index is {index}");
						continue; // 确保不超过3个技能
					}

					var skillJson = (JObject)skillEntry.Value;
					if (!skillJson.TryGetValue(ChargeType, out var charge)
						|| !skillJson.TryGetValue(AutoTrigger, out var autoTrigger)
						|| !skillJson.TryGetValue(AutoUpdateActive, out var autoUpdateActive)
						|| !skillJson.TryGetValue(SummonSkill, out var summonSkill)
						|| !skillJson.TryGetValue(LevelData, out var levelData)) {
						logger.Error($"{name}-{skillName} missing data!");
						continue;
					}

					var skillData = datas[index++] = new SkillData {
						Name = name + "." + skillName,
						ChargeType = (SkillChargeType)charge.Value<int>(),
						AutoTrigger = autoTrigger.Value<bool>(),
						AutoUpdateActive = autoUpdateActive.Value<bool>(),
						SummonSkill = summonSkill.Value<bool>(),
					};

					// 处理等级数据
					if (levelData is JObject levelDatas) {
						foreach (var levelEntry in levelDatas.Properties()) {
							if (!int.TryParse(levelEntry.Name, out int level) || level < 1 || level > 10) {
								logger.Error($"{name}-{skillName} error level key");
								continue;
							}

							JObject levelJson = (JObject)levelEntry.Value;
							if (!levelJson.TryGetValue(InitSP, out var initSP)
								|| !levelJson.TryGetValue(MaxSp, out var maxSP)
								|| !levelJson.TryGetValue(ActiveTime, out var activeTime)
								|| !levelJson.TryGetValue(MaxStack, out var maxStack)) {
								logger.Error($"{name}-{skillName}-{level} missing level data");
								continue;
							}

							skillData.ForceReplaceLevel ??= level;
							skillData[level] = new SkillLevelData {
								InitSP = initSP.Value<int>(),
								MaxSP = maxSP.Value<int>(),
								ActiveTime = activeTime.Value<float>(),
								MaxStack = Math.Max(1, maxStack.Value<int>())
							};
						}
					}
				}
			}
		}

		public static void Unload() {
			weaponSkills.Clear();
		}
		public static SkillData GetSkill(string name, int index) {
			if (!weaponSkills.TryGetValue(name, out var datas)) {
				Main.NewText($"Skill don't contains {name}");
				return null;
			}
			if (index is < 0 or > 2) {
				Main.NewText("Try get skill index out of range");
				return null;
			}
			return datas[index];
		}
	}
}