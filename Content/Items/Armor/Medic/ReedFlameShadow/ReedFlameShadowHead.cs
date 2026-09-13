using Terraria.ModLoader;
using ArknightsMod.Content.Items.Armor.NeoArmorReforge;
using ArknightsMod.Content.Items.Material;

namespace ArknightsMod.Content.Items.Armor.Medic.ReedFlameShadow
{
	[AutoloadEquip(EquipType.Head)]
	public class ReedFlameShadowHead : NeoArmorReforgeVanityHead
	{
		public override int Rarity => 5;

		// 旧系统这里 IsArmorSet/UpdateArmorSet 只是把 player.setBonus 设成空字符串，
		// ArmorSets.hjson 里也从来没写过 ReedFlameShadow 的 HelmetEffect/SetEffect —— 套装效果
		// 始终是空的。SetBonusKey 留 null 就是这个"没有文本"的等价物。

		public override NeoArmorReforgeSetProfile SetProfile => new() {
			Defense = 0,
			LifeBonus = 166,
			LocalizationPrefix = "Mods.ArknightsMod.ArmorSets.ReedFlameShadow",
			Materials = recipe => recipe
				.AddIngredient<Orundum>(50)
				.AddIngredient<OrirockConcentration>(10)
				.AddIngredient<LoxicKohl>(10),
		};
	}
}
