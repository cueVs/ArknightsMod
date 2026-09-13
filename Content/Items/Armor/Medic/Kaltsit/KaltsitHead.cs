using Terraria.ModLoader;
using ArknightsMod.Content.Items.Armor.NeoArmorReforge;
using ArknightsMod.Content.Items.Material;

namespace ArknightsMod.Content.Items.Armor.Medic.Kaltsit
{
	[AutoloadEquip(EquipType.Head)]
	public class KaltsitHead : NeoArmorReforgeVanityHead
	{
		public override int Rarity => 6;

		// 套装效果——召唤 M3(Mon3tr) 伴随战斗，见 KaltsitSetPlayer / Mon3trBuff / Mon3tr。
		// 效果本身是"生成一个会打会死会复活的召唤物"，不是简单的加数值，没法用
		// OnFullSetActive 一行 lambda 表达，所以这里不接 OnFullSetActive，全部逻辑
		// 放在 KaltsitSetPlayer.PostUpdateEquips 里（文档第 6 节写法 C）。
		public override NeoArmorReforgeSetProfile SetProfile => new() {
			Defense = 0,
			LifeBonus = 204,
			LocalizationPrefix = "Mods.ArknightsMod.ArmorSets.Kaltsit",
			Materials = recipe => recipe
				.AddIngredient<Orundum>(60)
				.AddIngredient<PolymerizationPreparation>(6)
				.AddIngredient<OrironBlock>(5),
			SetBonusKey = "Mods.ArknightsMod.ArmorSets.Kaltsit.SetBonus",
		};
	}
}
