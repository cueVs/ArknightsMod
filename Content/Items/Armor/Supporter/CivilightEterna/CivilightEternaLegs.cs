using Terraria.ModLoader;
using ArknightsMod.Content.Items.Armor.NeoArmorReforge;
using ArknightsMod.Content.Items.Material;

namespace ArknightsMod.Content.Items.Armor.Supporter.CivilightEterna
{
	[AutoloadEquip(EquipType.Legs)]
	public class CivilightEternaLegs : NeoArmorReforgeVanityLegs
	{
		public override int Rarity => 6;

		public override NeoArmorReforgeSetProfile SetProfile => new() {
			Defense = 6,
			LifeBonus = 97,
			LocalizationPrefix = "Mods.ArknightsMod.ArmorSets.CivilightEterna",
			Materials = recipe => recipe
				.AddIngredient<Orundum>(60)
				.AddIngredient<CrystallineElectronicUnit>(6)
				.AddIngredient<OrironBlock>(3),
		};
	}
}
