using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using ArknightsMod.Content.Items.Weapons;
using System.Collections.Generic;
using log4net.Core;

namespace ArknightsMod.Common.Players
{
	public class WeaponPlayer : ModPlayer
	{
		protected override bool CloneNewInstances => true;

		// Here we create a custom resource, similar to mana or health.
		// Creating some variables to define the current value of our example resource as well as the current maximum value. We also include a temporary max value, as well as some variables to handle the natural regeneration of this resource.
		public int SkillCharge = 0;
		public int SkillChargeMax = 0;
		public bool SkillActive = false;
		public int SkillTimer = 0;
		public int SP = 0;
		public int StockCount = 0;
		public int Div = 1;
		public int Skill = 0;// S1 = 0, S2 = 1, S3 = 2
		public bool SkillInitialize = true;

		// SkillSekect
		public int HowManySkills = 0;
		public List<int?> InitialSP = new() { null, null, null};
		public List<int?> InitialSPs1List = new() { null, null, null, null, null, null, null, null, null, null };
		public List<int?> InitialSPs2List = new() { null, null, null, null, null, null, null, null, null, null };
		public List<int?> InitialSPs3List = new() { null, null, null, null, null, null, null, null, null, null };
		public List<int?> MaxSP = new() { null, null, null };
		public List<int?> MaxSPs1List = new() { null, null, null, null, null, null, null, null, null, null };
		public List<int?> MaxSPs2List = new() { null, null, null, null, null, null, null, null, null, null };
		public List<int?> MaxSPs3List = new() { null, null, null, null, null, null, null, null, null, null };
		public List<float> SkillActiveTime = new() { 0, 0, 0 };
		public List<float> SkillActiveTimeS1List = new() { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
		public List<float> SkillActiveTimeS2List = new() { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
		public List<float> SkillActiveTimeS3List = new() { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
		public List<int> SkillLevel = new() { 0, 0, 0 };
		public List<int> StockMax = new() { 0, 0, 0 };
		public List<int> StockMaxS1List = new() { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
		public List<int> StockMaxS2List = new() { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
		public List<int> StockMaxS3List = new() { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
		public List<bool> AutoTrigger = new() { false, false, false };
		public List<bool> ChargeTypeIsPerSecond = new() { false, false, false };
		public string IconName = "";

		//SummonMode
		public bool SummonMode = false;
		public List<bool> ShowSummonIconBySkills = new() { false, false, false };

		public float mousePositionX = 0;
		public float mousePositionY = 0;
		public float playerPositionX = 0;
		public float playerPositionY = 0;

		public bool HoldBagpipeSpear = false;
		public bool HoldChenSword = false;
		public bool HoldKroosCrossbow = false;
		public bool HoldPozemkaCrossbow = false;

		//public int exampleResourceMax2; // Maximum amount of our example resource. We will change that variable to increase maximum amount of our resource
		//public float RegenRate = 1f; // By changing that variable we can increase/decrease regeneration rate of our resource
		//internal int Timer = 0; // A variable that is required for our timer
		// public static readonly Color HealExampleResource = new(187, 91, 201); // We can use this for CombatText, if you create an item that replenishes exampleResourceCurrent.

		// In order to make the Example Resource example straightforward, several things have been left out that would be needed for a fully functional resource similar to mana and health. 
		// Here are additional things you might need to implement if you intend to make a custom resource:
		// - Multiplayer Syncing: The current example doesn't require MP code, but pretty much any additional functionality will require this. ModPlayer.SendClientChanges and clientClone will be necessary, as well as SyncPlayer if you allow the user to increase exampleResourceMax.
		// - Save/Load permanent changes to max resource: You'll need to implement Save/Load to remember increases to your exampleResourceMax cap.
		// - Resouce replenishment item: Use GlobalNPC.NPCLoot to drop the item. ModItem.OnPickup and ModItem.ItemSpace will allow it to behave like Mana Star or Heart. Use code similar to Player.HealEffect to spawn (and sync) a colored number suitable to your resource.

		// InitialSP, MaxSP, Auto?(yes:60, no:1), stock, SlillActiveTime(/s)(if the skill doesn't have active time, any number), StockSkill?
		public void SetSkill(int skill) {
			if (SkillInitialize) {
				// initialize
				Skill = skill;
				if (ChargeTypeIsPerSecond[Skill]) {
					Div = 60;
				}
				else {
					Div = 1;
				}
				if(InitialSP is not null) {
					SkillCharge = (int)InitialSP[Skill] * Div;
					SP = (int)InitialSP[Skill];
				}
				else {
					SkillCharge = 0;
					SP = 0;
				}
				if (InitialSP is not null) {
					SkillChargeMax = (int)MaxSP[Skill] * Div;
				}
				else {
					SkillChargeMax = 0;
				}
				SkillTimer = 0;
				StockCount = 0;
				SkillTimer = 0;
				SkillActive = false;
				if (InitialSP[Skill] == MaxSP[Skill]) {
					SkillCharge = 0;
					StockCount++;
					if (StockCount == StockMax[Skill]) {
						SP = (int)MaxSP[Skill];
					}
					else
						SP = 0;
				}
				SummonMode = false;

				SkillInitialize = false;
			}
		}

		public override void ResetEffects() {
			if (Main.LocalPlayer.HeldItem.ModItem is not BagpipeSpear) {
				HoldBagpipeSpear = false;
			}
			if (Main.LocalPlayer.HeldItem.ModItem is not KroosCrossbow) {
				HoldKroosCrossbow = false;
			}
			if (Main.LocalPlayer.HeldItem.ModItem is not ChenSword) {
				HoldChenSword = false;
			}
			if (Main.LocalPlayer.HeldItem.ModItem is not PozemkaCrossbow) {
				HoldPozemkaCrossbow = false;
			}
		}

		public void AutoCharge() {
			if (!SkillActive && StockCount < StockMax[Skill]) {
				SkillCharge += 1;

				if (SkillCharge != 0 && SkillCharge % 60 == 0) {
					SP += 1;
				}

				if (SkillCharge == SkillChargeMax) {
					SkillCharge = 0;
					StockCount += 1;
					if (StockCount == StockMax[Skill]) {
						SP = (int)MaxSP[Skill];
					}
					else
						SP = 0;
				}

			}
		}

		public void OffensiveRecovery() {
			if (!SkillActive && StockCount < StockMax[Skill]) {
				SkillCharge += 1;
				if (SkillCharge != 0) {
					SP += 1;
				}
			}
			if (SkillCharge == SkillChargeMax) {
				SkillCharge = 0;
				StockCount += 1;
				if (StockCount == StockMax[Skill]) {
					SP = (int)MaxSP[Skill];
				}
				else
					SP = 0;
			}
		}

		public void SkillActiveTimer() {
			if (SkillActive) {
				SkillTimer++;
				if (SkillTimer == SkillActiveTime[Skill] * 60) {
					SkillActive = false;
				}
			}
		}

		public void StrikeSkill() {
			if (SkillActive) {
				SkillTimer++;
				if (SkillTimer == 10) {
					SkillActive = false;
				}
			}
		}

		public void DelStockCount() {
			if (StockCount == StockMax[Skill]) {
				SP = 0;
			}

			StockCount -= 1;
		}

		public void SetSkillData() {
			if (HowManySkills < 1) {
				InitialSPs1List = new() { null, null, null, null, null, null, null, null, null, null };
				MaxSPs1List = new() { null, null, null, null, null, null, null, null, null, null };
			}
			if (HowManySkills < 2) {
				InitialSPs2List = new() { null, null, null, null, null, null, null, null, null, null };
				MaxSPs2List = new() { null, null, null, null, null, null, null, null, null, null };
			}
			if (HowManySkills < 3) {
				InitialSPs3List = new() { null, null, null, null, null, null, null, null, null, null };
				MaxSPs3List = new() { null, null, null, null, null, null, null, null, null, null };
			}
			InitialSP = new() { InitialSPs1List[SkillLevel[0] - 1], InitialSPs2List[SkillLevel[1] - 1], InitialSPs3List[SkillLevel[2] - 1] };
			MaxSP = new() { MaxSPs1List[SkillLevel[0] - 1], MaxSPs2List[SkillLevel[1] - 1], MaxSPs3List[SkillLevel[2] - 1] };
			SkillActiveTime = new() { SkillActiveTimeS1List[SkillLevel[0] - 1], SkillActiveTimeS2List[SkillLevel[1] - 1], SkillActiveTimeS3List[SkillLevel[2] - 1] };
			StockMax = new() { StockMaxS1List[SkillLevel[0] - 1], StockMaxS2List[SkillLevel[1] - 1], StockMaxS3List[SkillLevel[2] - 1] };
		}

		public void SetAllSkillsData() {
			if (Main.LocalPlayer.HeldItem.ModItem is BagpipeSpear) {
				IconName = "BagpipeSpear";
				HowManySkills = 3;
				SkillLevel = new() { 10, 10, 10 }; // per Skills
				ChargeTypeIsPerSecond = new() { true, true, true }; // Charge Type is Per Second? or other type (Attacking Enemy or Getting Hit)?
				AutoTrigger = new() { false, true, false };
				ShowSummonIconBySkills = new() { false, false, false };

				// Skilll Data
				InitialSPs1List = new() { 0, 0, 0, 0, 0, 0, 0, 0, 0, 15 }; // per Skill Level
				InitialSPs2List = new() { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
				InitialSPs3List = new() { 0, 0, 0, 0, 0, 0, 0, 0, 0, 25 };
				MaxSPs1List = new() { 0, 0, 0, 0, 0, 0, 0, 0, 0, 33 };
				MaxSPs2List = new() { 0, 0, 0, 0, 0, 0, 0, 0, 0, 4 };
				MaxSPs3List = new() { 0, 0, 0, 0, 0, 0, 0, 0, 0, 40 };
				SkillActiveTimeS1List = new() { 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 35f };
				SkillActiveTimeS2List = new() { 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0.5f };
				SkillActiveTimeS3List = new() { 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 20f };
				StockMaxS1List = new() { 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 };
				StockMaxS2List = new() { 0, 0, 0, 0, 0, 0, 0, 0, 0, 3 };
				StockMaxS3List = new() { 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 };
				SetSkillData(); // Don't forget!
			}

			if (Main.LocalPlayer.HeldItem.ModItem is ChenSword) {
				IconName = "ChenSword";
				HowManySkills = 2;
				SkillLevel = new() { 10, 10, 10 }; // per Skills
				ChargeTypeIsPerSecond = new() { false, false, false }; // Charge Type is Per Second? or other type (Attacking Enemy or Getting Hit)?
				AutoTrigger = new() { false, false, false };
				ShowSummonIconBySkills = new() { false, false, false };

				// Skilll Data
				InitialSPs1List = new() { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }; // per Skill Level
				InitialSPs2List = new() { 10, 10, 10, 10, 10, 10, 10, 13, 16, 20 };
				//InitialSPs3List = new() { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0};
				MaxSPs1List = new() { 7, 7, 7, 6, 6, 6, 5, 5, 5, 4 };
				MaxSPs2List = new() { 40, 40, 40, 38, 38, 38, 36, 34, 32, 30 };
				//MaxSPs3List = new() { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0};
				SkillActiveTimeS1List = new() { 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f };
				SkillActiveTimeS2List = new() { 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f};
				//SkillActiveTimeS3List = new() { 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0};
				StockMaxS1List = new() { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 };
				StockMaxS2List = new() { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 };
				//StockMaxS3List = new() { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
				SetSkillData(); // Don't forget!
			}

			if (Main.LocalPlayer.HeldItem.ModItem is KroosCrossbow) {
				IconName = "KroosCrossbow";
				HowManySkills = 1;
				SkillLevel = new() { 7, 7, 7 }; // per Skills
				ChargeTypeIsPerSecond = new() { false, true, false }; // Charge Type is Per Second? or other type (Attacking Enemy or Getting Hit)?
				AutoTrigger = new() { true, false, false };
				ShowSummonIconBySkills = new() { false, false, false };

				// Skilll Data
				InitialSPs1List = new() { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }; // per Skill Level
				MaxSPs1List = new() { 0, 0, 0, 0, 0, 0, 4, 0, 0, 0 };
				SkillActiveTimeS1List = new() { 0f, 0f, 0f, 0f, 0f, 0f, 0.2f, 0f, 0f, 0f };
				StockMaxS1List = new() { 0, 0, 0, 0, 0, 0, 1, 0, 0, 0 };
				SetSkillData(); // Don't forget!
			}

			if (Main.LocalPlayer.HeldItem.ModItem is PozemkaCrossbow) {
				IconName = "PozemkaCrossbow";
				HowManySkills = 3;
				SkillLevel = new() { 10, 10, 10 }; // per Skills
				ChargeTypeIsPerSecond = new() { false, true, true }; // Charge Type is Per Second? or other type (Attacking Enemy or Getting Hit)?
				AutoTrigger = new() { true, false, false };
				ShowSummonIconBySkills = new() { true, true, true };

				// Skilll Data
				InitialSPs1List = new() { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }; // per Skill Level
				InitialSPs2List = new() { 0, 0, 0, 0, 0, 0, 0, 0, 0, 9 };
				InitialSPs3List = new() { 0, 0, 0, 0, 0, 0, 0, 0, 0, 23 };
				MaxSPs1List = new() { 0, 0, 0, 0, 0, 0, 0, 0, 0, 20 };
				MaxSPs2List = new() { 0, 0, 0, 0, 0, 0, 0, 0, 0, 9 };
				MaxSPs3List = new() { 0, 0, 0, 0, 0, 0, 0, 0, 0, 35 };
				SkillActiveTimeS1List = new() { 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 30f };
				SkillActiveTimeS2List = new() { 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0.4f };
				SkillActiveTimeS3List = new() { 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 30f };
				StockMaxS1List = new() { 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 };
				StockMaxS2List = new() { 0, 0, 0, 0, 0, 0, 0, 0, 0, 2 };
				StockMaxS3List = new() { 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 };
				SetSkillData(); // Don't forget!
			}
		}

	}
}
