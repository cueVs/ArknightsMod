using ArknightsMod.Content.Items.Armor.NeoArmorReforge;
using ArknightsMod.Content.Items.Armor.Vanguard.Bagpipe;
using ArknightsMod.Content.Items.Armor.Vanguard.Fang;
using ArknightsMod.Content.Items.Armor.Vanguard.Plume;
using ArknightsMod.Content.Items.Armor.Vanguard.Texas;
using ArknightsMod.Content.Items.Armor.Vanguard.Vanilla;
using Terraria;

namespace ArknightsMod.Content.Items.Armor
{
	internal static class OperatorVanguardSetHelper
	{
		// NeoArmor Reforge：五位先锋（风笛/芬/翎羽/德克萨斯/香草）迁到新系统后，
		// 套装件是独立 ItemID，不再需要 hasUpgraded 判断，改用 GetSetType 查询。
		public static bool WearsFullVanguardSet(Player player) {
			return HasSet(player, NeoArmorReforgeSetLoader.GetSetType<BagpipeHead>(), NeoArmorReforgeSetLoader.GetSetType<BagpipeBody>(), NeoArmorReforgeSetLoader.GetSetType<BagpipeLegs>())
				|| HasSet(player, NeoArmorReforgeSetLoader.GetSetType<FangHead>(), NeoArmorReforgeSetLoader.GetSetType<FangBody>(), NeoArmorReforgeSetLoader.GetSetType<FangLegs>())
				|| HasSet(player, NeoArmorReforgeSetLoader.GetSetType<PlumeHead>(), NeoArmorReforgeSetLoader.GetSetType<PlumeBody>(), NeoArmorReforgeSetLoader.GetSetType<PlumeLegs>())
				|| HasSet(player, NeoArmorReforgeSetLoader.GetSetType<TexasHead>(), NeoArmorReforgeSetLoader.GetSetType<TexasBody>(), NeoArmorReforgeSetLoader.GetSetType<TexasLegs>())
				|| HasSet(player, NeoArmorReforgeSetLoader.GetSetType<VanillaHead>(), NeoArmorReforgeSetLoader.GetSetType<VanillaBody>(), NeoArmorReforgeSetLoader.GetSetType<VanillaLegs>());
		}

		private static bool HasSet(Player player, int helmet, int chest, int greaves) {
			return player.armor[0].type == helmet && player.armor[1].type == chest && player.armor[2].type == greaves;
		}
	}
}
