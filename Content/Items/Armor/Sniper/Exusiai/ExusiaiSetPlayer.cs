using ArknightsMod.Content.Items.Armor;
using ArknightsMod.Content.Items.Armor.NeoArmorReforge;
using Terraria;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Armor.Sniper.Exusiai
{
	internal class ExusiaiSetPlayer : ArknightsArmorPlayer
	{
		public bool ExusiaiHelmetActive;
		public bool ExusiaiSetActive;

		public override void ResetEffects() {
			ExusiaiHelmetActive = false;
			ExusiaiSetActive = false;
		}

		// NeoArmor Reforge：套装件是独立 ItemID，穿上它本身就代表"已经是套装形态"，
		// 不需要再查 hasUpgraded；player.setBonus 文本交给 ExusiaiHead 的
		// SetProfile.SetBonusKey 统一设置，这里不再重复设置一遍。
		public override void PostUpdateEquips() {
			ExusiaiHelmetActive = Player.armor[0].type == NeoArmorReforgeSetLoader.GetSetType<ExusiaiHead>();
			ExusiaiSetActive = ExusiaiHelmetActive
				&& Player.armor[1].type == NeoArmorReforgeSetLoader.GetSetType<ExusiaiBody>()
				&& Player.armor[2].type == NeoArmorReforgeSetLoader.GetSetType<ExusiaiLegs>();

		}

		public override void ModifyMaxStats(out StatModifier health, out StatModifier mana) {
			health = StatModifier.Default;
			mana = StatModifier.Default;

			if (ShouldReceiveExusiaiBonus(Player))
				health *= 1.1f;
		}

		public override void ModifyWeaponDamage(Item item, ref StatModifier damage) {
			if (ShouldReceiveExusiaiBonus(Player))
				damage *= 1.06f;
		}

		public static bool ShouldReceiveExusiaiBonus(Player player) {
			ExusiaiSetPlayer self = player.GetModPlayer<ExusiaiSetPlayer>();
			if (self.ExusiaiSetActive)
				return true;

			for (int i = 0; i < Main.maxPlayers; i++) {
				Player other = Main.player[i];
				if (!other.active || other.dead || other.whoAmI == player.whoAmI)
					continue;

				ExusiaiSetPlayer otherSet = other.GetModPlayer<ExusiaiSetPlayer>();
				if (otherSet.ExusiaiSetActive && OperatorTeammateHelper.HasTeammates(other))
					return true;
			}

			return false;
		}

		public override float UseSpeedMultiplier(Item item) {
			if (ExusiaiHelmetActive && item.DamageType.CountsAsClass(DamageClass.Ranged))
				return 1.2f;

			return 1f;
		}

	}
}
