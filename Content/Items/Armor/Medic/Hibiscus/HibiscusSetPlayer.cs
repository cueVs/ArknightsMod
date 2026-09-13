using ArknightsMod.Content.Items.Armor;
using ArknightsMod.Content.Items.Armor.NeoArmorReforge;
using Terraria;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Armor.Medic.Hibiscus
{
	internal class HibiscusSetPlayer : ArknightsArmorPlayer
	{
		public bool HibiscusHelmetActive;
		public bool HibiscusSetActive;

		public override void ResetEffects() {
			HibiscusHelmetActive = false;
			HibiscusSetActive = false;
		}

		// NeoArmor Reforge：套装件是独立 ItemID，穿上它本身就代表"已经是套装形态"，
		// 不需要再查 hasUpgraded；player.setBonus 文本交给 HibiscusHead 的
		// SetProfile.SetBonusKey 统一设置，这里不再重复设置一遍。
		public override void PostUpdateEquips() {
			HibiscusHelmetActive = Player.armor[0].type == NeoArmorReforgeSetLoader.GetSetType<HibiscusHead>();
			HibiscusSetActive = HibiscusHelmetActive
				&& Player.armor[1].type == NeoArmorReforgeSetLoader.GetSetType<HibiscusBody>()
				&& Player.armor[2].type == NeoArmorReforgeSetLoader.GetSetType<HibiscusLegs>();
		}

		public override void ModifyMaxStats(out StatModifier health, out StatModifier mana) {
			health = StatModifier.Default;
			mana = StatModifier.Default;

			if (HibiscusSetActive)
				health.Base += 30;
		}
	}
}
