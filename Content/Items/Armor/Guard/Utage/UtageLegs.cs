using Terraria.ModLoader;
using ArknightsMod.Content.Items.Armor.NeoArmorReforge;
using ArknightsMod.Content.Items.Material;

namespace ArknightsMod.Content.Items.Armor.Guard.Utage
{
	[AutoloadEquip(EquipType.Legs)]
	public class UtageLegs : NeoArmorReforgeVanityLegs
	{
		public override int Rarity => 4;

		public override NeoArmorReforgeSetProfile SetProfile => new() {
			Defense = 5,
			LifeBonus = 98,
			LocalizationPrefix = "Mods.ArknightsMod.ArmorSets.Utage",
			Materials = recipe => recipe
				.AddIngredient<Orundum>(40)
				.AddIngredient<Oriron>(2)
				.AddIngredient<Grindstone>(2),
		};
	}
}
