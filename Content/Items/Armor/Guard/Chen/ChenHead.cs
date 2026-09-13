using Terraria.ModLoader;
using ArknightsMod.Content.Items.Armor.NeoArmorReforge;
using ArknightsMod.Content.Items.Material;

namespace ArknightsMod.Content.Items.Armor.Guard.Chen
{
	[AutoloadEquip(EquipType.Head)]
	public class ChenHead : NeoArmorReforgeVanityHead
	{
		public override int Rarity => 6;

		public override NeoArmorReforgeSetProfile SetProfile => new() {
			Defense = 0,
			LifeBonus = 288,
			LocalizationPrefix = "Mods.ArknightsMod.ArmorSets.Chen",
			Materials = recipe => recipe
				.AddIngredient<Orundum>(60)
				.AddIngredient<PolymerizationPreparation>(6)
				.AddIngredient<WhiteHorseKohl>(7),
			SetBonusKey = "Mods.ArknightsMod.ArmorSets.Chen.SetBonus",
		};
	}
}
