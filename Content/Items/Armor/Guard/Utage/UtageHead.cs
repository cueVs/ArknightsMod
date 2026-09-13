using Terraria.ModLoader;
using ArknightsMod.Content.Items.Armor.NeoArmorReforge;
using ArknightsMod.Content.Items.Material;

namespace ArknightsMod.Content.Items.Armor.Guard.Utage
{
	[AutoloadEquip(EquipType.Head)]
	public class UtageHead : NeoArmorReforgeVanityHead
	{
		public override int Rarity => 4;

		// 迁移补记：Utage 三件在旧 NeoArmor 系统里从来没有写过 AddRecipes，套装升不出来，
		// 套装效果一直够不到，尽管 ArmorSets.hjson 四条文案都写全了。这里按电弧/W 的
		// 先例补齐配方，材料参照同星级的其它干员。

		public override NeoArmorReforgeSetProfile SetProfile => new() {
			Defense = 0,
			LifeBonus = 197,
			LocalizationPrefix = "Mods.ArknightsMod.ArmorSets.Utage",
			Materials = recipe => recipe
				.AddIngredient<Orundum>(40)
				.AddIngredient<OptimizedDevice>(2)
				.AddIngredient<SugarPack>(8),
			SetBonusKey = "Mods.ArknightsMod.ArmorSets.Utage.SetBonus",
		};
	}
}
