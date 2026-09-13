using Terraria;
using Terraria.Localization;

namespace ArknightsMod.Content.Items.Armor
{
	// 干员套装装备判定：统一基于 NeoArmor 件（Head/Body/Legs）判定。
	// 迁移说明：原先套装效果绑定在已废弃的 OperatorSet 盔甲件（Helmet/Chestplate/Greaves）上，
	// 现已改为判定 NeoArmor 件且处于「已升级为盔甲」（hasUpgraded）形态——即玩家把 NeoArmor
	// 时装升级成盔甲并穿在对应盔甲栏时，套装效果才生效（与原 OperatorSet 盔甲栏体验一致）。
	// 参数名沿用旧签名以最小化调用点改动，但传入的应是对应干员的 NeoArmor 件（Head/Body/Legs）类型。
	internal static class OperatorSetEquipHelper
	{
		// 头部件：作为已升级盔甲穿在头部盔甲栏（armor[0]）。
		public static bool HasHelmet(Player player, int headItemType) {
			return player.armor[0].type == headItemType && player.armor[0].neoarmor().hasUpgraded;
		}

		// 整套：头/身/腿三件 NeoArmor 均作为已升级盔甲穿在对应盔甲栏。
		public static bool HasFullSet(Player player, int headItemType, int bodyItemType, int legsItemType) {
			return player.armor[0].type == headItemType && player.armor[0].neoarmor().hasUpgraded
				&& player.armor[1].type == bodyItemType && player.armor[1].neoarmor().hasUpgraded
				&& player.armor[2].type == legsItemType && player.armor[2].neoarmor().hasUpgraded;
		}

		public static void ApplySetBonusText(Player player, bool fullSetActive, string setBonusKey) {
			if (fullSetActive)
				player.setBonus = Language.GetTextValue(setBonusKey);
		}
	}
}
