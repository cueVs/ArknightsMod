using ArknightsMod.Content.Items.Armor;
using ArknightsMod.Content.Items.Armor.NeoArmorReforge;
using Terraria;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Armor.Sniper.Adnachiel
{
	internal class AdnachielSetPlayer : ArknightsArmorPlayer
	{
		public bool AdnachielHelmetActive;
		public bool AdnachielSetActive;

		public override void ResetEffects() {
			AdnachielHelmetActive = false;
			AdnachielSetActive = false;
		}

		// NeoArmor Reforge：套装件是独立 ItemID，穿上它本身就代表"已经是套装形态"，
		// 不需要再查 hasUpgraded；player.setBonus 文本交给 AdnachielHead 的
		// SetProfile.SetBonusKey 统一设置，这里不再重复设置一遍。
		public override void PostUpdateEquips() {
			AdnachielHelmetActive = Player.armor[0].type == NeoArmorReforgeSetLoader.GetSetType<AdnachielHead>();
			AdnachielSetActive = AdnachielHelmetActive
				&& Player.armor[1].type == NeoArmorReforgeSetLoader.GetSetType<AdnachielBody>()
				&& Player.armor[2].type == NeoArmorReforgeSetLoader.GetSetType<AdnachielLegs>();
		}

		public override void ModifyWeaponDamage(Item item, ref StatModifier damage) {
			if (AdnachielHelmetActive && item.DamageType.CountsAsClass(DamageClass.Ranged))
				damage *= 1.04f;
		}
	}
}
