using System.ComponentModel;
using Terraria.ModLoader.Config;

// 武器数值全局覆写开关
// 开启后，WeaponStatOverride 中定义的数值将覆盖各武器自身的 SetDefaults 值
// 关闭时，各武器按照自身文件中的原始数值生效

namespace ArknightsMod.Common.Configs
{
	public class WeaponStatOverrideConfig : ModConfig
	{
		// 武器伤害与攻速属于联机权威数据，必须由服务器配置统一同步。
		public override ConfigScope Mode => ConfigScope.ServerSide;

		// 在游戏内 Mod 配置界面中显示的开关
		[DefaultValue(true)]
		public bool UseGlobalWeaponStats { get; set; }
	}
}
