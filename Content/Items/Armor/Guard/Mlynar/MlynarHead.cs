using Terraria.ModLoader;
using ArknightsMod.Content.Items.Armor.NeoArmorReforge;
using ArknightsMod.Content.Items.Material;

namespace ArknightsMod.Content.Items.Armor.Guard.Mlynar
{
	[AutoloadEquip(EquipType.Head)]
	public class MlynarHead : NeoArmorReforgeVanityHead
	{
		public override int Rarity => 6;

		public override NeoArmorReforgeSetProfile SetProfile => new() {
			Defense = 0,
			LifeBonus = 427,
			LocalizationPrefix = "Mods.ArknightsMod.ArmorSets.Mlynar",
			Materials = recipe => recipe
				.AddIngredient<Orundum>(60)
				.AddIngredient<PolymerizationPreparation>(6)
				.AddIngredient<WhiteHorseKohl>(7),
			SetBonusKey = "Mods.ArknightsMod.ArmorSets.Mlynar.SetBonus",
		};
	}
}
