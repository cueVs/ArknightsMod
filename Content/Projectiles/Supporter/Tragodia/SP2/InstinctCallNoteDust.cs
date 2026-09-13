using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace ArknightsMod.Content.Projectiles.Supporter.Tragodia.SP2
{
	public class InstinctCallNoteDust : ModProjectile
	{
		public float OrbitRadius { get; set; } = 200f;
		public float AngularSpeed { get; set; } = 0.02f;
		public float NoteAlpha { get; set; } = 1.0f;

		private const int MaxTrailPoints = 8;
		private const float TrailWidth = 6f;

		private struct NoteParticle
		{
			public float Angle;
			public float BaseHeight;
			public float HeightOffset;
			public float BobPhase;
			public float BobAmplitude;
			public int TextureIndex;
			public List<Vector3> Trail;
		}
		private NoteParticle[] particles;
		private Texture2D[] noteTextures;
		private BasicEffect trailEffect;
		private float time;
		private Vector2 spawnCenter;
		private bool texturesLoaded;

		public override void SetDefaults() {
			Projectile.width = 0;
			Projectile.height = 0;
			Projectile.friendly = false;
			Projectile.hostile = false;
			Projectile.DamageType = DamageClass.Magic;
			Projectile.penetrate = -1;
			Projectile.tileCollide = false;
			Projectile.timeLeft = 130;
			Projectile.alpha = 255;
			Projectile.ignoreWater = true;
		}

		public override void AI() {
			if (particles == null) {
				int count = Main.rand.Next(10, 17);
				particles = new NoteParticle[count];

				for (int i = 0; i < count; i++) {
					particles[i].Angle = Main.rand.NextFloat(MathHelper.TwoPi);
					particles[i].BaseHeight = MathHelper.PiOver2 * 0.5f + Main.rand.NextFloat(-0.12f, 0.12f) * MathHelper.PiOver2;
					particles[i].HeightOffset = 0f;
					particles[i].BobPhase = Main.rand.NextFloat(MathHelper.TwoPi);
					particles[i].BobAmplitude = Main.rand.NextFloat(0.01f, 0.04f) * MathHelper.PiOver2;
					particles[i].TextureIndex = Main.rand.Next(4);
					particles[i].Trail = new List<Vector3>();
				}

				spawnCenter = Projectile.Center;
			}

			if (!texturesLoaded && time > 1) {
				noteTextures = new Texture2D[4];
				for (int j = 0; j < 4; j++) {
					string texName = $"ArknightsMod/Content/Projectiles/Guard/Saki/Note{j + 1}";
					if (ModContent.HasAsset(texName))
						noteTextures[j] = ModContent.Request<Texture2D>(texName).Value;
				}
				texturesLoaded = true;
			}

			time++;

			for (int i = 0; i < particles.Length; i++) {
				particles[i].Angle += AngularSpeed;
				particles[i].HeightOffset = (float)Math.Sin(time * 0.03f + particles[i].BobPhase) * particles[i].BobAmplitude;

				float theta = particles[i].BaseHeight + particles[i].HeightOffset;
				float cosT = (float)Math.Cos(theta);
				float sinT = (float)Math.Sin(theta);
				float circleR = OrbitRadius * sinT;
				float yH = OrbitRadius * cosT;

				float phi = particles[i].Angle;
				Vector3 pos3D = new Vector3(
					(float)Math.Cos(phi) * circleR,
					yH,
					(float)Math.Sin(phi) * circleR
				);

				particles[i].Trail.Add(pos3D);
				while (particles[i].Trail.Count > MaxTrailPoints)
					particles[i].Trail.RemoveAt(0);
			}
		}

		public override bool PreDraw(ref Color lightColor) {
			if (particles == null || time < 2)
				return false;

			Main.spriteBatch.End();

			try {
				DrawTrails();
				DrawNotes();
			}
			finally {
				Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
					SamplerState.PointClamp, DepthStencilState.None,
					RasterizerState.CullNone, null, Matrix.Identity);
			}

			return false;
		}

		public override void OnKill(int timeLeft) {
			trailEffect?.Dispose();
			trailEffect = null;
			particles = null;
			noteTextures = null;
		}


		private void DrawNotes() {
			if (!texturesLoaded || noteTextures == null)
				return;

			float t = time / 130f;
			float alpha = NoteAlpha;
			if (time <= 6f)
				alpha *= time / 6f;
			else if (t >= 0.3f)
				alpha *= (float)Math.Pow(1f - (t - 0.3f) / 0.7f, 0.8f);

			float zoom = Main.GameViewMatrix.Zoom.X;
			Matrix transform = Main.GameViewMatrix.TransformationMatrix;

			float scale = MathHelper.Lerp(0.85f, 1.45f, (float)Math.Pow(Math.Min(t * 1.08f, 1f), 0.5f)) * zoom;
			float baseRotationAngle = t * MathHelper.TwoPi * 0.5f;
			float cosR = (float)Math.Cos(baseRotationAngle);
			float sinR = (float)Math.Sin(baseRotationAngle);

			Vector2 screenCenter = Vector2.Transform(spawnCenter - Main.screenPosition, transform);
			screenCenter.Y += 30f * zoom;

			Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive,
				SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone,
				null, Matrix.Identity);

			for (int i = 0; i < particles.Length; i++) {
				float theta = particles[i].BaseHeight + particles[i].HeightOffset;
				float cosT = (float)Math.Cos(theta);
				float sinT = (float)Math.Sin(theta);
				float circleR = OrbitRadius * sinT * zoom;
				float yH = OrbitRadius * cosT * zoom;
				float phi = particles[i].Angle;

				Vector3 pos3D = new Vector3(
					(float)Math.Cos(phi) * circleR,
					yH,
					(float)Math.Sin(phi) * circleR
				) * scale;

				float rx = pos3D.X * cosR - pos3D.Z * sinR;
				float rz = pos3D.X * sinR + pos3D.Z * cosR;

				Vector2 scrPos = new Vector2(
					screenCenter.X + rx,
					screenCenter.Y - pos3D.Y * 0.85f + rz * 0.6f
				);

				int texIndex = particles[i].TextureIndex;
				if (texIndex < 0 || texIndex >= noteTextures.Length)
					continue;
				Texture2D tex = noteTextures[texIndex];
				if (tex == null || tex.IsDisposed)
					continue;

				float size = 10f * scale;

				float glowSize = size * 1.5f;
				Main.spriteBatch.Draw(tex,
					new Rectangle((int)(scrPos.X - glowSize * 0.5f), (int)(scrPos.Y - glowSize * 0.5f), (int)glowSize, (int)glowSize),
					null, new Color(180, 100, 255) * alpha * 0.4f);

				Main.spriteBatch.Draw(tex,
					new Rectangle((int)(scrPos.X - size * 0.5f), (int)(scrPos.Y - size * 0.5f), (int)size, (int)size),
					null, new Color(220, 160, 255) * alpha * 0.8f);
			}

			Main.spriteBatch.End();
		}

		private void DrawTrails() {
			GraphicsDevice device = Main.graphics.GraphicsDevice;
			if (device == null)
				return;

			if (trailEffect == null) {
				trailEffect = new BasicEffect(device);
				trailEffect.VertexColorEnabled = true;
			}

			trailEffect.World = Matrix.Identity;
			trailEffect.View = Matrix.Identity;
			trailEffect.Projection = Matrix.CreateOrthographicOffCenter(
				0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

			float t = time / 130f;
			float alpha = NoteAlpha;
			if (time <= 6f)
				alpha *= time / 6f;
			else if (t >= 0.3f)
				alpha *= (float)Math.Pow(1f - (t - 0.3f) / 0.7f, 0.8f);

			float zoom = Main.GameViewMatrix.Zoom.X;
			float scale = MathHelper.Lerp(0.85f, 1.45f, (float)Math.Pow(Math.Min(t * 1.08f, 1f), 0.5f)) * zoom;
			float baseRotationAngle = t * MathHelper.TwoPi * 0.5f;
			float cosR = (float)Math.Cos(baseRotationAngle);
			float sinR = (float)Math.Sin(baseRotationAngle);

			Matrix transform = Main.GameViewMatrix.TransformationMatrix;
			Vector2 screenCenter = Vector2.Transform(spawnCenter - Main.screenPosition, transform);
			screenCenter.Y += 30f * zoom;

			BlendState oldBlend = device.BlendState;
			DepthStencilState oldDepth = device.DepthStencilState;
			RasterizerState oldRaster = device.RasterizerState;

			try {
				device.BlendState = BlendState.Additive;
				device.DepthStencilState = DepthStencilState.None;
				device.RasterizerState = RasterizerState.CullNone;

				float orbitRadiusScaled = OrbitRadius * zoom;

				for (int i = 0; i < particles.Length; i++) {
					var trail = particles[i].Trail;
					if (trail.Count >= 2) {
						List<Vector2> screenTrail = new List<Vector2>();
						foreach (var p3D in trail) {
							Vector3 s = p3D * scale;
							float rx = s.X * cosR - s.Z * sinR;
							float rz = s.X * sinR + s.Z * cosR;
							screenTrail.Add(new Vector2(
								screenCenter.X + rx,
								screenCenter.Y - s.Y * 0.85f + rz * 0.6f
							));
						}
						DrawOneTrail(device, screenTrail, alpha);
					}
				}
			}
			finally {
				device.BlendState = oldBlend;
				device.DepthStencilState = oldDepth;
				device.RasterizerState = oldRaster;
			}
		}

		private void DrawOneTrail(GraphicsDevice device, List<Vector2> points, float alpha) {
			if (points.Count < 2)
				return;

			int count = points.Count;
			VertexPositionColor[] verts = new VertexPositionColor[count * 2];
			short[] inds = new short[(count - 1) * 6];

			for (int i = 0; i < count; i++) {
				Vector2 tangent;
				if (i == 0)
					tangent = points[1] - points[0];
				else if (i == count - 1)
					tangent = points[i] - points[i - 1];
				else
					tangent = points[i + 1] - points[i - 1];

				if (tangent.Length() > 0.01f)
					tangent.Normalize();
				else
					tangent = Vector2.UnitX;

				Vector2 normal = new Vector2(-tangent.Y, tangent.X);
				float progress = i / (float)(count - 1);
				float fade = (float)Math.Pow(1f - progress, 1.2f);
				Color c = new Color(100, 50, 180) * alpha * fade * 0.4f;
				float halfW = TrailWidth * 0.5f;

				verts[i * 2] = new VertexPositionColor(
					new Vector3(points[i] - normal * halfW, 0), c);
				verts[i * 2 + 1] = new VertexPositionColor(
					new Vector3(points[i] + normal * halfW, 0), c);
			}

			int idx = 0;
			for (int i = 0; i < count - 1; i++) {
				short b = (short)(i * 2);
				inds[idx++] = b;
				inds[idx++] = (short)(b + 1);
				inds[idx++] = (short)(b + 2);
				inds[idx++] = (short)(b + 1);
				inds[idx++] = (short)(b + 3);
				inds[idx++] = (short)(b + 2);
			}

			trailEffect.CurrentTechnique.Passes[0].Apply();
			device.DrawUserIndexedPrimitives(PrimitiveType.TriangleList,
				verts, 0, verts.Length, inds, 0, inds.Length / 3);
		}
	}
}