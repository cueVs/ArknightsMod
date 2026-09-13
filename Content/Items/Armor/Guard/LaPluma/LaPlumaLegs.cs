using Terraria.ModLoader;
using ArknightsMod.Content.Items.Armor.NeoArmorReforge;
using ArknightsMod.Content.Items.Material;

namespace ArknightsMod.Content.Items.Armor.Guard.LaPluma
{
	[AutoloadEquip(EquipType.Legs)]
	public class LaPlumaLegs : NeoArmorReforgeVanityLegs
	{
		public override int Rarity => 5;

		public override NeoArmorReforgeSetProfile SetProfile => new() {
			Defense = 11,
			LifeBonus = 113,
			LocalizationPrefix = "Mods.ArknightsMod.ArmorSets.LaPluma",
			Materials = recipe => recipe
				.AddIngredient<Orundum>(50)
				.AddIngredient<OptimizedDevice>(2)
				.AddIngredient<OrironCluster>(3),
		};
	}
}
