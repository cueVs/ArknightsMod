using ArknightsMod.Content.Items.Armor.NeoArmorReforge;
using ArknightsMod.Content.Items.Material;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Armor.Supporter.Radian
{
	[AutoloadEquip(EquipType.Legs)]
	public class RaidianLegs : NeoArmorReforgeVanityLegs
	{
		public override int Rarity => 6;

		public override NeoArmorReforgeSetProfile SetProfile => new() {
			Defense = 4,
			LifeBonus = 69,
			LocalizationPrefix = "Mods.ArknightsMod.ArmorSets.Raidian",
			Materials = recipe => recipe
				.AddIngredient<Orundum>(60)
				.AddIngredient<D32Steel>(6)
				.AddIngredient<OrirockConcentration>(4),
		};
	}
}
