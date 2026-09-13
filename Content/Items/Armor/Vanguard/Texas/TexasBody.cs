using Terraria.ModLoader;
using ArknightsMod.Content.Items.Armor.NeoArmorReforge;
using ArknightsMod.Content.Items.Material;

namespace ArknightsMod.Content.Items.Armor.Vanguard.Texas
{
	[AutoloadEquip(EquipType.Body)]
	public class TexasBody : NeoArmorReforgeVanityBody
	{
		public override int Rarity => 5;

		public override NeoArmorReforgeSetProfile SetProfile => new() {
			Defense = 26,
			LifeBonus = 98,
			LocalizationPrefix = "Mods.ArknightsMod.ArmorSets.Texas",
			Materials = recipe => recipe
				.AddIngredient<Orundum>(50)
				.AddIngredient<ManganeseTrihydrate>(3)
				.AddIngredient<IntegratedDevice>(2),
		};
	}
}
