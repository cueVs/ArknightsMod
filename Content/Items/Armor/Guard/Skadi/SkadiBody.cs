using Terraria.ModLoader;
using ArknightsMod.Content.Items.Armor.NeoArmorReforge;
using ArknightsMod.Content.Items.Material;

namespace ArknightsMod.Content.Items.Armor.Guard.Skadi
{
	[AutoloadEquip(EquipType.Body)]
	public class SkadiBody : NeoArmorReforgeVanityBody
	{
		public override int Rarity => 6;

		public override NeoArmorReforgeSetProfile SetProfile => new() {
			Defense = 20,
			LifeBonus = 193,
			LocalizationPrefix = "Mods.ArknightsMod.ArmorSets.Skadi",
			Materials = recipe => recipe
				.AddIngredient<Orundum>(60)
				.AddIngredient<BipolarNanoflake>(6)
				.AddIngredient<OrirockConcentration>(6),
		};
	}
}
