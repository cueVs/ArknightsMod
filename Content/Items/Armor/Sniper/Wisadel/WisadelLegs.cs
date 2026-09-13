using Terraria.ModLoader;
using ArknightsMod.Content.Items.Armor.NeoArmorReforge;
using ArknightsMod.Content.Items.Material;

namespace ArknightsMod.Content.Items.Armor.Sniper.Wisadel
{
	[AutoloadEquip(EquipType.Legs)]
	public class WisadelLegs : NeoArmorReforgeVanityLegs
	{
		public override int Rarity => 6;

		public override NeoArmorReforgeSetProfile SetProfile => new() {
			Defense = 7,
			LifeBonus = 95,
			LocalizationPrefix = "Mods.ArknightsMod.ArmorSets.Wisadel",
			Materials = recipe => recipe
				.AddIngredient<Orundum>(60)
				.AddIngredient<NucleicCrystalSinter>(6)
				.AddIngredient<KetonColloid>(4),
		};
	}
}
