using ArknightsMod.Content.Items.Armor.NeoArmorReforge;
using Terraria.ModLoader;
using ArknightsMod.Content.Items.Material;

namespace ArknightsMod.Content.Items.Armor.Sniper.Rosmontis
{
	[AutoloadEquip(EquipType.Head)]
	public class RosmontisHead : NeoArmorReforgeVanityHead
	{
		public override int Rarity => 6;

		public override NeoArmorReforgeSetProfile SetProfile => new() {
			Defense = 0,
			LifeBonus = 195,
			LocalizationPrefix = "Mods.ArknightsMod.ArmorSets.Rosmontis",
			Materials = recipe => recipe
				.AddIngredient<Orundum>(60)
				.AddIngredient<CrystallineElectronicUnit>(6)
				.AddIngredient<OrirockConcentration>(4),
			SetBonusKey = "Mods.ArknightsMod.ArmorSets.Rosmontis.SetBonus",
		};
	}
}
