using System;
using ArknightsMod.Common.GlobalNPCs;
using ArknightsMod.Content.Buffs.ArmorSets;
using ArknightsMod.Content.Items.Armor;
using ArknightsMod.Content.Items.Armor.NeoArmorReforge;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Armor.Guard.Entelechia
{
	internal class EntelechiaSetPlayer : ArknightsArmorPlayer
	{
		public const int MaxBonusLife = 180;
		public const int LifePerHit = 10;

		public bool EntelechiaHelmetActive;
		public bool EntelechiaSetActive;

		public int BonusLifeFromSet;

		public override void ResetEffects() {
			EntelechiaHelmetActive = false;
			EntelechiaSetActive = false;
		}

		// NeoArmor Reforge：套装件是独立 ItemID，穿上它本身就代表"已经是套装形态"，
		// 不需要再查 hasUpgraded；player.setBonus 文本交给 EntelechiaHead 的
		// SetProfile.SetBonusKey 统一设置，这里不再重复设置一遍。
		public override void PostUpdateEquips() {
			EntelechiaHelmetActive = Player.armor[0].type == NeoArmorReforgeSetLoader.GetSetType<EntelechiaHead>();
			EntelechiaSetActive = EntelechiaHelmetActive
				&& Player.armor[1].type == NeoArmorReforgeSetLoader.GetSetType<EntelechiaBody>()
				&& Player.armor[2].type == NeoArmorReforgeSetLoader.GetSetType<EntelechiaLegs>();
		}

		public override void ModifyMaxStats(out StatModifier health, out StatModifier mana) {
			health = StatModifier.Default;
			mana = StatModifier.Default;

			if (BonusLifeFromSet > 0)
				health.Base += BonusLifeFromSet;
		}

		public override void OnHitNPCWithItem(Item item, NPC target, NPC.HitInfo hit, int damageDone) {
			OnMeleeHit(item, target, damageDone);
		}

		public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone) {
			OnMeleeHit(proj, target, damageDone);
		}

		private void OnMeleeHit(Item item, NPC target, int damageDone) {
			if (damageDone <= 0 || !item.DamageType.CountsAsClass(DamageClass.Melee))
				return;

			if (EntelechiaHelmetActive)
				ApplyBleed(target);

			if (EntelechiaSetActive)
				ApplyLifeSteal(target);
		}

		private void OnMeleeHit(Projectile proj, NPC target, int damageDone) {
			if (damageDone <= 0 || !proj.DamageType.CountsAsClass(DamageClass.Melee))
				return;

			if (EntelechiaHelmetActive)
				ApplyBleed(target);

			if (EntelechiaSetActive)
				ApplyLifeSteal(target);
		}

		private static void ApplyBleed(NPC target) {
			if (Main.netMode == NetmodeID.MultiplayerClient)
				return;

			if (target.friendly || target.lifeMax <= 5 || target.dontTakeDamage)
				return;

			target.AddBuff(ModContent.BuffType<EntelechiaBleedDebuff>(), 5 * 60);
		}

		private void ApplyLifeSteal(NPC target) {
			if (Main.netMode == NetmodeID.MultiplayerClient)
				return;

			if (target.friendly || target.lifeMax <= 5 || target.dontTakeDamage)
				return;

			EntelechiaGlobalNPC data = target.GetGlobalNPC<EntelechiaGlobalNPC>();
			data.LifeMaxReduction += LifePerHit;
			target.lifeMax = Math.Max(target.lifeMax - LifePerHit, 5);
			if (target.life > target.lifeMax)
				target.life = target.lifeMax;

			if (BonusLifeFromSet < MaxBonusLife)
				BonusLifeFromSet = Math.Min(BonusLifeFromSet + LifePerHit, MaxBonusLife);

			target.netUpdate = true;
		}

		public override void UpdateDead() {
			BonusLifeFromSet = 0;
		}
	}
}
