using Terraria.ModLoader;
using ArknightsMod.Content.Items.Armor.NeoArmorReforge;
using ArknightsMod.Content.Items.Material;

namespace ArknightsMod.Content.Items.Armor.Guard.Ulpianus
{
	[AutoloadEquip(EquipType.Body)]
	public class UlpianusBody : NeoArmorReforgeVanityBody
	{
		public override int Rarity => 6;

		public override NeoArmorReforgeSetProfile SetProfile => new() {
			Defense = 0,
			LifeBonus = 326,
			LocalizationPrefix = "Mods.ArknightsMod.ArmorSets.Ulpianus",
			Materials = recipe => recipe
				.AddIngredient<Orundum>(60)
				.AddIngredient<NucleicCrystalSinter>(6)
				.AddIngredient<RefinedSolvent>(2),
		};
	}
}
