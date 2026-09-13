using Terraria.ModLoader;
using ArknightsMod.Content.Items.Armor.NeoArmorReforge;
using ArknightsMod.Content.Items.Material;

namespace ArknightsMod.Content.Items.Armor.Specialist.Mizuki
{
	[AutoloadEquip(EquipType.Head)]
	public class MizukiHead : NeoArmorReforgeVanityHead
	{
		public override int Rarity => 6;

		public override NeoArmorReforgeSetProfile SetProfile => new() {
			Defense = 0,
			LifeBonus = 176,
			LocalizationPrefix = "Mods.ArknightsMod.ArmorSets.Mizuki",
			Materials = recipe => recipe
				.AddIngredient<Orundum>(60)
				.AddIngredient<D32Steel>(6)
				.AddIngredient<OrirockConcentration>(7),
			SetBonusKey = "Mods.ArknightsMod.ArmorSets.Mizuki.SetBonus",
		};
	}
}
