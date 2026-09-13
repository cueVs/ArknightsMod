using Terraria.ModLoader;
using ArknightsMod.Content.Items.Armor.NeoArmorReforge;
using ArknightsMod.Content.Items.Material;

namespace ArknightsMod.Content.Items.Armor.Caster.Indigo
{
	[AutoloadEquip(EquipType.Legs)]
	public class IndigoLegs : NeoArmorReforgeVanityLegs
	{
		public override int Rarity => 4;

		public override NeoArmorReforgeSetProfile SetProfile => new() {
			Defense = 3,
			LifeBonus = 72,
			LocalizationPrefix = "Mods.ArknightsMod.ArmorSets.Indigo",
			Materials = recipe => recipe
				.AddIngredient<Orundum>(40)
				.AddIngredient<Device>(1)
				.AddIngredient<Grindstone>(2),
		};
	}
}
