using ArknightsMod.Content.Items.Armor.NeoArmorReforge;
using ArknightsMod.Content.Items.Material;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Armor.Supporter.Deepcolor
{
	// PRTS 官方数据（精二满级）：生命 1050 / 防御 125，由 OperatorArmorStatFormula 统一换算。
	// ⚠ 保留公式调用、不写死数字：换算比例是全模组共用的常量，将来调比例时写死的数字
	// 不会跟着变，会和其它按公式来的干员脱节。
	[AutoloadEquip(EquipType.Head)]
	public class DeepcolorHead : NeoArmorReforgeVanityHead
	{
		public override int Rarity => 4;

		public override NeoArmorReforgeSetProfile SetProfile => new() {
			Defense = OperatorArmorStatFormula.HeadDefenseBonus(125),
			LifeBonus = OperatorArmorStatFormula.HeadLifeBonus(1050),
			LocalizationPrefix = "Mods.ArknightsMod.ArmorSets.Deepcolor",
			Materials = recipe => recipe
				.AddIngredient<Orundum>(40)
				.AddIngredient<Device>(1)
				.AddIngredient<ManganeseOre>(9),
			// 头盔效果「自身获得 7% 物理与法术闪避」：旧代码在 UpdateArmorEquip 里给
			// DeepcolorSetPlayer 打标记，新系统改用 OnHelmetActive 回调（文档第 6 节写法 B）。
			// 旧代码手写的 ModifyArmorTooltips 不再需要——套装件的 HelmetEffect/SetEffect
			// tooltip 由 NeoArmorReforgeSetPiece 统一无条件显示。
			OnHelmetActive = DeepcolorSetPlayer.OnHelmetActive,
			SetBonusKey = "Mods.ArknightsMod.ArmorSets.Deepcolor.SetBonus",
		};
	}
}
