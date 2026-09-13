using Terraria.ModLoader;
using ArknightsMod.Content.Items.Armor.NeoArmorReforge;

namespace ArknightsMod.Content.Items.Armor.Defender.Mudrock
{
	// 泥岩：带"手持右键切换头盔造型"的干员。只要声明下面这三个属性，切换能力
	// （右键触发、图标替换、穿戴贴图替换、提示文案）就由框架自动提供，时装和它
	// 对应的套装件（MudrockHeadSet）都会获得同样的切换能力。
	[AutoloadEquip(EquipType.Head)]
	internal class MudrockHead : NeoArmorReforgeVanityHead
	{
		public override int Rarity => 6;

		// 切换后：图标换成头盔图标，穿戴帧表换成头盔帧表（两张不同的图，都要换）。
		public override string AltIconTexture => "ArknightsMod/Content/Items/Armor/Defender/Mudrock/MudrockHelmet";
		internal override string AltEquipTexture => "ArknightsMod/Content/Items/Armor/Defender/Mudrock/MudrockHelmet_Head";
		protected override string ToggleHintKey => "Mods.ArknightsMod.ArmorSets.Mudrock.ToggleHint";

		// 旧版本从来没给泥岩写过 AddRecipes，也就是说泥岩套装此前在正常游玩中根本
		// 合成不出来，只能靠 debug 配置或抽卡直接拿到已升级状态。这里保留"不需要
		// 额外材料"（原本就没有材料数据可参考），顺手把这个缺口补成真正可合成。
		public override NeoArmorReforgeSetProfile SetProfile => new() {
			Defense = 0,
			LifeBonus = 443,
			LocalizationPrefix = "Mods.ArknightsMod.ArmorSets.Mudrock",
			SetBonusKey = "Mods.ArknightsMod.ArmorSets.Mudrock.SetBonus",
		};
	}
}
