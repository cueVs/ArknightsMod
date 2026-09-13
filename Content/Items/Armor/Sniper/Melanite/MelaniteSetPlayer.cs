using ArknightsMod.Content.Items.Armor;
using ArknightsMod.Content.Items.Armor.NeoArmorReforge;
using ArknightsMod.Players;
using Terraria;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Armor.Sniper.Melanite
{
	internal class MelaniteSetPlayer : ArknightsArmorPlayer
	{
		public bool MelaniteHelmetActive;
		public bool MelaniteSetActive;

		private bool firstSkillBonusPending;
		private bool spawnGranted;
		private bool lastBossActive;
		private bool skillBonusActive;

		public override void ResetEffects() {
			MelaniteHelmetActive = false;
			MelaniteSetActive = false;
		}

		// NeoArmor Reforge：套装件是独立 ItemID，穿上它本身就代表"已经是套装形态"，
		// 不需要再查 hasUpgraded；player.setBonus 文本交给 MelaniteHead 的
		// SetProfile.SetBonusKey 统一设置，这里不再重复设置一遍。
		public override void PostUpdateEquips() {
			MelaniteHelmetActive = Player.armor[0].type == NeoArmorReforgeSetLoader.GetSetType<MelaniteHead>();
			MelaniteSetActive = MelaniteHelmetActive
				&& Player.armor[1].type == NeoArmorReforgeSetLoader.GetSetType<MelaniteBody>()
				&& Player.armor[2].type == NeoArmorReforgeSetLoader.GetSetType<MelaniteLegs>();
		}

		public override void ModifyWeaponDamage(Item item, ref StatModifier damage) {
			if (MelaniteHelmetActive && item.DamageType.CountsAsClass(DamageClass.Ranged))
				damage *= 1.2f;

			if (skillBonusActive && item.DamageType.CountsAsClass(DamageClass.Ranged))
				damage *= 1.2f;
		}

		public override void PostUpdate() {
			if (!MelaniteSetActive) {
				spawnGranted = false;
				lastBossActive = false;
				skillBonusActive = false;
				return;
			}

			bool bossActive = OperatorSetBossHelper.AnyBossActive();
			if (!spawnGranted || (bossActive && !lastBossActive)) {
				firstSkillBonusPending = true;
				spawnGranted = true;
			}

			lastBossActive = bossActive;

			WeaponPlayer wp = Player.GetModPlayer<WeaponPlayer>();
			if (firstSkillBonusPending && wp.SkillActive) {
				firstSkillBonusPending = false;
				skillBonusActive = true;
			}

			if (!wp.SkillActive)
				skillBonusActive = false;

			if (Player.dead) {
				spawnGranted = false;
				lastBossActive = false;
				firstSkillBonusPending = false;
				skillBonusActive = false;
			}
		}

		public override void OnRespawn() {
			if (MelaniteSetActive)
				firstSkillBonusPending = true;
		}
	}
}
