using ArknightsMod.Content.Items.Armor;
using ArknightsMod.Content.Items.Armor.NeoArmorReforge;
using Terraria;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Armor.Defender.Beagle
{
	internal class BeagleSetPlayer : ArknightsArmorPlayer
	{
		public bool BeagleHelmetActive;
		public bool BeagleSetActive;

		public override void ResetEffects() {
			BeagleHelmetActive = false;
			BeagleSetActive = false;
		}

		// NeoArmor Reforge：套装件是独立 ItemID，穿上它本身就代表"已经是套装形态"，
		// 不需要再查 hasUpgraded；player.setBonus 文本交给 BeagleHead 的
		// SetProfile.SetBonusKey 统一设置，这里不再重复设置一遍。
		public override void PostUpdateEquips() {
			BeagleHelmetActive = Player.armor[0].type == NeoArmorReforgeSetLoader.GetSetType<BeagleHead>();
			BeagleSetActive = BeagleHelmetActive
				&& Player.armor[1].type == NeoArmorReforgeSetLoader.GetSetType<BeagleBody>()
				&& Player.armor[2].type == NeoArmorReforgeSetLoader.GetSetType<BeagleLegs>();

			if (BeagleSetActive) {
				int emptySlots = CountEmptyAccessorySlots();
				int bonusSlots = System.Math.Min(emptySlots, 3);
				Player.statDefense += 6 + bonusSlots * 6;
			}
		}

		public override void PostUpdate() {
			if (BeagleHelmetActive)
				Player.noKnockback = true;
		}

		private int CountEmptyAccessorySlots() {
			int count = 0;
			int accessorySlots = 5 + (Player.extraAccessory ? 1 : 0);
			for (int i = 3; i < 3 + accessorySlots; i++) {
				if (Player.armor[i].IsAir)
					count++;
			}

			return count;
		}
	}
}
