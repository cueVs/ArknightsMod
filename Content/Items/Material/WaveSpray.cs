using ArknightsMod.Players;
using Terraria;

namespace ArknightsMod.Content.Items.Material
{
	public class WaveSpray : RareCollectibleItem
	{
		public override int BaseOriginiumIngotValue => WaveSprayPlayer.BaseValue;

		public override void UpdateInventory(Player player) {
			var wavePlayer = player.GetModPlayer<WaveSprayPlayer>();
			wavePlayer.HasWaveSpray = true;
			Item.value = wavePlayer.Value;
		}
	}
}
