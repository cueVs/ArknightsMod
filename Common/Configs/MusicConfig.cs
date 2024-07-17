using System.ComponentModel;
using Terraria.ModLoader.Config;

public class MusicConfig : ModConfig
{
	public override ConfigScope Mode => (ConfigScope)1;

	[DefaultValue(true)]
	[ReloadRequired]
	public bool EnableArknightsSpace { get; set; }

	[DefaultValue(true)]
	[ReloadRequired]
	public bool EnableArknightsDaytime { get; set; }

    [DefaultValue(true)]
	[ReloadRequired]
	public bool EnableArknightsNighttime { get; set; }

    [DefaultValue(true)]
	[ReloadRequired]
	public bool EnableArknightsOceanDaytime { get; set; }

    [DefaultValue(true)]
	[ReloadRequired]
	public bool EnableArknightsOceanNighttimeScene { get; set; }
}