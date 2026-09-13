using ArknightsMod.Content.Items.Armor;
using ArknightsMod.Content.Items.Armor.NeoArmorReforge;
using Terraria;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Armor.Vanguard.Fang
{
	internal class FangSetPlayer : ArknightsArmorPlayer
	{
		public bool FangHelmetActive;
		public bool FangSetActive;

		private bool spawnSpGranted;
		private int noDamageTimer;

		public override void ResetEffects() {
			FangHelmetActive = false;
			FangSetActive = false;
		}

		// NeoArmor Reforge：套装件是独立 ItemID，穿上它本身就代表"已经是套装形态"，
		// 不需要再查 hasUpgraded；player.setBonus 文本交给 FangHead 的
		// SetProfile.SetBonusKey 统一设置，这里不再重复设置一遍。
		public override void PostUpdateEquips() {
			FangHelmetActive = Player.armor[0].type == NeoArmorReforgeSetLoader.GetSetType<FangHead>();
			FangSetActive = FangHelmetActive
				&& Player.armor[1].type == NeoArmorReforgeSetLoader.GetSetType<FangBody>()
				&& Player.armor[2].type == NeoArmorReforgeSetLoader.GetSetType<FangLegs>();
		}

		public override void PostUpdate() {
			if (FangHelmetActive && !Player.dead && !spawnSpGranted) {
				OperatorSPHelper.TryGainSP(Player, 4);
				spawnSpGranted = true;
			}

			if (Player.dead)
				spawnSpGranted = false;

			if (!FangSetActive)
				return;

			noDamageTimer++;
			if (noDamageTimer >= 4 * 60) {
				OperatorSPHelper.TryGainSP(Player, 2);
				noDamageTimer = 0;
			}
		}

		public override void OnRespawn() {
			if (FangHelmetActive)
				OperatorSPHelper.TryGainSP(Player, 4);
		}

		public override void ModifyHurt(ref Player.HurtModifiers modifiers) {
			if (FangSetActive)
				noDamageTimer = 0;
		}
	}
}
