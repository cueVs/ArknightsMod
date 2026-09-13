using ArknightsMod.Content.Items.Armor;
using ArknightsMod.Content.Items.Armor.NeoArmorReforge;
using Terraria;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Armor.Guard.LaPluma
{
	internal class LaPlumaSetPlayer : ArknightsArmorPlayer
	{
		public bool LaPlumaHelmetActive;
		public bool LaPlumaSetActive;

		public int KillSpeedStacks;

		public override void ResetEffects() {
			LaPlumaHelmetActive = false;
			LaPlumaSetActive = false;
		}

		// NeoArmor Reforge：套装件是独立 ItemID，穿上它本身就代表"已经是套装形态"，
		// 不需要再查 hasUpgraded；player.setBonus 文本交给 LaPlumaHead 的
		// SetProfile.SetBonusKey 统一设置，这里不再重复设置一遍。
		public override void PostUpdateEquips() {
			LaPlumaHelmetActive = Player.armor[0].type == NeoArmorReforgeSetLoader.GetSetType<LaPlumaHead>();
			LaPlumaSetActive = LaPlumaHelmetActive
				&& Player.armor[1].type == NeoArmorReforgeSetLoader.GetSetType<LaPlumaBody>()
				&& Player.armor[2].type == NeoArmorReforgeSetLoader.GetSetType<LaPlumaLegs>();
		}

		public override float UseSpeedMultiplier(Item item) {
			if (LaPlumaSetActive && item.DamageType.CountsAsClass(DamageClass.Melee))
				return 1f + 0.03f * KillSpeedStacks;

			return 1f;
		}

		public override void OnHitNPCWithItem(Item item, NPC target, NPC.HitInfo hit, int damageDone) {
			TryHelmetHeal(item, target, damageDone);
			TryAddKillStack(item, target, damageDone);
		}

		public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone) {
			TryHelmetHeal(proj, target, damageDone);
			TryAddKillStack(proj, target, damageDone);
		}

		private void TryHelmetHeal(Item item, NPC target, int damageDone) {
			if (!LaPlumaHelmetActive || damageDone <= 0)
				return;

			if (!item.DamageType.CountsAsClass(DamageClass.Melee))
				return;

			if (target.friendly || target.lifeMax <= 5 || target.dontTakeDamage || target.immortal)
				return;

			if (Player.statLife >= Player.statLifeMax2)
				return;

			Player.Heal(2);
		}

		private void TryHelmetHeal(Projectile proj, NPC target, int damageDone) {
			if (!LaPlumaHelmetActive || damageDone <= 0)
				return;

			if (!proj.DamageType.CountsAsClass(DamageClass.Melee))
				return;

			if (target.friendly || target.lifeMax <= 5 || target.dontTakeDamage || target.immortal)
				return;

			if (Player.statLife >= Player.statLifeMax2)
				return;

			Player.Heal(2);
		}

		private void TryAddKillStack(Item item, NPC target, int damageDone) {
			if (!LaPlumaSetActive || damageDone <= 0)
				return;

			if (!item.DamageType.CountsAsClass(DamageClass.Melee))
				return;

			if (target.life > 0 || target.lifeMax <= 5)
				return;

			KillSpeedStacks = System.Math.Min(12, KillSpeedStacks + 1);
		}

		private void TryAddKillStack(Projectile proj, NPC target, int damageDone) {
			if (!LaPlumaSetActive || damageDone <= 0)
				return;

			if (!proj.DamageType.CountsAsClass(DamageClass.Melee))
				return;

			if (target.life > 0 || target.lifeMax <= 5)
				return;

			KillSpeedStacks = System.Math.Min(12, KillSpeedStacks + 1);
		}

		public override void UpdateDead() {
			KillSpeedStacks = 0;
		}
	}
}
