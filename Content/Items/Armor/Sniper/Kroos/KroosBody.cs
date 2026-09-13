using Terraria.ModLoader;
using ArknightsMod.Content.Items.Armor.NeoArmorReforge;
using ArknightsMod.Content.Items.Material;

namespace ArknightsMod.Content.Items.Armor.Sniper.Kroos
{
	[AutoloadEquip(EquipType.Body)]
	public class KroosBody : NeoArmorReforgeVanityBody
	{
		public override int Rarity => 3;

		public override NeoArmorReforgeSetProfile SetProfile => new() {
			Defense = 10,
			LifeBonus = 53,
			LocalizationPrefix = "Mods.ArknightsMod.ArmorSets.Kroos",
			Materials = recipe => recipe
				.AddIngredient<Orundum>(30)
				.AddIngredient<Sugar>(2),
		};
	}
}
