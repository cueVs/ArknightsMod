using Terraria.ModLoader;
using ArknightsMod.Content.Items.Armor.NeoArmorReforge;
using ArknightsMod.Content.Items.Material;

namespace ArknightsMod.Content.Items.Armor.Medic.Hibiscus
{
	[AutoloadEquip(EquipType.Body)]
	public class HibiscusBody : NeoArmorReforgeVanityBody
	{
		public override int Rarity => 3;

		public override NeoArmorReforgeSetProfile SetProfile => new() {
			Defense = 8,
			LifeBonus = 61,
			LocalizationPrefix = "Mods.ArknightsMod.ArmorSets.Hibiscus",
			Materials = recipe => recipe
				.AddIngredient<Orundum>(30)
				.AddIngredient<Polyketon>(2),
		};
	}
}
