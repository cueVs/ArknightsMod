#if DEBUG
using System.ComponentModel;
using Terraria.ModLoader.Config;

namespace ArknightsMod.Common.Configs
{
	public class DebugConfig : ModConfig
	{
		public override ConfigScope Mode => ConfigScope.ClientSide;

		[DefaultValue(false)]
		public bool AutoUpgradeNeoArmor { get; set; }
	}
}
#endif
