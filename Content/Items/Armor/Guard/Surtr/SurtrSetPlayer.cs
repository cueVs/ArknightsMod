using System;
using ArknightsMod.Content.Items.Armor;
using ArknightsMod.Content.Items.Armor.NeoArmorReforge;
using Terraria;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Armor.Guard.Surtr
{
	internal class SurtrSetPlayer : ArknightsArmorPlayer
	{
		public bool SurtrHelmetActive;
		public bool SurtrSetActive;

		public bool SurtrUndyingActive;
		private int undyingCooldown;
		private int undyingIFrameTimer;

		public override void ResetEffects() {
			SurtrHelmetActive = false;
			SurtrSetActive = false;
		}

		// NeoArmor Reforge：套装件是独立 ItemID，穿上它本身就代表"已经是套装形态"，
		// 不需要再查 hasUpgraded；player.setBonus 文本交给 SurtrHead 的
		// SetProfile.SetBonusKey 统一设置，这里不再重复设置一遍。
		public override void PostUpdateEquips() {
			SurtrHelmetActive = Player.armor[0].type == NeoArmorReforgeSetLoader.GetSetType<SurtrHead>();
			SurtrSetActive = SurtrHelmetActive
				&& Player.armor[1].type == NeoArmorReforgeSetLoader.GetSetType<SurtrBody>()
				&& Player.armor[2].type == NeoArmorReforgeSetLoader.GetSetType<SurtrLegs>();
		}

		public override void ModifyHitNPCWithItem(Item item, NPC target, ref NPC.HitModifiers modifiers) {
			if (SurtrHelmetActive && item.DamageType.CountsAsClass(DamageClass.Melee))
				modifiers.ArmorPenetration += 20;
		}

		public override void ModifyHitNPCWithProj(Projectile proj, NPC target, ref NPC.HitModifiers modifiers) {
			if (SurtrHelmetActive && proj.DamageType.CountsAsClass(DamageClass.Melee))
				modifiers.ArmorPenetration += 20;
		}

		public override float UseSpeedMultiplier(Item item) {
			if (SurtrUndyingActive && item.DamageType.CountsAsClass(DamageClass.Melee))
				return 1.22f;

			return 1f;
		}

		public override bool FreeDodge(Player.HurtInfo info) {
			if (!SurtrSetActive || undyingCooldown > 0)
				return false;

			if (Player.statLife - info.Damage > 0)
				return false;

			TriggerUndying();
			return true;
		}

		public override void PostUpdate() {
			if (undyingCooldown > 0)
				undyingCooldown--;

			if (undyingIFrameTimer > 0) {
				undyingIFrameTimer--;
				SurtrUndyingActive = true;
				Player.immune = true;
				Player.immuneTime = Math.Max(Player.immuneTime, undyingIFrameTimer);
			}
			else {
				SurtrUndyingActive = false;
			}
		}

		public override void GetHealLife(Item item, bool quickHeal, ref int healValue) {
			if (undyingIFrameTimer > 0)
				healValue = 0;
		}

		private void TriggerUndying() {
			Player.statLife = 1;
			undyingIFrameTimer = 8 * 60;
			undyingCooldown = 120 * 60;
			SurtrUndyingActive = true;
		}
	}
}
