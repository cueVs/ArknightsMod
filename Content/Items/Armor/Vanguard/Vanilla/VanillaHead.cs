using Terraria.ModLoader;
using ArknightsMod.Content.Items.Armor.NeoArmorReforge;
using ArknightsMod.Content.Items.Material;

namespace ArknightsMod.Content.Items.Armor.Vanguard.Vanilla
{
	[AutoloadEquip(EquipType.Head)]
	public class VanillaHead : NeoArmorReforgeVanityHead
	{
		public override int Rarity => 3;

		public override NeoArmorReforgeSetProfile SetProfile => new() {
			Defense = 0,
			LifeBonus = 127,
			LocalizationPrefix = "Mods.ArknightsMod.ArmorSets.Vanilla",
			Materials = recipe => recipe
				.AddIngredient<Orundum>(30)
				.AddIngredient<SugarSubstitute>(1),
			SetBonusKey = "Mods.ArknightsMod.ArmorSets.Vanilla.SetBonus",
		};
	}
}
