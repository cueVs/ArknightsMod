using Terraria.ModLoader;
using ArknightsMod.Content.Items.Armor.NeoArmorReforge;
using ArknightsMod.Content.Items.Material;

namespace ArknightsMod.Content.Items.Armor.Sniper.Melanite
{
	[AutoloadEquip(EquipType.Head)]
	public class MelaniteHead : NeoArmorReforgeVanityHead
	{
		public override int Rarity => 5;

		public override NeoArmorReforgeSetProfile SetProfile => new() {
			Defense = 0,
			LifeBonus = 167,
			LocalizationPrefix = "Mods.ArknightsMod.ArmorSets.Melanite",
			Materials = recipe => recipe
				.AddIngredient<Orundum>(50)
				.AddIngredient<RefinedSolvent>(8)
				.AddIngredient<LoxicKohl>(15),
			SetBonusKey = "Mods.ArknightsMod.ArmorSets.Melanite.SetBonus",
		};
	}
}
