using ArknightsMod.Content.Items.Armor.NeoArmorReforge;
using ArknightsMod.Content.Items.Material;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Armor.Sniper.Wisadel
{
	[AutoloadEquip(EquipType.Head)]
	public class WisadelHead : NeoArmorReforgeVanityHead
	{
		public override int Rarity => 6;

		public override NeoArmorReforgeSetProfile SetProfile => new() {
			Defense = 0,
			LifeBonus = 189,
			LocalizationPrefix = "Mods.ArknightsMod.ArmorSets.Wisadel",
			Materials = recipe => recipe
				.AddIngredient<Orundum>(60)
				.AddIngredient<D32Steel>(6)
				.AddIngredient<PolymerizedGel>(6),
			// 头盔效果「魂灵之影环绕」：旧代码在 UpdateArmorEquip 里给 WisadelSetPlayer
			// 打标记，新系统改用 OnHelmetActive 回调（文档第 6 节写法 B）。
			// 旧代码手写的 ModifyArmorTooltips 不再需要——套装件的 HelmetEffect/SetEffect
			// tooltip 由 NeoArmorReforgeSetPiece 统一无条件显示。
			OnHelmetActive = WisadelSetPlayer.OnHelmetActive,
			SetBonusKey = "Mods.ArknightsMod.ArmorSets.Wisadel.SetBonus",
		};
	}
}
