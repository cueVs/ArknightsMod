using Terraria.ModLoader;
using ArknightsMod.Content.Items.Armor.NeoArmorReforge;
using ArknightsMod.Content.Items.Material;

namespace ArknightsMod.Content.Items.Armor.Medic.Hibiscus
{
	[AutoloadEquip(EquipType.Head)]
	public class HibiscusHead : NeoArmorReforgeVanityHead
	{
		public override int Rarity => 3;

		public override NeoArmorReforgeSetProfile SetProfile => new() {
			Defense = 0,
			LifeBonus = 122,
			LocalizationPrefix = "Mods.ArknightsMod.ArmorSets.Hibiscus",
			Materials = recipe => recipe
				.AddIngredient<Orundum>(30)
				.AddIngredient<Ester>(1),
			SetBonusKey = "Mods.ArknightsMod.ArmorSets.Hibiscus.SetBonus",
		};
	}
}
