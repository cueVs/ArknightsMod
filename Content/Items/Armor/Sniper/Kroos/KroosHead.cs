using Terraria.ModLoader;
using ArknightsMod.Content.Items.Armor.NeoArmorReforge;
using ArknightsMod.Content.Items.Material;

namespace ArknightsMod.Content.Items.Armor.Sniper.Kroos
{
	[AutoloadEquip(EquipType.Head)]
	public class KroosHead : NeoArmorReforgeVanityHead
	{
		public override int Rarity => 3;

		public override NeoArmorReforgeSetProfile SetProfile => new() {
			Defense = 0,
			LifeBonus = 106,
			LocalizationPrefix = "Mods.ArknightsMod.ArmorSets.Kroos",
			Materials = recipe => recipe
				.AddIngredient<Orundum>(30)
				.AddIngredient<DamagedDevice>(1),
			SetBonusKey = "Mods.ArknightsMod.ArmorSets.Kroos.SetBonus",
		};
	}
}
