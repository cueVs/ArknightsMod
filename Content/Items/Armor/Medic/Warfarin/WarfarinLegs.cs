using Terraria.ModLoader;
using ArknightsMod.Content.Items.Armor.NeoArmorReforge;
using ArknightsMod.Content.Items.Material;

namespace ArknightsMod.Content.Items.Armor.Medic.Warfarin
{
	[AutoloadEquip(EquipType.Legs)]
	public class WarfarinLegs : NeoArmorReforgeVanityLegs
	{
		public override int Rarity => 5;

		public override NeoArmorReforgeSetProfile SetProfile => new() {
			Defense = 3,
			LifeBonus = 76,
			LocalizationPrefix = "Mods.ArknightsMod.ArmorSets.Warfarin",
			Materials = recipe => recipe
				.AddIngredient<Orundum>(50)
				.AddIngredient<PolyesterLump>(3)
				.AddIngredient<OrirockCluster>(4),
		};
	}
}
