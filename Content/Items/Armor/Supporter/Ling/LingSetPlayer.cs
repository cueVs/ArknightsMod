using System;
using ArknightsMod.Content.Items.Armor;
using ArknightsMod.Content.Items.Armor.NeoArmorReforge;
using Terraria;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Armor.Supporter.Ling
{
	internal class LingSetPlayer : ArknightsArmorPlayer
	{
		public bool LingHelmetActive;
		public bool LingSetActive;

		public int SummonDamageStacks;
		private int lastUnusedSlots = -1;

		public override void ResetEffects() {
			LingHelmetActive = false;
			LingSetActive = false;
		}

		// NeoArmor Reforge：套装件是独立 ItemID，穿上它本身就代表"已经是套装形态"，
		// 不需要再查 hasUpgraded；player.setBonus 文本交给 LingHead 的
		// SetProfile.SetBonusKey 统一设置，这里不再重复设置一遍。
		public override void PostUpdateEquips() {
			LingHelmetActive = Player.armor[0].type == NeoArmorReforgeSetLoader.GetSetType<LingHead>();
			LingSetActive = LingHelmetActive
				&& Player.armor[1].type == NeoArmorReforgeSetLoader.GetSetType<LingBody>()
				&& Player.armor[2].type == NeoArmorReforgeSetLoader.GetSetType<LingLegs>();

			if (LingHelmetActive)
				Player.maxMinions++;

			if (LingSetActive)
				Player.maxMinions++;
		}

		public override void ModifyWeaponDamage(Item item, ref StatModifier damage) {
			if (!item.DamageType.CountsAsClass(DamageClass.Summon))
				return;

			int unused = OperatorMinionSlotHelper.CountUnusedMinionSlots(Player);
			if (LingHelmetActive && unused > 0)
				damage *= 1f + 0.2f * unused;

			if (LingSetActive && SummonDamageStacks > 0)
				damage *= 1f + 0.05f * SummonDamageStacks;
		}

		public override void PostUpdate() {
			if (!LingSetActive) {
				lastUnusedSlots = -1;
				return;
			}

			int unused = OperatorMinionSlotHelper.CountUnusedMinionSlots(Player);
			if (lastUnusedSlots >= 0 && unused > lastUnusedSlots) {
				int gained = unused - lastUnusedSlots;
				for (int i = 0; i < gained; i++) {
					if (SummonDamageStacks < 5)
						SummonDamageStacks++;
					OperatorSPHelper.TryGainSP(Player, 2);
				}
			}

			lastUnusedSlots = unused;
		}
	}
}
