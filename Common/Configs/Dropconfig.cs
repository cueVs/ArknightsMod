using System.ComponentModel;
using Terraria.ModLoader.Config;


public class Dropconfig : ModConfig
{
	public override ConfigScope Mode => (ConfigScope)1;

	[DefaultValue(8)]
	[Range(1, 100)]
	public int DropOriginiumSlug;

	[DefaultValue(1)]
	[Range(1, 100)]
	public int DropLS;

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
	public int DropAcidOgSlug1;

	[DefaultValue(8)]
	[Range(1, 100)]
	public int DropAcidOgSlug2;

	[DefaultValue(1)]
	[Range(1, 100)]
	public int DropDrone1;

	[DefaultValue(10)]
	[Range(1, 100)]
	public int DropDrone2;
}