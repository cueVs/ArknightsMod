using ArknightsMod.Content.Projectiles.Supporter.Tragodia;
using ArknightsMod.Content.Projectiles.Supporter.Tragodia.SP2;
using ArknightsMod.Content.Projectiles.Supporter.Tragodia.SP1;
using ArknightsMod.Content.Projectiles.Supporter.Tragodia.SP3;
using ArknightsMod.Content.Projectiles.Supporter.Tragodia.EffectImage;
using ArknightsMod.Content.Projectiles.Supporter.Tragodia.NormalAttack;
using ArknightsMod.Players;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using ArknightsMod.Content.ElementalImpairment.Effect;

namespace ArknightsMod.Content.Projectiles.Supporter.Tragodia.SP3
{
	public class SP3TragodiaNormalAttack : TragodiaNormalAttack
	{
		protected override float DamageMultiplier => 2.25f;
		protected override float ImpairmentInnerRadius => 80f;
		protected override float ImpairmentOuterRadius => 160f;
		protected override float ImpairmentInnerRatio => 0.4f;
		protected override float ImpairmentOuterRatio => 0.2f;

		private const int DotInterval = 20;
		private const int DotDuration = 300;

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

			if (hasDealtDamage && frameCounter >= DotDuration) {
				Projectile.Kill();
			}
		}

		protected override void DealAreaDamage() {
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
		}

		protected override void ApplyImpairment(NPC npc, float ratio) {
			int damage = baseWeaponDamage > 0 ? baseWeaponDamage : Projectile.damage;
			if (damage <= 0)
				return;

			int nerveValue = (int)(damage * ratio);
			if (nerveValue > 0) {
				npc.GetGlobalNPC<AfflictionGlobalNPC>().Container
					.AddAfflictionValue<NervousImpairment>(nerveValue);

				var sp3NPC = npc.GetGlobalNPC<SP3NervousImpairmentNPC>();
				sp3NPC.isMarkedBySP3 = true;
				sp3NPC.markOwner = Projectile.owner;
				sp3NPC.dotDamage = (int)(damage * 0.1f);
				if (sp3NPC.dotDamage < 1)
					sp3NPC.dotDamage = 1;
				sp3NPC.dotInterval = DotInterval;
				sp3NPC.dotTimer = 0;
			}
		}

		public override bool PreDraw(ref Color lightColor) => false;
	}

	public class SP3NervousImpairmentNPC : GlobalNPC
	{
		public bool isMarkedBySP3 = false;
		public int markOwner = -1;
		public int dotDamage = 0;
		public int dotInterval = 20;
		public int dotTimer = 0;
		public int dotTotalDuration = 0;
		public const int MaxDotDuration = 300;

		private AfflictionState previousNervousState = AfflictionState.Idle;

		public override bool InstancePerEntity => true;

		public override void PostAI(NPC npc) {
			if (isMarkedBySP3 && !IsPlayerSP3Active()) {
				ClearMark();
				return;
			}

			if (!isMarkedBySP3)
				return;

			var container = npc.GetGlobalNPC<AfflictionGlobalNPC>().Container;
			if (container != null) {
				var nervous = container.GetOrAdd<NervousImpairment>();
				if (nervous != null) {
					// ÀäÈ´¼ÓËÙ
					if (nervous.State == AfflictionState.Cooldown && nervous.CooldownTimer > 0)
						nervous.CooldownTimer--;

	
					
					previousNervousState = nervous.State;
				}
			}

			dotTotalDuration++;
			dotTimer++;
			if (dotTimer >= dotInterval) {
				dotTimer = 0;
				if (dotDamage > 0)
					container?.AddAfflictionValue<NervousImpairment>(dotDamage);
			}

			if (dotTotalDuration >= MaxDotDuration || !IsPlayerSP3Active())
				ClearMark();
		}

		

		private bool IsPlayerSP3Active() {
			if (markOwner < 0 || markOwner >= Main.player.Length)
				return false;
			Player player = Main.player[markOwner];
			if (player == null || !player.active)
				return false;
			var modPlayer = player.GetModPlayer<WeaponPlayer>();
			return modPlayer.Skill == 2 && modPlayer.SkillActive;
		}

		public void ClearMark() {
			isMarkedBySP3 = false;
			dotDamage = 0;
			dotTotalDuration = 0;
			dotTimer = 0;
			markOwner = -1;
		}

		public override void ResetEffects(NPC npc) {
			if (isMarkedBySP3 && !IsPlayerSP3Active())
				ClearMark();
		}
	}
}