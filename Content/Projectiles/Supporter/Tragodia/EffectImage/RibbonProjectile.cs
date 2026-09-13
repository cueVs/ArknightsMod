using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using ArknightsMod.Content.Projectiles.Supporter.Tragodia.EffectImage;

namespace ArknightsMod.Content.Projectiles.Supporter.Tragodia.EffectImage
{
	public class RibbonProjectile : ModProjectile
	{
		private const int Duration = 65;
		private const int StarCount = 5;
		private const int ArcSegments = 60;
		private const float BaseWidth = 5f;
		private const float WidthPower = 1.6f;
		private const float DissolveSpeed = 1.0f;
		private const float DissolveEdgeSharp = 4.0f;
		private const float ScaleStart = 1.0f;
		private const float ScaleEnd = 2f;
		private static readonly Color CenterColor = new Color(80, 40, 130);
		private static readonly Color EdgeColor = new Color(30, 10, 60);
		private static readonly Color DissolveGlow = new Color(120, 70, 180);
		private static readonly Vector2 SpawnOffset = new Vector2(0, -25f);

		private List<RibbonCurve> ribbons;
		private bool initialized = false;
		private BasicEffect effect;
		private float dissolveAngle;

		public override void SetDefaults() {
			Projectile.width = 2;
			Projectile.height = 2;
			Projectile.friendly = false;
			Projectile.hostile = false;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
			Projectile.timeLeft = Duration;
			Projectile.penetrate = -1;
			Projectile.aiStyle = -1;
		}

		public override void AI() {
			if (!initialized) {
				initialized = true;
				dissolveAngle = Main.rand.NextFloat(MathHelper.TwoPi);
				GenerateRibbons();
				SpawnStars();
			}
		}

		private void GenerateRibbons() {
			ribbons = new List<RibbonCurve>();
			Vector2 center = Vector2.Zero;

			Vector2 p1Start = center + new Vector2(-38f, 30f);
			Vector2 p1End = center + new Vector2(38f, -30f);
			Vector2 p1Control = center + new Vector2(Main.rand.NextFloat(-50f, 50f), Main.rand.NextFloat(-40f, 40f));
			ribbons.Add(new RibbonCurve { Start = p1Start, Control = p1Control, End = p1End });

			Vector2 p2Start = center + new Vector2(-35f, -28f);
			Vector2 p2End = center + new Vector2(35f, 28f);
			Vector2 p2Control = center + new Vector2(Main.rand.NextFloat(-45f, 45f), Main.rand.NextFloat(-45f, 45f));
			ribbons.Add(new RibbonCurve { Start = p2Start, Control = p2Control, End = p2End });
		}

		private void SpawnStars() {
			int count = Main.rand.Next(2, StarCount + 1);
			for (int i = 0; i < count; i++) {
				Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero,
					ModContent.ProjectileType<StarParticle>(), 0, 0, Projectile.owner);
			}
		}

		public override bool PreDraw(ref Color lightColor) {
			if (!initialized)
				return false;

			float time = Duration - Projectile.timeLeft;
			if (time < 0)
				return false;
			float progress = Math.Min(time / Duration, 1f);
			float globalAlpha = 1f - progress;
			float dissolveGlobal = MathHelper.Clamp(progress * DissolveSpeed, 0f, 1f);

			float zoom = Main.GameViewMatrix.Zoom.X;
			Matrix transform = Main.GameViewMatrix.TransformationMatrix;
			Vector2 screenPos = Projectile.Center - Main.screenPosition;
			Vector2 transformedPos = Vector2.Transform(screenPos, transform);
			Vector2 screenOrigin = transformedPos + SpawnOffset * zoom;

			float scale = MathHelper.Lerp(ScaleStart, ScaleEnd, progress) * zoom;

			GraphicsDevice device = Main.graphics.GraphicsDevice;
			if (effect == null) {
				effect = new BasicEffect(device) { VertexColorEnabled = true };
			}
			effect.World = Matrix.Identity;
			effect.View = Matrix.Identity;
			effect.Projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

			Main.spriteBatch.End();
			device.BlendState = BlendState.Additive;
			device.DepthStencilState = DepthStencilState.None;
			device.RasterizerState = RasterizerState.CullNone;
			effect.CurrentTechnique.Passes[0].Apply();

			for (int pass = 0; pass < 4; pass++)
				foreach (var ribbon in ribbons)
					DrawRibbonMesh(device, ribbon, screenOrigin, dissolveGlobal, globalAlpha, scale);

			Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
				SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone,
				null, Matrix.Identity);

			return false;
		}

		private void DrawRibbonMesh(GraphicsDevice device, RibbonCurve curve, Vector2 screenCenter,
			float dissolveGlobal, float globalAlpha, float scale) {
			Vector2 start = curve.Start * scale;
			Vector2 control = curve.Control * scale;
			Vector2 end = curve.End * scale;

			var positions = new List<Vector2>();
			for (int i = 0; i <= ArcSegments; i++) {
				float t = i / (float)ArcSegments;
				positions.Add(EvaluateBezier(start, control, end, t));
			}

			var vertices = new List<VertexPositionColor>();
			for (int i = 0; i < positions.Count; i++) {
				float u = i / (float)ArcSegments;
				Vector2 tangent = i == 0 ? positions[1] - positions[0] :
								  i == positions.Count - 1 ? positions[i] - positions[i - 1] :
								  positions[i + 1] - positions[i - 1];
				if (tangent.Length() < 0.01f)
					tangent = Vector2.UnitX;
				tangent.Normalize();
				Vector2 normal = new Vector2(-tangent.Y, tangent.X);

				float halfWidth = BaseWidth * (float)Math.Pow(Math.Sin(u * MathHelper.Pi), WidthPower) * 0.5f * scale;
				Vector2 left = positions[i] - normal * halfWidth;
				Vector2 right = positions[i] + normal * halfWidth;

				float dL = ComputeDissolve(positions[i], u, -1f, dissolveGlobal);
				float dR = ComputeDissolve(positions[i], u, 1f, dissolveGlobal);
				float aL = globalAlpha * (1f - dL);
				float aR = globalAlpha * (1f - dR);

				Color baseColor = Color.Lerp(EdgeColor, CenterColor, (float)Math.Sin(u * MathHelper.Pi));
				float gL = MathHelper.Clamp((dL - 0.7f) / 0.3f, 0f, 1f);
				float gR = MathHelper.Clamp((dR - 0.7f) / 0.3f, 0f, 1f);

				vertices.Add(new VertexPositionColor(new Vector3(screenCenter.X + left.X, screenCenter.Y + left.Y, 0),
					Color.Lerp(baseColor, DissolveGlow, gL * 0.5f) * aL));
				vertices.Add(new VertexPositionColor(new Vector3(screenCenter.X + right.X, screenCenter.Y + right.Y, 0),
					Color.Lerp(baseColor, DissolveGlow, gR * 0.5f) * aR));
			}

			var indices = new List<short>();
			for (int i = 0; i < positions.Count - 1; i++) {
				short i0 = (short)(i * 2), i1 = (short)(i * 2 + 1), i2 = (short)((i + 1) * 2), i3 = (short)((i + 1) * 2 + 1);
				indices.Add(i0);
				indices.Add(i1);
				indices.Add(i2);
				indices.Add(i2);
				indices.Add(i1);
				indices.Add(i3);
			}

			if (vertices.Count > 0)
				device.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, vertices.ToArray(), 0, vertices.Count, indices.ToArray(), 0, indices.Count / 3);
		}

		private float ComputeDissolve(Vector2 pos, float u, float v, float progress) {
			float angleDiff = MathF.Abs(MathHelper.WrapAngle(MathF.Atan2(pos.Y, pos.X) - dissolveAngle)) / MathHelper.Pi;
			float noise = MathF.Sin(u * 6.3f + v * 3.1f) * MathF.Cos(u * 4.7f - v * 2.5f) * 0.5f + 0.5f;
			float d = MathHelper.Clamp((progress - (angleDiff * 0.8f + noise * 0.2f) + 0.2f) * DissolveEdgeSharp, 0f, 1f);
			d += (MathF.Abs(v) * 0.15f + (1f - (float)Math.Sin(u * MathHelper.Pi)) * 0.2f) * progress;
			return MathHelper.Clamp(MathHelper.SmoothStep(0f, 1f, MathHelper.Clamp(d, 0f, 1f)), 0f, 1f);
		}

		private Vector2 EvaluateBezier(Vector2 a, Vector2 b, Vector2 c, float t) {
			float u = 1f - t;
			return u * u * a + 2f * u * t * b + t * t * c;
		}

		private struct RibbonCurve
		{
			public Vector2 Start, Control, End;
		}
	}
}