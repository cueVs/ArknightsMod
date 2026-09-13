using System;
using ArknightsMod.Content.Items.Weapons.Specialist.Scene;
using ArknightsMod.Content.Projectiles.Guard.Blaze;
using ArknightsMod.Players;
using ArknightsMod.Systems.Gameplay.Skill;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Weapons.Guard.Blaze
{
	/// <summary>
	/// 煌链锯的“每次部署”状态。专属状态留在武器目录，避免把角色语义塞进共享 WeaponPlayer。
	/// </summary>
	public sealed class BlazeGreatswordPlayer : ModPlayer
	{
		public const int PowerStrikeAttackCost = 2;
		public const int ExtensionWarmupMaxTicks = 70 * 60;

		public int PowerStrikeCharge { get; private set; }
		public int ExtensionWarmupTicks { get; private set; }
		public bool ExtensionDeployed { get; private set; }
		public bool BoilingBurstActive { get; private set; }
		public float BoilingBurstRamp { get; private set; }

		public float ExtensionWarmupProgress => MathHelper.Clamp(
			ExtensionWarmupTicks / (float)ExtensionWarmupMaxTicks, 0f, 1f);

		private bool wasHoldingBlaze;
		private int lastHeldBlazeSkill = -1;
		private float extensionWarmupFraction;

		public override void OnEnterWorld() {
			ResetDeployment();
		}

		public override void UpdateDead() {
			ResetDeployment();
			if (Player.whoAmI == Main.myPlayer) {
				var weaponPlayer = Player.GetModPlayer<WeaponPlayer>();
				weaponPlayer.SkillActive = false;
				weaponPlayer.SkillTimer = 0;
			}
		}

		public override void PostUpdate() {
			if (Player.dead) {
				wasHoldingBlaze = false;
				return;
			}

			bool holdingBlaze = Player.HeldItem.ModItem is BlazeGreatsword;
			if (holdingBlaze) {
				var weaponPlayer = Player.GetModPlayer<WeaponPlayer>();
				int selectedSkill = weaponPlayer.Skill;

				// 连续手持时切换技能等价于重新部署；临时切到其它快捷栏只暂停暖机，不抹掉成果。
				if (wasHoldingBlaze && lastHeldBlazeSkill >= 0 && selectedSkill != lastHeldBlazeSkill)
					ResetSkillState(lastHeldBlazeSkill);
				lastHeldBlazeSkill = selectedSkill;

				if (selectedSkill == 1)
					UpdateExtensionWarmup(weaponPlayer);
			}
			else if (wasHoldingBlaze && lastHeldBlazeSkill == 0) {
				// 公共技力在卸下技能武器时会清空；S1 的专属命中计数必须同时清空，
				// 否则重持后两套计数会错位，出现只命中一次就触发强化的情况。
				PowerStrikeCharge = 0;
			}

			wasHoldingBlaze = holdingBlaze;
		}

		public override void PostUpdateEquips() {
			if (Player.HeldItem.ModItem is not BlazeGreatsword)
				return;

			var weaponPlayer = Player.GetModPlayer<WeaponPlayer>();
			float defenseBonus = 0f;
			if (weaponPlayer.Skill == 1 && ExtensionDeployed)
				defenseBonus = 0.35f;
			// S3 控制器会在服务端/远端自行回写这份状态，不依赖未联网同步的技能槽索引。
			if (BoilingBurstActive)
				defenseBonus = Math.Max(defenseBonus, 0.8f * BoilingBurstRamp);

			// 原作是“防御力 +X%”，不是 X% 伤害减免；在装备结算后按当前防御同比增加。
			if (defenseBonus > 0f)
				Player.statDefense += Math.Max(1, (int)MathF.Round(Player.statDefense * defenseBonus));
		}

		private void UpdateExtensionWarmup(WeaponPlayer weaponPlayer) {
			if (ExtensionDeployed) {
				if (Player.whoAmI == Main.myPlayer) {
					// 切到其它武器时公共系统会重新初始化；再次拿起煌后恢复永久技能的唯一合法状态。
					weaponPlayer.SkillActive = true;
					weaponPlayer.SkillTimer = 0;
					weaponPlayer.SkillCharge = 0;
					weaponPlayer.StockCount = 0;
					weaponPlayer.SP = 0;
				}
				return;
			}

			if (Player.whoAmI != Main.myPlayer)
				return;

			// 专属进度是唯一时间源，公共技力条只做镜像。这样临时切快捷栏会暂停并保留同一份进度，
			// 同时仍能接收部署费用/调试填充等直接写入公共条的技力。
			if (weaponPlayer.StockCount > 0)
				ExtensionWarmupTicks = ExtensionWarmupMaxTicks;
			else
				ExtensionWarmupTicks = Math.Max(ExtensionWarmupTicks,
					Math.Clamp(weaponPlayer.SkillCharge, 0, ExtensionWarmupMaxTicks));

			if (ExtensionWarmupTicks < ExtensionWarmupMaxTicks
				&& !SceneCameraSkills.BlocksSkillCharge(Player)) {
				extensionWarmupFraction += WeaponPlayer.GlobalSPRegenSpeedMultiplier
					* Math.Max(0f, weaponPlayer.SPRegenMultiplier);
				int wholeTicks = (int)extensionWarmupFraction;
				if (wholeTicks > 0) {
					extensionWarmupFraction -= wholeTicks;
					ExtensionWarmupTicks = Math.Min(ExtensionWarmupMaxTicks,
						ExtensionWarmupTicks + wholeTicks);
				}
			}

			if (ExtensionWarmupTicks < ExtensionWarmupMaxTicks) {
				weaponPlayer.Div = 60;
				weaponPlayer.SkillChargeMax = ExtensionWarmupMaxTicks;
				weaponPlayer.SkillCharge = ExtensionWarmupTicks;
				weaponPlayer.SP = ExtensionWarmupTicks / 60;
				weaponPlayer.StockCount = 0;
				weaponPlayer.SkillActive = false;
				weaponPlayer.SkillTimer = 0;
				return;
			}

			ExtensionWarmupTicks = ExtensionWarmupMaxTicks;
			ExtensionDeployed = true;
			if (weaponPlayer.StockCount > 0)
				weaponPlayer.DelStockCount();

			// S2 是永久技能：保持激活态才能冻结公共运行态，并让技能条持续显示金色。
			weaponPlayer.SkillActive = true;
			weaponPlayer.SkillTimer = 0;
			weaponPlayer.SkillCharge = 0;
			weaponPlayer.StockCount = 0;
			weaponPlayer.SP = 0;

			if (!Main.dedServ) {
				SoundEngine.PlaySound(new SoundStyle("ArknightsMod/Sounds/SkillActive1") {
					Volume = 0.68f,
					Pitch = 0.08f,
					MaxInstances = 2
				}, Player.Center);
				SoundEngine.PlaySound(BlazeVisuals.ButcherMotorSound with { Volume = 0.7f, Pitch = 0.35f }, Player.Center);
				SoundEngine.PlaySound(SoundID.Item74 with { Volume = 0.5f, Pitch = 0.15f }, Player.Center);
				BlazeVisuals.SpawnHeatMotes(Player.Center, 16, 0.85f, 38f);
				BlazeVisuals.SpawnMetalSparks(Player.Center, -Vector2.UnitY, 20, 0.85f, 8f);
				BlazeVisuals.AddShake(Player, 9, 4f);
			}
		}

		/// <summary>一整次挥砍只调用一次；群攻命中多个 NPC 不会多次回复。</summary>
		public void RegisterPowerStrikeAttack() {
			if (Player.whoAmI != Main.myPlayer || Player.HeldItem.ModItem is not BlazeGreatsword)
				return;

			var weaponPlayer = Player.GetModPlayer<WeaponPlayer>();
			if (weaponPlayer.Skill != 0 || PowerStrikeCharge >= PowerStrikeAttackCost)
				return;

			PowerStrikeCharge++;
			if (weaponPlayer.CurrentSkill?.ChargeType == SkillChargeType.Attack)
				weaponPlayer.OffensiveRecovery();
		}

		public bool TryConsumePowerStrike(WeaponPlayer weaponPlayer) {
			if (weaponPlayer.Skill != 0)
				return false;

			bool sharedReady = weaponPlayer.CurrentSkill != null
				&& weaponPlayer.CurrentSkill.ChargeType == SkillChargeType.Attack
				&& weaponPlayer.CurrentSkill.CurrentLevelData.MaxSP <= 3
				&& weaponPlayer.StockCount > 0;
			if (PowerStrikeCharge < PowerStrikeAttackCost && !sharedReady)
				return false;

			PowerStrikeCharge = 0;
			if (Player.whoAmI == Main.myPlayer && sharedReady)
				weaponPlayer.DelStockCount();
			return true;
		}

		public void BeginBoilingBurst() {
			BoilingBurstActive = true;
			BoilingBurstRamp = 0f;
		}

		public void UpdateBoilingBurst(float ramp) {
			BoilingBurstActive = true;
			BoilingBurstRamp = MathHelper.Clamp(ramp, 0f, 1f);
		}

		public void EndBoilingBurst() {
			BoilingBurstActive = false;
			BoilingBurstRamp = 0f;
		}

		private void ResetSkillState(int skill) {
			if (skill == 0)
				PowerStrikeCharge = 0;
			else if (skill == 1) {
				ExtensionWarmupTicks = 0;
				extensionWarmupFraction = 0f;
				ExtensionDeployed = false;
			}
		}

		private void ResetDeployment() {
			PowerStrikeCharge = 0;
			ExtensionWarmupTicks = 0;
			extensionWarmupFraction = 0f;
			ExtensionDeployed = false;
			BoilingBurstActive = false;
			BoilingBurstRamp = 0f;
			wasHoldingBlaze = false;
			lastHeldBlazeSkill = -1;
		}
	}
}
