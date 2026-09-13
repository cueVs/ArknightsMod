using Terraria.ModLoader;
using ArknightsMod.Content.Items.Armor.NeoArmorReforge;
using ArknightsMod.Content.Items.Material;

namespace ArknightsMod.Content.Items.Armor.Sniper.Kroos
{
	[AutoloadEquip(EquipType.Legs)]
	public class KroosLegs : NeoArmorReforgeVanityLegs
	{
		public override int Rarity => 3;

		public override NeoArmorReforgeSetProfile SetProfile => new() {
			Defense = 3,
			LifeBonus = 53,
			LocalizationPrefix = "Mods.ArknightsMod.ArmorSets.Kroos",
			Materials = recipe => recipe
				.AddIngredient<Orundum>(30)
				.AddIngredient<OrirockCube>(2),
		};
	}
}
