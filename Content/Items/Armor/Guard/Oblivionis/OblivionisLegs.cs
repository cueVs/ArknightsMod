using Terraria.ModLoader;
using ArknightsMod.Content.Items.Armor.NeoArmorReforge;
using ArknightsMod.Content.Items.Material;

namespace ArknightsMod.Content.Items.Armor.Guard.Oblivionis
{
	[AutoloadEquip(EquipType.Legs)]
	public class OblivionisLegs : NeoArmorReforgeVanityLegs
	{
		public override int Rarity => 6;

		public override NeoArmorReforgeSetProfile SetProfile => new() {
			Defense = 11,
			LifeBonus = 118,
			LocalizationPrefix = "Mods.ArknightsMod.ArmorSets.Oblivionis",
			Materials = recipe => recipe
				.AddIngredient<Orundum>(60)
				.AddIngredient<NucleicCrystalSinter>(6)
				.AddIngredient<PolymerizedGel>(2),
		};
	}
}
