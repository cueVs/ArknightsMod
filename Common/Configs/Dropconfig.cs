using System.Collections.Generic;
using System.ComponentModel;
using Terraria.ModLoader.Config;
using Newtonsoft.Json;
using Terraria;
using Terraria.Localization;


public class Dropconfig : ModConfig
{
	public override ConfigScope Mode => (ConfigScope)1;

	[DefaultValue(8)]
	[Range(1, 100)]
	public int DropOriginiumSlug;

	[DefaultValue(7)]
	[Range(1, 100)]
	public int DropOriginiumSlugAlpha;

	[DefaultValue(6)]
	[Range(1, 100)]
	public int DropOriginiumSlugBeta;

	[DefaultValue(8)]
	[Range(1, 100)]
	public int DropSoldier1;

	[DefaultValue(8)]
	[Range(1, 100)]
	public int DropSoldier2;

	[DefaultValue(8)]
	[Range(1, 100)]
	public int DropAcidOgSlug;
}