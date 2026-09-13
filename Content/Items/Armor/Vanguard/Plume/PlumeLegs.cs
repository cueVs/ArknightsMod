using Terraria.ModLoader;
using ArknightsMod.Content.Items.Armor.NeoArmorReforge;
using ArknightsMod.Content.Items.Material;

namespace ArknightsMod.Content.Items.Armor.Vanguard.Plume
{
	[AutoloadEquip(EquipType.Legs)]
	public class PlumeLegs : NeoArmorReforgeVanityLegs
	{
		public override int Rarity => 3;

		public override NeoArmorReforgeSetProfile SetProfile => new() {
			Defense = 7,
			LifeBonus = 61,
			LocalizationPrefix = "Mods.ArknightsMod.ArmorSets.Plume",
			Materials = recipe => recipe
				.AddIngredient<Orundum>(30)
				.AddIngredient<OrirockCube>(2),
		};
	}
}
