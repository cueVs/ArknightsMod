using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
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
		public readonly int Level;
		public int InitSP;
		public int MaxSP;
		public float ActiveTime;

		private int maxStock;
		public int MaxStock {
			readonly get => Math.Max(1, maxStock);
			set => maxStock = value;
		}
	}

	/// <summary>
	/// 在初始化时
	/// </summary>
	public class SkillData
	{
		private const string _Key = "Mods.ArknightsMod.Skills.";
		private const string _Label = "Label";
		private const string _Desc = "Desc";
		private const string _IconPath = "ArknightsMod/Common/UI/SkillIcons/";
		private const string _SummonPath = "ArknightsMod/Common/UI/SummonIcon/";
		private string name;
		public int Level = 0;

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
		public string Name {
			get => name;
			init {
				name = value;
				Label = Language.GetOrRegister(_Key + name + _Label);
				Desc = Language.GetOrRegister(_Key + name + _Desc);
				Icon = ModContent.Request<Texture2D>(_IconPath + name, AssetRequestMode.ImmediateLoad);
				SummonIcon = ModContent.HasAsset(_SummonPath + name) ?
					ModContent.Request<Texture2D>(_SummonPath + name, AssetRequestMode.ImmediateLoad) : null;
			}
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
