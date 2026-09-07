using System;
using ArknightsMod.Players;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Weapons.Guard.Hellagur
{
	/// <summary>
	/// 赫拉格专属的武者状态。这里只维护武器自己的命中充能、短时技能、招架和脱战恢复，
	/// 不把角色语义塞进全局 WeaponPlayer。
	/// </summary>
	public sealed class HellagurCombatPlayer : ModPlayer
	{
		internal const int Skill1HitsRequired = 2;
		internal const int Skill2DurationTicks = 13 * 60;
		internal const int Skill3DurationTicks = 15 * 60;
		private const int CombatRegenDelayTicks = 5 * 60;
		private const float Skill2ParryChance = 0.75f;

		private int skill1Charge;
		private int activeSkillMode;
		private int activeSkillTimer;
		private int activeSkillDuration;
		private int combatCooldown;
		private int regenTick;

		public bool Skill1Ready => skill1Charge >= Skill1HitsRequired;
		public int Skill1Charge => skill1Charge;
		public int ActiveSkillMode => activeSkillMode;
		public int ActiveSkillTimeLeft => activeSkillTimer;

		private bool HoldingOdachi => Player.HeldItem.ModItem is HellagurOdachi;

		public override void Initialize() => ResetCombatState();

		public override void UpdateDead() => ResetCombatState();

		public override void OnHurt(Player.HurtInfo info) => MarkCombat(CombatRegenDelayTicks);

		public override bool FreeDodge(Player.HurtInfo info) {
			if (Player.whoAmI != Main.myPlayer || !HoldingOdachi || activeSkillMode != HellagurSkillBridge.Skill2
				|| activeSkillTimer <= 0)
				return false;
			// 泰拉瑞亚没有“物理/法术伤害”标签：只对 NPC / 弹幕造成的战斗伤害判定，排除坠落、溺水等环境伤害。
			if (!info.DamageSource.TryGetCausingEntity(out Entity causingEntity)
				|| (causingEntity is not NPC && causingEntity is not Projectile)
				|| Main.rand.NextFloat() >= Skill2ParryChance) {
				return false;
			}

			MarkCombat(CombatRegenDelayTicks);

			if (!Main.dedServ) {
				SoundEngine.PlaySound(SoundID.Item37 with { Volume = 0.55f, Pitch = -0.15f }, Player.Center);
				for (int i = 0; i < 18; i++) {
					Vector2 velocity = Vector2.UnitX.RotatedBy(MathHelper.TwoPi * i / 18f) * Main.rand.NextFloat(2.4f, 4.8f);
					Dust dust = Dust.NewDustPerfect(Player.Center, i % 3 == 0 ? DustID.GoldFlame : DustID.Blood,
						velocity, 40, new Color(255, 196, 92), Main.rand.NextFloat(0.85f, 1.25f));
					dust.noGravity = true;
				}
			}
			return true;
		}

		public override void PostUpdate() {
			if (Player.whoAmI != Main.myPlayer)
				return;
			if (Player.dead) {
				ResetCombatState();
				return;
			}

			if (!HoldingOdachi) {
				if (activeSkillMode != HellagurSkillBridge.Normal)
					EndTimedSkill(Player.GetModPlayer<WeaponPlayer>());
				skill1Charge = 0;
				if (combatCooldown > 0)
					combatCooldown--;
				regenTick = 0;
				return;
			}

			WeaponPlayer weaponPlayer = Player.GetModPlayer<WeaponPlayer>();
			// 切离 S1 就视为放弃这次攻击回复进度，不能切去其它技能后再带着半条充能回来。
			if (weaponPlayer.Skill != 0)
				skill1Charge = 0;
			if (Player.lifeRegen < 0)
				MarkCombat(CombatRegenDelayTicks);
			if (activeSkillMode != HellagurSkillBridge.Normal
				&& weaponPlayer.Skill != activeSkillMode - 1) {
				EndTimedSkill(weaponPlayer);
			}

			if (activeSkillMode != HellagurSkillBridge.Normal)
				UpdateTimedSkill(weaponPlayer);

			// S1 由专属命中计数驱动；末尾覆写仪表状态，保证群攻只按一次母刀攻击回复技力。
			if (weaponPlayer.Skill == 0)
				MirrorSkill1Gauge(weaponPlayer);

			if (combatCooldown > 0) {
				combatCooldown--;
				regenTick = 0;
			}
			else {
				UpdateOutOfCombatRecovery();
			}

			DrawLowHealthFeedback();
		}

		internal bool StartTimedSkill(int skillMode) {
			if (skillMode is not (HellagurSkillBridge.Skill2 or HellagurSkillBridge.Skill3)
				|| activeSkillMode != HellagurSkillBridge.Normal) {
				return false;
			}

			activeSkillMode = skillMode;
			int baseDuration = skillMode == HellagurSkillBridge.Skill2
				? Skill2DurationTicks
				: Skill3DurationTicks;
			activeSkillDuration = Math.Max(1, (int)Math.Round(baseDuration * WeaponPlayer.ActiveDurationMultiplier));
			activeSkillTimer = activeSkillDuration;
			MarkCombat(CombatRegenDelayTicks);
			SpawnSkillActivationFeedback(skillMode);
			return true;
		}

		internal void MarkSwingStarted() => MarkCombat(2 * 60);

		internal void RegisterMotherBladeHit(bool grantAttackRecovery) {
			MarkCombat(CombatRegenDelayTicks);
			if (!grantAttackRecovery)
				return;

			WeaponPlayer weaponPlayer = Player.GetModPlayer<WeaponPlayer>();
			if (weaponPlayer.Skill != 0 || Skill1Ready)
				return;

			int gain = Math.Max(1, WeaponPlayer.GlobalSPRegenSpeedMultiplier);
			bool wasReady = Skill1Ready;
			skill1Charge = Math.Min(Skill1HitsRequired, skill1Charge + gain);
			MirrorSkill1Gauge(weaponPlayer);

			if (!wasReady && Skill1Ready)
				SpawnSkill1ReadyFeedback();
		}

		internal int HealFromMotherBlade(ref int healedThisSwing) {
			if (Player.dead || Player.statLife <= 0 || Player.statLife >= Player.statLifeMax2)
				return 0;

			int healPerHit = Math.Max(2, (int)Math.Ceiling(Player.statLifeMax2 * 0.017f));
			int healCap = Math.Max(6, (int)Math.Ceiling(Player.statLifeMax2 * 0.051f));
			int remainingBudget = Math.Max(0, healCap - healedThisSwing);
			int actualHeal = Math.Min(Math.Min(healPerHit, remainingBudget), Player.statLifeMax2 - Player.statLife);
			if (actualHeal <= 0)
				return 0;

			healedThisSwing += actualHeal;
			Player.Heal(actualHeal);
			return actualHeal;
		}

		internal bool ConsumeSkill1() {
			if (!Skill1Ready)
				return false;

			WeaponPlayer weaponPlayer = Player.GetModPlayer<WeaponPlayer>();
			if (weaponPlayer.StockCount > 0)
				weaponPlayer.DelStockCount();
			skill1Charge = 0;
			MirrorSkill1Gauge(weaponPlayer);
			return true;
		}

		private void UpdateTimedSkill(WeaponPlayer weaponPlayer) {
			if (--activeSkillTimer <= 0) {
				EndTimedSkill(weaponPlayer);
				return;
			}

			weaponPlayer.SkillActive = true;
			float progress = 1f - activeSkillTimer / (float)Math.Max(1, activeSkillDuration);
			float uiDuration = weaponPlayer.CurrentSkill?.CurrentLevelData.ActiveTime * 60f
				* WeaponPlayer.ActiveDurationMultiplier ?? activeSkillDuration;
			weaponPlayer.SkillTimer = (int)Math.Round(Math.Max(1f, uiDuration) * progress);
		}

		private void EndTimedSkill(WeaponPlayer weaponPlayer) {
			activeSkillMode = HellagurSkillBridge.Normal;
			activeSkillTimer = 0;
			activeSkillDuration = 0;
			weaponPlayer.SkillActive = false;
			weaponPlayer.SkillTimer = 0;
		}

		private void MirrorSkill1Gauge(WeaponPlayer weaponPlayer) {
			weaponPlayer.Div = 1;
			weaponPlayer.SkillChargeMax = Skill1HitsRequired;
			weaponPlayer.SkillCharge = Skill1Ready ? 0 : skill1Charge;
			weaponPlayer.SP = Skill1Ready ? Skill1HitsRequired : skill1Charge;
			weaponPlayer.StockCount = Skill1Ready ? 1 : 0;
			weaponPlayer.SkillActive = false;
			weaponPlayer.SkillTimer = 0;
		}

		private void UpdateOutOfCombatRecovery() {
			if (Player.statLife >= Player.statLifeMax2) {
				regenTick = 0;
				return;
			}

			if (++regenTick < 60)
				return;
			regenTick = 0;
			int heal = Math.Max(1, (int)Math.Ceiling(Player.statLifeMax2 * 0.015f));
			Player.Heal(Math.Min(heal, Player.statLifeMax2 - Player.statLife));
		}

		private void DrawLowHealthFeedback() {
			float intensity = HellagurSkillBridge.GetLowHealthIntensity(Player);
			if (intensity <= 0f || Main.dedServ)
				return;

			Lighting.AddLight(Player.Center, new Vector3(0.42f, 0.045f, 0.025f) * intensity);
			int period = Math.Max(3, (int)MathHelper.Lerp(12f, 4f, intensity));
			if (Main.GameUpdateCount % (ulong)period != 0)
				return;

			Vector2 position = Player.Center + Main.rand.NextVector2Circular(Player.width * 0.7f, Player.height * 0.55f);
			Dust dust = Dust.NewDustPerfect(position, Main.rand.NextBool(4) ? DustID.GoldFlame : DustID.Blood,
				new Vector2(0f, Main.rand.NextFloat(-1.8f, -0.6f)), 60,
				new Color(245, 70, 46), Main.rand.NextFloat(0.7f, 1.15f));
			dust.noGravity = true;
		}

		private void SpawnSkillActivationFeedback(int skillMode) {
			if (Main.dedServ)
				return;

			int count = skillMode == HellagurSkillBridge.Skill3 ? 28 : 20;
			for (int i = 0; i < count; i++) {
				float angle = MathHelper.TwoPi * i / count;
				Vector2 velocity = Vector2.UnitX.RotatedBy(angle) * Main.rand.NextFloat(2f, skillMode == 3 ? 5.6f : 4.2f);
				Dust dust = Dust.NewDustPerfect(Player.Center, i % 4 == 0 ? DustID.GoldFlame : DustID.Blood,
					velocity, 30, new Color(255, 174, 74), Main.rand.NextFloat(0.8f, 1.35f));
				dust.noGravity = true;
			}
		}

		private void SpawnSkill1ReadyFeedback() {
			if (Main.dedServ)
				return;

			SoundEngine.PlaySound(SoundID.Item4 with { Volume = 0.35f, Pitch = 0.35f }, Player.Center);
			for (int i = 0; i < 12; i++) {
				float angle = -MathHelper.PiOver2 + MathHelper.Lerp(-1.1f, 1.1f, i / 11f);
				Dust dust = Dust.NewDustPerfect(Player.Center + new Vector2(0f, -22f), DustID.GoldFlame,
					Vector2.UnitX.RotatedBy(angle) * Main.rand.NextFloat(1.5f, 3.2f), 20,
					new Color(255, 222, 145), Main.rand.NextFloat(0.75f, 1.1f));
				dust.noGravity = true;
			}
		}

		private void MarkCombat(int duration) {
			combatCooldown = Math.Max(combatCooldown, duration);
			regenTick = 0;
		}

		private void ResetCombatState() {
			skill1Charge = 0;
			activeSkillMode = HellagurSkillBridge.Normal;
			activeSkillTimer = 0;
			activeSkillDuration = 0;
			combatCooldown = 0;
			regenTick = 0;
		}
	}

	/// <summary>
	/// 赫拉格技能桥。公共技能代理只需登记第三技能槽，并在需要时调用 TryActivate；
	/// 实际攻击模式始终通过挥砍弹幕 ai[1] 同步。
	/// </summary>
	public static class HellagurSkillBridge
	{
		public const int Normal = 0;
		public const int Skill1 = 1;
		public const int Skill2 = 2;
		public const int Skill3 = 3;

		public static int GetAttackMode(Player player) {
			WeaponPlayer weaponPlayer = player.GetModPlayer<WeaponPlayer>();
			HellagurCombatPlayer combatPlayer = player.GetModPlayer<HellagurCombatPlayer>();
			if (weaponPlayer.Skill == 0 && combatPlayer.Skill1Ready)
				return Skill1;
			if (combatPlayer.ActiveSkillMode is Skill2 or Skill3
				&& weaponPlayer.Skill == combatPlayer.ActiveSkillMode - 1) {
				return combatPlayer.ActiveSkillMode;
			}
			return Normal;
		}

		public static bool TryActivateSelected(Player player) {
			WeaponPlayer weaponPlayer = player.GetModPlayer<WeaponPlayer>();
			return TryActivate(player, weaponPlayer.Skill + 1);
		}

		public static bool TryActivate(Player player, int skillMode, bool consumeStock = true) {
			if (player.HeldItem.ModItem is not HellagurOdachi
				|| skillMode is not (Skill2 or Skill3)) {
				return false;
			}

			WeaponPlayer weaponPlayer = player.GetModPlayer<WeaponPlayer>();
			HellagurCombatPlayer combatPlayer = player.GetModPlayer<HellagurCombatPlayer>();
			if (weaponPlayer.Skill != skillMode - 1 || combatPlayer.ActiveSkillMode != Normal)
				return false;
			if (consumeStock && (weaponPlayer.StockCount <= 0 || weaponPlayer.SkillActive))
				return false;
			if (!combatPlayer.StartTimedSkill(skillMode))
				return false;

			if (consumeStock)
				weaponPlayer.DelStockCount();
			weaponPlayer.SkillActive = true;
			weaponPlayer.SkillTimer = 0;
			return true;
		}

		public static float GetLowHealthActionSpeed(Player player) {
			float lifeRatio = player.statLifeMax2 <= 0
				? 1f
				: MathHelper.Clamp(player.statLife / (float)player.statLifeMax2, 0f, 1f);
			// 原作 E2 天赋：损失 70% 生命时达到 +100 ASPD；换算为动作时长即 30% 生命约 2 倍速。
			return 1f + MathHelper.Clamp((1f - lifeRatio) / 0.7f, 0f, 1f);
		}

		public static float GetLowHealthIntensity(Player player) {
			float lifeRatio = player.statLifeMax2 <= 0
				? 1f
				: MathHelper.Clamp(player.statLife / (float)player.statLifeMax2, 0f, 1f);
			return MathHelper.Clamp((0.7f - lifeRatio) / 0.4f, 0f, 1f);
		}
	}
}
