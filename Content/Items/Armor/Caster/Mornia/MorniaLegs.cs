using Terraria.ModLoader;
using ArknightsMod.Content.Items.Armor.NeoArmorReforge;
using ArknightsMod.Content.Items.Material;

namespace ArknightsMod.Content.Items.Armor.Caster.Mornia
{
	[AutoloadEquip(EquipType.Legs)]
	public class MorniaLegs : NeoArmorReforgeVanityLegs
	{
		public override int Rarity => 4;

		public override NeoArmorReforgeSetProfile SetProfile => new() {
			Defense = 6,
			LifeBonus = 50,
			LocalizationPrefix = "Mods.ArknightsMod.ArmorSets.Mornia",
			Materials = recipe => recipe
				.AddIngredient<Orundum>(40)
				.AddIngredient<Polyester>(3),
		};
	}
}
