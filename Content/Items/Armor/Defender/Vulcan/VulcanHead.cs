using Terraria.ModLoader;
using ArknightsMod.Content.Items.Armor.NeoArmorReforge;
using ArknightsMod.Content.Items.Material;

namespace ArknightsMod.Content.Items.Armor.Defender.Vulcan
{
	[AutoloadEquip(EquipType.Head)]
	public class VulcanHead : NeoArmorReforgeVanityHead
	{
		public override int Rarity => 5;

		public override NeoArmorReforgeSetProfile SetProfile => new() {
			Defense = 0,
			LifeBonus = 409,
			LocalizationPrefix = "Mods.ArknightsMod.ArmorSets.Vulcan",
			Materials = recipe => recipe
				.AddIngredient<Orundum>(50)
				.AddIngredient<OrirockConcentration>(8)
				.AddIngredient<Aketon>(15),
			SetBonusKey = "Mods.ArknightsMod.ArmorSets.Vulcan.SetBonus",
		};
	}
}
