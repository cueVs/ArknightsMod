using Terraria.ModLoader;
using ArknightsMod.Content.Items.Armor.NeoArmorReforge;
using ArknightsMod.Content.Items.Material;

namespace ArknightsMod.Content.Items.Armor.Specialist.Manticore
{
	[AutoloadEquip(EquipType.Legs)]
	public class ManticoreLegs : NeoArmorReforgeVanityLegs
	{
		public override int Rarity => 5;

		public override NeoArmorReforgeSetProfile SetProfile => new() {
			Defense = 9,
			LifeBonus = 82,
			LocalizationPrefix = "Mods.ArknightsMod.ArmorSets.Manticore",
			Materials = recipe => recipe
				.AddIngredient<Orundum>(50)
				.AddIngredient<KetonColloid>(3)
				.AddIngredient<PolyesterPack>(3),
		};
	}
}
