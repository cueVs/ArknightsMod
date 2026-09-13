using Terraria.ModLoader;
using ArknightsMod.Content.Items.Armor.NeoArmorReforge;
using ArknightsMod.Content.Items.Material;

namespace ArknightsMod.Content.Items.Armor.Sniper.W
{
	[AutoloadEquip(EquipType.Head)]
	public class WHead : NeoArmorReforgeVanityHead
	{
		public override int Rarity => 6;

		// 迁移补记：W 三件在旧 NeoArmor 系统里从来没有写过 AddRecipes，套装升不出来，
		// WSetPlayer 里的效果（眩晕增伤、17% 闪避、仇恨降低）一直够不到，尽管
		// ArmorSets.hjson 四条文案都写全了。这里按电弧的先例补齐配方，材料参照
		// 同为六星的能天使/远牙（源石 ×60 + 两种高级材料）。

		public override NeoArmorReforgeSetProfile SetProfile => new() {
			Defense = 0,
			LifeBonus = 161,
			LocalizationPrefix = "Mods.ArknightsMod.ArmorSets.W",
			Materials = recipe => recipe
				.AddIngredient<Orundum>(60)
				.AddIngredient<BipolarNanoflake>(6)
				.AddIngredient<PolymerizedGel>(5),
			SetBonusKey = "Mods.ArknightsMod.ArmorSets.W.SetBonus",
		};
	}
}
