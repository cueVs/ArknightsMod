using ArknightsMod.Content.Items.Armor;
using ArknightsMod.Content.Items.Armor.NeoArmorReforge;
using ArknightsMod.Players;
using Terraria;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Armor.Vanguard.Texas
{
	internal class TexasSetPlayer : ArknightsArmorPlayer
	{
		public bool TexasHelmetActive;
		public bool TexasSetActive;

		private bool deployBonusGranted;

		public override void ResetEffects() {
			TexasHelmetActive = false;
			TexasSetActive = false;
		}

		// NeoArmor Reforge：套装件是独立 ItemID，穿上它本身就代表"已经是套装形态"，
		// 不需要再查 hasUpgraded；player.setBonus 文本交给 TexasHead 的
		// SetProfile.SetBonusKey 统一设置，这里不再重复设置一遍。
		public override void PostUpdateEquips() {
			TexasHelmetActive = Player.armor[0].type == NeoArmorReforgeSetLoader.GetSetType<TexasHead>();
			TexasSetActive = TexasHelmetActive
				&& Player.armor[1].type == NeoArmorReforgeSetLoader.GetSetType<TexasBody>()
				&& Player.armor[2].type == NeoArmorReforgeSetLoader.GetSetType<TexasLegs>();
		}

		public override void ModifyWeaponDamage(Item item, ref StatModifier damage) {
			if (!TexasSetActive)
				return;

			if (Player.GetModPlayer<WeaponPlayer>().SkillActive)
				damage *= OperatorSetSkillEffectHelper.GetSkillDamageMultiplier(Player);
		}

		public override void PostUpdate() {
			if (!TexasHelmetActive || Player.dead)
				return;

			if (!deployBonusGranted) {
				Player.GetModPlayer<OperatorDeployCostPlayer>().DeployCost += 2;
				deployBonusGranted = true;
			}
		}

		public override void OnRespawn() {
			if (TexasHelmetActive)
				Player.GetModPlayer<OperatorDeployCostPlayer>().DeployCost += 2;
		}

		public override void UpdateDead() {
			deployBonusGranted = false;
		}
	}
}
