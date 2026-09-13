using System;
using ArknightsMod.Content.Items.Armor;
using ArknightsMod.Content.Items.Armor.NeoArmorReforge;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Armor.Guard.Skadi
{
	internal class SkadiSetPlayer : ArknightsArmorPlayer
	{
		public bool SkadiHelmetActive;
		public bool SkadiSetActive;

		public bool UndyingTriggered;
		public bool UndyingBuffActive;
		private int lifeMaxPenalty;

		// 死亡瞬间锁存"当时是否穿着整套"，以及本次死亡是否已经缩短过复活时间。
		// 说明：死亡期间 Player.ResetEffects 实际上不会执行（Player.Update 在死亡分支
		// 调用 UpdateDead 之后就提前返回了），所以 SkadiSetActive 会保持死前的值；
		// 这里仍然显式锁存，避免把逻辑正确性依赖在这个实现细节上。
		private bool setActiveAtDeath;
		private bool respawnAdjusted;

		public override void ResetEffects() {
			SkadiHelmetActive = false;
			SkadiSetActive = false;
		}

		// NeoArmor Reforge：套装件是独立 ItemID，穿上它本身就代表"已经是套装形态"，
		// 不需要再查 hasUpgraded；player.setBonus 文本交给 SkadiHead 的
		// SetProfile.SetBonusKey 统一设置，这里不再重复设置一遍。
		public override void PostUpdateEquips() {
			SkadiHelmetActive = Player.armor[0].type == NeoArmorReforgeSetLoader.GetSetType<SkadiHead>();
			SkadiSetActive = SkadiHelmetActive
				&& Player.armor[1].type == NeoArmorReforgeSetLoader.GetSetType<SkadiBody>()
				&& Player.armor[2].type == NeoArmorReforgeSetLoader.GetSetType<SkadiLegs>();
		}

		public override void ModifyMaxStats(out StatModifier health, out StatModifier mana) {
			health = StatModifier.Default;
			mana = StatModifier.Default;

			if (lifeMaxPenalty > 0)
				health.Base -= lifeMaxPenalty;
		}

		public override void ModifyWeaponDamage(Item item, ref StatModifier damage) {
			if (SkadiHelmetActive && item.DamageType.CountsAsClass(DamageClass.Melee))
				damage *= 1.2f;
		}

		public override float UseSpeedMultiplier(Item item) {
			if (UndyingBuffActive && item.DamageType.CountsAsClass(DamageClass.Melee))
				return 1.3f;

			return 1f;
		}

		public override void Kill(double damage, int hitDirection, bool pvp, PlayerDeathReason damageSource) {
			setActiveAtDeath = SkadiSetActive;
			respawnAdjusted = false;
		}

		public override void UpdateDead() {
			// ⚠ UpdateDead 是**每帧**都会被调用的。
			// 修复前这里是无条件的 `respawnTimer -= 5 * 60`，等于每帧扣掉 300 tick——
			// 常规死亡的复活计时才 600 tick 左右，两帧就归零，实际效果是「秒复活」，
			// 而不是设计意图的「少等 5 秒」。
			// 正确做法：只在死亡后的第一帧扣一次，用 respawnAdjusted 一次性开关兜住。
			if (setActiveAtDeath && !respawnAdjusted) {
				respawnAdjusted = true;
				Player.respawnTimer = Math.Max(0, Player.respawnTimer - 5 * 60);
			}
		}

		public override bool FreeDodge(Player.HurtInfo info) {
			if (!SkadiSetActive || UndyingTriggered || !OperatorSetBossHelper.AnyBossActive())
				return false;

			if (Player.statLife - info.Damage > 0)
				return false;

			TriggerUndying();
			return true;
		}

		public override void PostUpdate() {
			if (!OperatorSetBossHelper.AnyBossActive())
				UndyingBuffActive = false;
		}

		public override void OnRespawn() {
			ResetDeploymentState();
		}

		private void TriggerUndying() {
			UndyingTriggered = true;
			UndyingBuffActive = true;

			int currentMax = Player.statLifeMax2;
			lifeMaxPenalty += (int)(currentMax * 0.7f);
			Player.statLife = currentMax - lifeMaxPenalty;
			Player.HealEffect(Player.statLife);
		}

		private void ResetDeploymentState() {
			UndyingTriggered = false;
			UndyingBuffActive = false;
			lifeMaxPenalty = 0;
			setActiveAtDeath = false;
			respawnAdjusted = false;
		}
	}
}
