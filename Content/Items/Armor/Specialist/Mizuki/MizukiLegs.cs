using Terraria.ModLoader;
using ArknightsMod.Content.Items.Armor.NeoArmorReforge;
using ArknightsMod.Content.Items.Material;

namespace ArknightsMod.Content.Items.Armor.Specialist.Mizuki
{
	[AutoloadEquip(EquipType.Legs)]
	public class MizukiLegs : NeoArmorReforgeVanityLegs
	{
		public override int Rarity => 6;

		public override NeoArmorReforgeSetProfile SetProfile => new() {
			Defense = 9,
			LifeBonus = 88,
			LocalizationPrefix = "Mods.ArknightsMod.ArmorSets.Mizuki",
			Materials = recipe => recipe
				.AddIngredient<Orundum>(60)
				.AddIngredient<BipolarNanoflake>(6)
				.AddIngredient<OrironBlock>(4),
		};
	}
}
