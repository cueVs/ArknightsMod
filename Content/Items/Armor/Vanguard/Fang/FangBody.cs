using Terraria.ModLoader;
using ArknightsMod.Content.Items.Armor.NeoArmorReforge;
using ArknightsMod.Content.Items.Material;

namespace ArknightsMod.Content.Items.Armor.Vanguard.Fang
{
	[AutoloadEquip(EquipType.Body)]
	public class FangBody : NeoArmorReforgeVanityBody
	{
		public override int Rarity => 3;

		public override NeoArmorReforgeSetProfile SetProfile => new() {
			Defense = 23,
			LifeBonus = 66,
			LocalizationPrefix = "Mods.ArknightsMod.ArmorSets.Fang",
			Materials = recipe => recipe
				.AddIngredient<Orundum>(30)
				.AddIngredient<Polyester>(2),
		};
	}
}
