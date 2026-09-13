using System;
using ArknightsMod.Content.Items.Weapons.Supporter.Deepcolor;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Projectiles.Supporter.Deepcolor
{
	// ai[0] 阶段，ai[1] 计时，ai[2] 右键松开；打击坐标见 DeepcolorSketchPlayer.LogoStrikeWorld
	public class DeepcolorSketchLogoAttack : ModProjectile
	{
		public override string Texture => ArknightsMod.noTexture;

		private enum ChargePhase : byte
		{
			FrontDraw,
			FrontHold,
			FrontFade,
		}

		private const int FrontDrawTicks = 36;
		private const int FrontFadeTicks = 18;
		private const float FrontOffsetNear = 38f;
		private const float FrontOffsetLaunch = 50f;
		private const float FadeScaleEnd = 1.55f;

		private ref float Phase => ref Projectile.ai[0];
		private ref float PhaseTimer => ref Projectile.ai[1];
		private ref float MouseReleased => ref Projectile.ai[2];

		private bool strikeSpawned;

		public override void SetStaticDefaults() {
			ProjectileID.Sets.TrailCacheLength[Type] = 0;
			ProjectileID.Sets.DrawScreenCheckFluff[Type] = 400;
		}

		public override void SetDefaults() {
			Projectile.width = 8;
			Projectile.height = 8;
			Projectile.friendly = true;
			Projectile.hostile = false;
			Projectile.penetrate = -1;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
			Projectile.timeLeft = 600;
			Projectile.DamageType = DamageClass.Magic;
			Projectile.aiStyle = -1;
		}

		public override void OnSpawn(IEntitySource source) {
			Phase = (float)ChargePhase.FrontDraw;
			PhaseTimer = 0f;
			MouseReleased = 0f;
		}

		public override void AI() {
			Player owner = Main.player[Projectile.owner];
			if (!owner.active) {
				Projectile.Kill();
				return;
			}

			Projectile.Center = owner.Center;
			Projectile.timeLeft = Math.Max(Projectile.timeLeft, 3);

			SyncMouseReleased();

			var phase = (ChargePhase)(int)Phase;
			PhaseTimer++;

			switch (phase) {
				case ChargePhase.FrontDraw:
					if (PhaseTimer >= FrontDrawTicks)
						BeginPostDrawPhase();
					break;
				case ChargePhase.FrontHold:
					if (MouseReleased >= 1f)
						BeginFrontFade();
					break;
				case ChargePhase.FrontFade:
					TrySpawnStrike();
					if (PhaseTimer >= FrontFadeTicks)
						Projectile.Kill();
					break;
			}

			var state = BuildDrawState(owner, phase);
			if (state.Alpha > 0.01f) {
				if (phase != ChargePhase.FrontFade || PhaseTimer / (float)FrontFadeTicks <= 0.85f)
					DeepcolorLogoRenderer.EmitLogoLight(state.WorldPoints, state.Alpha);

				if (phase != ChargePhase.FrontFade || PhaseTimer / (float)FrontFadeTicks <= 0.85f)
					DeepcolorLogoEffects.SpawnChargeParticles(state.WorldPoints.GetCentroid());
			}
		}

		private void SyncMouseReleased() {
			if (Projectile.owner != Main.myPlayer || Main.mouseRight)
				return;

			if (MouseReleased < 1f) {
				MouseReleased = 1f;
				NetSync();
			}
		}

		private void BeginPostDrawPhase() {
			PhaseTimer = 0f;
			if (MouseReleased >= 1f) {
				Phase = (float)ChargePhase.FrontFade;
				TrySpawnStrike();
			}
			else {
				Phase = (float)ChargePhase.FrontHold;
			}

			NetSync();
		}

		private void BeginFrontFade() {
			Phase = (float)ChargePhase.FrontFade;
			PhaseTimer = 0f;
			TrySpawnStrike();
			NetSync();
		}

		private void NetSync() {
			if (Projectile.owner == Main.myPlayer)
				Projectile.netUpdate = true;
		}

		private void TrySpawnStrike() {
			if (strikeSpawned || Projectile.owner != Main.myPlayer)
				return;

			strikeSpawned = true;
			Vector2 strikePos = Main.player[Projectile.owner].GetModPlayer<DeepcolorSketchPlayer>().LogoStrikeWorld;
			Projectile.NewProjectile(
				Projectile.GetSource_FromThis(),
				strikePos,
				Vector2.Zero,
				ModContent.ProjectileType<DeepcolorSketchLogoStrike>(),
				Projectile.damage,
				Projectile.knockBack,
				Projectile.owner);
		}

		private DrawState BuildDrawState(Player owner, ChargePhase phase) {
			float chargeDirection = DeepcolorLogoRenderer.GetChargeDrawDirection(owner);
			float fadeT = phase == ChargePhase.FrontFade
				? MathHelper.Clamp(PhaseTimer / FrontFadeTicks, 0f, 1f)
				: 0f;

			float drawProgress = phase == ChargePhase.FrontDraw
				? MathHelper.Clamp((PhaseTimer + 1f) / FrontDrawTicks, 0f, 1f)
				: 1f;
			float scale = phase == ChargePhase.FrontFade
				? MathHelper.Lerp(1f, FadeScaleEnd, fadeT)
				: 1f;
			float alpha = GetDrawAlpha(phase);

			var local = DeepcolorLogoPoints.CreateDefault()
				.WithHorizontalScale(DeepcolorLogoRenderer.ChargePerspectiveXScale);
			var worldPoints = local.ToWorldPoints(
				GetChargeCenter(owner, phase, fadeT, chargeDirection),
				chargeDirection,
				DeepcolorLogoRenderer.DefaultWorldScale * scale);

			return new DrawState(worldPoints, drawProgress, scale, alpha);
		}

		private static Vector2 GetChargeCenter(Player owner, ChargePhase phase, float fadeT, float chargeDirection) {
			int facing = owner.direction == 0 ? 1 : owner.direction;
			float offsetX = phase == ChargePhase.FrontFade
				? MathHelper.Lerp(FrontOffsetNear, FrontOffsetLaunch, fadeT * (2f - fadeT))
				: FrontOffsetNear;

			Vector2 center = owner.Center + new Vector2(facing * offsetX, 0f);
			if (phase == ChargePhase.FrontHold) {
				float t = Main.GameUpdateCount * 0.38f;
				center += new Vector2(
					MathF.Sin(t * 1.17f) * 1.6f * chargeDirection,
					MathF.Cos(t * 1.43f) * 0.9f);
			}

			return center;
		}

		private float GetDrawAlpha(ChargePhase phase) {
			if (phase != ChargePhase.FrontFade)
				return 1f;

			return 1f - MathHelper.Clamp(PhaseTimer / FrontFadeTicks, 0f, 1f);
		}

		public override bool PreDraw(ref Color lightColor) {
			Player owner = Main.player[Projectile.owner];
			if (!owner.active)
				return false;

			var phase = (ChargePhase)(int)Phase;
			var state = BuildDrawState(owner, phase);

			if (phase == ChargePhase.FrontDraw)
				DeepcolorLogoRenderer.DrawLogo(Main.spriteBatch, state.WorldPoints, state.DrawProgress, state.Scale, state.Alpha);
			else
				DeepcolorLogoRenderer.DrawChargeLogoFull(Main.spriteBatch, state.WorldPoints, state.Scale, state.Alpha);

			return false;
		}

		public override bool? CanDamage() => false;

		private readonly struct DrawState(
			DeepcolorLogoPoints worldPoints,
			float drawProgress,
			float scale,
			float alpha) {
			public DeepcolorLogoPoints WorldPoints { get; } = worldPoints;
			public float DrawProgress { get; } = drawProgress;
			public float Scale { get; } = scale;
			public float Alpha { get; } = alpha;
		}
	}
}
