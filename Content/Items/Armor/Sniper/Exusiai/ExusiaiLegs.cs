using Terraria.ModLoader;
using ArknightsMod.Content.Items.Armor.NeoArmorReforge;
using ArknightsMod.Content.Items.Material;

namespace ArknightsMod.Content.Items.Armor.Sniper.Exusiai
{
	[AutoloadEquip(EquipType.Legs)]
	public class ExusiaiLegs : NeoArmorReforgeVanityLegs
	{
		public override int Rarity => 6;

		public override NeoArmorReforgeSetProfile SetProfile => new() {
			Defense = 4,
			LifeBonus = 84,
			LocalizationPrefix = "Mods.ArknightsMod.ArmorSets.Exusiai",
			Materials = recipe => recipe
				.AddIngredient<Orundum>(60)
				.AddIngredient<D32Steel>(6)
				.AddIngredient<PolyesterLump>(6),
		};
	}
}
