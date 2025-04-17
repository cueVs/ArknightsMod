using System.ComponentModel;
using Terraria.ModLoader.Config;

public class MusicConfig : ModConfig
{
	public override ConfigScope Mode => (ConfigScope)1;
	//Ì«¿Õ
	[DefaultValue(true)]
	[ReloadRequired]
	public bool EnableArknightsSpaceDaytime { get; set; }
	[DefaultValue(true)]
	[ReloadRequired]
	public bool EnableArknightsSpaceNighttimeLow { get; set; }
	[DefaultValue(true)]
	[ReloadRequired]
	public bool EnableArknightsSpaceNighttimeHigh { get; set; }
	//É­ÁÖ
	[DefaultValue(true)]
	[ReloadRequired]
	public bool EnableArknightsForestDaytime { get; set; }
    [DefaultValue(true)]
	[ReloadRequired]
	public bool EnableArknightsForestNighttime { get; set; }
	//º£Ñó
    [DefaultValue(true)]
	[ReloadRequired]
	public bool EnableArknightsOceanDaytime { get; set; }
    [DefaultValue(true)]
	[ReloadRequired]
	public bool EnableArknightsOceanNighttime { get; set; }
	//ÉñÊ¥º£Ñó
	[DefaultValue(true)]
	[ReloadRequired]
	public bool EnableArknightsHallowedOceanDaytime { get; set; }
	[DefaultValue(true)]
	[ReloadRequired]
	public bool EnableArknightsHallowedOceanNighttime { get; set; }
	//¸¯»¯º£Ñó
	[DefaultValue(true)]
	[ReloadRequired]
	public bool EnableArknightsCorruptedOceanDaytime { get; set; }
	[DefaultValue(true)]
	[ReloadRequired]
	public bool EnableArknightsCorruptedOceanNighttime { get; set; }
	//ÐÉºìº£Ñó
	[DefaultValue(true)]
	[ReloadRequired]
	public bool EnableArknightsCrimsonOceanDaytime { get; set; }
	[DefaultValue(true)]
	[ReloadRequired]
	public bool EnableArknightsCrimsonOceanNighttime { get; set; }
}