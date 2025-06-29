using ArknightsMod.Common.Items;
using ArknightsMod.Common.UI.BattleRecord;
using ArknightsMod.Common.UI.BattleRecord.Calculators;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.GameContent;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace ArknightsMod.Content.Items.Weapons
{
	public abstract class UpgradeWeaponBase : UpgradeItemBase
	{
		protected int[] skillLevel = new int[3];
		protected int[][] weaponData = new int[3][];
		protected readonly static Dictionary<string, SkillData[]> skillDatas = [];
		public override void DrawUpgradePreview(SpriteBatch spriteBatch, Rectangle rectangle, BattleRecordCalculator battleRecordCalculator, ExperienceCalculator experienceCalculator) {
			var pos = rectangle.Location.ToVector2();
			pos.Y += 20f;
			var previewLevel = Level + experienceCalculator.UpgradeLevelPreview;
			int nowDamage = GetDamage(Level);
			int previewDamage = GetDamage(previewLevel);

			var font = FontAssets.MouseText.Value;
			string text = Language.GetTextValue("Mods.ArknightsMod.UpgradeWeapon.LevelPreview.Damage");
			Vector2 textSize = font.MeasureString(text);
			pos.X = rectangle.X + (rectangle.Width - textSize.X) / 2f;
			spriteBatch.DrawString(font, text, pos, Color.White);
			pos.Y += textSize.Y;

			text = Level == previewLevel ? $"{nowDamage}" : $"{nowDamage}->{previewDamage}";
			textSize = font.MeasureString(text);
			pos.X = rectangle.X + (rectangle.Width - textSize.X) / 2f;
			spriteBatch.DrawString(font, text, pos, Color.White);
			pos.Y += textSize.Y;
		}

		protected virtual int GetDamage(int level) => weaponData[m_EliteStage][level - 1];
		public override void SaveData(TagCompound tag) {
			base.SaveData(tag);
			tag.Add("SkillLevel", skillLevel);
		}

		public override void LoadData(TagCompound tag) {
			base.LoadData(tag);
			if (!tag.TryGet("SkillLevel", out skillLevel)) {
				skillLevel = new int[3];
			}
		}

		public static void LoadSkillData(Mod mod) {
			skillDatas.Clear();
			var logger = mod.Logger;
			using (var sr = new StreamReader(mod.GetFileStream("Assets/LevelDatas/SkillDatas.csv"))) {
				sr.ReadLine();
				while (!sr.EndOfStream) {
					string[] info = sr.ReadLine().Split(',');
					string item = info[0];
					if (!skillDatas.TryGetValue(item, out var datas))
						datas = skillDatas[item] = new SkillData[3];
					try {
						int index = int.Parse(info[1]);
						SkillData data = new() {
							ChargeType = (SkillChargeType)int.Parse(info[3]),
							AutoTrigger = int.Parse(info[4]) == 1,
							AutoUpdateActive = int.Parse(info[5]) == 1,
							SummonSkill = int.Parse(info[6]) == 1,
						};
						string name = info[2];
						data.BindKey(item, index, name);
						datas[index] = data;
					}
					catch (Exception e) {
						logger.Error(e);
						continue;
					}
				}
			}
			foreach (var (item, datas) in skillDatas) {
				foreach (var data in datas) {
					if (data == null)
						continue;
					using var sr = new StreamReader(mod.GetFileStream($"Assets/LevelDatas/Skills/{item}_{data.Name}.csv"));
					sr.ReadLine();
					try {
						for (int i = 1; i <= 10; i++) {
							string content = sr.ReadLine();
							if (content[0] == ',')
								continue;
							string[] info = content.Split(",");
							data[i] = new() {
								InitSP = int.Parse(info[0]),
								MaxSP = int.Parse(info[1]),
								ActiveTime = float.Parse(info[2]),
								MaxStack = int.Parse(info[3])
							};
							if (data[i].MaxSP > 0) {
								data.ForceReplaceLevel = i;
							}
						}
					}
					catch (Exception e) {
						logger.Error(e);
						continue;
					}
				}
			}
		}
		public SkillData GetSkillData(int index) {
			if (!skillDatas.TryGetValue(Name, out var datas)) {
				Main.NewText(Name + " hasn't skill datas");
				return null;
			}
			return datas[index];
		}
	}
}