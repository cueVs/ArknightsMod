using ArknightsMod.Content.Items.Armor.NeoArmorReforge;
using ArknightsMod.Content.Items.Material;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Armor.Caster.Mornia
{
	// 开发者时装，无任何获取途径（不参与「博士的档案袋」抽奖，也没有配方）。
	//
	// 说明：原本这里有一个 MorniaBodyLayer（PlayerDrawLayer），负责在角色背后额外
	// 画一条尾巴（MorniaBody_Tail.png）。那条尾巴属于早期的占位美术，和现在导入的
	// 这套真实时装是两套完全不同的设计（配色/轮廓都不一致），继续画上去只会是残留
	// 瑕疵，因此该图层已移除。MorniaBody_Tail.png 仍保留在目录里，将来若这套时装
	// 补了配套的尾巴/背饰美术，把图层加回来即可。
	[AutoloadEquip(EquipType.Body)]
	public class MorniaBody : NeoArmorReforgeVanityBody
	{
		public override int Rarity => 4;

		public override NeoArmorReforgeSetProfile SetProfile => new() {
			Defense = 14,
			LifeBonus = 70,
			LocalizationPrefix = "Mods.ArknightsMod.ArmorSets.Mornia",
			Materials = recipe => recipe
				.AddIngredient<Orundum>(40)
				.AddIngredient<ManganeseTrihydrate>(3)
				.AddIngredient<IntegratedDevice>(1),
		};
	}
}
