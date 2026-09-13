using ArknightsMod.Content.Items.Armor;
using ArknightsMod.Content.Items.Armor.NeoArmorReforge;
using Terraria;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Armor.Sniper.Kroos
{
	internal class KroosSetPlayer : ArknightsArmorPlayer
	{
		public bool KroosHelmetActive;
		public bool KroosSetActive;

		public override void ResetEffects() {
			KroosHelmetActive = false;
			KroosSetActive = false;
		}

		// NeoArmor Reforge：套装件是独立 ItemID，穿上它本身就代表"已经是套装形态"，
		// 不需要再查 hasUpgraded；player.setBonus 文本交给 KroosHead 的
		// SetProfile.SetBonusKey 统一设置，这里不再重复设置一遍。
		public override void PostUpdateEquips() {
			KroosHelmetActive = Player.armor[0].type == NeoArmorReforgeSetLoader.GetSetType<KroosHead>();
			KroosSetActive = KroosHelmetActive
				&& Player.armor[1].type == NeoArmorReforgeSetLoader.GetSetType<KroosBody>()
				&& Player.armor[2].type == NeoArmorReforgeSetLoader.GetSetType<KroosLegs>();
		}

		public override bool CanConsumeAmmo(Item weapon, Item ammo) {
			if (KroosHelmetActive
				&& weapon.DamageType.CountsAsClass(DamageClass.Ranged)
				&& Main.rand.NextFloat() < 0.4f) {
				return false;
			}

			return base.CanConsumeAmmo(weapon, ammo);
		}

		public override void ModifyHitNPCWithItem(Item item, NPC target, ref NPC.HitModifiers modifiers) {
			if (KroosSetActive && item.DamageType.CountsAsClass(DamageClass.Ranged) && Main.rand.NextFloat() < 0.1f)
				modifiers.SourceDamage *= 1.5f;
		}

		public override void ModifyHitNPCWithProj(Projectile proj, NPC target, ref NPC.HitModifiers modifiers) {
			if (KroosSetActive && proj.DamageType.CountsAsClass(DamageClass.Ranged) && Main.rand.NextFloat() < 0.1f)
				modifiers.SourceDamage *= 1.5f;
		}
	}
}
