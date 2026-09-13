using Terraria.ModLoader;
using ArknightsMod.Content.Items.Armor.NeoArmorReforge;
using ArknightsMod.Content.Items.Material;

namespace ArknightsMod.Content.Items.Armor.Guard.Mlynar
{
	[AutoloadEquip(EquipType.Legs)]
	public class MlynarLegs : NeoArmorReforgeVanityLegs
	{
		public override int Rarity => 6;

		public override NeoArmorReforgeSetProfile SetProfile => new() {
			Defense = 13,
			LifeBonus = 213,
			LocalizationPrefix = "Mods.ArknightsMod.ArmorSets.Mlynar",
			Materials = recipe => recipe
				.AddIngredient<Orundum>(60)
				.AddIngredient<CrystallineElectronicUnit>(6)
				.AddIngredient<CrystallineCircuit>(4),
		};
	}
}
