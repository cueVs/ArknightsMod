using System;
using System.Collections.Generic;
using ArknightsMod.Content.Projectiles.Supporter.Pramanix;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;

namespace ArknightsMod.Content.Items.Weapons.Supporter.Pramanix
{
	// 三技能折线/路径的 BasicEffect 绘制
	internal static class PramanixSkill3MeshRenderer
	{
		private static BasicEffect sharedEffect;
		private static readonly List<VertexPositionColor> VertBuffer = new(16384);
		private static readonly List<VertexPositionColor> EmblemMainBuffer = new(8192);
		private static readonly List<VertexPositionColor> EmblemEmissiveBuffer = new(8192);

		private static readonly (float width, float alphaScale)[] BoltGlowLayers = [
			(7f, 0.2f),
			(4f, 0.45f),
			(2f, 0.85f),
			(1.2f, 1f),
		];

		private const float EmblemHardStrokeWidth = 2.6f;

		private static readonly (float width, float alphaScale)[] GoldTrailGlowLayers = [
			(5f, 0.28f),
			(3f, 0.62f),
			(1.8f, 1f),
		];

		private const float BlueTrailLineWidth = 1.5f;
		private const float SparkDotSize = 2.8f;
		private const float MistMaxRadius = 248f;

		private static readonly Color GoldHead = new(255, 235, 120, 255);
		private static readonly Color GoldMid = new(255, 210, 70, 240);
		private static readonly Color GoldTail = new(200, 170, 90, 180);
		private static readonly Color BlueStrong = new(100, 145, 225, 210);
		private static readonly Color BlueWeak = new(55, 85, 165, 45);
		private static readonly Color EmblemBlue = new(86, 116, 147, 255);
		private static readonly Color EmblemEmissiveTint = new(96, 128, 168, 255);
		private static readonly Color SparkGold = new(255, 215, 90, 255);
		private static readonly Color MistWhite = new(245, 250, 255, 255);

		internal static void Clear() {
			VertBuffer.Clear();
			EmblemMainBuffer.Clear();
			EmblemEmissiveBuffer.Clear();
		}

		internal static void AppendSkill3Volley(PramanixSkill3VolleyFx volley, Vector2 origin) {
			if (Main.dedServ)
				return;

			var state = volley.GetVisualState();
			if (state.Alpha <= 0.01f)
				return;

			int direction = volley.LockedDirection;
			float tMax = state.TMax;
			float tTrailEnd = Math.Max(0f, tMax - PramanixSkill3VolleyFx.TrailHeadGap);
			float goldSpan = PramanixSkill3VolleyFx.GoldTrailSpan * state.SpanScale;
			byte masterAlpha = (byte)MathHelper.Clamp(state.Alpha * 255f, 0f, 255f);

			AppendWhiteMistBurst(origin, volley.GetMistExpandProgress(), state.Alpha, volley.MistWobbleSeed);

			for (int path = 0; path < PramanixSkill3Geometry.PathCount; path++) {
				AppendFullPathBlueTrail(path, tTrailEnd, origin, direction, masterAlpha);

				AppendGoldTrail(
					path, tTrailEnd, origin, direction, goldSpan,
					ScaleAlpha(GoldTail, masterAlpha),
					ScaleAlpha(GoldMid, masterAlpha),
					ScaleAlpha(GoldHead, masterAlpha));

				foreach ((int dotPath, float t) in volley.SparkDots) {
					if (dotPath != path)
						continue;
					Vector2 dotPos = PramanixSkill3Geometry.SamplePathWorld(path, t, origin, direction);
					AppendMeshSparkDot(dotPos, state.Alpha);
				}

				if (tMax > 0.01f) {
					Vector2 boltCenter = PramanixSkill3Geometry.SamplePathWorld(path, tMax, origin, direction);
					float rotation = PramanixSkill3Geometry.PathRotation(path, tMax, direction);
					AppendBoltShape(boltCenter, rotation, direction, ScaleAlpha(GoldHead, masterAlpha));
				}
			}
		}

		internal static void AppendEmblems(Vector2 playerCenter, float displayAlpha) {
			if (Main.dedServ || displayAlpha <= 0.01f)
				return;

			float pulse = (float)Math.Sin(Main.GlobalTimeWrappedHourly * 3.2f) * 0.05f + 1f;
			float lineBoost = PramanixSkill3Geometry.EmblemLineBoost;
			Color emblemColor = ScaleEmblemDisplayAlpha(EmblemBlue, displayAlpha);
			Color emissiveColor = MakeEmblemEmissiveColor(displayAlpha, playerCenter);

			for (int i = 0; i < PramanixSkill3Geometry.EmblemCount; i++) {
				float angle = i * MathHelper.TwoPi / PramanixSkill3Geometry.EmblemCount - MathHelper.PiOver2;
				DrawEmblemAt(
					playerCenter + angle.ToRotationVector2() * PramanixSkill3Geometry.EmblemOrbitRadius,
					PramanixSkill3Geometry.EmblemRotation(angle),
					PramanixSkill3Geometry.EmblemDrawScale * pulse,
					PramanixSkill3Geometry.EmblemPoints,
					PramanixSkill3Geometry.EmblemStrokeChains,
					PramanixSkill3Geometry.EmblemChainClosed,
					emblemColor, emissiveColor, lineBoost);
			}

			for (int i = 0; i < PramanixSkill3Geometry.CornerEmblemCount; i++) {
				float angle = PramanixSkill3Geometry.CornerEmblemAngles[i];
				DrawEmblemAt(
					playerCenter + angle.ToRotationVector2() * PramanixSkill3Geometry.CornerEmblemOrbitRadius,
					PramanixSkill3Geometry.EmblemRotation(angle),
					PramanixSkill3Geometry.CornerEmblemDrawScale * pulse,
					PramanixSkill3Geometry.CornerEmblemPoints,
					PramanixSkill3Geometry.CornerEmblemStrokeChains,
					PramanixSkill3Geometry.CornerEmblemChainClosed,
					emblemColor, emissiveColor, lineBoost);
			}
		}

		private static void DrawEmblemAt(
			Vector2 center, float rotation, float scale,
			Vector2[] points, int[][] chains, bool[] closed,
			Color mainColor, Color emissiveColor, float lineBoost) {
			AppendEmblemShape(points, chains, closed, center, rotation, scale, 1, mainColor, lineBoost, EmblemMainBuffer);
			AppendEmblemShape(points, chains, closed, center, rotation, scale, 1, emissiveColor, lineBoost, EmblemEmissiveBuffer);
		}

		private static Color ScaleEmblemDisplayAlpha(Color color, float displayAlpha) {
			float t = displayAlpha / Math.Max(PramanixSkill3Geometry.EmblemMaxDisplayAlpha, 0.001f);
			t = MathHelper.Clamp(t, 0f, 1f);
			byte a = (byte)MathHelper.Clamp(displayAlpha * 255f, 0f, 255f);
			return new Color(
				(byte)MathHelper.Clamp(color.R * t, 0f, 255f),
				(byte)MathHelper.Clamp(color.G * t, 0f, 255f),
				(byte)MathHelper.Clamp(color.B * t, 0f, 255f),
				a);
		}

		private static Color MakeEmblemEmissiveColor(float displayAlpha, Vector2 worldPos) {
			float ambient = Lighting.Brightness(
				(int)(worldPos.X / 16f),
				(int)(worldPos.Y / 16f));
			float nightNeed = MathHelper.Clamp(1f - ambient, 0f, 1f);
			float emissiveAlpha = displayAlpha * PramanixSkill3Geometry.EmblemEmissiveScale * (0.35f + nightNeed * 0.65f);
			byte a = (byte)MathHelper.Clamp(emissiveAlpha * 255f, 0f, 64f);
			float tint = MathHelper.Clamp(displayAlpha / PramanixSkill3Geometry.EmblemMaxDisplayAlpha, 0f, 1f);
			return new Color(
				(byte)MathHelper.Clamp(EmblemEmissiveTint.R * tint, 0f, 255f),
				(byte)MathHelper.Clamp(EmblemEmissiveTint.G * tint, 0f, 255f),
				(byte)MathHelper.Clamp(EmblemEmissiveTint.B * tint, 0f, 255f),
				a);
		}

		internal static void FlushIfAny() {
			if (Main.dedServ)
				return;

			GraphicsDevice gd = Main.graphics?.GraphicsDevice;
			if (gd == null)
				return;

			EnsureEffect(gd);
			if (sharedEffect == null)
				return;

			if (VertBuffer.Count >= 3)
				FlushBuffer(gd, VertBuffer, BlendState.NonPremultiplied, SamplerState.LinearClamp);
			if (EmblemEmissiveBuffer.Count >= 3)
				FlushBuffer(gd, EmblemEmissiveBuffer, BlendState.Additive, SamplerState.PointClamp);
			if (EmblemMainBuffer.Count >= 3)
				FlushBuffer(gd, EmblemMainBuffer, BlendState.NonPremultiplied, SamplerState.PointClamp);
		}

		private static void FlushBuffer(GraphicsDevice gd, List<VertexPositionColor> buffer, BlendState blend, SamplerState sampler) {
			SpriteBatch sb = Main.spriteBatch;
			bool batchWasOpen = TryEndSpriteBatch(sb);

			BlendState oldBlend = gd.BlendState;
			RasterizerState oldRaster = gd.RasterizerState;
			DepthStencilState oldDepth = gd.DepthStencilState;
			SamplerState oldSampler = gd.SamplerStates[0];

			try {
				gd.BlendState = blend;
				gd.RasterizerState = RasterizerState.CullNone;
				gd.DepthStencilState = DepthStencilState.None;
				gd.SamplerStates[0] = sampler;

				Matrix projection = Matrix.CreateOrthographicOffCenter(0f, Main.screenWidth, Main.screenHeight, 0f, 0f, 1f);
				Matrix model = Matrix.CreateTranslation(new Vector3(-Main.screenPosition.X, -Main.screenPosition.Y, 0f)) * Main.GameViewMatrix.ZoomMatrix;
				sharedEffect.World = model;
				sharedEffect.View = Matrix.Identity;
				sharedEffect.Projection = projection;

				VertexPositionColor[] arr = buffer.ToArray();
				foreach (EffectPass pass in sharedEffect.CurrentTechnique.Passes) {
					pass.Apply();
					gd.DrawUserPrimitives(PrimitiveType.TriangleList, arr, 0, arr.Length / 3);
				}
			}
			finally {
				gd.BlendState = oldBlend;
				gd.RasterizerState = oldRaster;
				gd.DepthStencilState = oldDepth;
				gd.SamplerStates[0] = oldSampler;

				if (batchWasOpen)
					TryBeginSpriteBatch(sb);
			}
		}

		private static void AppendWhiteMistBurst(Vector2 center, float burstProgress, float masterAlpha, float wobbleSeed) {
			if (burstProgress <= 0.001f || masterAlpha <= 0.01f)
				return;

			float expand = 1f - (1f - burstProgress) * (1f - burstProgress);
			float maxRadius = MistMaxRadius * expand;
			if (maxRadius < 12f)
				return;

			float mistAlpha = masterAlpha * 0.46f;
			if (mistAlpha <= 0.01f)
				return;

			float time = Main.GlobalTimeWrappedHourly;
			const int segments = 56;
			const int rings = 11;

			for (int ring = 0; ring < rings; ring++) {
				float tInner = ring / (float)rings;
				float tOuter = (ring + 1) / (float)rings;
				float rInner = maxRadius * tInner;
				float rOuter = maxRadius * tOuter;

				for (int i = 0; i < segments; i++) {
					float ang0 = MathHelper.TwoPi * i / segments;
					float ang1 = MathHelper.TwoPi * (i + 1) / segments;

					float ro0 = MistWobbleRadius(rOuter, ang0, wobbleSeed, time);
					float ro1 = MistWobbleRadius(rOuter, ang1, wobbleSeed, time);
					float ri0 = ring == 0 ? 0f : MistWobbleRadius(rInner, ang0, wobbleSeed, time);
					float ri1 = ring == 0 ? 0f : MistWobbleRadius(rInner, ang1, wobbleSeed, time);

					Vector2 pOi = center + ang0.ToRotationVector2() * ro0;
					Vector2 pOj = center + ang1.ToRotationVector2() * ro1;
					Vector2 pIi = center + ang0.ToRotationVector2() * ri0;
					Vector2 pIj = center + ang1.ToRotationVector2() * ri1;

					float aOi = MistCellAlpha(ang0, ro0, maxRadius, time, wobbleSeed) * mistAlpha;
					float aOj = MistCellAlpha(ang1, ro1, maxRadius, time, wobbleSeed) * mistAlpha;
					float aIi = ring == 0 ? aOi * 0.92f : MistCellAlpha(ang0, ri0, maxRadius, time, wobbleSeed) * mistAlpha;
					float aIj = ring == 0 ? aOj * 0.92f : MistCellAlpha(ang1, ri1, maxRadius, time, wobbleSeed) * mistAlpha;

					if (ring == 0) {
						VertBuffer.Add(Vpc(center, MistWhite * (aOi * 0.75f)));
						VertBuffer.Add(Vpc(pOi, MistWhite * aOi));
						VertBuffer.Add(Vpc(pOj, MistWhite * aOj));
					}
					else {
						VertBuffer.Add(Vpc(pIi, MistWhite * aIi));
						VertBuffer.Add(Vpc(pOi, MistWhite * aOi));
						VertBuffer.Add(Vpc(pOj, MistWhite * aOj));

						VertBuffer.Add(Vpc(pIi, MistWhite * aIi));
						VertBuffer.Add(Vpc(pOj, MistWhite * aOj));
						VertBuffer.Add(Vpc(pIj, MistWhite * aIj));
					}
				}
			}
		}

		private static float MistWobbleRadius(float baseRadius, float angle, float seed, float time) {
			if (baseRadius <= 0f)
				return 0f;
			float wobble =
				MathF.Sin(angle * 5f + time * 2.6f + seed) * 0.048f +
				MathF.Sin(angle * 9f - time * 3.8f + seed * 1.6f) * 0.028f +
				MathF.Sin(angle * 13f + time * 1.9f - seed * 0.4f) * 0.016f;
			return baseRadius * (1f + wobble);
		}

		private static float MistCellAlpha(float angle, float radius, float maxRadius, float time, float seed) {
			float radial = radius / Math.Max(maxRadius, 1f);
			const float edgeBand = 0.55f;
			float edgeSoft = MathHelper.Clamp((1f - radial) / edgeBand, 0f, 1f);
			edgeSoft = edgeSoft * edgeSoft * edgeSoft;

			if (radial > 0.68f) {
				float t = (radial - 0.68f) / 0.32f;
				float outerFeather = 1f - t * t * t;
				edgeSoft *= outerFeather;
			}

			float coreLift = 0.48f + 0.52f * MathHelper.Clamp(radial / 0.5f, 0f, 1f);
			float ripple =
				0.58f +
				0.22f * MathF.Sin(angle * 7f + time * 5.2f + seed) +
				0.20f * MathF.Sin(radius * 0.11f - time * 6.4f + seed * 0.7f);
			ripple = MathHelper.Clamp(ripple, 0.12f, 1f);
			return edgeSoft * coreLift * ripple;
		}

		private static Color ScaleAlpha(Color color, byte masterAlpha) {
			float scale = masterAlpha / 255f;
			return new Color(color.R, color.G, color.B, (byte)MathHelper.Clamp(color.A * scale, 0f, 255f));
		}

		private static void AppendFullPathBlueTrail(int pathIndex, float tEnd, Vector2 origin, int direction, byte masterAlpha) {
			if (tEnd <= 0.001f)
				return;

			const int segments = 32;
			for (int s = 0; s < segments; s++) {
				float u0 = s / (float)segments;
				float u1 = (s + 1) / (float)segments;
				float ta = u0 * tEnd;
				float tb = u1 * tEnd;
				float fade0 = 0.08f + 0.92f * u0 * u0;
				float fade1 = 0.08f + 0.92f * u1 * u1;
				Color ca = Color.Lerp(BlueWeak, BlueStrong, fade0);
				Color cb = Color.Lerp(BlueWeak, BlueStrong, fade1);
				ca = ScaleAlpha(ca, masterAlpha);
				cb = ScaleAlpha(cb, masterAlpha);
				Vector2 pa = PramanixSkill3Geometry.SamplePathWorld(pathIndex, ta, origin, direction);
				Vector2 pb = PramanixSkill3Geometry.SamplePathWorld(pathIndex, tb, origin, direction);
				AppendThickLine(pa, pb, BlueTrailLineWidth, Color.Lerp(ca, cb, 0.5f));
			}
		}

		private static void AppendGoldTrail(
			int pathIndex, float tEnd, Vector2 origin, int direction, float tSpan,
			Color tailColor, Color midColor, Color headColor) {
			if (tEnd <= 0.001f || tSpan <= 0.001f)
				return;

			float tStart = Math.Max(0f, tEnd - tSpan);
			const int segments = 18;
			for (int s = 0; s < segments; s++) {
				float u0 = s / (float)segments;
				float u1 = (s + 1) / (float)segments;
				float ta = MathHelper.Lerp(tStart, tEnd, u0);
				float tb = MathHelper.Lerp(tStart, tEnd, u1);
				Color ca = u0 < 0.5f ? Color.Lerp(tailColor, midColor, u0 * 2f) : Color.Lerp(midColor, headColor, (u0 - 0.5f) * 2f);
				Color cb = u1 < 0.5f ? Color.Lerp(tailColor, midColor, u1 * 2f) : Color.Lerp(midColor, headColor, (u1 - 0.5f) * 2f);
				Vector2 pa = PramanixSkill3Geometry.SamplePathWorld(pathIndex, ta, origin, direction);
				Vector2 pb = PramanixSkill3Geometry.SamplePathWorld(pathIndex, tb, origin, direction);
				AppendGoldGlowingLine(pa, pb, Color.Lerp(ca, cb, 0.5f));
			}
		}

		private static void AppendGoldGlowingLine(Vector2 worldA, Vector2 worldB, Color coreColor) {
			foreach (var (width, alphaScale) in GoldTrailGlowLayers)
				AppendThickLine(worldA, worldB, width, coreColor * alphaScale);
		}

		private static void AppendMeshSparkDot(Vector2 worldPos, float alpha) {
			if (alpha <= 0.02f)
				return;

			Color core = SparkGold * alpha;
			Color glow = SparkGold * (alpha * 0.35f);
			float r = SparkDotSize;
			AppendThickLine(worldPos + new Vector2(-r, 0f), worldPos + new Vector2(r, 0f), 2.2f, glow);
			AppendThickLine(worldPos + new Vector2(0f, -r), worldPos + new Vector2(0f, r), 2.2f, glow);
			AppendThickLine(worldPos + new Vector2(-r * 0.55f, -r * 0.55f), worldPos + new Vector2(r * 0.55f, r * 0.55f), 1.4f, core);
			AppendThickLine(worldPos + new Vector2(-r * 0.55f, r * 0.55f), worldPos + new Vector2(r * 0.55f, -r * 0.55f), 1.4f, core);
		}

		private static bool TryEndSpriteBatch(SpriteBatch sb) {
			try {
				sb.End();
				return true;
			}
			catch (InvalidOperationException) {
				return false;
			}
		}

		private static void TryBeginSpriteBatch(SpriteBatch sb) {
			try {
				sb.Begin(
					SpriteSortMode.Deferred,
					BlendState.AlphaBlend,
					Main.DefaultSamplerState,
					DepthStencilState.None,
					Main.Rasterizer,
					null,
					Main.GameViewMatrix.TransformationMatrix);
			}
			catch (InvalidOperationException) {
			}
		}

		private static void AppendBoltShape(Vector2 worldOrigin, float rotation, int direction, Color color) {
			Vector2[][] polygons = [
				PramanixSkill3Geometry.BoltOuter,
				PramanixSkill3Geometry.BoltInner,
				PramanixSkill3Geometry.BoltSpike,
			];

			foreach (Vector2[] poly in polygons) {
				if (poly == null || poly.Length < 2)
					continue;
				for (int i = 0; i < poly.Length; i++) {
					Vector2 a = PramanixSkill3Geometry.TransformBoltPoint(poly[i], worldOrigin, rotation, 1f, direction);
					Vector2 b = PramanixSkill3Geometry.TransformBoltPoint(poly[(i + 1) % poly.Length], worldOrigin, rotation, 1f, direction);
					AppendGlowingLine(a, b, color, BoltGlowLayers);
				}
			}
		}

		private static void AppendEmblemShape(
			Vector2[] points,
			int[][] chains,
			bool[] chainClosed,
			Vector2 worldCenter,
			float rotation,
			float scale,
			int direction,
			Color color,
			float lineBoost,
			List<VertexPositionColor> targetBuffer) {
			var world = new Vector2[points.Length];
			for (int i = 0; i < world.Length; i++)
				world[i] = PramanixSkill3Geometry.TransformEmblemPoint(points[i], worldCenter, rotation, scale, direction);

			float halfWidth = EmblemHardStrokeWidth * lineBoost * 0.5f;
			for (int c = 0; c < chains.Length; c++) {
				int[] chain = chains[c];
				var chainPts = new Vector2[chain.Length];
				for (int i = 0; i < chain.Length; i++)
					chainPts[i] = world[chain[i]];

				PramanixSkill3StrokeMesh.AppendChain(chainPts, chainClosed[c], halfWidth, color, targetBuffer);
			}
		}

		private static void AppendGlowingLine(Vector2 worldA, Vector2 worldB, Color coreColor, (float width, float alphaScale)[] layers) {
			foreach (var (width, alphaScale) in layers)
				AppendThickLine(worldA, worldB, width, coreColor * alphaScale);
		}

		private static void AppendThickLine(Vector2 worldA, Vector2 worldB, float width, Color color) {
			if (color.A <= 0)
				return;

			Vector2 delta = worldB - worldA;
			float lenSq = delta.LengthSquared();
			if (lenSq < 0.001f)
				return;

			Vector2 tangent = delta / MathF.Sqrt(lenSq);
			Vector2 normal = tangent.RotatedBy(MathHelper.PiOver2) * (width * 0.5f);
			Vector2 a0 = worldA - normal;
			Vector2 a1 = worldA + normal;
			Vector2 b0 = worldB - normal;
			Vector2 b1 = worldB + normal;

			VertBuffer.Add(Vpc(a0, color));
			VertBuffer.Add(Vpc(a1, color));
			VertBuffer.Add(Vpc(b1, color));

			VertBuffer.Add(Vpc(a0, color));
			VertBuffer.Add(Vpc(b1, color));
			VertBuffer.Add(Vpc(b0, color));
		}

		private static VertexPositionColor Vpc(Vector2 world, Color col) =>
			new(new Vector3(world.X, world.Y, 0f), col);

		private static void EnsureEffect(GraphicsDevice gd) {
			if (sharedEffect == null || sharedEffect.IsDisposed) {
				sharedEffect?.Dispose();
				sharedEffect = new BasicEffect(gd) {
					VertexColorEnabled = true,
					TextureEnabled = false,
				};
			}
		}
	}
}
