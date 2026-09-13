using ArknightsMod.Content.Items.Armor;
using ArknightsMod.Content.Items.Armor.NeoArmorReforge;
using ArknightsMod.Players;
using Terraria;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Armor.Sniper.Fiammetta
{
	internal class FiammettaSetPlayer : ArknightsArmorPlayer
	{
		public bool FiammettaHelmetActive;
		public bool FiammettaSetActive;

		private int drainTimer;

		public override void ResetEffects() {
			FiammettaHelmetActive = false;
			FiammettaSetActive = false;
		}

		// NeoArmor Reforge：套装件是独立 ItemID，穿上它本身就代表"已经是套装形态"，
		// 不需要再查 hasUpgraded；player.setBonus 文本交给 FiammettaHead 的
		// SetProfile.SetBonusKey 统一设置，这里不再重复设置一遍。
		public override void PostUpdateEquips() {
			FiammettaHelmetActive = Player.armor[0].type == NeoArmorReforgeSetLoader.GetSetType<FiammettaHead>();
			FiammettaSetActive = FiammettaHelmetActive
				&& Player.armor[1].type == NeoArmorReforgeSetLoader.GetSetType<FiammettaBody>()
				&& Player.armor[2].type == NeoArmorReforgeSetLoader.GetSetType<FiammettaLegs>();
		}

		public override float UseSpeedMultiplier(Item item) {
			if (FiammettaHelmetActive
				&& !Player.GetModPlayer<WeaponPlayer>().SkillActive
				&& item.DamageType.CountsAsClass(DamageClass.Ranged)) {
				return 1.27f;
			}

			return 1f;
		}

		public override void PostUpdate() {
			if (!FiammettaSetActive || Player.dead)
				return;

			drainTimer++;
			if (drainTimer >= 60 && Player.statLife > 1) {
				Player.statLife--;
				drainTimer = 0;
			}
		}

		public override void ModifyWeaponDamage(Item item, ref StatModifier damage) {
			if (!FiammettaSetActive || !item.DamageType.CountsAsClass(DamageClass.Ranged))
				return;

			float lifeRatio = Player.statLife / (float)Player.statLifeMax2;
			if (lifeRatio > 0.8f)
				damage *= 1.5f;
			else if (lifeRatio > 0.5f)
				damage *= 1.25f;
		}
	}
}
