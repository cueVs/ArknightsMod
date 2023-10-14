using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using ArknightsMod.Content.Items.Weapons;

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
		public float SkillActiveTime = 0;
		public int SkillTimer = 0;
		public int SP = 0;
		public int InitialSP = 0;
		public int MaxSP = 0;
		public int StockMax = 0; //How many charges can the Operator store up to?
		public int StockCount = 0;
		public int Div = 1;
		public int Skill = 0;// S1 = 0, S2 = 1, S3 = 2
		public bool StockSkill = false; //If the skill is normal skill or overcharge skill, this is false.
		public bool AutoTrigger = false;
		public bool SkillInitialize = true;
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
		public void SetSkillData(int initialsp, int maxsp, int div, int stockmax, float skillactivetime, bool stockskill, bool autotrigger) {
			if (SkillInitialize) {
				// initialize
				InitialSP = initialsp;
				MaxSP = maxsp;
				Div = div;
				SkillChargeMax = maxsp * div;
				StockMax = stockmax;
				SkillActiveTime = skillactivetime;
				StockSkill = stockskill;
				AutoTrigger = autotrigger;
				SkillCharge = initialsp * div;
				SkillTimer = 0;
				StockCount = 0;
				SP = InitialSP;
				SkillTimer = 0;
				SkillActive = false;
				if (InitialSP == MaxSP) {
					SkillCharge = 0;
					StockCount++;
					if (StockCount == StockMax) {
						SP = MaxSP;
					}
					else
						SP = 0;
				}

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
			if (!SkillActive && StockCount < StockMax) {
				SkillCharge += 1;

				if (SkillCharge != 0 && SkillCharge % 60 == 0) {
					SP += 1;
				}

				if (SkillCharge == SkillChargeMax) {
					SkillCharge = 0;
					StockCount += 1;
					if (StockCount == StockMax) {
						SP = MaxSP;
					}
					else
						SP = 0;
				}

			}
		}

		public void OffensiveRecovery() {
			if (!SkillActive && StockCount < StockMax) {
				SkillCharge += 1;
				if (SkillCharge != 0) {
					SP += 1;
				}
			}
			if (SkillCharge == SkillChargeMax) {
				SkillCharge = 0;
				StockCount += 1;
				if (StockCount == StockMax) {
					SP = MaxSP;
				}
				else
					SP = 0;
			}
		}

		public void SkillActiveTimer() {
			if (SkillActive) {
				SkillTimer++;
				if (SkillTimer == SkillActiveTime * 60) {
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
			if (StockCount == StockMax) {
				SP = 0;
			}

			StockCount -= 1;
		}

		//public override void ResetEffects() {
		//	ResetVariables();
		//}

		//public override void UpdateDead() {
		//	ResetVariables();
		//}

		// We need this to ensure that regeneration rate and maximum amount are reset to default values after increasing when conditions are no longer satisfied (e.g. we unequip an accessory that increaces our recource)
		//private void ResetVariables() {
		//	exampleResourceRegenRate = 1f;
		//	exampleResourceMax2 = exampleResourceMax;
		//}

		//public override void PostUpdateMiscEffects() {
		//	UpdateResource();
		//}

		// Lets do all our logic for the custom resource here, such as limiting it, increasing it and so on.
		//private void UpdateResource() {
		//	// For our resource lets make it regen slowly over time to keep it simple, let's use exampleResourceRegenTimer to count up to whatever value we want, then increase currentResource.
		//	Timer++; // Increase it by 60 per second, or 1 per tick.

		//	// A simple timer that goes up to 3 seconds, increases the exampleResourceCurrent by 1 and then resets back to 0.
		//	if (Timer > 180 / RegenRate) {
		//		SkillCharge += 1;
		//		Timer = 0;
		//	}

		//	// Limit exampleResourceCurrent from going over the limit imposed by exampleResourceMax.
		//	SkillCharge = Utils.Clamp(SkillCharge, 0, Percentage);
		//}
	}
}
