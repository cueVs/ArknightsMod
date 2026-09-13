using ArknightsMod.Content.Items.Armor;
using ArknightsMod.Content.Items.Armor.NeoArmorReforge;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Armor.Caster.Amiya
{
	internal class AmiyaSetPlayer : ArknightsArmorPlayer
	{
		public bool AmiyaHelmetActive;
		public bool AmiyaSetActive;

		public override void ResetEffects() {
			AmiyaHelmetActive = false;
			AmiyaSetActive = false;
		}

		// NeoArmor Reforge：套装件是独立 ItemID，穿上它本身就代表"已经是套装形态"，
		// 不需要再查 hasUpgraded；player.setBonus 文本交给 AmiyaHead 的
		// SetProfile.SetBonusKey 统一设置，这里不再重复设置一遍。
		public override void PostUpdateEquips() {
			AmiyaHelmetActive = Player.armor[0].type == NeoArmorReforgeSetLoader.GetSetType<AmiyaHead>();
			AmiyaSetActive = AmiyaHelmetActive
				&& Player.armor[1].type == NeoArmorReforgeSetLoader.GetSetType<AmiyaBody>()
				&& Player.armor[2].type == NeoArmorReforgeSetLoader.GetSetType<AmiyaLegs>();
		}

		public override void ModifyManaCost(Item item, ref float reduce, ref float mult) {
			if (AmiyaHelmetActive)
				mult *= 0.8f;
		}

		public override void OnHitNPCWithItem(Item item, NPC target, NPC.HitInfo hit, int damageDone) {
			TryGainSpOnMagicHit(item, target, damageDone);
		}

		public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone) {
			TryGainSpOnMagicHit(proj, target, damageDone);
		}

		private void TryGainSpOnMagicHit(Item item, NPC target, int damageDone) {
			if (!AmiyaSetActive || damageDone <= 0 || !item.DamageType.CountsAsClass(DamageClass.Magic))
				return;

			int gain = target.boss ? 4 : 2;
			OperatorSPHelper.TryGainSP(Player, gain);

			if (target.life <= 0)
				OperatorSPHelper.TryGainSP(Player, 8);
		}

		private void TryGainSpOnMagicHit(Projectile proj, NPC target, int damageDone) {
			if (!AmiyaSetActive || damageDone <= 0 || !proj.DamageType.CountsAsClass(DamageClass.Magic))
				return;

			int gain = target.boss ? 4 : 2;
			OperatorSPHelper.TryGainSP(Player, gain);

			if (target.life <= 0)
				OperatorSPHelper.TryGainSP(Player, 8);
		}
	}
}
