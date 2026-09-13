using ArknightsMod.Content.Buffs.ArmorSets;
using ArknightsMod.Content.Items.Armor;
using ArknightsMod.Content.Items.Armor.NeoArmorReforge;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Armor.Supporter.Orchid
{
	internal class OrchidSetPlayer : ArknightsArmorPlayer
	{
		public bool OrchidHelmetActive;
		public bool OrchidSetActive;

		public override void ResetEffects() {
			OrchidHelmetActive = false;
			OrchidSetActive = false;
		}

		// NeoArmor Reforge：套装件是独立 ItemID，穿上它本身就代表"已经是套装形态"，
		// 不需要再查 hasUpgraded；player.setBonus 文本交给 OrchidHead 的
		// SetProfile.SetBonusKey 统一设置，这里不再重复设置一遍。
		public override void PostUpdateEquips() {
			OrchidHelmetActive = Player.armor[0].type == NeoArmorReforgeSetLoader.GetSetType<OrchidHead>();
			OrchidSetActive = OrchidHelmetActive
				&& Player.armor[1].type == NeoArmorReforgeSetLoader.GetSetType<OrchidBody>()
				&& Player.armor[2].type == NeoArmorReforgeSetLoader.GetSetType<OrchidLegs>();
		}

		public override void ModifyWeaponDamage(Item item, ref StatModifier damage) {
			if (OrchidHelmetActive && item.DamageType.CountsAsClass(DamageClass.Magic))
				damage *= 1.1f;
		}

		public override void OnHitNPCWithItem(Item item, NPC target, NPC.HitInfo hit, int damageDone) {
			TrySlow(item, target, damageDone);
		}

		public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone) {
			TrySlow(proj, target, damageDone);
		}

		private void TrySlow(Item item, NPC target, int damageDone) {
			if (!OrchidSetActive || damageDone <= 0 || !item.DamageType.CountsAsClass(DamageClass.Magic))
				return;

			ApplySlow(target);
		}

		private void TrySlow(Projectile proj, NPC target, int damageDone) {
			if (!OrchidSetActive || damageDone <= 0 || !proj.DamageType.CountsAsClass(DamageClass.Magic))
				return;

			ApplySlow(target);
		}

		private static void ApplySlow(NPC target) {
			if (Main.netMode == NetmodeID.MultiplayerClient)
				return;

			if (target.friendly || target.lifeMax <= 5 || target.dontTakeDamage || target.boss)
				return;

			target.AddBuff(ModContent.BuffType<OrchidSlowDebuff>(), 48);
		}
	}
}
