using Terraria.ModLoader;
using ArknightsMod.Content.Items.Armor.NeoArmorReforge;
using ArknightsMod.Content.Items.Material;

namespace ArknightsMod.Content.Items.Armor.Medic.Warfarin
{
	[AutoloadEquip(EquipType.Head)]
	public class WarfarinHead : NeoArmorReforgeVanityHead
	{
		public override int Rarity => 5;

		public override NeoArmorReforgeSetProfile SetProfile => new() {
			Defense = 0,
			LifeBonus = 152,
			LocalizationPrefix = "Mods.ArknightsMod.ArmorSets.Warfarin",
			Materials = recipe => recipe
				.AddIngredient<Orundum>(50)
				.AddIngredient<OptimizedDevice>(5)
				.AddIngredient<SugarPack>(17),
			SetBonusKey = "Mods.ArknightsMod.ArmorSets.Warfarin.SetBonus",
		};
	}
}
