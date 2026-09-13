using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Projectiles.Supporter.Tragodia.EffectImage
{
	public class WaveProjectile : ModProjectile
	{
		//ÏÅÈË
		private const int CirclePoints = 120;
		private const int DepthSegments = 8;
		private const float StartRadius = 15f;
		private const float MaxRadius = 75f;
		private const float EllipseRatio = 0.6f;
		private const int MaxLife = 70;
		private const float PeakProgress = 0.35f;
		private const float RiseExponent = 2.5f;
		private const float FallExponent = 2.5f;
		private const float MaxRiseHeight = 35f;
		private const float FinalBaselineOffset = 5f;
		private const float MaxWaveAmplitude = 18f;
		private const float WaveTimeScale = 0.05f;
		private const float WaveFreq1 = 3f;
		private const float WaveFreq2 = 6f;
		private const float WaveFreq3 = 15f;
		private static readonly Color TopGlowColor = new Color(180, 100, 255);
		private static readonly Color DepthColor = new Color(70, 90, 200);
		private const int CrossTotalFrames = 25;
		private const int CrossShrinkStartFrame = 4;
		private const float CrossMainStartWidth = 80f;
		private const float CrossMainStartHeight = 14f;
		private const float CrossMainEndWidth = 8f;
		private const float CrossMainEndHeight = 3f;
		private const float CrossOuterScaleStart = 1.5f;
		private const float CrossOuterScaleEnd = 1.1f;
		private static readonly Color CrossMainColor = new Color(180, 100, 255);
		private static readonly Color CrossOuterColor = new Color(120, 60, 200);
		private const int CoreGlowTotalFrames = 25;
		private const int CoreGlowPeakFrame = 4;
		private const float CoreGlowStartSize = 20f;
		private const float CoreGlowPeakSize = 70f;
		private const float CoreGlowEndSize = 8f;
		private const float CoreGlowOuterScale = 2.2f;
		private static readonly Color CoreGlowMainColor = new Color(220, 180, 255);
		private static readonly Color CoreGlowOuterColor = new Color(120, 40, 200, 160);
		private const int MidGlowTotalFrames = 25;
		private const int MidGlowPeakFrame = 4;
		private const float MidGlowStartSize = 30f;
		private const float MidGlowPeakSize = 110f;
		private const float MidGlowEndSize = 12f;
		private const float MidGlowOuterScale = 2.0f;
		private static readonly Color MidGlowMainColor = new Color(180, 130, 255);
		private static readonly Color MidGlowOuterColor = new Color(80, 20, 150, 140);

		private float time = 0f;
		private BasicEffect effect;
		private Texture2D projLightCoreTexture;
		private Texture2D crossTexture;
		private Vector2 effectCenter;
		private float effectScale = 1f;
		private float waveAmplitudeMultiplier = 1f;

		public override void SetDefaults() {
			Projectile.width = 10;
			Projectile.height = 10;
			Projectile.friendly = false;
			Projectile.hostile = false;
			Projectile.DamageType = DamageClass.Magic;
			Projectile.penetrate = -1;
			Projectile.tileCollide = false;
			Projectile.timeLeft = MaxLife;
			Projectile.alpha = 255;
			Projectile.ignoreWater = true;
		}

		public override void AI() {
			time++;
			if (time == 1) {
				effectScale = Projectile.ai[0] > 0 ? Projectile.ai[0] : 1f;
				waveAmplitudeMultiplier = Projectile.ai[1] > 0 ? Projectile.ai[1] : 1f;
				effectCenter = Projectile.Center + new Vector2(0, -20f * effectScale);
			}
		}

		public override bool PreDraw(ref Color lightColor) {
			if (Main.spriteBatch == null || Main.graphics.GraphicsDevice == null)
				return false;

			if (projLightCoreTexture == null)
				projLightCoreTexture = ModContent.Request<Texture2D>("ArknightsMod/Content/Projectiles/Supporter/Tragodia/EffectImage/ProjLightCore").Value;
			if (crossTexture == null)
				crossTexture = ModContent.Request<Texture2D>("ArknightsMod/Content/Projectiles/Supporter/Tragodia/EffectImage/Light_horizontal").Value;

			GraphicsDevice device = Main.graphics.GraphicsDevice;

			float rawProgress = (float)(MaxLife - Projectile.timeLeft) / MaxLife;
			float easedProgress = 1f - (float)Math.Pow(1f - rawProgress, 3);


			float zoom = Main.GameViewMatrix.Zoom.X;
			float totalScale = zoom * effectScale;

			Matrix transform = Main.GameViewMatrix.TransformationMatrix;
			Vector2 screenPos = Projectile.Center - Main.screenPosition;
			Vector2 transformedPos = Vector2.Transform(screenPos, transform);
			Vector2 effectScreenPos = effectCenter - Main.screenPosition;
			Vector2 transformedEffectPos = Vector2.Transform(effectScreenPos, transform);

	
			float currentRadius = MathHelper.Lerp(StartRadius * totalScale, MaxRadius * totalScale, easedProgress);

			float heightCurve;
			if (rawProgress < PeakProgress) {
				float t = rawProgress / PeakProgress;
				heightCurve = 1f - (float)Math.Pow(1f - t, RiseExponent);
			}
			else {
				float t = (rawProgress - PeakProgress) / (1f - PeakProgress);
				heightCurve = (float)Math.Pow(1f - t, FallExponent);
			}

			float baseRise = MaxRiseHeight * totalScale * heightCurve;
			float amplitudeEnvelope = heightCurve;
			float alpha = 1f - rawProgress;

			if (effect == null) {
				effect = new BasicEffect(device);
				effect.VertexColorEnabled = true;
			}
			effect.World = Matrix.Identity;
			effect.View = Matrix.Identity;
			effect.Projection = Matrix.CreateOrthographicOffCenter(
				0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

			Main.spriteBatch.End();


			DrawWaveRing(device, transformedPos, currentRadius, baseRise, amplitudeEnvelope, alpha, MaxWaveAmplitude * totalScale);


			Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
				SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone,
				null, Matrix.Identity);

			DrawCrossAndGlowEffects(transformedEffectPos, alpha, totalScale);

			return false;
		}

		private void DrawCrossAndGlowEffects(Vector2 center, float waveAlpha, float scale) {
			int crossFrame = (int)MathHelper.Clamp(time, 0f, (float)(CrossTotalFrames - 1));
			DrawMidGlow(center, crossFrame, scale);
			DrawCoreGlow(center, crossFrame, scale);
			DrawCrossEffect(center, crossFrame, scale);
		}

		private void DrawCoreGlow(Vector2 center, int frame, float scale) {
			DrawGlow(center, frame, CoreGlowTotalFrames, CoreGlowPeakFrame,
				CoreGlowStartSize * scale, CoreGlowPeakSize * scale, CoreGlowEndSize * scale,
				CoreGlowOuterScale, CoreGlowMainColor, CoreGlowOuterColor, 1f);
		}

		private void DrawMidGlow(Vector2 center, int frame, float scale) {
			DrawGlow(center, frame, MidGlowTotalFrames, MidGlowPeakFrame,
				MidGlowStartSize * scale, MidGlowPeakSize * scale, MidGlowEndSize * scale,
				MidGlowOuterScale, MidGlowMainColor, MidGlowOuterColor, 0.9f);
		}

		private void DrawGlow(Vector2 center, int frame, int totalFrames, int peakFrame,
			float startSize, float peakSize, float endSize, float outerScale,
			Color mainColor, Color outerColor, float baseAlpha) {
			if (frame >= totalFrames || projLightCoreTexture == null)
				return;

			float progress = (float)frame / totalFrames;
			float size;
			if (frame <= peakFrame) {
				float t = frame / (float)peakFrame;
				float e = 1f - (1f - t) * (1f - t);
				size = startSize + (peakSize - startSize) * e;
			}
			else {
				float t = (frame - peakFrame) / (float)(totalFrames - 1 - peakFrame);
				float e = t * t;
				size = peakSize - (peakSize - endSize) * e;
			}

			float alpha = baseAlpha * (1f - progress * progress);

			Main.spriteBatch.End();
			Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive,
				SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone,
				null, Matrix.Identity);

			Rectangle mainRect = new Rectangle(
				(int)(center.X - size * 0.5f), (int)(center.Y - size * 0.5f),
				(int)size, (int)size);
			Main.spriteBatch.Draw(projLightCoreTexture, mainRect, null, mainColor * alpha);

			float outerSize = size * outerScale;
			Rectangle outerRect = new Rectangle(
				(int)(center.X - outerSize * 0.5f), (int)(center.Y - outerSize * 0.5f),
				(int)outerSize, (int)outerSize);
			Main.spriteBatch.Draw(projLightCoreTexture, outerRect, null, outerColor * alpha * 0.7f);

			Main.spriteBatch.End();
			Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
				Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer,
				null, Matrix.Identity);
		}

		private void DrawCrossEffect(Vector2 center, int frame, float scale) {
			GetCrossSize(frame, out float mW, out float mH, out float oW, out float oH);
			mW *= scale;
			mH *= scale;
			oW *= scale;
			oH *= scale;
			float alpha = CalculateCrossAlpha(frame);

			Main.spriteBatch.End();
			Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive,
				SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone,
				null, Matrix.Identity);

			DrawRotatedCrossPart(center, mW, mH, oW, oH, alpha, 0f);
			DrawRotatedCrossPart(center, mW, mH, oW, oH, alpha, MathHelper.PiOver2);

			Main.spriteBatch.End();
			Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
				Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer,
				null, Matrix.Identity);
		}

		private void DrawRotatedCrossPart(Vector2 center, float mW, float mH,
			float oW, float oH, float alpha, float rotation) {
			if (crossTexture == null)
				return;
			Vector2 origin = crossTexture.Size() / 2f;

			Main.spriteBatch.Draw(crossTexture, center, null, CrossMainColor * alpha,
				rotation, origin, new Vector2(mW / crossTexture.Width, mH / crossTexture.Height),
				SpriteEffects.None, 0f);
			Main.spriteBatch.Draw(crossTexture, center, null, CrossOuterColor * alpha,
				rotation, origin, new Vector2(oW / crossTexture.Width, oH / crossTexture.Height),
				SpriteEffects.None, 0f);
		}

		private void GetCrossSize(int frame, out float mW, out float mH,
			out float oW, out float oH) {
			if (frame >= CrossTotalFrames) {
				mW = CrossMainEndWidth;
				mH = CrossMainEndHeight;
				oW = mW * CrossOuterScaleEnd;
				oH = mH * CrossOuterScaleEnd;
				return;
			}

			float t;
			if (frame <= CrossShrinkStartFrame)
				t = 0f;
			else
				t = (float)(frame - CrossShrinkStartFrame) / (CrossTotalFrames - 1 - CrossShrinkStartFrame);

			float e = t * t;
			mW = CrossMainStartWidth + (CrossMainEndWidth - CrossMainStartWidth) * e;
			mH = CrossMainStartHeight + (CrossMainEndHeight - CrossMainStartHeight) * e;

			float s = CrossOuterScaleStart + (CrossOuterScaleEnd - CrossOuterScaleStart) * e;
			oW = mW * s;
			oH = mH * s;

			mW = Math.Max(1, mW);
			mH = Math.Max(1, mH);
		}

		private float CalculateCrossAlpha(int frame) {
			float t = frame / (float)(CrossTotalFrames - 1);
			return 1f - t * t;
		}

		private void DrawWaveRing(GraphicsDevice device, Vector2 center, float radius,
			float baseRise, float amplitudeEnvelope, float alpha, float maxAmplitude) {
			Color mainColor = TopGlowColor * alpha;

			List<VertexPositionColor> vertices = new List<VertexPositionColor>();
			List<short> indices = new List<short>();

			float[] bottomYs = new float[CirclePoints];
			float[] finalBLs = new float[CirclePoints];
			float[] topYs = new float[CirclePoints];
			float[] skirtAlphas = new float[CirclePoints];

			for (int i = 0; i < CirclePoints; i++) {
				float angle = (float)i / CirclePoints * MathHelper.TwoPi;
				float sinY = MathF.Sin(angle) * radius * EllipseRatio;

				float bottomY = center.Y + sinY;
				bottomYs[i] = bottomY;

				float finalBL = center.Y + sinY + FinalBaselineOffset * (radius / MaxRadius);
				finalBLs[i] = finalBL;

				float wave = CalculateWaveHeight(angle) * amplitudeEnvelope * maxAmplitude * waveAmplitudeMultiplier;
				float topY = finalBL - baseRise + wave;
				topYs[i] = topY;

				float diff = bottomY - topY;
				skirtAlphas[i] = MathHelper.Clamp(diff / (8f * (radius / MaxRadius)), 0f, 1f);
			}

			for (int depth = 0; depth <= DepthSegments; depth++) {
				float t = (float)depth / DepthSegments;
				float depthAlphaBase = 1f - t * 0.5f;

				for (int i = 0; i < CirclePoints; i++) {
					float angle = (float)i / CirclePoints * MathHelper.TwoPi;
					float x = center.X + MathF.Cos(angle) * radius;
					float y = MathHelper.Lerp(topYs[i], bottomYs[i], t);

					Color vertexColor;
					if (depth == 0)
						vertexColor = Color.Lerp(mainColor, Color.White, 0.3f) * depthAlphaBase * alpha;
					else {
						Color darkColor = DepthColor * depthAlphaBase * alpha;
						darkColor *= skirtAlphas[i];
						vertexColor = darkColor;
					}

					vertices.Add(new VertexPositionColor(new Vector3(x, y, 0f), vertexColor));
				}
			}

			for (int depth = 0; depth < DepthSegments; depth++) {
				for (int i = 0; i < CirclePoints; i++) {
					int cur = depth * CirclePoints + i;
					int next = depth * CirclePoints + (i + 1) % CirclePoints;
					int curBelow = (depth + 1) * CirclePoints + i;
					int nextBelow = (depth + 1) * CirclePoints + (i + 1) % CirclePoints;

					if (skirtAlphas[i] > 0.01f || skirtAlphas[(i + 1) % CirclePoints] > 0.01f) {
						indices.Add((short)cur);
						indices.Add((short)next);
						indices.Add((short)curBelow);

						indices.Add((short)next);
						indices.Add((short)nextBelow);
						indices.Add((short)curBelow);
					}
				}
			}

			device.BlendState = BlendState.Additive;
			device.DepthStencilState = DepthStencilState.None;
			device.RasterizerState = RasterizerState.CullNone;
			effect.CurrentTechnique.Passes[0].Apply();

			if (vertices.Count > 0 && indices.Count > 0) {
				device.DrawUserIndexedPrimitives(PrimitiveType.TriangleList,
					vertices.ToArray(), 0, vertices.Count,
					indices.ToArray(), 0, indices.Count / 3);
			}

			DrawTopGlowLine(device, center, radius, topYs, skirtAlphas);
		}

		private float CalculateWaveHeight(float angle) {
			float t = time * WaveTimeScale;
			float w1 = MathF.Sin(t * 0.6f + angle * WaveFreq1);
			float w2 = MathF.Sin(t * 0.9f + angle * WaveFreq2) * 0.5f;
			float w3 = MathF.Sin(t * 1.2f + angle * WaveFreq3) * 0.2f;
			return w1 + w2 + w3;
		}

		private void DrawTopGlowLine(GraphicsDevice device, Vector2 center, float radius,
			float[] topYs, float[] skirtAlphas) {
			int glowLayers = 5;

			for (int layer = 0; layer < glowLayers; layer++) {
				float layerProgress = (float)layer / (glowLayers - 1);
				float offset = (2f + layerProgress * 8f) * (radius / MaxRadius); // Ëõ·ÅÆ«ÒÆ
				float alpha = (1f - layerProgress) * 0.9f;
				Color layerColor = Color.Lerp(TopGlowColor, Color.White, layerProgress * 0.5f);

				List<VertexPositionColor> lineVerts = new List<VertexPositionColor>();

				for (int i = 0; i <= CirclePoints; i++) {
					int idx = i % CirclePoints;
					float angle = (float)idx / CirclePoints * MathHelper.TwoPi;
					float x = center.X + MathF.Cos(angle) * radius;
					float y = topYs[idx] + offset;

					Color pointColor = layerColor * alpha * skirtAlphas[idx];
					lineVerts.Add(new VertexPositionColor(new Vector3(x, y, 0f), pointColor));
				}

				if (lineVerts.Count >= 2) {
					device.BlendState = BlendState.Additive;
					device.DepthStencilState = DepthStencilState.None;
					effect.CurrentTechnique.Passes[0].Apply();
					device.DrawUserPrimitives(PrimitiveType.LineStrip,
						lineVerts.ToArray(), 0, lineVerts.Count - 1);
				}
			}
		}
	}
}