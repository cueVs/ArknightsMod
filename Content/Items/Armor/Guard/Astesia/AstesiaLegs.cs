using Terraria.ModLoader;
using ArknightsMod.Content.Items.Armor.NeoArmorReforge;
using ArknightsMod.Content.Items.Material;

namespace ArknightsMod.Content.Items.Armor.Guard.Astesia
{
	[AutoloadEquip(EquipType.Legs)]
	public class AstesiaLegs : NeoArmorReforgeVanityLegs
	{
		public override int Rarity => 5;

		public override NeoArmorReforgeSetProfile SetProfile => new() {
			Defense = 8,
			LifeBonus = 96,
			LocalizationPrefix = "Mods.ArknightsMod.ArmorSets.Astesia",
			Materials = recipe => recipe
				.AddIngredient<Orundum>(50)
				.AddIngredient<KetonColloid>(3)
				.AddIngredient<PolyesterPack>(3),
		};
	}
}
