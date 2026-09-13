using ArknightsMod.Players;
using Terraria;

namespace ArknightsMod.Content.Items.Material
{
	// 背包里只要有它就持续生效，对话计数/估价升降都在 EchoCornPlayer 里。
	public class EchoCorn : RareCollectibleItem
	{
		public override int BaseOriginiumIngotValue => EchoCornPlayer.BaseValue;

		public override void UpdateInventory(Player player) {
			var echoPlayer = player.GetModPlayer<EchoCornPlayer>();
			echoPlayer.HasEchoCorn = true;
			Item.value = echoPlayer.Value;
		}
	}
}
