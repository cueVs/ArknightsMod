using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using System.Linq;
using ArknightsMod.Content.Items;
using ArknightsMod.Content.Items.Accessories.Rogue;

using ArknightsMod.Content.Items.Accessories.Rogue.Rarity_l2;
using ArknightsMod.Content.Items.Accessories.Rogue.Rarity_l3;
using ArknightsMod.Content.Items.Accessories.Rogue.Rarity_l4;


namespace ArknightsMod.Players
{
	public class OriginiumIngotsAttackspeedPlayer : ModPlayer
	{
		public float totalAttackSpeedBonus = 0f;
		private const float MaxBonus = 6.00f;

		public override void ResetEffects() {
			totalAttackSpeedBonus = 0f;
		}

		public override void PostUpdateEquips() {
			float coinToyBonus = CalculateCoinToyBonus();
			float goldenGinChaliceBonus = CalculateGoldenGinChaliceBonus();
			float knightlyCodexBonus = CalculateKnightlyCodexBonus();

			totalAttackSpeedBonus = coinToyBonus + goldenGinChaliceBonus + knightlyCodexBonus;
			if (totalAttackSpeedBonus > MaxBonus) {
				totalAttackSpeedBonus = MaxBonus;
			}

			Player.GetAttackSpeed(DamageClass.Generic) += totalAttackSpeedBonus;
		}

		private float CalculateCoinToyBonus() {
			if (HasAccessory(ModContent.ItemType<CoinOperatedToy>())) {
				int totalOriginiumIngots = CountPlayerOriginiumIngots(Player);
				int effectiveIngots = (totalOriginiumIngots / 5) * 5;
				return (effectiveIngots / 5) * 0.03f;
			}
			return 0f;
		}

		private float CalculateGoldenGinChaliceBonus() {
			if (HasAccessory(ModContent.ItemType<GoldenGinChalice>())) {
				int totalOriginiumIngots = CountPlayerOriginiumIngots(Player);
				int effectiveIngots = (totalOriginiumIngots / 5) * 5;
				return (effectiveIngots / 5) * 0.07f;
			}
			return 0f;
		}

		private float CalculateKnightlyCodexBonus() {
			if (HasAccessory(ModContent.ItemType<KnightlyCodexRenewed>())) {
				int totalOriginiumIngots = CountPlayerOriginiumIngots(Player);
				int effectiveIngots = (totalOriginiumIngots / 5) * 5;
				return (effectiveIngots / 5) * 0.05f;
			}
			return 0f;
		}

		private bool HasAccessory(int itemType) {
			for (int i = 3; i < 8 + Player.extraAccessorySlots; i++) {
				if (i < Player.armor.Length) {
					Item accessory = Player.armor[i];
					if (!accessory.IsAir && accessory.type == itemType) {
						return true;
					}
				}
			}
			return false;
		}

		private int CountPlayerOriginiumIngots(Player player) {
			int totalIngots = 0;
			totalIngots += CountContainerOriginiumIngots(player.inventory);

			

			

			return totalIngots;
		}

		private int CountContainerOriginiumIngots(Item[] container) {
			int ingots = 0;
			int originiumIngotType = ModContent.ItemType<OriginiumIngot>();

			foreach (Item item in container) {
				if (item.type == originiumIngotType) {
					ingots += item.stack;
				}
			}

			return ingots;
		}
	}
}