using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;

namespace ArknightsMod.Content.Projectiles.Supporter.Deepcolor
{
	public static class DeepcolorLogoEffects
	{
		public static Color EffectColor => DeepcolorLogoRenderer.LogoColor;

		public static void SpawnChargeParticles(Vector2 center) {
			if (Main.netMode == NetmodeID.Server)
				return;
			if (!Main.rand.NextBool(2))
				return;

			Vector2 spawnPos = center + Main.rand.NextVector2Circular(28f, 22f);
			int d = Dust.NewDust(spawnPos, 0, 0, DustID.TintableDustLighted, newColor: EffectColor);
			ref Dust dust = ref Main.dust[d];
			dust.noGravity = true;
			dust.scale = Main.rand.NextFloat(0.5f, 0.95f);
			float phase = Main.GameUpdateCount * 0.15f + d * 0.37f;
			dust.velocity = new Vector2(
				MathF.Sin(phase) * 0.9f,
				-Main.rand.NextFloat(3.8f, 6.5f));
			dust.fadeIn = 0.45f;
		}

		public static void SpawnStrikeBurst(Vector2 center) {
			if (Main.netMode == NetmodeID.Server)
				return;

			const int count = 52;
			for (int i = 0; i < count; i++) {
				float angle = MathHelper.TwoPi * i / count + Main.rand.NextFloat(-0.18f, 0.18f);
				Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(9f, 18f);
				int d = Dust.NewDust(center, 0, 0, DustID.TintableDustLighted, vel.X, vel.Y, newColor: EffectColor);
				ref Dust dust = ref Main.dust[d];
				dust.noGravity = true;
				dust.scale = Main.rand.NextFloat(0.95f, 1.85f);
				dust.fadeIn = 0.2f;
			}

			for (int i = 0; i < 20; i++) {
				float angle = Main.rand.NextFloat(MathHelper.TwoPi);
				Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(12f, 22f);
				int d = Dust.NewDust(center, 0, 0, DustID.TintableDustLighted, vel.X, vel.Y, newColor: EffectColor);
				ref Dust dust = ref Main.dust[d];
				dust.noGravity = true;
				dust.scale = Main.rand.NextFloat(1.2f, 2.1f);
				dust.fadeIn = 0.12f;
			}
		}

		public static void DrawShockwaveRing(SpriteBatch spriteBatch, Vector2 worldCenter, float radius, float thickness, float alpha) {
			if (alpha <= 0.01f || radius <= 1f)
				return;

			DeepcolorLogoRenderer.DrawRingMesh(spriteBatch, worldCenter, radius, thickness, EffectColor * alpha);
		}
	}

	public struct LogoShockwaveState
	{
		public bool Active;
		public float Radius;
		public float Alpha;

		public void Trigger(float startRadius = 9f) {
			Active = true;
			Radius = startRadius;
			Alpha = 1f;
		}

		public void Update() {
			if (!Active)
				return;

			Radius += 4.5f;
			Alpha -= 0.045f;
			if (Alpha <= 0f)
				Active = false;
		}
	}
}
