using Terraria.ModLoader;
using ArknightsMod.Content.Items.Armor.NeoArmorReforge;
using ArknightsMod.Content.Items.Material;

namespace ArknightsMod.Content.Items.Armor.Guard.Lappland
{
	[AutoloadEquip(EquipType.Legs)]
	public class LapplandLegs : NeoArmorReforgeVanityLegs
	{
		public override int Rarity => 5;

		public override NeoArmorReforgeSetProfile SetProfile => new() {
			Defense = 9,
			LifeBonus = 118,
			LocalizationPrefix = "Mods.ArknightsMod.ArmorSets.Lappland",
			Materials = recipe => recipe
				.AddIngredient<Orundum>(50)
				.AddIngredient<SugarLump>(3)
				.AddIngredient<RMA7012>(3),
		};
	}
}
