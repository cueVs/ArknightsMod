using Terraria.ModLoader;
using ArknightsMod.Content.Items.Armor.NeoArmorReforge;
using ArknightsMod.Content.Items.Material;

namespace ArknightsMod.Content.Items.Armor.Sniper.KroosAlter
{
	[AutoloadEquip(EquipType.Legs)]
	public class KkdyAlterLegs : NeoArmorReforgeVanityLegs
	{
		public override int Rarity => 5;

		public override NeoArmorReforgeSetProfile SetProfile => new() {
			Defense = 4,
			LifeBonus = 62,
			LocalizationPrefix = "Mods.ArknightsMod.ArmorSets.KkdyAlter",
			Materials = recipe => recipe
				.AddIngredient<Orundum>(50)
				.AddIngredient<OrironBlock>(3)
				.AddIngredient<SemiSyntheticSolvent>(1),
		};
	}
}
