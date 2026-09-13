using Terraria.ModLoader;
using ArknightsMod.Content.Items.Armor.NeoArmorReforge;
using ArknightsMod.Content.Items.Material;

namespace ArknightsMod.Content.Items.Armor.Defender.Beagle
{
	[AutoloadEquip(EquipType.Legs)]
	public class BeagleLegs : NeoArmorReforgeVanityLegs
	{
		public override int Rarity => 3;

		public override NeoArmorReforgeSetProfile SetProfile => new() {
			Defense = 6,
			LifeBonus = 57,
			LocalizationPrefix = "Mods.ArknightsMod.ArmorSets.Beagle",
			Materials = recipe => recipe
				.AddIngredient<Orundum>(30)
				.AddIngredient<Device>(1),
		};
	}
}
