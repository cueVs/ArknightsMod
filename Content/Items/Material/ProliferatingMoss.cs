using ArknightsMod.Common;
using Terraria;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Material
{
	// 拿到手就立刻送 3 个枯苔藓球（OnPickup，只在从世界拾取时触发一次，合成/交易得到的不会重复触发）。
	public class ProliferatingMoss : RareCollectibleItem
	{
		public override int BaseOriginiumIngotValue => 1;

		public override void UpdateInventory(Player player) {
			int otherCount = RareCollectibleInventoryHelper.CountOtherTypesInInventory(player, Item.type);
			Item.value = 1 + otherCount;
		}

		public override bool OnPickup(Player player) {
			player.QuickSpawnItem(player.GetSource_Loot(), ModContent.ItemType<WitheredMossBall>(), 3);
			return true;
		}
	}
}
