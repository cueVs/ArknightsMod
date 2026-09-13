using ArknightsMod.Content.Items.Armor;
using ArknightsMod.Content.Items.Armor.NeoArmorReforge;
using Terraria;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Armor.Specialist.Manticore
{
	internal class ManticoreSetPlayer : ArknightsArmorPlayer
	{
		public bool ManticoreHelmetActive;
		public bool ManticoreSetActive;

		public bool Stealthed;
		public bool BreakStealthBonus;
		private int noAttackTimer;

		public override void ResetEffects() {
			ManticoreHelmetActive = false;
			ManticoreSetActive = false;
		}

		// NeoArmor Reforge：套装件是独立 ItemID，穿上它本身就代表"已经是套装形态"，
		// 不需要再查 hasUpgraded；player.setBonus 文本交给 ManticoreHead 的
		// SetProfile.SetBonusKey 统一设置，这里不再重复设置一遍。
		public override void PostUpdateEquips() {
			ManticoreHelmetActive = Player.armor[0].type == NeoArmorReforgeSetLoader.GetSetType<ManticoreHead>();
			ManticoreSetActive = ManticoreHelmetActive
				&& Player.armor[1].type == NeoArmorReforgeSetLoader.GetSetType<ManticoreBody>()
				&& Player.armor[2].type == NeoArmorReforgeSetLoader.GetSetType<ManticoreLegs>();
		}

		public override void ModifyHurt(ref Player.HurtModifiers modifiers) {
			if (ManticoreHelmetActive && Main.rand.NextFloat() < 0.1f)
				modifiers.Cancel();
		}

		public override void PostUpdate() {
			if (!ManticoreSetActive)
				return;

			bool attacking = Player.itemAnimation > 0;
			if (attacking) {
				if (Stealthed)
					BreakStealthBonus = true;

				Stealthed = false;
				noAttackTimer = 0;
			}
			else {
				noAttackTimer++;
				if (noAttackTimer >= 150)
					Stealthed = true;
			}

			if (Stealthed)
				Player.aggro -= 1200;
		}

		public override void ModifyWeaponDamage(Item item, ref StatModifier damage) {
			if (ManticoreSetActive && BreakStealthBonus) {
				damage *= 1.5f;
				BreakStealthBonus = false;
			}
		}
	}
}
