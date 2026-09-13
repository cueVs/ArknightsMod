using ArknightsMod.Common;
using ArknightsMod.Content.Tiles.Natural;
using ArknightsMod.Systems;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Players
{
	public class BoardVinePlayer : ModPlayer
	{
		public const int BaseValue = 8;
		private const int PerOtherItemBonus = 4;

		private const int GrowthCheckInterval = 60; // 约1秒判定一次
		private const float GrowthRollChance = 0.001f;
		private const int GrowthRadius = 30;

		public int Value = BaseValue;

		private int growthTimer;

		// 板藤只在背包里已经有别的自然物时才会尝试刷新，所以不走通用的 IsAmbient 环境刷新，单独判定。
		public override void PostUpdate() {
			if (Main.netMode == NetmodeID.MultiplayerClient) {
				growthTimer = 0;
				return;
			}

			growthTimer++;
			if (growthTimer < GrowthCheckInterval) return;
			growthTimer = 0;

			int boardVineType = ModContent.ItemType<Content.Items.Material.BoardVine>();
			if (RareCollectibleInventoryHelper.CountOtherTypesInInventory(Player, boardVineType) <= 0) return;

			if (Main.rand.NextFloat() < GrowthRollChance)
				NaturalGrowthSystem.TryGrowRandomNear(Player, GrowthRadius, ModContent.TileType<BoardVine>());
		}
	}
}
