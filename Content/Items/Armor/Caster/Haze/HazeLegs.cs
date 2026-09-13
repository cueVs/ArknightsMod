using Terraria.ModLoader;
using ArknightsMod.Content.Items.Armor.NeoArmorReforge;
using ArknightsMod.Content.Items.Material;

namespace ArknightsMod.Content.Items.Armor.Caster.Haze
{
	[AutoloadEquip(EquipType.Legs)]
	public class HazeLegs : NeoArmorReforgeVanityLegs
	{
		public override int Rarity => 4;

		public override NeoArmorReforgeSetProfile SetProfile => new() {
			Defense = 2,
			LifeBonus = 44,
			LocalizationPrefix = "Mods.ArknightsMod.ArmorSets.Haze",
			Materials = recipe => recipe
				.AddIngredient<Orundum>(40)
				.AddIngredient<Sugar>(2)
				.AddIngredient<Aketon>(3),
		};
	}
}
