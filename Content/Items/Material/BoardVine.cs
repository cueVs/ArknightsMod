using ArknightsMod.Common;
using ArknightsMod.Players;
using Terraria;

namespace ArknightsMod.Content.Items.Material
{
	public class BoardVine : RareCollectibleItem
	{
		public override int BaseOriginiumIngotValue => BoardVinePlayer.BaseValue;

		public override void UpdateInventory(Player player) {
			int otherCount = RareCollectibleInventoryHelper.CountOtherTypesInInventory(player, Item.type);
			Item.value = 8 + otherCount * 4;
		}
	}
}
