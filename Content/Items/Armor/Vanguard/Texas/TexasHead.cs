using Terraria.ModLoader;
using ArknightsMod.Content.Items.Armor.NeoArmorReforge;
using ArknightsMod.Content.Items.Material;

namespace ArknightsMod.Content.Items.Armor.Vanguard.Texas
{
	[AutoloadEquip(EquipType.Head)]
	public class TexasHead : NeoArmorReforgeVanityHead
	{
		public override int Rarity => 5;

		public override NeoArmorReforgeSetProfile SetProfile => new() {
			Defense = 0,
			LifeBonus = 195,
			LocalizationPrefix = "Mods.ArknightsMod.ArmorSets.Texas",
			Materials = recipe => recipe
				.AddIngredient<Orundum>(50)
				.AddIngredient<PolyesterLump>(8)
				.AddIngredient<OrirockCluster>(16),
			SetBonusKey = "Mods.ArknightsMod.ArmorSets.Texas.SetBonus",
		};
	}
}
