using ArknightsMod.Common.UI.BattleRecord;
using ArknightsMod.Common.UI.BattleRecord.Calculators;
using ArknightsMod.Content.Items.Weapons.Defender.Beagle;
using ArknightsMod.Systems.Gameplay.Skill;
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
	/// <summary>
	/// 新武器接入"技能开启键"（<see cref="ArknightsMod.Content.ArknightsKeybinds.SkillActivate"/>，
	/// 默认 C，玩家可在 设置-控制-模组按键 里自由改绑）的方法：
	///
	/// 1. 只要武器类继承自 <see cref="UpgradeWeaponBase"/>（本类），就不需要额外注册任何东西——
	///    <see cref="ArknightsMod.Players.WeaponPlayer.ProcessTriggers"/> 已经按基类做了统一判断，
	///    按住技能键时会自动帮你把 <c>Player.controlUseItem</c> 置为 true，把按键"翻译"成一次
	///    左键点击，交给原版的物品使用管线。
	/// 2. 武器代码里原来判断"右键是否按下"（<c>player.altFunctionUse == 2</c>）的地方，一律换成
	///    <c>ArknightsMod.Content.ArknightsKeybinds.SkillActivatePressed(player)</c>；判断"没按住"
	///    则换成 <c>!ArknightsKeybinds.SkillActivatePressed(player)</c>。不要直接读
	///    <c>ArknightsKeybinds.SkillActivate.Current</c>——那个字段只反映本地客户端状态，不经过
	///    联机同步，在 CanUseItem/Shoot 这类对任意 Player 实例都会调用的钩子里直接读会在联机时
	///    把本地玩家的按键状态错误套用到其他玩家身上；SkillActivatePressed 已经内置了
	///    <c>whoAmI == Main.myPlayer</c> 的本地判定，直接用它就行。
	/// 3. 如果右键本身还保留着其它专属功能（部署/召唤之类，和技能开启是两件事——参考
	///    GoldenglowWand/PozemkaCrossbow/SceneCamera/DeepcolorSketch 的写法），就把
	///    <c>AltFunctionUse(Player player)</c> 继续返回 true，在 CanUseItem 里把"技能键判断"分支
	///    放在"右键判断"分支前面并各自 return，两者不会互相干扰。如果右键没有其它用途，把
	///    <c>AltFunctionUse</c> 改成返回 false 即可（不再需要 altFunctionUse 字段参与判断）。
	/// 4. 只有当武器类不继承 UpgradeWeaponBase（历史遗留，例如 LupineScarlet/OblivionisSword 直接
	///    继承 ModItem）时，才需要额外去 WeaponPlayer.ProcessTriggers 里把这个类加进判断列表，
	///    否则按键不会被翻译成点击，武器里写的 SkillActivatePressed 判断永远不会被触发到。
	/// 5. 如果技能触发逻辑压根不走物品使用管线（比如像 DeepcolorSketchPlayer 那样在
	///    ModPlayer.PostUpdate 里直接读输入），就不需要 ProcessTriggers 这层桥接，直接在对应的
	///    ModPlayer 钩子里判断 ArknightsKeybinds.SkillActivatePressed(Player) 即可。
	/// </summary>
	public abstract class UpgradeWeaponBase : UpgradeItemBase
	{
		protected int[] skillLevel = new int[3];
		protected int[][] weaponData = new int[3][];
		protected readonly static Dictionary<string, SkillData[]> skillDatas = [];
		// 新获得的武器第一次被拿起时也必须按技能表发放 InitSP；此前默认 false 会让
		// CSV 技能在首次装备时一律从 0 开始，只有死亡或脱战重置后才得到正确初始技力。
		public bool[] chargeReady = [true, true, true];
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
					string line = sr.ReadLine();
					if (string.IsNullOrWhiteSpace(line))
						continue;
					string[] info = line.Split(',');
					if (info.Length < 7) {
						logger.Warn($"[LoadSkillData] Invalid SkillDatas.csv line (expected 7 cols): '{line}'");
						continue;
					}
					string item = info[0];
					if (string.IsNullOrWhiteSpace(item))
						continue;
					if (!skillDatas.TryGetValue(item, out var datas))
						datas = skillDatas[item] = new SkillData[3];
					try {
						if (!int.TryParse(info[1], out int index))
							throw new FormatException($"Invalid skill index: '{info[1]}'");
						if (index < 0 || index >= datas.Length)
							throw new IndexOutOfRangeException($"Skill index {index} out of range for item '{item}'");
						SkillData data = new() {
							ChargeType       = (SkillChargeType)int.Parse(info[3]),
							AutoTrigger      = int.Parse(info[4]) == 1,
							AutoUpdateActive = int.Parse(info[5]) == 1,
							SummonSkill      = int.Parse(info[6]) == 1,
							SuppressReadyPulse = info.Length >= 8 && info[7].Trim() == "1",
							IsPermanent      = info.Length >= 9 && info[8].Trim() == "1",
							UsesCustomCharge = info.Length >= 10 && info[9].Trim() == "1",
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
							if (sr.EndOfStream)
								break;
							string content = sr.ReadLine();
							if (string.IsNullOrEmpty(content))
								continue;
							if (content[0] == ',')
								continue;
							string[] info = content.Split(",");
							if (info.Length < 4)
								continue;
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
			if (skillDatas.TryGetValue("SceneCamera", out var sceneCameraSkills) && sceneCameraSkills[0] != null)
				sceneCameraSkills[0].IsPermanent = true;
		}
		public SkillData GetSkillData(int index) {
			return GetSkillDataForItem(Name, index);
		}

		public static SkillData GetSkillDataForItem(string itemName, int index) {
			if (!skillDatas.TryGetValue(itemName, out var datas))
				return null;
			return datas[index];
		}

		/// <summary>覆盖默认「右键开启技能」说明；为 null 时不追加。</summary>
		public virtual string GetSkillActivateKeyHint() => null;
	}
}
