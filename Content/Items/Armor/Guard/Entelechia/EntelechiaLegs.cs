using Terraria.ModLoader;
using ArknightsMod.Content.Items.Armor.NeoArmorReforge;
using ArknightsMod.Content.Items.Material;

namespace ArknightsMod.Content.Items.Armor.Guard.Entelechia
{
	[AutoloadEquip(EquipType.Legs)]
	public class EntelechiaLegs : NeoArmorReforgeVanityLegs
	{
		public override int Rarity => 6;

		public override NeoArmorReforgeSetProfile SetProfile => new() {
			Defense = 11,
			LifeBonus = 129,
			LocalizationPrefix = "Mods.ArknightsMod.ArmorSets.Entelechia",
			Materials = recipe => recipe
				.AddIngredient<Orundum>(60)
				.AddIngredient<D32Steel>(6)
				.AddIngredient<CrystallineCircuit>(5),
		};
	}
}
