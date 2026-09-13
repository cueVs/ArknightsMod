using Terraria.ModLoader;
using ArknightsMod.Content.Items.Armor.NeoArmorReforge;
using ArknightsMod.Content.Items.Material;

namespace ArknightsMod.Content.Items.Armor.Supporter.Deepcolor
{
	[AutoloadEquip(EquipType.Legs)]
	public class DeepcolorLegs : NeoArmorReforgeVanityLegs
	{
		public override int Rarity => 4;

		public override NeoArmorReforgeSetProfile SetProfile => new() {
			Defense = OperatorArmorStatFormula.LegsDefenseBonus(125),
			LifeBonus = OperatorArmorStatFormula.LegsLifeBonus(1050),
			LocalizationPrefix = "Mods.ArknightsMod.ArmorSets.Deepcolor",
			Materials = recipe => recipe
				.AddIngredient<Orundum>(40)
				.AddIngredient<Oriron>(2)
				.AddIngredient<Aketon>(3),
		};
	}
}
