using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.Localization;
using Terraria.ModLoader;

namespace ArknightsMod.Common.Items
{
	public enum SkillChargeType
	{
		None,
		Auto,
		Attack,
		Hurt
	}

	public struct SkillLevelData
	{
		public int InitSP;
		public int MaxSP;
		public float ActiveTime;
		public int MaxStack;
	}
	public struct SkillKey(string item, int index, string name)
	{
		public int Index = index;
		public string Name = name;
		public string Item = item;
	}

	public class SkillData
	{
		private const string _Key = "Mods.ArknightsMod.Skills.";
		private const string _Label = ".Label";
		private const string _Desc = ".Desc";
		private const string _IconPath = "ArknightsMod/Common/UI/SkillIcons/";
		private const string _SummonPath = "ArknightsMod/Common/UI/SummonIcon/";
		public string Name => Key.Name;
		public SkillKey Key { get; private set; }
		public int Level = 1;

		/// <summary>
		/// 这是一个临时的字段，后续等级系统做完了就可以删了
		/// <br/>This is a temporary field and can be deleted after the level system is completed
		/// </summary>
		public int? ForceReplaceLevel;

		/// <summary>
		/// 充能类型
		/// </summary>
		public SkillChargeType ChargeType;

		/// <summary>
		/// 自动触发技能
		/// </summary>
		public bool AutoTrigger;

		/// <summary>
		/// 自动扣除技能活跃时间
		/// </summary>
		public bool AutoUpdateActive;

		/// <summary>
		/// 是否附加召唤模式
		/// <br/>Whether have extra summon skill
		/// </summary>
		public bool SummonSkill;

		/// <summary>
		/// 等级数据
		/// </summary>
		public SkillLevelData[] LevelData;

		public Asset<Texture2D> Icon { get; private set; }
		public Asset<Texture2D> SummonIcon { get; private set; }

		public LocalizedText Label { get; private set; }
		public LocalizedText Desc { get; private set; }
		public void BindKey(string item, int index, string name) {
			Key = new(item, index, name);
			string path = item + "." + name;
			Label = Language.GetOrRegister(_Key + path + _Label);
			Desc = Language.GetOrRegister(_Key + path + _Desc);
			path = item + "_" + name;
			Icon = ModContent.Request<Texture2D>(_IconPath + path, AssetRequestMode.ImmediateLoad);
			SummonIcon = ModContent.HasAsset(path = _SummonPath + path) ?
				ModContent.Request<Texture2D>(path, AssetRequestMode.ImmediateLoad) : null;
		}
		public SkillData() {
			LevelData = new SkillLevelData[10];
		}

		/// <summary>
		/// 此处使用直接等级而不是索引
		/// <br/>Here use the level instead of index
		/// </summary>
		/// <param name="level"></param>
		/// <returns></returns>
		public SkillLevelData this[int level] {
			get => LevelData[level - 1];
			set => LevelData[level - 1] = value;
		}
		public SkillLevelData CurrentLevelData => this[ForceReplaceLevel ?? Level];
	}
}
