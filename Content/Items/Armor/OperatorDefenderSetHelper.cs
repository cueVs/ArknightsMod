using ArknightsMod.Content.Items.Armor.NeoArmorReforge;
using ArknightsMod.Content.Items.Armor.Defender.Beagle;
using ArknightsMod.Content.Items.Armor.Defender.Cardigan;
using ArknightsMod.Content.Items.Armor.Defender.Mudrock;
using ArknightsMod.Content.Items.Armor.Defender.Nian;
using ArknightsMod.Content.Items.Armor.Defender.Saria;
using ArknightsMod.Content.Items.Armor.Defender.Spot;
using ArknightsMod.Content.Items.Armor.Defender.Vulcan;
using Terraria;

namespace ArknightsMod.Content.Items.Armor
{
	internal static class OperatorDefenderSetHelper
	{
		// NeoArmor Reforge：重装职业已整体迁到新系统（泥岩在首批，米格鲁/卡缇/年/塞雷娅/
		// 斑点/火神在第三批），套装件都是独立 ItemID，不再需要 hasUpgraded 判断，
		// 统一用 GetSetType 查询，泥岩不再需要单独开一个分支。
		public static bool WearsFullDefenderSet(Player player) {
			return HasSet(player, NeoArmorReforgeSetLoader.GetSetType<BeagleHead>(), NeoArmorReforgeSetLoader.GetSetType<BeagleBody>(), NeoArmorReforgeSetLoader.GetSetType<BeagleLegs>())
				|| HasSet(player, NeoArmorReforgeSetLoader.GetSetType<CardiganHead>(), NeoArmorReforgeSetLoader.GetSetType<CardiganBody>(), NeoArmorReforgeSetLoader.GetSetType<CardiganLegs>())
				|| HasSet(player, NeoArmorReforgeSetLoader.GetSetType<SariaHead>(), NeoArmorReforgeSetLoader.GetSetType<SariaBody>(), NeoArmorReforgeSetLoader.GetSetType<SariaLegs>())
				|| HasSet(player, NeoArmorReforgeSetLoader.GetSetType<NianHead>(), NeoArmorReforgeSetLoader.GetSetType<NianBody>(), NeoArmorReforgeSetLoader.GetSetType<NianLegs>())
				|| HasSet(player, NeoArmorReforgeSetLoader.GetSetType<SpotHead>(), NeoArmorReforgeSetLoader.GetSetType<SpotBody>(), NeoArmorReforgeSetLoader.GetSetType<SpotLegs>())
				|| HasSet(player, NeoArmorReforgeSetLoader.GetSetType<VulcanHead>(), NeoArmorReforgeSetLoader.GetSetType<VulcanBody>(), NeoArmorReforgeSetLoader.GetSetType<VulcanLegs>())
				|| HasSet(player, NeoArmorReforgeSetLoader.GetSetType<MudrockHead>(), NeoArmorReforgeSetLoader.GetSetType<MudrockBody>(), NeoArmorReforgeSetLoader.GetSetType<MudrockLegs>());
		}

		private static bool HasSet(Player player, int helmet, int chest, int greaves) {
			return player.armor[0].type == helmet && player.armor[1].type == chest && player.armor[2].type == greaves;
		}
	}
}
