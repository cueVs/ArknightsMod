using Terraria.ModLoader;
using ArknightsMod.Content.Items.Armor.NeoArmorReforge;
using ArknightsMod.Content.Items.Material;

namespace ArknightsMod.Content.Items.Armor.Sniper.Fiammetta
{
	[AutoloadEquip(EquipType.Legs)]
	public class FiammettaLegs : NeoArmorReforgeVanityLegs
	{
		public override int Rarity => 6;

		public override NeoArmorReforgeSetProfile SetProfile => new() {
			Defense = 4,
			LifeBonus = 96,
			LocalizationPrefix = "Mods.ArknightsMod.ArmorSets.Fiammetta",
			Materials = recipe => recipe
				.AddIngredient<Orundum>(60)
				.AddIngredient<PolymerizationPreparation>(6)
				.AddIngredient<ManganeseTrihydrate>(7),
		};
	}
}
