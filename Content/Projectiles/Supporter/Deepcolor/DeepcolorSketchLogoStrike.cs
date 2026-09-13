using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Projectiles.Supporter.Deepcolor
{
	public class DeepcolorSketchLogoStrike : ModProjectile
	{
		public override string Texture => ArknightsMod.noTexture;

		private enum StrikePhase : byte
		{
			FadeIn,
			Hit,
			FadeOut,
		}

		private const int FadeInTicks = 16;
		private const int HitTicks = 4;
		private const int FadeOutTicks = 14;
		private const float ScalePeak = 1.28f;
		private const float ScaleEnd = 1.05f;
		private const float HitRadius = 56f;
		private const float RingThickness = 14f;

		private ref float Phase => ref Projectile.ai[0];
		private ref float PhaseTimer => ref Projectile.ai[1];

		private bool dealtDamage;
		private bool strikeBurstSpawned;
		private LogoShockwaveState shockwave;

		public override void SetStaticDefaults() {
			ProjectileID.Sets.TrailCacheLength[Type] = 0;
			ProjectileID.Sets.DrawScreenCheckFluff[Type] = 1200;
		}

		public override void SetDefaults() {
			Projectile.width = 96;
			Projectile.height = 96;
			Projectile.friendly = true;
			Projectile.hostile = false;
			Projectile.penetrate = -1;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
			Projectile.timeLeft = FadeInTicks + HitTicks + FadeOutTicks + 10;
			Projectile.DamageType = DamageClass.Magic;
			Projectile.aiStyle = -1;
		}

		public override void OnSpawn(IEntitySource source) {
			Phase = (float)StrikePhase.FadeIn;
			PhaseTimer = 0f;
			TriggerStrikeVisuals();
			Projectile.netUpdate = true;
		}

		public override void AI() {
			Player owner = Main.player[Projectile.owner];
			if (!owner.active) {
				Projectile.Kill();
				return;
			}

			var phase = (StrikePhase)(int)Phase;
			PhaseTimer++;

			switch (phase) {
				case StrikePhase.FadeIn:
					if (PhaseTimer >= FadeInTicks)
						AdvancePhase(StrikePhase.Hit);
					break;
				case StrikePhase.Hit:
					if (Main.netMode != NetmodeID.MultiplayerClient)
						TryDealDamage(owner);
					if (PhaseTimer >= HitTicks)
						AdvancePhase(StrikePhase.FadeOut);
					break;
				case StrikePhase.FadeOut:
					if (PhaseTimer >= FadeOutTicks)
						Projectile.Kill();
					break;
			}

			var visual = BuildVisualState(phase);
			if (visual.Alpha > 0.01f)
				DeepcolorLogoRenderer.EmitLogoLight(visual.WorldPoints, visual.Alpha, intensityScale: 1.15f);

			shockwave.Update();
		}

		private void AdvancePhase(StrikePhase next) {
			Phase = (float)next;
			PhaseTimer = 0f;
			Projectile.netUpdate = true;
		}

		private void TriggerStrikeVisuals() {
			if (strikeBurstSpawned)
				return;

			strikeBurstSpawned = true;
			DeepcolorLogoEffects.SpawnStrikeBurst(Projectile.Center);
			shockwave.Trigger();
		}

		private (float Scale, float Alpha, DeepcolorLogoPoints WorldPoints) BuildVisualState(StrikePhase phase) {
			float fadeInT = phase == StrikePhase.FadeIn
				? MathHelper.Clamp((PhaseTimer + 1f) / FadeInTicks, 0f, 1f)
				: 1f;
			float fadeOutT = phase == StrikePhase.FadeOut
				? MathHelper.Clamp(PhaseTimer / FadeOutTicks, 0f, 1f)
				: 0f;

			float scale = phase == StrikePhase.FadeOut
				? MathHelper.Lerp(ScalePeak, ScaleEnd, fadeOutT)
				: ScalePeak;
			float alpha = phase switch {
				StrikePhase.FadeIn => fadeInT,
				StrikePhase.FadeOut => 1f - fadeOutT,
				_ => 1f,
			};

			return (scale, alpha, BuildWorldPoints(scale));
		}

		private DeepcolorLogoPoints BuildWorldPoints(float scale) {
			Player owner = Main.player[Projectile.owner];
			float direction = Projectile.Center.X >= owner.Center.X ? 1f : -1f;
			float worldScale = DeepcolorLogoRenderer.DefaultWorldScale * scale * DeepcolorLogoRenderer.StrikeWorldScaleBonus;
			var local = DeepcolorLogoPoints.CreateDefault();
			Vector2 anchor = local.GetAnchorForTriangleCentroidAt(Projectile.Center, direction, worldScale);
			return local.ToWorldPoints(anchor, direction, worldScale);
		}

		private void TryDealDamage(Player owner) {
			if (dealtDamage)
				return;

			dealtDamage = true;
			float radiusSq = HitRadius * HitRadius;
			Vector2 center = Projectile.Center;

			for (int i = 0; i < Main.maxNPCs; i++) {
				NPC npc = Main.npc[i];
				if (!npc.active || !npc.CanBeChasedBy(Projectile))
					continue;
				if (Vector2.DistanceSquared(npc.Center, center) > radiusSq)
					continue;

				var modifiers = new NPC.HitModifiers { DamageType = DamageClass.Magic };
				ModifyHitNPC(npc, ref modifiers);

				bool crit = Main.rand.Next(100) < owner.GetTotalCritChance(DamageClass.Magic);
				NPC.HitInfo hit = modifiers.ToHitInfo(Projectile.damage, crit, Projectile.knockBack, damageVariation: true);
				hit.HitDirection = center.X >= npc.Center.X ? 1 : -1;
				npc.StrikeNPC(hit);
			}
		}

		public override bool PreDraw(ref Color lightColor) {
			Player owner = Main.player[Projectile.owner];
			if (!owner.active)
				return false;

			if (shockwave.Active)
				DeepcolorLogoEffects.DrawShockwaveRing(Main.spriteBatch, Projectile.Center, shockwave.Radius, RingThickness, shockwave.Alpha);

			var (scale, alpha, worldPoints) = BuildVisualState((StrikePhase)(int)Phase);
			DeepcolorLogoRenderer.DrawStrikeLogoFull(Main.spriteBatch, worldPoints, scale, alpha);
			return false;
		}

		public override bool? CanDamage() => false;
	}
}
