using Terraria.ModLoader;
using ArknightsMod.Content.Items.Armor.NeoArmorReforge;
using ArknightsMod.Content.Items.Material;

namespace ArknightsMod.Content.Items.Armor.Specialist.Mortis
{
	[AutoloadEquip(EquipType.Legs)]
	public class MortisLegs : NeoArmorReforgeVanityLegs
	{
		public override int Rarity => 5;

		public override NeoArmorReforgeSetProfile SetProfile => new() {
			Defense = 8,
			LifeBonus = 111,
			LocalizationPrefix = "Mods.ArknightsMod.ArmorSets.Mortis",
			Materials = recipe => recipe
				.AddIngredient<Orundum>(50)
				.AddIngredient<CrystallineCircuit>(3)
				.AddIngredient<LoxicKohl>(1),
		};
	}
}
