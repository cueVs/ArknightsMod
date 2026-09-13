using Terraria.ModLoader;
using ArknightsMod.Content.Items.Armor.NeoArmorReforge;
using ArknightsMod.Content.Items.Material;

namespace ArknightsMod.Content.Items.Armor.Guard.Popukar
{
	[AutoloadEquip(EquipType.Head)]
	public class PopukarHead : NeoArmorReforgeVanityHead
	{
		public override int Rarity => 3;

		public override NeoArmorReforgeSetProfile SetProfile => new() {
			Defense = 0,
			LifeBonus = 186,
			LocalizationPrefix = "Mods.ArknightsMod.ArmorSets.Popukar",
			Materials = recipe => recipe
				.AddIngredient<Orundum>(30)
				.AddIngredient<SugarSubstitute>(1),
			SetBonusKey = "Mods.ArknightsMod.ArmorSets.Popukar.SetBonus",
		};
	}
}
