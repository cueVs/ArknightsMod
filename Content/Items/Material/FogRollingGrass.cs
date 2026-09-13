using ArknightsMod.Players;
using Terraria;

namespace ArknightsMod.Content.Items.Material
{
	// 雾滚草是“捕捉型”自然物：世界里飘的是同名的 Critter NPC，用捕虫网抓到后掉落这个物品。
	public class FogRollingGrass : RareCollectibleItem
	{
		public override int BaseOriginiumIngotValue => FogRollingGrassPlayer.BaseValue;

		public override void UpdateInventory(Player player) {
			var fogPlayer = player.GetModPlayer<FogRollingGrassPlayer>();
			fogPlayer.HasFogRollingGrass = true;
			Item.value = fogPlayer.Value;
		}
	}
}
