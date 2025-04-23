using System.ComponentModel;
using Terraria.ModLoader.Config;

public class MonsterConfig : ModConfig
{
	public override ConfigScope Mode => (ConfigScope)1;

	[DefaultValue(true)]
	[ReloadRequired]
	public bool EnableOriginiumSlug { get; set; }

	[DefaultValue(true)]
	[ReloadRequired]
	public bool EnableOriginiumSlugAlpha { get; set; }

	[DefaultValue(true)]
	[ReloadRequired]
	public bool EnableOriginiumSlugBeta { get; set; }

	[DefaultValue(true)]
	[ReloadRequired]
	public bool EnableAcidOgSlug { get; set; }

	[DefaultValue(true)]
	[ReloadRequired]
	public bool EnableSoldier { get; set; }
}