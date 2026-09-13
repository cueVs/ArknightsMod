using ArknightsMod.Content.Items.Armor;
using ArknightsMod.Content.Items.Armor.NeoArmorReforge;
using ArknightsMod.Players;
using Terraria;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Armor.Guard.Chen
{
	internal class ChenSetPlayer : ArknightsArmorPlayer
	{
		public bool ChenHelmetActive;
		public bool ChenSetActive;

		private int spTimer;

		public override void ResetEffects() {
			ChenHelmetActive = false;
			ChenSetActive = false;
		}

		// NeoArmor Reforge：套装件是独立 ItemID，穿上它本身就代表"已经是套装形态"，
		// 不需要再查 hasUpgraded；player.setBonus 文本交给 ChenHead 的
		// SetProfile.SetBonusKey 统一设置，这里不再重复设置一遍。
		public override void PostUpdateEquips() {
			ChenHelmetActive = Player.armor[0].type == NeoArmorReforgeSetLoader.GetSetType<ChenHead>();
			ChenSetActive = ChenHelmetActive
				&& Player.armor[1].type == NeoArmorReforgeSetLoader.GetSetType<ChenBody>()
				&& Player.armor[2].type == NeoArmorReforgeSetLoader.GetSetType<ChenLegs>();

			if (ChenHelmetActive) {
				Player.GetDamage(DamageClass.Generic) += 0.05f;
				extraDefenseBonus += 0.05f;
			}
		}

		public override void ModifyHurt(ref Player.HurtModifiers modifiers) {
			if (!ChenHelmetActive)
				return;

			if (Main.rand.NextFloat() < 0.1f)
				modifiers.Cancel();
		}

		public override void PostUpdate() {
			if (!ShouldReceiveChenSp(Player))
				return;

			spTimer++;
			if (spTimer < 3 * 60)
				return;

			spTimer = 0;
			int amount = ChenSetActive ? 2 : 1;
			OperatorSPHelper.TryGainSP(Player, amount);
		}

		public static bool ShouldReceiveChenSp(Player player) {
			WeaponPlayer wp = player.GetModPlayer<WeaponPlayer>();
			if (wp.HowManySkills <= 0)
				return false;

			for (int i = 0; i < Main.maxPlayers; i++) {
				Player other = Main.player[i];
				if (!other.active || other.dead)
					continue;

				ChenSetPlayer chen = other.GetModPlayer<ChenSetPlayer>();
				if (chen.ChenSetActive && OperatorTeammateHelper.HasTeammates(other))
					return true;
			}

			return false;
		}
	}
}
