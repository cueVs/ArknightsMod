using ArknightsMod.Common.Items;
using ArknightsMod.Common.UI;
using ArknightsMod.Content.Items.Weapons;
using Terraria;
using Terraria.ModLoader;

namespace ArknightsMod.Common.Players
{
	public class WeaponPlayer : ModPlayer
	{
		protected override bool CloneNewInstances => true;
		public SkillData CurrentSkill => SkillData[Skill];
		public int SkillCount { get; private set; }
		public readonly SkillData[] SkillData = new SkillData[3];

		public int SkillCharge;
		public int SkillChargeMax;
		public bool SkillActive;
		public int SkillTimer;
		public int SP;
		public int StockCount;
		public int Div;
		public int Skill;
		public bool SummonMode;

		public float mousePositionX;
		public float mousePositionY;
		public float playerPositionX;
		public float playerPositionY;

		private int oldHeld;
		private int oldSkill;
		public void InitSkill() {
			SkillData skill = CurrentSkill;
			SkillLevelData data = skill[skill.ForceReplaceLevel ?? skill.Level];
			Div = skill.ChargeType == SkillChargeType.Auto ? 60 : 1;
			int initSP = data.InitSP;
			int maxSP = data.MaxSP;
			if (initSP == maxSP) {
				SkillCharge = 0;
				StockCount = 1;
				SP = StockCount == data.MaxStack ? maxSP : 0;
			}
			else {
				SkillCharge = initSP * Div;
				StockCount = 0;
				SP = initSP;
			}
			SkillChargeMax = maxSP * Div;
			SkillTimer = 0;
			SkillActive = false;
			SummonMode = false;
		}

		public override void ResetEffects() {
			Item item = Main.LocalPlayer.HeldItem;
			if (item.ModItem is UpgradeWeaponBase ark) {
				int type = item.type;
				if (type != oldHeld) {
					oldHeld = type;
					oldSkill = -1;
					Skill = 0;
					SkillCount = 0;
					for (int i = 0; i < 3; i++) {
						SkillData data = ark.GetSkillData(i);
						SkillData[i] = data;
						SkillCount += data == null ? 0 : 1;
					}
					SelectSkills.ChangeSkillSlot(ark);
				}
				if (oldSkill != Skill) {
					oldSkill = Skill;
					InitSkill();
				}
			}
		}
		public void TryAutoCharge() {
			if (CurrentSkill.ChargeType == SkillChargeType.Auto)
				AutoCharge();
		}
		public void AutoCharge() {
			SkillLevelData data = CurrentSkill.CurrentLevelData;
			if (!SkillActive && StockCount < data.MaxStack) {
				if (++SkillCharge % 60 == 0)
					SP++;
				if (SkillCharge == SkillChargeMax) {
					SkillCharge = 0;
					SP = ++StockCount == data.MaxStack ? data.MaxSP : 0;
				}
			}
		}

		public void OffensiveRecovery() {
			SkillLevelData data = CurrentSkill.CurrentLevelData;
			if (!SkillActive && StockCount < data.MaxStack) {
				SkillCharge++;
				SP++;
			}
			if (SkillCharge == SkillChargeMax) {
				SkillCharge = 0;
				SP = ++StockCount == data.MaxStack ? data.MaxSP : 0;
			}
		}

		public void UpdateActiveSkill() {
			if (SkillActive && ++SkillTimer >= CurrentSkill.CurrentLevelData.ActiveTime * 60)
				SkillActive = false;
		}

		public void StrikeSkill() {
			if (SkillActive && ++SkillTimer == 10)
				SkillActive = false;
		}

		public void DelStockCount() {
			if (StockCount-- == CurrentSkill.CurrentLevelData.MaxStack)
				SP = 0;
		}
	}
}
