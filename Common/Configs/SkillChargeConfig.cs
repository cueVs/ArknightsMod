using System.ComponentModel;
using Terraria.ModLoader.Config;

// 技力（SP）节奏相关的全局开关。两个选项都只在运行时改判定阈值，不动任何数值来源，
// 所以 CSV/武器数据一律按"开关关闭时"的 1 倍速数值填写，不需要为了适配开关手动折算。

namespace ArknightsMod.Common.Configs
{
	public class SkillChargeConfig : ModConfig
	{
		public override ConfigScope Mode => ConfigScope.ClientSide;

		// 注意：[Label]/[Tooltip] 特性在当前 tModLoader 已废弃（编译会报 CS0618），运行时完全不生效。
		// 配置项在游戏里显示的文字一律以 Localization/<语言>/Mods.ArknightsMod.Configs.hjson 为准；
		// 某语言的条目若被注释掉，会回落到 en-US 那份。改文案请去改 hjson，不要改这里。
		[DefaultValue(true)]
		public bool DoubleSPRegenSpeed { get; set; }

		// 实现在 WeaponPlayer.ActiveDurationMultiplier：把"技能持续时间到期"的判断阈值乘 0.5。
		[DefaultValue(true)]
		public bool DoubleSkillDurationConsumption { get; set; }
	}
}
