using Terraria.ModLoader;
using ArknightsMod.Content.Items.Armor.NeoArmorReforge;
using ArknightsMod.Content.Items.Material;

namespace ArknightsMod.Content.Items.Armor.Supporter.Ling
{
	[AutoloadEquip(EquipType.Body)]
	public class LingBody : NeoArmorReforgeVanityBody
	{
		public override int Rarity => 6;

		public override NeoArmorReforgeSetProfile SetProfile => new() {
			Defense = 11,
			LifeBonus = 72,
			LocalizationPrefix = "Mods.ArknightsMod.ArmorSets.Ling",
			Materials = recipe => recipe
				.AddIngredient<Orundum>(60)
				.AddIngredient<CrystallineElectronicUnit>(6)
				.AddIngredient<OrirockConcentration>(4),
		};
	}
}
