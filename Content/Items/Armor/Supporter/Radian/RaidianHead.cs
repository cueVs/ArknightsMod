using ArknightsMod.Content.Items.Armor.NeoArmorReforge;
using ArknightsMod.Content.Items.Material;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Armor.Supporter.Radian
{
	[AutoloadEquip(EquipType.Head)]
	public class RaidianHead : NeoArmorReforgeVanityHead
	{
		public override int Rarity => 6;

		// 迁移补记：电弧三件在旧 NeoArmor 系统里从来没有写过 AddRecipes，套装升不出来，
		// RaidianSetPlayer 里的效果一直够不到。这里补齐配方，材料参照同为六星的
		// 凌/多萝西（源石 ×60 + 两种高级材料），并按"电弧=电"的主题选了晶体类材料。
		public override NeoArmorReforgeSetProfile SetProfile => new() {
			Defense = 0,
			LifeBonus = 137,
			LocalizationPrefix = "Mods.ArknightsMod.ArmorSets.Raidian",
			Materials = recipe => recipe
				.AddIngredient<Orundum>(60)
				.AddIngredient<CrystallineElectronicUnit>(6)
				.AddIngredient<CrystallineCircuit>(5),
			SetBonusKey = "Mods.ArknightsMod.ArmorSets.Raidian.SetBonus",
		};
	}
}
