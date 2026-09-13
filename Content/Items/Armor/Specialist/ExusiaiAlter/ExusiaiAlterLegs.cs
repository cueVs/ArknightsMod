using Terraria.ModLoader;
using ArknightsMod.Content.Items.Armor.NeoArmorReforge;
using ArknightsMod.Content.Items.Material;

namespace ArknightsMod.Content.Items.Armor.Specialist.ExusiaiAlter
{
	[AutoloadEquip(EquipType.Legs)]
	public class ExusiaiAlterLegs : NeoArmorReforgeVanityLegs
	{
		public override int Rarity => 6;

		public override NeoArmorReforgeSetProfile SetProfile => new() {
			Defense = 4,
			LifeBonus = 118,
			LocalizationPrefix = "Mods.ArknightsMod.ArmorSets.ExusiaiAlter",
			Materials = recipe => recipe
				.AddIngredient<Orundum>(60)
				.AddIngredient<D32Steel>(6)
				.AddIngredient<RefinedSolvent>(6),
		};
	}
}
