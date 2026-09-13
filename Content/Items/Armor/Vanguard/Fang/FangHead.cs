using Terraria.ModLoader;
using ArknightsMod.Content.Items.Armor.NeoArmorReforge;
using ArknightsMod.Content.Items.Material;

namespace ArknightsMod.Content.Items.Armor.Vanguard.Fang
{
	[AutoloadEquip(EquipType.Head)]
	public class FangHead : NeoArmorReforgeVanityHead
	{
		public override int Rarity => 3;

		public override NeoArmorReforgeSetProfile SetProfile => new() {
			Defense = 0,
			LifeBonus = 133,
			LocalizationPrefix = "Mods.ArknightsMod.ArmorSets.Fang",
			Materials = recipe => recipe
				.AddIngredient<Orundum>(30)
				.AddIngredient<Orirock>(2),
			SetBonusKey = "Mods.ArknightsMod.ArmorSets.Fang.SetBonus",
		};
	}
}
