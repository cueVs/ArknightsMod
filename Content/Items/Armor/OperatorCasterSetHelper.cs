using ArknightsMod.Content.Items.Armor.Caster.Amiya;
using ArknightsMod.Content.Items.Armor.Caster.Haze;
using ArknightsMod.Content.Items.Armor.Caster.Indigo;
using ArknightsMod.Content.Items.Armor.Caster.Lava;
using ArknightsMod.Content.Items.Armor.Caster.Mostima;
using ArknightsMod.Content.Items.Armor.Caster.Steward;
using ArknightsMod.Content.Items.Armor.NeoArmorReforge;
using Terraria;

namespace ArknightsMod.Content.Items.Armor
{
	internal static class OperatorCasterSetHelper
	{
		// NeoArmor Reforge：Amiya/Haze/Indigo/Lava/Mostima/Steward 六位迁到新系统后，
		// 套装件是独立 ItemID，不再需要 hasUpgraded 判断，改用 GetSetType 查询。
		// Necrass 不在这个列表里（旧系统里它就没接入这个跨干员判定，迁移时保持原样）。
		public static bool WearsFullCasterSet(Player player) {
			return HasSet(player, NeoArmorReforgeSetLoader.GetSetType<HazeHead>(), NeoArmorReforgeSetLoader.GetSetType<HazeBody>(), NeoArmorReforgeSetLoader.GetSetType<HazeLegs>())
				|| HasSet(player, NeoArmorReforgeSetLoader.GetSetType<AmiyaHead>(), NeoArmorReforgeSetLoader.GetSetType<AmiyaBody>(), NeoArmorReforgeSetLoader.GetSetType<AmiyaLegs>())
				|| HasSet(player, NeoArmorReforgeSetLoader.GetSetType<IndigoHead>(), NeoArmorReforgeSetLoader.GetSetType<IndigoBody>(), NeoArmorReforgeSetLoader.GetSetType<IndigoLegs>())
				|| HasSet(player, NeoArmorReforgeSetLoader.GetSetType<LavaHead>(), NeoArmorReforgeSetLoader.GetSetType<LavaBody>(), NeoArmorReforgeSetLoader.GetSetType<LavaLegs>())
				|| HasSet(player, NeoArmorReforgeSetLoader.GetSetType<MostimaHead>(), NeoArmorReforgeSetLoader.GetSetType<MostimaBody>(), NeoArmorReforgeSetLoader.GetSetType<MostimaLegs>())
				|| HasSet(player, NeoArmorReforgeSetLoader.GetSetType<StewardHead>(), NeoArmorReforgeSetLoader.GetSetType<StewardBody>(), NeoArmorReforgeSetLoader.GetSetType<StewardLegs>());
		}

		private static bool HasSet(Player player, int helmet, int chest, int greaves) {
			return player.armor[0].type == helmet && player.armor[1].type == chest && player.armor[2].type == greaves;
		}
	}
}
