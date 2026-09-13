using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using Terraria.Utilities;

namespace ArknightsMod.Content.Projectiles.Supporter.Tragodia.SP2
{
	public class InstinctCallSuctionLines : ModProjectile
	{
		private BasicEffect trailEffect;

		private const int MaxLines = 16;
		private const float SpawnInterval = 3f;
		private const float LineStartRadius = 250f;
		private const float LineEndRadius = 5f;
		private const float LineWidth = 6.0f;
		private const int TailLength = 20;
		private const float AttractSpeed = 1.2f;
		private const float RotationSpeed = 10.0f;

		private static readonly Color BrightColor = new Color(80, 50, 140);
		private static readonly Color DarkColor = new Color(70, 40, 130);
		private static readonly Color DotColor = new Color(210, 190, 255);

		private const float FadeInDuration = 50f;

		private class TrailLine
		{
			public float BaseAngle;
			public float DriftSpeed;
			public float Life;
			public float SpeedMult;
			public float RadiusMult;
			public bool Finished;
			public bool IsDark;
			public float CurrentAngle;
			public List<Vector2> Trail;
			public int MaxTrail;

			public TrailLine(int maxTrail) {
				BaseAngle = Main.rand.NextFloat(0, MathHelper.TwoPi);
				DriftSpeed = Main.rand.NextFloat(-0.3f, 0.3f);
				Life = 0f;
				SpeedMult = Main.rand.NextFloat(0.85f, 1.15f);
				RadiusMult = Main.rand.NextFloat(0.92f, 1.08f);
				Finished = false;
				IsDark = Main.rand.NextFloat() < 0.4f;
				CurrentAngle = BaseAngle;
				Trail = new List<Vector2>();
				MaxTrail = maxTrail;
				UpdatePosition(0f, 0f);
			}

			public Vector2 UpdatePosition(float progress, float globalRotation) {
				CurrentAngle += DriftSpeed * 0.005f;
				float angle = BaseAngle + CurrentAngle * 0.2f + globalRotation * 0.6f;
				float radius = MathHelper.Lerp(
					LineStartRadius * RadiusMult,
					LineEndRadius,
					Math.Min(1f, progress * 1.1f)
				);
				return new Vector2((float)Math.Cos(angle) * radius, (float)Math.Sin(angle) * radius);
			}

			public bool Update(float speed, float globalRotation) {
				Life += speed * SpeedMult * 0.012f;
				if (Life >= 1f) {
					Life = 1f;
					Finished = true;
					return true;
				}

				Trail.Add(UpdatePosition(Life, globalRotation));
				if (Trail.Count > MaxTrail) {
					Trail.RemoveAt(0);
				}
				return false;
			}

			public void InitTrail(int count, float globalRotation) {
				for (int i = 0; i < count; i++) {
					float progress = Math.Max(0f, Life - i * 0.02f);
					Trail.Add(UpdatePosition(progress, globalRotation));
				}
			}
		}

		private List<TrailLine> lines;
		private float spawnTimer;
		private float globalRotation;
		private UnifiedRandom rand;
		private float fadeInTimer = 0f;

		private List<VertexPositionColor> verticesCache = new List<VertexPositionColor>(4096);
		private List<short> indicesCache = new List<short>(6144);
		private VertexPositionColor[] vertexArray;
		private short[] indexArray;

		public override string Texture => "ArknightsMod/Content/Projectiles/Supporter/Tragodia/SP2/InstinctCall";

		public override void SetDefaults() {
			Projectile.width = 2;
			Projectile.height = 2;
			Projectile.friendly = false;
			Projectile.hostile = false;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
			Projectile.penetrate = -1;
			Projectile.timeLeft = 600;
			Projectile.alpha = 0;
		}

		public override void AI() {
			if (fadeInTimer < FadeInDuration) {
				fadeInTimer++;
			}

			if (rand == null)
				rand = new UnifiedRandom(Projectile.identity.GetHashCode());
			if (lines == null) {
				lines = new List<TrailLine>();
				int preCount = Math.Min(MaxLines, 10);
				for (int i = 0; i < preCount; i++) {
					var line = new TrailLine(TailLength);
					line.Life = Main.rand.NextFloat(0.3f, 0.7f);
					line.BaseAngle = Main.rand.NextFloat(0, MathHelper.TwoPi);
					line.UpdatePosition(line.Life, globalRotation);
					line.InitTrail((int)(Main.rand.NextFloat(8f, TailLength * 0.5f)), globalRotation);
					lines.Add(line);
				}
			}

			Projectile.velocity = Vector2.Zero;
			globalRotation += RotationSpeed * 0.008f;

			spawnTimer++;
			if (spawnTimer >= SpawnInterval && lines.Count < MaxLines) {
				spawnTimer = 0;
				int spawnCount = Math.Min(2, MaxLines - lines.Count);
				for (int i = 0; i < spawnCount; i++) {
					var newLine = new TrailLine(TailLength);
					newLine.BaseAngle = Main.rand.NextFloat(0, MathHelper.TwoPi);
					lines.Add(newLine);
				}
			}

			foreach (var line in lines) {
				line.Update(AttractSpeed, globalRotation);
			}

			lines.RemoveAll(l => l.Finished);
		}

		public override bool PreDraw(ref Color lightColor) {
			if (lines == null || lines.Count == 0)
				return true;

			DrawTrails();
			return false;
		}

		public override void OnKill(int timeLeft) {
			trailEffect?.Dispose();
			trailEffect = null;
		}

		private float GetCurrentAlpha() {
			if (fadeInTimer >= FadeInDuration)
				return 1f;
			float t = fadeInTimer / FadeInDuration;
			return t * t * t * (t * (6f * t - 15f) + 10f);
		}

		private float GetRadiusScale() {
			if (fadeInTimer >= FadeInDuration)
				return 1f;
			float t = fadeInTimer / FadeInDuration;
			float eased = t * t * (3f - 2f * t);
			return 0.05f + 0.95f * eased;
		}

		private void DrawTrails() {
			float globalAlpha = GetCurrentAlpha();
			if (globalAlpha < 0.005f)
				return;

			float radiusScale = GetRadiusScale();
			GraphicsDevice device = Main.graphics.GraphicsDevice;

			float zoom = Main.GameViewMatrix.Zoom.X;
			Matrix transform = Main.GameViewMatrix.TransformationMatrix;
			Vector2 screenCenter = Vector2.Transform(Projectile.Center - Main.screenPosition, transform);

			Main.spriteBatch.End();

			DrawTrailLines(device, screenCenter, globalAlpha, radiusScale * zoom);

			Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp,
				DepthStencilState.None, RasterizerState.CullNone, null, Matrix.Identity);
		}

		private void DrawTrailLines(GraphicsDevice device, Vector2 center, float globalAlpha, float radiusScale) {
			if (trailEffect == null || trailEffect.IsDisposed) {
				trailEffect = new BasicEffect(device) {
					VertexColorEnabled = true
				};
			}
			trailEffect.World = Matrix.Identity;
			trailEffect.View = Matrix.Identity;
			trailEffect.Projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

			BlendState originalBlend = device.BlendState;
			device.BlendState = BlendState.Additive;

			verticesCache.Clear();
			indicesCache.Clear();

			foreach (var line in lines) {
				if (line.IsDark)
					BuildTrailMesh(line, center, verticesCache, indicesCache, globalAlpha, radiusScale);
			}
			foreach (var line in lines) {
				if (!line.IsDark)
					BuildTrailMesh(line, center, verticesCache, indicesCache, globalAlpha, radiusScale);
			}

			if (verticesCache.Count > 0 && indicesCache.Count > 0) {
				vertexArray = verticesCache.ToArray();
				indexArray = indicesCache.ToArray();
				trailEffect.CurrentTechnique.Passes[0].Apply();
				device.DrawUserIndexedPrimitives(
					PrimitiveType.TriangleList,
					vertexArray, 0, vertexArray.Length,
					indexArray, 0, indexArray.Length / 3
				);
			}

			device.BlendState = originalBlend;
		}

		private void BuildTrailMesh(TrailLine line, Vector2 center, List<VertexPositionColor> vertices, List<short> indices, float globalAlpha, float radiusScale) {
			var trail = line.Trail;
			int len = trail.Count;
			if (len < 2)
				return;

			Color baseColor = line.IsDark ? DarkColor : BrightColor;
			byte baseR = baseColor.R;
			byte baseG = baseColor.G;
			byte baseB = baseColor.B;
			float darkMult = line.IsDark ? 0.65f : 1f;

			for (int i = 0; i < len - 1; i++) {
				Vector2 p1 = trail[i] * radiusScale;
				Vector2 p2 = trail[i + 1] * radiusScale;

				float t = i / (float)len;

				float alpha;
				if (t < 0.15f)
					alpha = t / 0.15f * 0.85f;
				else if (t > 0.85f)
					alpha = 0.85f;
				else
					alpha = 0.595f + 0.255f * (t - 0.15f) / 0.7f;
				alpha *= darkMult * globalAlpha;

				float widthMult = 0.1f + 0.9f * t;
				float halfW = LineWidth * widthMult * 0.35f * radiusScale;

				Vector2 dir = p2 - p1;
				float len2 = dir.Length();
				if (len2 < 0.1f)
					continue;
				dir.Normalize();
				Vector2 perp = new Vector2(-dir.Y, dir.X);

				Vector2 sp1 = center + p1;
				Vector2 sp2 = center + p2;

				float brightBoost = 0.6f + 0.4f * t;
				byte r = (byte)Math.Min(255, baseR * brightBoost);
				byte g = (byte)Math.Min(255, baseG * brightBoost);
				byte b = (byte)Math.Min(255, baseB * brightBoost);
				byte a = (byte)Math.Min(255, alpha * 255);

				Color color = new Color(r, g, b, a);

				Vector2 p1l = sp1 + perp * halfW;
				Vector2 p1r = sp1 - perp * halfW;
				Vector2 p2l = sp2 + perp * halfW;
				Vector2 p2r = sp2 - perp * halfW;

				short baseIdx = (short)vertices.Count;

				vertices.Add(new VertexPositionColor(new Vector3(p1l, 0), color));
				vertices.Add(new VertexPositionColor(new Vector3(p1r, 0), color));
				vertices.Add(new VertexPositionColor(new Vector3(p2l, 0), color));
				vertices.Add(new VertexPositionColor(new Vector3(p2r, 0), color));

				indices.Add((short)(baseIdx + 0));
				indices.Add((short)(baseIdx + 1));
				indices.Add((short)(baseIdx + 2));
				indices.Add((short)(baseIdx + 2));
				indices.Add((short)(baseIdx + 1));
				indices.Add((short)(baseIdx + 3));
			}

			if (len > 0) {
				Vector2 head = trail[len - 1] * radiusScale;
				Vector2 screenHead = center + head;
				float dotR = (line.IsDark ? 1.5f : 2.5f) * radiusScale;
				float dotAlpha = (line.IsDark ? 0.2f : 0.35f) * globalAlpha;

				Color dotCol = new Color(DotColor.R, DotColor.G, DotColor.B, (byte)(dotAlpha * 255));

				short baseIdx = (short)vertices.Count;
				Vector2 p1 = screenHead + new Vector2(-dotR, -dotR);
				Vector2 p2 = screenHead + new Vector2(dotR, -dotR);
				Vector2 p3 = screenHead + new Vector2(-dotR, dotR);
				Vector2 p4 = screenHead + new Vector2(dotR, dotR);

				vertices.Add(new VertexPositionColor(new Vector3(p1, 0), dotCol));
				vertices.Add(new VertexPositionColor(new Vector3(p2, 0), dotCol));
				vertices.Add(new VertexPositionColor(new Vector3(p3, 0), dotCol));
				vertices.Add(new VertexPositionColor(new Vector3(p4, 0), dotCol));

				indices.Add((short)(baseIdx + 0));
				indices.Add((short)(baseIdx + 1));
				indices.Add((short)(baseIdx + 2));
				indices.Add((short)(baseIdx + 2));
				indices.Add((short)(baseIdx + 1));
				indices.Add((short)(baseIdx + 3));
			}
		}
	}
}