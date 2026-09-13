using System.Collections.Generic;
using ArknightsMod.Content.Items.Material;
using Terraria;

namespace ArknightsMod.Common
{
	// 统计背包里“除了自己以外，还有几种别的稀有自然物”，供多生苔藓/板藤这类跟其它自然物联动估价的物品复用。
	public static class RareCollectibleInventoryHelper
	{
		public static int CountOtherTypesInInventory(Player player, int excludeItemType) {
			HashSet<int> seen = [];

			for (int i = 0; i < player.inventory.Length; i++) {
				Item item = player.inventory[i];
				if (item == null || item.IsAir || item.type == excludeItemType) continue;
				if (item.ModItem is RareCollectibleItem)
					seen.Add(item.type);
			}
			return seen.Count;
		}
	}
}
