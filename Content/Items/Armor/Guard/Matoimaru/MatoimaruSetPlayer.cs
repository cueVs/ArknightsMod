using ArknightsMod.Content.Items.Armor;
using ArknightsMod.Content.Items.Armor.NeoArmorReforge;
using Terraria;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Armor.Guard.Matoimaru
{
	internal class MatoimaruSetPlayer : ArknightsArmorPlayer
	{
		public bool MatoimaruHelmetActive;
		public bool MatoimaruSetActive;

		public override void ResetEffects() {
			MatoimaruHelmetActive = false;
			MatoimaruSetActive = false;
		}

		// NeoArmor Reforge：套装件是独立 ItemID，穿上它本身就代表"已经是套装形态"，
		// 不需要再查 hasUpgraded；player.setBonus 文本交给 MatoimaruHead 的
		// SetProfile.SetBonusKey 统一设置，这里不再重复设置一遍。
		public override void PostUpdateEquips() {
			MatoimaruHelmetActive = Player.armor[0].type == NeoArmorReforgeSetLoader.GetSetType<MatoimaruHead>();
			MatoimaruSetActive = MatoimaruHelmetActive
				&& Player.armor[1].type == NeoArmorReforgeSetLoader.GetSetType<MatoimaruBody>()
				&& Player.armor[2].type == NeoArmorReforgeSetLoader.GetSetType<MatoimaruLegs>();

			if (MatoimaruSetActive)
				extraDefenseBonus -= 0.2f;
		}

		public override void ModifyMaxStats(out StatModifier health, out StatModifier mana) {
			health = StatModifier.Default;
			mana = StatModifier.Default;

			if (MatoimaruSetActive)
				health *= 1.3f;
		}

		public override void GetHealLife(Item item, bool quickHeal, ref int healValue) {
			if (MatoimaruHelmetActive)
				healValue = (int)(healValue * 1.25f);
		}
	}
}
