using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using ArknightsMod.Common.Items;
using System;

namespace ArknightsMod.Content.Items.Accessories.Rogue.Rarity_l4
{
	public class KingOfSarkazVessel : ModItem
	{
		public override void SetStaticDefaults() {
			ItemID.Sets.ItemNoGravity[Item.type] = true;
		}

		public override void SetDefaults() {
			Item.width = 30;
			Item.height = 30;
			Item.value = Item.sellPrice(0, 16, 0, 0);
			Item.rare = ItemRarityID.Purple;
			Item.accessory = true;
			Item.GetGlobalItem<SarkazKingGlobalItem>().isSarkazKing = true;
		}

		private int CountSarkazKingItems(Player player) {
			int count = 0;
			int startIndex = 3;
			int endIndex = 8 + player.extraAccessorySlots;

			for (int i = startIndex; i < endIndex; i++) {
				if (i < player.armor.Length) {
					Item accessory = player.armor[i];
					if (!accessory.IsAir &&
						accessory.TryGetGlobalItem(out SarkazKingGlobalItem sarkazKing) &&
						sarkazKing.isSarkazKing) {
						count++;
					}
				}
			}
			return count;
		}

		public override void UpdateAccessory(Player player, bool hideVisual) {
			var modPlayer = player.GetModPlayer<KingOfSarkazVesselPlayer>();
			modPlayer.effectActive = true;

			int sarkazKingCount = CountSarkazKingItems(player);
			modPlayer.effectLevel = sarkazKingCount >= 3 ? 2 : 1;

			// 直接在这里应用生命值加成
			float healthBonus = sarkazKingCount >= 3 ? 1.2f : 0.4f;
			player.statLifeMax2 += (int)(player.statLifeMax2 * healthBonus);
		}
	}

	public class KingOfSarkazVesselPlayer : ModPlayer
	{
		public bool effectActive;
		public int effectLevel;

		// 恢复计时器
		private int regenTimer = 0;
		private const int REGEN_INTERVAL = 60; // 60帧 = 1秒

		private float RegenBase => effectLevel == 2 ? 9f : 3f;
		private float RegenMax => effectLevel == 2 ? 36f : 12f;

		public override void ResetEffects() {
			effectActive = false;
			effectLevel = 0;
		}

		public override void PostUpdate() {
			if (!effectActive || effectLevel == 0)
				return;

			float healthPercentage = (float)Player.statLife / Player.statLifeMax2;
			bool isAbove85 = healthPercentage >= 0.85f;

			if (isAbove85 && Player.statLife < Player.statLifeMax2) {
				// 线性插值：血量越接近100%，恢复越高
				float t = (healthPercentage - 0.85f) / 0.15f;
				t = Math.Clamp(t, 0f, 1f);

				// 计算每秒恢复量
				float regenPerSecond = RegenBase + (RegenMax - RegenBase) * t;

				// 转换为每帧恢复量（每秒60帧）
				float regenPerFrame = regenPerSecond / 60f;

				// 累加计时器
				regenTimer++;

				// 当累计恢复值达到1点时回血
				if (regenTimer >= 60 / regenPerSecond) {
					Player.statLife++;
					if (Player.statLife > Player.statLifeMax2)
						Player.statLife = Player.statLifeMax2;
					regenTimer = 0;
				}
			}
			else {
				// 不满足条件时重置计时器
				regenTimer = 0;
			}
		}

		public override void UpdateDead() {
			regenTimer = 0;
		}
	}
}