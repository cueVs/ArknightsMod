using ArknightsMod.Content.Items.Armor;
using ArknightsMod.Content.Items.Armor.NeoArmorReforge;
using Terraria;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Armor.Medic.Ansel
{
	internal class AnselSetPlayer : ArknightsArmorPlayer
	{
		public bool AnselHelmetActive;
		public bool AnselSetActive;

		public override void ResetEffects() {
			AnselHelmetActive = false;
			AnselSetActive = false;
		}

		// NeoArmor Reforge：套装件是独立 ItemID，穿上它本身就代表"已经是套装形态"，
		// 不需要再查 hasUpgraded；player.setBonus 文本交给 AnselHead 的
		// SetProfile.SetBonusKey 统一设置，这里不再重复设置一遍。
		public override void PostUpdateEquips() {
			AnselHelmetActive = Player.armor[0].type == NeoArmorReforgeSetLoader.GetSetType<AnselHead>();
			AnselSetActive = AnselHelmetActive
				&& Player.armor[1].type == NeoArmorReforgeSetLoader.GetSetType<AnselBody>()
				&& Player.armor[2].type == NeoArmorReforgeSetLoader.GetSetType<AnselLegs>();
		}
	}
}
