using ArknightsMod.Content.Items.Armor;
using ArknightsMod.Content.Items.Armor.NeoArmorReforge;
using Terraria;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Armor.Caster.Steward
{
	internal class StewardSetPlayer : ArknightsArmorPlayer
	{
		public bool StewardHelmetActive;
		public bool StewardSetActive;

		public override void ResetEffects() {
			StewardHelmetActive = false;
			StewardSetActive = false;
		}

		// NeoArmor Reforge：套装件是独立 ItemID，穿上它本身就代表"已经是套装形态"，
		// 不需要再查 hasUpgraded；player.setBonus 文本交给 StewardHead 的
		// SetProfile.SetBonusKey 统一设置，这里不再重复设置一遍。
		public override void PostUpdateEquips() {
			StewardHelmetActive = Player.armor[0].type == NeoArmorReforgeSetLoader.GetSetType<StewardHead>();
			StewardSetActive = StewardHelmetActive
				&& Player.armor[1].type == NeoArmorReforgeSetLoader.GetSetType<StewardBody>()
				&& Player.armor[2].type == NeoArmorReforgeSetLoader.GetSetType<StewardLegs>();
		}

		public override void ModifyWeaponDamage(Item item, ref StatModifier damage) {
			if (StewardHelmetActive && item.DamageType.CountsAsClass(DamageClass.Magic))
				damage *= 1.04f;
		}
	}
}
