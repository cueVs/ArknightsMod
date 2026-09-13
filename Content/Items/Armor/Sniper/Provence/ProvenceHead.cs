using Terraria.ModLoader;
using ArknightsMod.Content.Items.Armor.NeoArmorReforge;
using ArknightsMod.Content.Items.Material;

namespace ArknightsMod.Content.Items.Armor.Sniper.Provence
{
	[AutoloadEquip(EquipType.Head)]
	public class ProvenceHead : NeoArmorReforgeVanityHead
	{
		public override int Rarity => 5;

		public override NeoArmorReforgeSetProfile SetProfile => new() {
			Defense = 0,
			LifeBonus = 168,
			LocalizationPrefix = "Mods.ArknightsMod.ArmorSets.Provence",
			Materials = recipe => recipe
				.AddIngredient<Orundum>(50)
				.AddIngredient<SugarLump>(9)
				.AddIngredient<IntegratedDevice>(7),
			SetBonusKey = "Mods.ArknightsMod.ArmorSets.Provence.SetBonus",
		};
	}
}
