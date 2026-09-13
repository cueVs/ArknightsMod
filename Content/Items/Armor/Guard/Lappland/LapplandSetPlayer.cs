using ArknightsMod.Content.Buffs.ArmorSets;
using ArknightsMod.Content.Items.Armor;
using ArknightsMod.Content.Items.Armor.NeoArmorReforge;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Armor.Guard.Lappland
{
	internal class LapplandSetPlayer : ArknightsArmorPlayer
	{
		public bool LapplandHelmetActive;
		public bool LapplandSetActive;

		public override void ResetEffects() {
			LapplandHelmetActive = false;
			LapplandSetActive = false;
		}

		// NeoArmor Reforge：套装件是独立 ItemID，穿上它本身就代表"已经是套装形态"，
		// 不需要再查 hasUpgraded；player.setBonus 文本交给 LapplandHead 的
		// SetProfile.SetBonusKey 统一设置，这里不再重复设置一遍。
		public override void PostUpdateEquips() {
			LapplandHelmetActive = Player.armor[0].type == NeoArmorReforgeSetLoader.GetSetType<LapplandHead>();
			LapplandSetActive = LapplandHelmetActive
				&& Player.armor[1].type == NeoArmorReforgeSetLoader.GetSetType<LapplandBody>()
				&& Player.armor[2].type == NeoArmorReforgeSetLoader.GetSetType<LapplandLegs>();
		}

		public override void OnHitNPCWithItem(Item item, NPC target, NPC.HitInfo hit, int damageDone) {
			TryBonusMagicDamage(item, target, hit, damageDone);
			TrySilence(target, damageDone);
		}

		public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone) {
			TryBonusMagicDamage(proj, target, hit, damageDone);
			TrySilence(target, damageDone);
		}

		private void TryBonusMagicDamage(Item item, NPC target, NPC.HitInfo hit, int damageDone) {
			if (!LapplandHelmetActive || damageDone <= 0)
				return;

			if (!item.DamageType.CountsAsClass(DamageClass.Melee))
				return;

			if (target.friendly || target.lifeMax <= 5)
				return;

			int bonus = System.Math.Max(1, (int)(hit.Damage * 0.1f));
			target.SimpleStrikeNPC(bonus, 0, false, 0, DamageClass.Magic);
		}

		private void TryBonusMagicDamage(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone) {
			if (!LapplandHelmetActive || damageDone <= 0)
				return;

			if (!proj.DamageType.CountsAsClass(DamageClass.Melee))
				return;

			if (target.friendly || target.lifeMax <= 5)
				return;

			int bonus = System.Math.Max(1, (int)(hit.Damage * 0.1f));
			target.SimpleStrikeNPC(bonus, 0, false, 0, DamageClass.Magic);
		}

		private void TrySilence(NPC target, int damageDone) {
			if (!LapplandSetActive || damageDone <= 0)
				return;

			if (Main.netMode == NetmodeID.MultiplayerClient)
				return;

			if (target.friendly || target.lifeMax <= 5 || target.dontTakeDamage)
				return;

			target.AddBuff(ModContent.BuffType<LapplandSilenceDebuff>(), 5 * 60);
		}
	}
}
