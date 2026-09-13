using System;
using System.Collections.Generic;
using ArknightsMod.Content.Items.Weapons.Supporter.Pramanix;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Projectiles.Supporter.Pramanix
{
	// 三技能齐射视觉占位投射物，绘制由 PramanixSkill3VisualSystem 负责
	public class PramanixSkill3VolleyFx : ModProjectile
	{
		public const int Lifetime = 52;
		public const int FadeTicks = 16;
		public const int AnimateTicks = Lifetime - FadeTicks;
		public const float GoldTrailSpan = 0.38f;
		public const float TrailHeadGap = 0.07f;
		public const float SparkPathSpacing = 0.085f;
		public const int MistExpandTicks = 18;
		private const int SparkFillInterval = 2;

		private readonly List<(int path, float t)> sparkDots = new();
		private readonly bool[] boltHitsApplied = new bool[PramanixSkill3Geometry.PathCount];
		private int sparkTimer;

		public override string Texture => "ArknightsMod/Assets/null";

		public readonly struct VisualState
		{
			public readonly float TMax;
			public readonly float Alpha;
			public readonly float SpanScale;
			public readonly bool SpawningSparks;

			public VisualState(float tMax, float alpha, float spanScale, bool spawningSparks) {
				TMax = tMax;
				Alpha = alpha;
				SpanScale = spanScale;
				SpawningSparks = spawningSparks;
			}
		}

		public override void SetStaticDefaults() {
			ProjectileID.Sets.DrawScreenCheckFluff[Type] = 2400;
		}

		public override void SetDefaults() {
			Projectile.width = 8;
			Projectile.height = 8;
			Projectile.friendly = false;
			Projectile.hostile = false;
			Projectile.penetrate = -1;
			Projectile.tileCollide = false;
			Projectile.timeLeft = Lifetime;
			Projectile.ignoreWater = true;
		}

		public override void OnSpawn(IEntitySource source) {
			Projectile.timeLeft = Lifetime;
			Player owner = Main.player[Projectile.owner];
			Projectile.ai[0] = owner.direction >= 0 ? 1f : -1f;
			Projectile.ai[1] = Main.rand.NextFloat() * 1000f;
			sparkDots.Clear();
			sparkTimer = 0;
			Array.Clear(boltHitsApplied, 0, boltHitsApplied.Length);
		}

		public int LockedDirection {
			get {
				int dir = (int)Projectile.ai[0];
				return dir == 0 ? 1 : dir;
			}
		}

		public float MistWobbleSeed => Projectile.ai[1];

		// 雾气外扩进度，维持至攻击结束并随 Alpha 淡出
		public float GetMistExpandProgress() {
			int elapsed = Lifetime - Projectile.timeLeft;
			float linear = MathHelper.Clamp(elapsed / (float)MistExpandTicks, 0f, 1f);
			return 1f - (1f - linear) * (1f - linear);
		}

		public IReadOnlyList<(int path, float t)> SparkDots => sparkDots;

		public VisualState GetVisualState() {
			if (Projectile.timeLeft > FadeTicks) {
				float anim = 1f - (Projectile.timeLeft - FadeTicks) / (float)AnimateTicks;
				return new VisualState(anim * PramanixSkill3Geometry.PathL, 1f, 1f, true);
			}

			float fade = Projectile.timeLeft / (float)FadeTicks;
			return new VisualState(PramanixSkill3Geometry.PathL, fade, fade, false);
		}

		public override void AI() {
			Player owner = Main.player[Projectile.owner];
			if (!owner.active || owner.dead) {
				Projectile.Kill();
				return;
			}

			Projectile.Center = owner.Center;

			VisualState state = GetVisualState();
			if (Main.netMode != NetmodeID.MultiplayerClient && owner.GetModPlayer<SaintBellPlayer>() is { Skill3Active: true } sb) {
				TryApplyBoltHits(sb, state);
			}

			if (Main.netMode == NetmodeID.Server)
				return;
			if (!state.SpawningSparks)
				return;

			sparkTimer++;
			if (sparkTimer < SparkFillInterval)
				return;

			sparkTimer = 0;
			float tTip = Math.Max(0f, state.TMax - TrailHeadGap);
			for (int path = 0; path < PramanixSkill3Geometry.PathCount; path++)
				FillPathDots(path, tTip);
		}

		private void FillPathDots(int path, float tEnd) {
			if (tEnd <= 0.001f)
				return;

			for (float t = SparkPathSpacing; t <= tEnd; t += SparkPathSpacing) {
				if (HasDotNear(path, t))
					continue;
				sparkDots.Add((path, t));
			}
		}

		private void TryApplyBoltHits(SaintBellPlayer sb, VisualState state) {
			if (state.TMax < PramanixSkill3Geometry.PathL * 0.92f)
				return;

			int damage = sb.GetSkill3VolleyDamage();
			for (int path = 0; path < PramanixSkill3Geometry.PathCount; path++) {
				if (boltHitsApplied[path])
					continue;
				boltHitsApplied[path] = true;
				sb.ApplySkill3BoltHit(path, LockedDirection, damage);
			}
		}

		private bool HasDotNear(int path, float t) {
			const float epsilon = SparkPathSpacing * 0.45f;
			foreach ((int dotPath, float dotT) in sparkDots) {
				if (dotPath == path && MathF.Abs(dotT - t) < epsilon)
					return true;
			}
			return false;
		}
	}
}
