using Terraria.ModLoader;
using ArknightsMod.Content.Items.Armor.NeoArmorReforge;
using ArknightsMod.Content.Items.Material;

namespace ArknightsMod.Content.Items.Armor.Guard.Surtr
{
	[AutoloadEquip(EquipType.Legs)]
	public class SurtrLegs : NeoArmorReforgeVanityLegs
	{
		public override int Rarity => 6;

		public override int Value => 560000;

		public override NeoArmorReforgeSetProfile SetProfile => new() {
			Defense = 10,
			LifeBonus = 146,
			LocalizationPrefix = "Mods.ArknightsMod.ArmorSets.Surtr",
			Materials = recipe => recipe
				.AddIngredient<Orundum>(60)
				.AddIngredient<D32Steel>(6)
				.AddIngredient<PolymerizedGel>(6),
		};
	}
}
