using Terraria.ModLoader;
using ArknightsMod.Content.Items.Armor.NeoArmorReforge;
using ArknightsMod.Content.Items.Material;

namespace ArknightsMod.Content.Items.Armor.Sniper.Catapult
{
	[AutoloadEquip(EquipType.Legs)]
	public class CatapultLegs : NeoArmorReforgeVanityLegs
	{
		public override int Rarity => 3;

		public override NeoArmorReforgeSetProfile SetProfile => new() {
			Defense = 2,
			LifeBonus = 58,
			LocalizationPrefix = "Mods.ArknightsMod.ArmorSets.Catapult",
			Materials = recipe => recipe
				.AddIngredient<Orundum>(30)
				.AddIngredient<Oriron>(1),
		};
	}
}
