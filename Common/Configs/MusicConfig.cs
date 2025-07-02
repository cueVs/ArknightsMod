using System.ComponentModel;
using Terraria.ModLoader.Config;

namespace ArknightsMod.Common.Configs
{
	public class MusicConfig : ModConfig
	{
		public override ConfigScope Mode => (ConfigScope)1;
		//太空
		[DefaultValue(true)]
		[ReloadRequired]
		public bool EnableArknightsSpaceDaytime { get; set; }
		[DefaultValue(true)]
		[ReloadRequired]
		public bool EnableArknightsSpaceNighttimeLow { get; set; }
		[DefaultValue(true)]
		[ReloadRequired]
		public bool EnableArknightsSpaceNighttimeHigh { get; set; }
		//森林
		[DefaultValue(true)]
		[ReloadRequired]
		public bool EnableArknightsForestDaytime { get; set; }
		[DefaultValue(true)]
		[ReloadRequired]
		public bool EnableArknightsForestNighttime { get; set; }
		//海洋
		[DefaultValue(true)]
		[ReloadRequired]
		public bool EnableArknightsOceanDaytime { get; set; }
		[DefaultValue(true)]
		[ReloadRequired]
		public bool EnableArknightsOceanNighttime { get; set; }
		//神圣海洋
		[DefaultValue(true)]
		[ReloadRequired]
		public bool EnableArknightsHallowedOceanDaytime { get; set; }
		[DefaultValue(true)]
		[ReloadRequired]
		public bool EnableArknightsHallowedOceanNighttime { get; set; }
		//腐化海洋
		[DefaultValue(true)]
		[ReloadRequired]
		public bool EnableArknightsCorruptedOceanDaytime { get; set; }
		[DefaultValue(true)]
		[ReloadRequired]
		public bool EnableArknightsCorruptedOceanNighttime { get; set; }
		//猩红海洋
		[DefaultValue(true)]
		[ReloadRequired]
		public bool EnableArknightsCrimsonOceanDaytime { get; set; }
		[DefaultValue(true)]
		[ReloadRequired]
		public bool EnableArknightsCrimsonOceanNighttime { get; set; }
	}
}