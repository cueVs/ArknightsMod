using Terraria.ModLoader;
using ArknightsMod.Content.Items.Armor.NeoArmorReforge;
using ArknightsMod.Content.Items.Material;

namespace ArknightsMod.Content.Items.Armor.Supporter.Ling
{
	[AutoloadEquip(EquipType.Head)]
	public class LingHead : NeoArmorReforgeVanityHead
	{
		public override int Rarity => 6;

		public override NeoArmorReforgeSetProfile SetProfile => new() {
			Defense = 0,
			LifeBonus = 143,
			LocalizationPrefix = "Mods.ArknightsMod.ArmorSets.Ling",
			Materials = recipe => recipe
				.AddIngredient<Orundum>(60)
				.AddIngredient<PolymerizationPreparation>(6)
				.AddIngredient<IncandescentAlloyBlock>(6),
			SetBonusKey = "Mods.ArknightsMod.ArmorSets.Ling.SetBonus",
		};
	}
}
