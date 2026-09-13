using Terraria.ModLoader;
using ArknightsMod.Content.Items.Armor.NeoArmorReforge;
using ArknightsMod.Content.Items.Material;

namespace ArknightsMod.Content.Items.Armor.Defender.Beagle
{
	[AutoloadEquip(EquipType.Head)]
	public class BeagleHead : NeoArmorReforgeVanityHead
	{
		public override int Rarity => 3;

		public override int Value => 560000;

		public override NeoArmorReforgeSetProfile SetProfile => new() {
			Defense = 0,
			LifeBonus = 115,
			LocalizationPrefix = "Mods.ArknightsMod.ArmorSets.Beagle",
			Materials = recipe => recipe
				.AddIngredient<Orundum>(30)
				.AddIngredient<Diketon>(1),
			SetBonusKey = "Mods.ArknightsMod.ArmorSets.Beagle.SetBonus",
		};
	}
}
