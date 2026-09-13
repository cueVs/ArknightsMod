using ArknightsMod.Content.Items.Armor;
using ArknightsMod.Content.Items.Armor.NeoArmorReforge;
using Terraria;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Armor.Defender.Cardigan
{
	internal class CardiganSetPlayer : ArknightsArmorPlayer
	{
		public bool CardiganHelmetActive;
		public bool CardiganSetActive;

		public override void ResetEffects() {
			CardiganHelmetActive = false;
			CardiganSetActive = false;
		}

		// NeoArmor Reforge：套装件是独立 ItemID，穿上它本身就代表"已经是套装形态"，
		// 不需要再查 hasUpgraded；player.setBonus 文本交给 CardiganHead 的
		// SetProfile.SetBonusKey 统一设置，这里不再重复设置一遍。
		public override void PostUpdateEquips() {
			CardiganHelmetActive = Player.armor[0].type == NeoArmorReforgeSetLoader.GetSetType<CardiganHead>();
			CardiganSetActive = CardiganHelmetActive
				&& Player.armor[1].type == NeoArmorReforgeSetLoader.GetSetType<CardiganBody>()
				&& Player.armor[2].type == NeoArmorReforgeSetLoader.GetSetType<CardiganLegs>();

			if (CardiganSetActive)
				Player.statDefense += Player.statLifeMax2 / 40;
		}

		public override void ModifyMaxStats(out StatModifier health, out StatModifier mana) {
			health = StatModifier.Default;
			mana = StatModifier.Default;

			if (CardiganHelmetActive)
				health.Base += 40;
		}
	}
}
