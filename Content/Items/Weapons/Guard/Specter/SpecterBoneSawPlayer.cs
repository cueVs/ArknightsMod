using System;
using ArknightsMod.Content;
using ArknightsMod.Content.Projectiles.Guard.Specter;
using ArknightsMod.Players;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Weapons.Guard.Specter
{
	/// <summary>
	/// 幽灵鲨的角色态：天赋再生、两个定时技能，以及二技能结束后的十秒失能。
	/// 状态属于武器自己，不向共享 WeaponPlayer 塞入新的角色字段。
	/// </summary>
	public sealed class SpecterBoneSawPlayer : ModPlayer
	{
		private const int Skill1BaseDuration = 30 * 60;
		private const int Skill2BaseDuration = 15 * 60;
		private const int ExhaustionDuration = 10 * 60;

		private int activeSkillMode;
		private int activeSkillTimer;
		private int activeSkillDuration;
		private int passiveRegenTimer;
		private bool skillKeyWasDown;

		public bool Skill1Active => activeSkillMode == 1 && activeSkillTimer > 0;
		public bool Skill2Active => activeSkillMode == 2 && activeSkillTimer > 0;
		public int ActiveSkillMode => activeSkillMode;
		public int ActiveSkillTimeLeft => activeSkillTimer;
		public int ExhaustionTimeLeft { get; private set; }

		private bool HoldingBoneSaw => Player.HeldItem.ModItem is SpecterBoneSaw;

		public override void OnEnterWorld() => ResetState();

		public override void UpdateDead() => ResetState();

		public override void PostUpdateEquips() {
			if (!HoldingBoneSaw)
				return;

			// “深海猎人”识别点：最大生命提高 10%。持续恢复由 PostUpdate 按整秒结算，
			// 不使用原版会被受伤延迟打断的 lifeRegen 计时器。
			Player.statLifeMax2 += Math.Max(1, (int)MathF.Round(Player.statLifeMax2 * 0.10f));
		}

		public override void PreUpdateMovement() {
			if (ExhaustionTimeLeft <= 0)
				return;

			Player.controlLeft = false;
			Player.controlRight = false;
			Player.controlUp = false;
			Player.controlDown = false;
			Player.controlJump = false;
			Player.controlUseItem = false;
			Player.controlUseTile = false;
			Player.noItems = true;
			Player.velocity.X *= 0.72f;
			if (MathF.Abs(Player.velocity.X) < 0.08f)
				Player.velocity.X = 0f;
		}

		public override bool FreeDodge(Player.HurtInfo info) {
			if (Player.whoAmI != Main.myPlayer || !Skill2Active
				|| Player.statLife - info.Damage > 0) {
				return false;
			}

			Player.statLife = 1;
			Player.immune = true;
			Player.immuneTime = Math.Max(Player.immuneTime, 24);
			if (!Main.dedServ) {
				SoundEngine.PlaySound(SoundID.Item37 with { Volume = 0.56f, Pitch = -0.34f }, Player.Center);
				SpecterVisuals.SpawnRefusalBurst(Player.Center);
				SpecterVisuals.AddShake(Player, 6, 3.6f);
			}
			return true;
		}

		public override void PostUpdate() {
			if (Player.dead)
				return;

			if (ExhaustionTimeLeft > 0) {
				ExhaustionTimeLeft--;
				Player.controlUseItem = false;
				Player.controlUseTile = false;
				Player.noItems = true;
				if (!Main.dedServ && ExhaustionTimeLeft % 14 == 0)
					SpecterVisuals.SpawnExhaustionMote(Player.Center + Main.rand.NextVector2Circular(18f, 24f));
			}

			if (Player.whoAmI == Main.myPlayer) {
				bool skillKeyDown = ArknightsKeybinds.SkillActivatePressed(Player);
				// 持续持锯时 itemTime 不会归零，原版物品使用管线不一定再次进入 CanUseItem；
				// 在按键按下沿补一次专属入口，技能即可在锯刃运转中无缝开启。
				if (HoldingBoneSaw && skillKeyDown && !skillKeyWasDown
					&& Player.HeldItem.ModItem is SpecterBoneSaw boneSaw) {
					boneSaw.TryActivateSelectedSkill(Player);
				}
				skillKeyWasDown = skillKeyDown;

				if (activeSkillMode != 0)
					UpdateActiveSkill();
			}

			if (HoldingBoneSaw)
				UpdatePassiveRegeneration();
			else
				passiveRegenTimer = 0;

			// 对直接改写生命值、没有走 Hurt/FreeDodge 的少数持续伤害再兜一层。
			if (Skill2Active && Player.statLife < 1)
				Player.statLife = 1;
		}

		internal bool TryStartSkill(int selectedSkill) {
			if (Player.whoAmI != Main.myPlayer || activeSkillMode != 0 || ExhaustionTimeLeft > 0
				|| selectedSkill is < 0 or > 1) {
				return false;
			}

			activeSkillMode = selectedSkill + 1;
			int baseDuration = selectedSkill == 0 ? Skill1BaseDuration : Skill2BaseDuration;
			activeSkillDuration = Math.Max(1,
				(int)MathF.Round(baseDuration * WeaponPlayer.ActiveDurationMultiplier));
			activeSkillTimer = activeSkillDuration;
			return true;
		}

		private void UpdateActiveSkill() {
			WeaponPlayer weaponPlayer = Player.GetModPlayer<WeaponPlayer>();
			int selectedMode = HoldingBoneSaw ? weaponPlayer.Skill + 1 : 0;
			if (!HoldingBoneSaw || selectedMode != activeSkillMode) {
				EndActiveSkill(weaponPlayer, applyExhaustion: activeSkillMode == 2);
				return;
			}

			if (--activeSkillTimer <= 0) {
				EndActiveSkill(weaponPlayer, applyExhaustion: activeSkillMode == 2);
				return;
			}

			weaponPlayer.SkillActive = true;
			float progress = 1f - activeSkillTimer / (float)Math.Max(1, activeSkillDuration);
			float uiDuration = (weaponPlayer.CurrentSkill?.CurrentLevelData.ActiveTime ??
				(activeSkillMode == 1 ? 30f : 15f)) * 60f * WeaponPlayer.ActiveDurationMultiplier;
			weaponPlayer.SkillTimer = (int)MathF.Round(Math.Max(1f, uiDuration) * progress);
		}

		private void EndActiveSkill(WeaponPlayer weaponPlayer, bool applyExhaustion) {
			int endedMode = activeSkillMode;
			activeSkillMode = 0;
			activeSkillTimer = 0;
			activeSkillDuration = 0;
			weaponPlayer.SkillActive = false;
			weaponPlayer.SkillTimer = 0;

			if (!applyExhaustion)
				return;

			ExhaustionTimeLeft = ExhaustionDuration;
			Player.channel = false;
			Player.noItems = true;
			if (!Main.dedServ && endedMode == 2) {
				SoundEngine.PlaySound(SoundID.NPCDeath6 with { Volume = 0.48f, Pitch = -0.42f }, Player.Center);
				SpecterVisuals.SpawnFrenzyCollapse(Player.Center);
			}
		}

		private void UpdatePassiveRegeneration() {
			if (Player.whoAmI != Main.myPlayer || Player.statLife <= 0 || Player.statLife >= Player.statLifeMax2) {
				passiveRegenTimer = 0;
				return;
			}

			if (++passiveRegenTimer < 60)
				return;
			passiveRegenTimer = 0;
			int heal = Math.Max(1, (int)MathF.Ceiling(Player.statLifeMax2 * 0.02f));
			Player.Heal(Math.Min(heal, Player.statLifeMax2 - Player.statLife));
		}

		private void ResetState() {
			activeSkillMode = 0;
			activeSkillTimer = 0;
			activeSkillDuration = 0;
			ExhaustionTimeLeft = 0;
			passiveRegenTimer = 0;
			skillKeyWasDown = false;
		}
	}
}
