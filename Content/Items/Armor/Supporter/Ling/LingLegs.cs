using Terraria.ModLoader;
using ArknightsMod.Content.Items.Armor.NeoArmorReforge;
using ArknightsMod.Content.Items.Material;

namespace ArknightsMod.Content.Items.Armor.Supporter.Ling
{
	[AutoloadEquip(EquipType.Legs)]
	public class LingLegs : NeoArmorReforgeVanityLegs
	{
		public override int Rarity => 6;

		public override NeoArmorReforgeSetProfile SetProfile => new() {
			Defense = 4,
			LifeBonus = 72,
			LocalizationPrefix = "Mods.ArknightsMod.ArmorSets.Ling",
			Materials = recipe => recipe
				.AddIngredient<Orundum>(60)
				.AddIngredient<D32Steel>(6)
				.AddIngredient<PolymerizedGel>(6),
		};
	}
}
