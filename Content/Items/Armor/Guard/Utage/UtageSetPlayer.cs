using System;
using ArknightsMod.Content.Items.Armor;
using ArknightsMod.Content.Items.Armor.NeoArmorReforge;
using Terraria;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Armor.Guard.Utage
{
	internal class UtageSetPlayer : ArknightsArmorPlayer
	{
		public bool UtageHelmetActive;
		public bool UtageSetActive;

		private int helmetHealCount;
		private int helmetHealTimer;

		public override void ResetEffects() {
			UtageHelmetActive = false;
			UtageSetActive = false;
		}

		// NeoArmor Reforge：套装件是独立 ItemID，穿上它本身就代表"已经是套装形态"，
		// 不需要再查 hasUpgraded；player.setBonus 文本交给 UtageHead 的
		// SetProfile.SetBonusKey 统一设置，这里不再重复设置一遍。
		public override void PostUpdateEquips() {
			UtageHelmetActive = Player.armor[0].type == NeoArmorReforgeSetLoader.GetSetType<UtageHead>();
			UtageSetActive = UtageHelmetActive
				&& Player.armor[1].type == NeoArmorReforgeSetLoader.GetSetType<UtageBody>()
				&& Player.armor[2].type == NeoArmorReforgeSetLoader.GetSetType<UtageLegs>();
		}

		public override void PostUpdate() {
			if (helmetHealTimer > 0) {
				helmetHealTimer--;
				if (helmetHealTimer <= 0)
					helmetHealCount = 0;
			}
		}

		public float GetMeleeAttackSpeedBonusPercent() {
			if (!UtageSetActive)
				return 0f;

			float missingRatio = 1f - Player.statLife / (float)Player.statLifeMax2;
			return Math.Min(missingRatio * 0.5f, 0.3f) * 100f;
		}

		public override float UseSpeedMultiplier(Item item) {
			if (!UtageSetActive || !item.DamageType.CountsAsClass(DamageClass.Melee))
				return 1f;

			return 1f + GetMeleeAttackSpeedBonusPercent() / 100f;
		}

		public override void OnHitNPCWithItem(Item item, NPC target, NPC.HitInfo hit, int damageDone) {
			if (!UtageHelmetActive || damageDone <= 0)
				return;

			if (!item.DamageType.CountsAsClass(DamageClass.Melee))
				return;

			TryUtageHelmetHeal(target);
		}

		public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone) {
			if (!UtageHelmetActive || damageDone <= 0)
				return;

			if (!proj.DamageType.CountsAsClass(DamageClass.Melee))
				return;

			TryUtageHelmetHeal(target);
		}

		private void TryUtageHelmetHeal(NPC target) {
			if (target.friendly || target.lifeMax <= 5 || target.dontTakeDamage || target.immortal)
				return;

			if (helmetHealTimer <= 0) {
				helmetHealTimer = 60;
				helmetHealCount = 0;
			}

			if (helmetHealCount >= 3)
				return;

			if (Player.statLife >= Player.statLifeMax2)
				return;

			Player.Heal(1);
			helmetHealCount++;
		}
	}
}
