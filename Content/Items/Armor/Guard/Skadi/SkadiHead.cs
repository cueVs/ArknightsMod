using Terraria.ModLoader;
using ArknightsMod.Content.Items.Armor.NeoArmorReforge;
using ArknightsMod.Content.Items.Material;

namespace ArknightsMod.Content.Items.Armor.Guard.Skadi
{
	[AutoloadEquip(EquipType.Head)]
	public class SkadiHead : NeoArmorReforgeVanityHead
	{
		public override int Rarity => 6;

		public override NeoArmorReforgeSetProfile SetProfile => new() {
			Defense = 0,
			LifeBonus = 387,
			LocalizationPrefix = "Mods.ArknightsMod.ArmorSets.Skadi",
			Materials = recipe => recipe
				.AddIngredient<Orundum>(60)
				.AddIngredient<D32Steel>(6)
				.AddIngredient<OrirockConcentration>(7),
			SetBonusKey = "Mods.ArknightsMod.ArmorSets.Skadi.SetBonus",
		};
	}
}
