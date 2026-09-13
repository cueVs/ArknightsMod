using Terraria.ModLoader;
using ArknightsMod.Content.Items.Armor.NeoArmorReforge;
using ArknightsMod.Content.Items.Material;

namespace ArknightsMod.Content.Items.Armor.Guard.Melantha
{
	[AutoloadEquip(EquipType.Legs)]
	public class MelanthaLegs : NeoArmorReforgeVanityLegs
	{
		public override int Rarity => 3;

		public override NeoArmorReforgeSetProfile SetProfile => new() {
			Defense = 2,
			LifeBonus = 70,
			LocalizationPrefix = "Mods.ArknightsMod.ArmorSets.Melantha",
			Materials = recipe => recipe
				.AddIngredient<Orundum>(30)
				.AddIngredient<Sugar>(1),
		};
	}
}
