using ArknightsMod.Content.Items.Armor;
using ArknightsMod.Content.Items.Armor.NeoArmorReforge;
using Terraria;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Armor.Vanguard.Vanilla
{
	internal class VanillaSetPlayer : ArknightsArmorPlayer
	{
		public bool VanillaHelmetActive;
		public bool VanillaSetActive;

		public override void ResetEffects() {
			VanillaHelmetActive = false;
			VanillaSetActive = false;
		}

		// NeoArmor Reforge：套装件是独立 ItemID，穿上它本身就代表"已经是套装形态"，
		// 不需要再查 hasUpgraded；player.setBonus 文本交给 VanillaHead 的
		// SetProfile.SetBonusKey 统一设置，这里不再重复设置一遍。
		public override void PostUpdateEquips() {
			VanillaHelmetActive = Player.armor[0].type == NeoArmorReforgeSetLoader.GetSetType<VanillaHead>();
			VanillaSetActive = VanillaHelmetActive
				&& Player.armor[1].type == NeoArmorReforgeSetLoader.GetSetType<VanillaBody>()
				&& Player.armor[2].type == NeoArmorReforgeSetLoader.GetSetType<VanillaLegs>();

			if (VanillaSetActive && Player.GetModPlayer<OperatorDeployCostPlayer>().DeployCost > 50)
				Player.statDefense += 8;
		}

		public override void ModifyWeaponDamage(Item item, ref StatModifier damage) {
			if (VanillaHelmetActive && item.DamageType.CountsAsClass(DamageClass.Melee))
				damage *= 1.03f;
		}
	}
}
