using Terraria.ModLoader;
using ArknightsMod.Content.Items.Armor.NeoArmorReforge;
using ArknightsMod.Content.Items.Material;

namespace ArknightsMod.Content.Items.Armor.Guard.Matoimaru
{
	[AutoloadEquip(EquipType.Legs)]
	public class MatoimaruLegs : NeoArmorReforgeVanityLegs
	{
		public override int Rarity => 4;

		public override NeoArmorReforgeSetProfile SetProfile => new() {
			Defense = 3,
			LifeBonus = 101,
			LocalizationPrefix = "Mods.ArknightsMod.ArmorSets.Matoimaru",
			Materials = recipe => recipe
				.AddIngredient<Orundum>(40)
				.AddIngredient<Sugar>(2)
				.AddIngredient<Grindstone>(2),
		};
	}
}
