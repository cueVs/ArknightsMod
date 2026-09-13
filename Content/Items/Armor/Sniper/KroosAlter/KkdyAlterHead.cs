using Terraria.ModLoader;
using ArknightsMod.Content.Items.Armor.NeoArmorReforge;
using ArknightsMod.Content.Items.Material;

namespace ArknightsMod.Content.Items.Armor.Sniper.KroosAlter
{
	[AutoloadEquip(EquipType.Head)]
	public class KkdyAlterHead : NeoArmorReforgeVanityHead
	{
		public override int Rarity => 5;

		public override NeoArmorReforgeSetProfile SetProfile => new() {
			Defense = 0,
			LifeBonus = 125,
			LocalizationPrefix = "Mods.ArknightsMod.ArmorSets.KkdyAlter",
			Materials = recipe => recipe
				.AddIngredient<Orundum>(50)
				.AddIngredient<CrystallineCircuit>(7)
				.AddIngredient<OrironCluster>(10),
			SetBonusKey = "Mods.ArknightsMod.ArmorSets.KkdyAlter.SetBonus",
		};
	}
}
