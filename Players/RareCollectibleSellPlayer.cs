using ArknightsMod.Content.Items;
using ArknightsMod.Content.Items.Material;
using ArknightsMod.Content.NPCs.Friendly;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Players
{
	// tModLoader 的自定义货币原生只接管购买；Player.SellItem 仍固定发放原版钱币。
	// 因此自然采集物的出售必须在交易发生前接管，才能真正以源石锭结算。
	public class RareCollectibleSellPlayer : ModPlayer
	{
		public override bool CanSellItem(NPC vendor, Item[] shopInventory, Item item) {
			if (item.ModItem is not RareCollectibleItem)
				return true;

			// 自然采集物只收给坎诺特，不能从其他 NPC 处换到原版钱币。
			if (vendor.ModNPC is not Cannot)
				return false;

			int ingotCount = System.Math.Max(0, item.value) * item.stack;
			if (ingotCount <= 0)
				return false;

			Player.QuickSpawnItem(Player.GetSource_Misc("CannotRareCollectibleSale"),
				ModContent.ItemType<OriginiumIngot>(), ingotCount);
			item.TurnToAir();
			SoundEngine.PlaySound(SoundID.Coins);

			// 已经完成交易；阻止原版 SellItem 再发放钱币或重复移除物品。
			return false;
		}
	}
}
