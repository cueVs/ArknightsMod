using Terraria.ModLoader;
using ArknightsMod.Content.Items.Armor.NeoArmorReforge;
using ArknightsMod.Content.Items.Material;

namespace ArknightsMod.Content.Items.Armor.Defender.Spot
{
	[AutoloadEquip(EquipType.Legs)]
	public class SpotLegs : NeoArmorReforgeVanityLegs
	{
		public override int Rarity => 3;

		public override NeoArmorReforgeSetProfile SetProfile => new() {
			Defense = 12,
			LifeBonus = 92,
			LocalizationPrefix = "Mods.ArknightsMod.ArmorSets.Spot",
			Materials = recipe => recipe
				.AddIngredient<Orundum>(30)
				.AddIngredient<Device>(1),
		};
	}
}
