using Terraria.ModLoader;
using ArknightsMod.Content.Items.Armor.NeoArmorReforge;
using ArknightsMod.Content.Items.Material;

namespace ArknightsMod.Content.Items.Armor.Medic.Hibiscus
{
	[AutoloadEquip(EquipType.Legs)]
	public class HibiscusLegs : NeoArmorReforgeVanityLegs
	{
		public override int Rarity => 3;

		public override NeoArmorReforgeSetProfile SetProfile => new() {
			Defense = 3,
			LifeBonus = 61,
			LocalizationPrefix = "Mods.ArknightsMod.ArmorSets.Hibiscus",
			Materials = recipe => recipe
				.AddIngredient<Orundum>(30)
				.AddIngredient<Oriron>(1),
		};
	}
}
