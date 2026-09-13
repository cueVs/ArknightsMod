using ArknightsMod.Players;
using ArknightsMod.Systems.Gameplay.Skill;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ModLoader;
using ArknightsMod.Content.ElementalImpairment.Effect;
using ArknightsMod.Content.Projectiles.Supporter.Tragodia;
using ArknightsMod.Content.Projectiles.Supporter.Tragodia.SP2;
using ArknightsMod.Content.Projectiles.Supporter.Tragodia.SP1;
using ArknightsMod.Content.Projectiles.Supporter.Tragodia.EffectImage;
using ArknightsMod.Content.Projectiles.Supporter.Tragodia.NormalAttack;
namespace ArknightsMod.Content.Projectiles.Supporter.Tragodia.NormalAttack
{
	public class TragodiaNormalAttack : ModProjectile
	{
		protected int frameCounter;
		protected bool hasDealtDamage;
		protected int baseWeaponDamage;

		protected virtual float DamageRadius => 50f;
		protected virtual float ImpairmentInnerRadius => 50f;
		protected virtual float ImpairmentOuterRadius => 100f;
		protected virtual float ImpairmentInnerRatio => 0.3f;
		protected virtual float ImpairmentOuterRatio => 0.15f;
		protected virtual float DamageMultiplier => 1f;

		public override void SetDefaults() {
			Projectile.width = 30;
			Projectile.height = 30;
			Projectile.friendly = true;
			Projectile.hostile = false;
			Projectile.DamageType = DamageClass.Magic;
			Projectile.penetrate = -1;
			Projectile.timeLeft = 300;
			Projectile.alpha = 255;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
			Projectile.aiStyle = -1;
		}

		public override void AI() {
			frameCounter++;
			if (frameCounter == 1) {
				Player player = Main.player[Projectile.owner];
				if (player != null && player.active && player.HeldItem != null)
					baseWeaponDamage = player.HeldItem.damage;
			}

			if (!hasDealtDamage) {
				Projectile.friendly = true;
				DealAreaDamage();
			}
		}

		protected virtual void DealAreaDamage() {
			if (hasDealtDamage)
				return;
			Player player = Main.player[Projectile.owner];
			if (player == null || !player.active)
				return;
			hasDealtDamage = true;

			foreach (NPC npc in Main.ActiveNPCs) {
				if (!npc.CanBeChasedBy(player) || npc.friendly)
					continue;
				float dist = Vector2.Distance(Projectile.Center, npc.Center);

				if (dist <= DamageRadius) {
					int dmg = (int)((Projectile.damage + Main.rand.Next(-5, 6)) * DamageMultiplier);
					if (dmg < 1)
						dmg = 1;
					npc.StrikeNPC(new NPC.HitInfo {
						Damage = dmg,
						Knockback = Projectile.knockBack,
						HitDirection = npc.Center.X > Projectile.Center.X ? 1 : -1,
						DamageType = DamageClass.Magic
					});
				}

				if (dist <= ImpairmentInnerRadius)
					ApplyImpairment(npc, ImpairmentInnerRatio);
				else if (dist <= ImpairmentOuterRadius)
					ApplyImpairment(npc, ImpairmentOuterRatio);
			}

			Projectile.friendly = false;
			Projectile.timeLeft = 0;
		}

		protected virtual void ApplyImpairment(NPC npc, float ratio) {
			if (baseWeaponDamage <= 0)
				return;
			int nerveValue = (int)(baseWeaponDamage * ratio);
			if (nerveValue > 0)
				npc.GetGlobalNPC<AfflictionGlobalNPC>().Container
					.AddAfflictionValue<NervousImpairment>(nerveValue);
		}

		public override bool PreDraw(ref Color lightColor) => false;
	}
}