using ArknightsMod.Content.Items.Armor;
using ArknightsMod.Content.Items.Armor.NeoArmorReforge;
using Terraria;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Armor.Sniper.Catapult
{
	internal class CatapultSetPlayer : ArknightsArmorPlayer
	{
		public bool CatapultHelmetActive;
		public bool CatapultSetActive;

		private bool spawnSpGranted;

		public override void ResetEffects() {
			CatapultHelmetActive = false;
			CatapultSetActive = false;
		}

		// NeoArmor Reforge：套装件是独立 ItemID，穿上它本身就代表"已经是套装形态"，
		// 不需要再查 hasUpgraded；player.setBonus 文本交给 CatapultHead 的
		// SetProfile.SetBonusKey 统一设置，这里不再重复设置一遍。
		public override void PostUpdateEquips() {
			CatapultHelmetActive = Player.armor[0].type == NeoArmorReforgeSetLoader.GetSetType<CatapultHead>();
			CatapultSetActive = CatapultHelmetActive
				&& Player.armor[1].type == NeoArmorReforgeSetLoader.GetSetType<CatapultBody>()
				&& Player.armor[2].type == NeoArmorReforgeSetLoader.GetSetType<CatapultLegs>();
		}

		public override void ModifyWeaponDamage(Item item, ref StatModifier damage) {
			if (CatapultSetActive && item.DamageType.CountsAsClass(DamageClass.Ranged))
				damage *= 1.03f;
		}

		public override void PostUpdate() {
			if (!CatapultSetActive)
				return;

			if (!Player.dead && !spawnSpGranted) {
				OperatorSPHelper.TryGainSP(Player, 5);
				spawnSpGranted = true;
			}

			if (Player.dead)
				spawnSpGranted = false;
		}

		public override void OnRespawn() {
			if (CatapultSetActive)
				OperatorSPHelper.TryGainSP(Player, 5);
		}
	}
}
