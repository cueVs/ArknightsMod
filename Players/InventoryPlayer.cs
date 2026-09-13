using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ModLoader;

namespace ArknightsMod.Players
{
	public class InventoryPlayer : ModPlayer
	{
		private int justPickupOriginiumIngot;
		internal int JustPickupOriginiumIngot { get => (int)MathHelper.Clamp(justPickupOriginiumIngot, 0, 20); set => justPickupOriginiumIngot = (int)MathHelper.Clamp(value, 0, 20); } // 刚刚拾起且未显示拾取效果的源石锭数量
		internal List<int> PickupOriginiumIngotEffectCount = []; // 源石锭拾取效果剩余时间计数列表
		internal const int MaxPickupOriginiumIngotEffectCount = 5; // 最大同时显示源石锭拾取效果数

		// AddStartingItems is a method you can use to add items to the player's starting inventory.
		// It is also called when the player dies a mediumcore death
		// Return an enumerable with the items you want to add to the inventory.
		// This method adds an ExampleItem and 256 gold ore to the player's inventory.
		//
		// If you know what 'yield return' is, you can also use that here, if you prefer so.
		public override IEnumerable<Item> AddStartingItems(bool mediumCoreDeath) {
			return mediumCoreDeath
				?
				[
					new Item(ModContent.ItemType<Content.Items.Consumables.StartBag>()),
					new Item(ModContent.ItemType<Content.Items.Placeable.Furniture.AnniversaryWheel>())
				]
				:
				[
					new Item(ModContent.ItemType<Content.Items.Consumables.StartBag>()),
					new Item(ModContent.ItemType<Content.Items.Placeable.Furniture.AnniversaryWheel>())
				];
		}

		public override void ResetEffects() {
			List<int> counts = PickupOriginiumIngotEffectCount;

			if (Player.miscCounter % 6 == 0 && JustPickupOriginiumIngot > 0) { // 如果未显示拾取效果的源石锭数量大于0
				if (counts.Count < MaxPickupOriginiumIngotEffectCount) { // 如果效果数小于最大显示数
					counts.Add(25); // 添加新的效果计数
					JustPickupOriginiumIngot--;
				}
				else {
					for (int i = 0; i < counts.Count; i++) {
						if (counts[i] == counts.Min()) { // 否则寻找剩余时间最少的计数
							counts[i] = 25; // 重置计数
							JustPickupOriginiumIngot--;
							break;
						}
					}
				}
			}

			// 更新效果计数列表
			List<int> needRemove = [];
			for (int i = 0; i < counts.Count; i++) {
				if (--counts[i] <= 0) { // 记录需要移除的索引
					needRemove.Add(i);
					break;
				}
			}

			for (int i = needRemove.Count - 1; i >= 0; i--) {
				counts.RemoveAt(needRemove[i]); // 移除记录的索引
			}
		}
	}
}