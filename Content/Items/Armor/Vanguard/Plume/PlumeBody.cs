using Terraria.ModLoader;
using ArknightsMod.Content.Items.Armor.NeoArmorReforge;
using ArknightsMod.Content.Items.Material;

namespace ArknightsMod.Content.Items.Armor.Vanguard.Plume
{
	[AutoloadEquip(EquipType.Body)]
	public class PlumeBody : NeoArmorReforgeVanityBody
	{
		public override int Rarity => 3;

		public override NeoArmorReforgeSetProfile SetProfile => new() {
			Defense = 21,
			LifeBonus = 61,
			LocalizationPrefix = "Mods.ArknightsMod.ArmorSets.Plume",
			Materials = recipe => recipe
				.AddIngredient<Orundum>(30)
				.AddIngredient<Sugar>(2),
		};
	}
}
