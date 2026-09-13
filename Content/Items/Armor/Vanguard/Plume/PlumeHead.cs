using Terraria.ModLoader;
using ArknightsMod.Content.Items.Armor.NeoArmorReforge;
using ArknightsMod.Content.Items.Material;

namespace ArknightsMod.Content.Items.Armor.Vanguard.Plume
{
	[AutoloadEquip(EquipType.Head)]
	public class PlumeHead : NeoArmorReforgeVanityHead
	{
		public override int Rarity => 3;

		public override NeoArmorReforgeSetProfile SetProfile => new() {
			Defense = 0,
			LifeBonus = 123,
			LocalizationPrefix = "Mods.ArknightsMod.ArmorSets.Plume",
			Materials = recipe => recipe
				.AddIngredient<Orundum>(30)
				.AddIngredient<DamagedDevice>(1),
			SetBonusKey = "Mods.ArknightsMod.ArmorSets.Plume.SetBonus",
		};
	}
}
