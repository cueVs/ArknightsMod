using Terraria.ModLoader;
using ArknightsMod.Content.Items.Armor.NeoArmorReforge;
using ArknightsMod.Content.Items.Material;

namespace ArknightsMod.Content.Items.Armor.Supporter.Deepcolor
{
	[AutoloadEquip(EquipType.Body)]
	public class DeepcolorBody : NeoArmorReforgeVanityBody
	{
		public override int Rarity => 4;

		public override NeoArmorReforgeSetProfile SetProfile => new() {
			Defense = OperatorArmorStatFormula.BodyDefenseBonus(125),
			LifeBonus = OperatorArmorStatFormula.BodyLifeBonus(1050),
			LocalizationPrefix = "Mods.ArknightsMod.ArmorSets.Deepcolor",
			Materials = recipe => recipe
				.AddIngredient<Orundum>(40)
				.AddIngredient<Polyketon>(2)
				.AddIngredient<OrironCluster>(2),
		};
	}
}
