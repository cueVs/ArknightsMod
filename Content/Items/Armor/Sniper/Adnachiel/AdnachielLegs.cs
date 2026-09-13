using Terraria.ModLoader;
using ArknightsMod.Content.Items.Armor.NeoArmorReforge;
using ArknightsMod.Content.Items.Material;

namespace ArknightsMod.Content.Items.Armor.Sniper.Adnachiel
{
	[AutoloadEquip(EquipType.Legs)]
	public class AdnachielLegs : NeoArmorReforgeVanityLegs
	{
		public override int Rarity => 3;

		public override NeoArmorReforgeSetProfile SetProfile => new() {
			Defense = 3,
			LifeBonus = 54,
			LocalizationPrefix = "Mods.ArknightsMod.ArmorSets.Adnachiel",
			Materials = recipe => recipe
				.AddIngredient<Orundum>(30)
				.AddIngredient<Sugar>(1),
		};
	}
}
