using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using ArknightsMod.Common.Items;
namespace ArknightsMod.Content.Items.Accessories.Rogue.Rarity_l3
{
	public class SarkazKingTornBanner : ModItem
	{
		public override void SetDefaults() {
			Item.width = 30;
			Item.height = 30;
			Item.accessory = true;
			Item.rare = ItemRarityID.Purple;
			Item.value = Item.sellPrice(0, 6, 0, 0);
			Item.GetGlobalItem<SarkazKingGlobalItem>().isSarkazKing = true;
		}

		public override void UpdateAccessory(Player player, bool hideVisual) {
			player.GetModPlayer<SarkazKingTornBannerPlayer>().effectActive = true;
		}
	}

	public class SarkazKingTornBannerPlayer : ModPlayer
	{
		public bool effectActive;

		public override void ResetEffects() {
			effectActive = false;
		}

		public override void PostUpdateEquips() {
			// 判断生命值是否大于 85%
			if (effectActive && Player.statLife >= Player.statLifeMax2 * 0.85f) {
				Player.GetAttackSpeed(DamageClass.Generic) += 0.3f;
			}
		}
	}
}