using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using ArknightsMod.Content.Projectiles.Supporter.Tragodia;
using ArknightsMod.Content.Projectiles.Supporter.Tragodia.SP2;
using ArknightsMod.Content.Projectiles.Supporter.Tragodia.SP1;
using ArknightsMod.Content.Projectiles.Supporter.Tragodia.EffectImage;
using ArknightsMod.Content.Projectiles.Supporter.Tragodia.NormalAttack;
namespace ArknightsMod.Content.Projectiles.Supporter.Tragodia.SP2
{
	public class InstinctCallDust : ModProjectile
	{

		public float EllipseA { get; set; } = 200f;
		public float EllipseB { get; set; } = 120f;
		public float AngularSpeed { get; set; } = 0.15f;
		public int ParticleCount { get; set; } = 4;
		public Color TrailColor { get; set; } = new Color(180, 120, 255, 240);
		public float DustAlpha { get; set; } = 1.0f;


		private const float TrailWidth = 65f;
		private const float ScrollSpeed = 2.0f;
		private const int MaxTrailPoints = 20;


		private float[] particleAngles;
		private List<Vector2>[] trailPositions;
		private Texture2D TailTex;
		private BasicEffect effect;
		private float time;
		private Vector2 spawnCenter;

		public override void SetDefaults() {
			Projectile.width = 0;
			Projectile.height = 0;
			Projectile.friendly = false;
			Projectile.hostile = false;
			Projectile.DamageType = DamageClass.Magic;
			Projectile.penetrate = -1;
			Projectile.tileCollide = false;
			Projectile.timeLeft = 120;
			Projectile.alpha = 255;
			Projectile.ignoreWater = true;
		}

		public override void AI() {
			if (particleAngles == null) {
				particleAngles = new float[ParticleCount];
				trailPositions = new List<Vector2>[ParticleCount];

				for (int i = 0; i < ParticleCount; i++) {
					particleAngles[i] = (i * MathHelper.TwoPi / ParticleCount) + (float)(Main.rand.NextFloat() * 0.5f);
					trailPositions[i] = new List<Vector2>();
				}

				spawnCenter = Projectile.Center;
			}

			if (TailTex == null)
				TailTex = ModContent.Request<Texture2D>("ArknightsMod/Content/Projectiles/Supporter/Tragodia/EffectImage/Tail").Value;

			time++;

			for (int i = 0; i < ParticleCount; i++) {
				particleAngles[i] += AngularSpeed;

				float x = spawnCenter.X + MathF.Cos(particleAngles[i]) * EllipseA;
				float y = spawnCenter.Y + MathF.Sin(particleAngles[i]) * EllipseB;

				trailPositions[i].Add(new Vector2(x, y));

				while (trailPositions[i].Count > MaxTrailPoints)
					trailPositions[i].RemoveAt(0);
			}
		}

		public override bool PreDraw(ref Color lightColor) {
			if (Main.spriteBatch == null || Main.graphics.GraphicsDevice == null || TailTex == null)
				return false;

			GraphicsDevice device = Main.graphics.GraphicsDevice;

			float t = time / 120f;
			float alpha;
			if (time <= 4f)
				alpha = time / 4f;
			else {
				alpha = (float)Math.Pow(1f - t, 1.5f);
			}
			alpha *= DustAlpha;

			if (effect == null || effect.IsDisposed) {
				effect = new BasicEffect(device);
				effect.VertexColorEnabled = true;
				effect.TextureEnabled = true;
			}
			effect.World = Matrix.Identity;
			effect.View = Matrix.Identity;
			effect.Projection = Matrix.CreateOrthographicOffCenter(
				0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

			float zoom = Main.GameViewMatrix.Zoom.X;
			Matrix transform = Main.GameViewMatrix.TransformationMatrix;
			Vector2 screenCenter = Vector2.Transform(spawnCenter - Main.screenPosition, transform);

			BlendState oldBlend = device.BlendState;
			DepthStencilState oldDepth = device.DepthStencilState;
			RasterizerState oldRaster = device.RasterizerState;
			Main.spriteBatch.End();

			try {
				effect.Texture = TailTex;
				device.BlendState = BlendState.Additive;
				device.DepthStencilState = DepthStencilState.None;
				device.RasterizerState = RasterizerState.CullNone;
				device.SamplerStates[0] = SamplerState.PointWrap;

				for (int i = 0; i < ParticleCount; i++) {
					var trail = trailPositions[i];
					if (trail.Count < 2)
						continue;

					List<Vector2> screenTrail = new List<Vector2>(trail.Count);
					foreach (var p in trail) {
						Vector2 transformed = Vector2.Transform(p - Main.screenPosition, transform);
						screenTrail.Add(transformed);
					}

					float scale = MathHelper.Lerp(0.85f, 1.45f, (float)Math.Pow(t, 0.5f)) * zoom;

					DrawSingleTrail(device, screenTrail, TrailWidth * scale, ScrollSpeed, TrailColor, alpha);
				}
			}
			finally {
				device.BlendState = oldBlend;
				device.DepthStencilState = oldDepth;
				device.RasterizerState = oldRaster;
				Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
					SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone,
					null, Matrix.Identity);
			}

			return false;
		}

		public override void OnKill(int timeLeft) {
			// 释放BasicEffect资源
			effect?.Dispose();
			effect = null;

			// 清理数组引用
			particleAngles = null;
			trailPositions = null;
			TailTex = null;
		}

		private void DrawSingleTrail(GraphicsDevice device, List<Vector2> points, float width,
			float scrollSpeed, Color color, float globalAlpha) {
			if (points.Count < 2)
				return;

			int count = points.Count;
			var vertices = new List<VertexPositionColorTexture>(count * 2);
			var indices = new List<short>();

			Vector2[] tangents = new Vector2[count];
			for (int i = 0; i < count; i++) {
				if (i == 0)
					tangents[i] = points[1] - points[0];
				else if (i == count - 1)
					tangents[i] = points[i] - points[i - 1];
				else
					tangents[i] = points[i + 1] - points[i - 1];

				if (tangents[i] != Vector2.Zero)
					tangents[i].Normalize();
			}

			float[] distances = new float[count];
			distances[0] = 0f;
			for (int i = 1; i < count; i++)
				distances[i] = distances[i - 1] + Vector2.Distance(points[i], points[i - 1]);

			float totalDist = distances[count - 1];
			if (totalDist < 1f)
				totalDist = 1f;

			for (int i = 0; i < count; i++) {
				Vector2 pos = points[i];
				Vector2 normal = new Vector2(-tangents[i].Y, tangents[i].X);

				float v = (distances[i] / totalDist) - time * scrollSpeed * 0.05f;
				v = v % 1.0f;
				if (v < 0)
					v += 1f;

				float trailProgress = (float)i / (count - 1);
				float fade = (float)Math.Pow(1f - trailProgress, 0.8f);

				Color finalColor = color * globalAlpha * fade;

				Vector3 leftPos = new Vector3(pos - normal * (width * 0.5f), 0f);
				Vector3 rightPos = new Vector3(pos + normal * (width * 0.5f), 0f);

				vertices.Add(new VertexPositionColorTexture(leftPos, finalColor, new Vector2(0f, v)));
				vertices.Add(new VertexPositionColorTexture(rightPos, finalColor, new Vector2(1f, v)));
			}

			for (int i = 0; i < count - 1; i++) {
				short baseIdx = (short)(i * 2);
				indices.Add(baseIdx);
				indices.Add((short)(baseIdx + 1));
				indices.Add((short)(baseIdx + 2));
				indices.Add((short)(baseIdx + 1));
				indices.Add((short)(baseIdx + 3));
				indices.Add((short)(baseIdx + 2));
			}

			if (vertices.Count > 0 && indices.Count > 0) {
				effect.CurrentTechnique.Passes[0].Apply();
				device.DrawUserIndexedPrimitives(PrimitiveType.TriangleList,
					vertices.ToArray(), 0, vertices.Count,
					indices.ToArray(), 0, indices.Count / 3);
			}
		}
	}
}