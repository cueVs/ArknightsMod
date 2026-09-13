using System;
using Terraria.ID;

namespace ArknightsMod.Content.Items.Armor.NeoArmorReforge
{
	// 干员星级 → 物品稀有度的统一换算。新旧两套系统共用同一份实现
	// （旧的 NeoArmorItem.GetRarity 委托到这里），避免两处各写一份导致数值走漏。
	public static class NeoArmorReforgeRarity
	{
		public static int Get(int rarity) {
			rarity = Math.Clamp(rarity, 1, 6);
			return rarity switch {
				1 => ItemRarityID.White,
				2 => ItemRarityID.White,
				3 => ItemRarityID.White,
				4 => ItemRarityID.Blue,
				5 => ItemRarityID.Orange,
				6 => ItemRarityID.Quest,
				_ => ItemRarityID.Quest
			};
		}
	}
}
