using ArknightsMod.Content.Items.Armor;
using ArknightsMod.Content.Items.Armor.NeoArmorReforge;
using Terraria;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Armor.Guard.Popukar
{
	internal class PopukarSetPlayer : ArknightsArmorPlayer
	{
		public bool PopukarHelmetActive;
		public bool PopukarSetActive;

		public override void ResetEffects() {
			PopukarHelmetActive = false;
			PopukarSetActive = false;
		}

		// NeoArmor Reforge：套装件是独立 ItemID，穿上它本身就代表"已经是套装形态"，
		// 不需要再查 hasUpgraded；player.setBonus 文本交给 PopukarHead 的
		// SetProfile.SetBonusKey 统一设置，这里不再重复设置一遍。
		public override void PostUpdateEquips() {
			PopukarHelmetActive = Player.armor[0].type == NeoArmorReforgeSetLoader.GetSetType<PopukarHead>();
			PopukarSetActive = PopukarHelmetActive
				&& Player.armor[1].type == NeoArmorReforgeSetLoader.GetSetType<PopukarBody>()
				&& Player.armor[2].type == NeoArmorReforgeSetLoader.GetSetType<PopukarLegs>();
		}

		public override void ModifyMaxStats(out StatModifier health, out StatModifier mana) {
			health = StatModifier.Default;
			mana = StatModifier.Default;

			if (PopukarSetActive)
				health.Base += 30;
		}

		public override void ModifyWeaponDamage(Item item, ref StatModifier damage) {
			if (PopukarHelmetActive && item.DamageType.CountsAsClass(DamageClass.Melee))
				damage *= 1.03f;
		}
	}
}
