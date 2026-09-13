using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;
using System;
using System.Collections.Generic;

namespace ArknightsMod.Content.Items.Accessories.Rogue.Rarity_l3
{
	public class ShieldHitParticles : ModProjectile
	{
		public override string Texture => "ArknightsMod/Content/Items/Accessories/Rogue/Rarity_l3/ShieldHitParticles";


		private struct LightningSegment
		{
			public Vector2 Start;
			public Vector2 End;
			public float Thickness;
			public float Life;
			public Color Color;
			public float UMin;
			public float UMax;
			public float VMin;
			public float VMax;
		}


		private struct SparkParticle
		{
			public Vector2 Position;
			public Vector2 Velocity;
			public float Life;
			public float Size;
			public Color Color;
		}

		private List<LightningSegment> segments;
		private List<SparkParticle> sparks;

		private const int SegmentCount = 20;
		private const float MaxThickness = 10f;
		private const float LightningDuration = 30;

		private bool initialized = false;
		private Texture2D cachedTexture;
		private Vector2 direction;
		private float timer;
		private float noiseOffset;

		public override void SetDefaults() {
			Projectile.width = 2;
			Projectile.height = 2;
			Projectile.penetrate = -1;
			Projectile.timeLeft = (int)LightningDuration;
			Projectile.ignoreWater = true;
			Projectile.tileCollide = false;
			Projectile.friendly = false;
			Projectile.hostile = false;
			Projectile.alpha = 255;
		}

		public override void AI() {
			if (!initialized) {
				InitializeLightning();
				initialized = true;
			}

			timer++;
			noiseOffset += 0.1f;


			if (segments != null) {
				float life = 1f - (timer / LightningDuration);
				life = MathHelper.Clamp(life, 0f, 1f);

				for (int i = 0; i < segments.Count; i++) {
					var seg = segments[i];
					seg.Life = life;

			
					if (life > 0.6f) {
						float flicker = 1f + 0.2f * (float)Math.Sin(timer * 0.3f + i * 0.5f);
						seg.Thickness = MaxThickness * flicker;
					}
					else {
						seg.Thickness = MaxThickness * (life / 0.6f);
					}

					segments[i] = seg;
				}
			}

			
			if (sparks != null) {
				for (int i = sparks.Count - 1; i >= 0; i--) {
					var spark = sparks[i];
					spark.Life -= 0.03f;
					spark.Position += spark.Velocity;
					spark.Velocity *= 0.95f;
					spark.Size *= 0.96f;

					if (spark.Life <= 0f) {
						sparks.RemoveAt(i);
					}
					else {
						sparks[i] = spark;
					}
				}
			}

			if (timer >= LightningDuration)
				Projectile.Kill();
		}

		private void InitializeLightning() {
			cachedTexture = ModContent.Request<Texture2D>(Texture).Value;
			if (cachedTexture == null || cachedTexture.IsDisposed)
				return;

			direction = Projectile.velocity.SafeNormalize(Vector2.Zero);
			if (direction == Vector2.Zero)
				direction = Vector2.UnitX;

			segments = new List<LightningSegment>();
			sparks = new List<SparkParticle>();

		
			float texWidth = cachedTexture.Width;
			float texHeight = cachedTexture.Height;

			
			List<Vector2> points = GenerateSmoothPath(Projectile.Center, direction);

			
			for (int i = 0; i < points.Count - 1; i++) {
				float progress = (float)i / (points.Count - 1);

				// 颜色
				Color baseColor = new Color(
					180 + Main.rand.Next(-20, 20),
					220 + Main.rand.Next(-20, 20),
					255
				);

				// UV随机偏移
				float uMin = Main.rand.NextFloat(0f, 0.3f);
				float uMax = uMin + 0.7f;
				float vMin = Main.rand.NextFloat(0f, 0.3f);
				float vMax = vMin + 0.7f;

				
				LightningSegment segment = new LightningSegment {
					Start = points[i],
					End = points[i + 1],
					Thickness = MaxThickness * (0.8f + 0.4f * (float)Math.Sin(progress * Math.PI)),
					Life = 1f,
					Color = baseColor,
					UMin = 0f,  
					UMax = 1f,  
					VMin = 0f,
					VMax = 1f
				};
				segments.Add(segment);
			}
			CreateBranches(points);
			CreateSparks(points);
		}

		private List<Vector2> GenerateSmoothPath(Vector2 start, Vector2 dir) {
			List<Vector2> points = new List<Vector2>();

			float totalLength = Main.rand.NextFloat(60f, 100f);
			int numPoints = SegmentCount;
			float stepLength = totalLength / numPoints;

			List<Vector2> controlPoints = new List<Vector2>();
			controlPoints.Add(start);

			Vector2 currentPos = start;
			Vector2 currentDir = dir;

			
			for (int i = 1; i <= numPoints; i++) {
				float progress = (float)i / numPoints;

				// 随机偏移角度
				float angleNoise = (Main.rand.NextFloat(-0.5f, 0.5f) +
								   (float)Math.Sin(i * 0.5f + noiseOffset) * 0.3f);

				currentDir = currentDir.RotatedBy(angleNoise * 0.3f);

				Vector2 nextPos = currentPos + currentDir * stepLength;

				// 添加垂直方向的抖动
				Vector2 perp = new Vector2(-currentDir.Y, currentDir.X);
				float perpOffset = (float)(Math.Sin(i * 0.8f + noiseOffset) * 4f * progress);
				nextPos += perp * perpOffset;

				controlPoints.Add(nextPos);
				currentPos = nextPos;
			}

			for (int i = 0; i < controlPoints.Count - 1; i++) {
				Vector2 p0 = i > 0 ? controlPoints[i - 1] : controlPoints[i];
				Vector2 p1 = controlPoints[i];
				Vector2 p2 = controlPoints[i + 1];
				Vector2 p3 = i < controlPoints.Count - 2 ? controlPoints[i + 2] : controlPoints[i + 1];

				int subSteps = 3;
				for (int j = 0; j < subSteps; j++) {
					float t = j / (float)subSteps;
					Vector2 point = CatmullRom(p0, p1, p2, p3, t);
					points.Add(point);
				}
			}

			points.Add(controlPoints[controlPoints.Count - 1]);

			return points;
		}

		private Vector2 CatmullRom(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, float t) {
			float t2 = t * t;
			float t3 = t2 * t;

			return 0.5f * ((2f * p1) +
				(-p0 + p2) * t +
				(2f * p0 - 5f * p1 + 4f * p2 - p3) * t2 +
				(-p0 + 3f * p1 - 3f * p2 + p3) * t3);
		}

		private void CreateBranches(List<Vector2> mainPath) {
			int branchCount = Main.rand.Next(2, 4);

			for (int b = 0; b < branchCount; b++) {
				int startIndex = Main.rand.Next(3, mainPath.Count - 5);
				Vector2 startPos = mainPath[startIndex];

				float branchAngle = Main.rand.NextFloat(-1.2f, 1.2f);
				Vector2 branchDir = direction.RotatedBy(branchAngle);

				int branchLength = Main.rand.Next(3, 6);
				float branchStep = Main.rand.NextFloat(6f, 12f);

				Vector2 currentPos = startPos;
				Vector2 currentDir = branchDir;

				for (int i = 0; i < branchLength; i++) {
					float progress = (float)i / branchLength;

					currentDir = currentDir.RotatedBy(Main.rand.NextFloat(-0.3f, 0.3f));

					Vector2 nextPos = currentPos + currentDir * branchStep * (1f - progress * 0.3f);

					Color branchColor = new Color(140, 200, 255, 150);

					LightningSegment segment = new LightningSegment {
						Start = currentPos,
						End = nextPos,
						Thickness = MaxThickness * 0.5f * (1f - progress * 0.4f),
						Life = 1f,
						Color = branchColor,
						UMin = 0.1f,
						UMax = 0.9f,
						VMin = 0.1f,
						VMax = 0.9f
					};
					segments.Add(segment);

					currentPos = nextPos;

					if (i > 1 && i < branchLength - 2 && Main.rand.NextBool(3)) {
						CreateTinyBranch(currentPos, currentDir, progress);
					}
				}
			}
		}

		private void CreateTinyBranch(Vector2 startPos, Vector2 dir, float progress) {
			int tinyLength = Main.rand.Next(2, 3);
			Vector2 tinyDir = dir.RotatedBy(Main.rand.NextFloat(-1f, 1f));
			Vector2 currentPos = startPos;

			for (int i = 0; i < tinyLength; i++) {
				Vector2 nextPos = currentPos + tinyDir * Main.rand.NextFloat(4f, 8f);

				LightningSegment segment = new LightningSegment {
					Start = currentPos,
					End = nextPos,
					Thickness = MaxThickness * 0.3f,
					Life = 1f,
					Color = new Color(100, 160, 255, 100),
					UMin = 0.2f,
					UMax = 0.8f,
					VMin = 0.2f,
					VMax = 0.8f
				};
				segments.Add(segment);

				currentPos = nextPos;
				tinyDir = tinyDir.RotatedBy(Main.rand.NextFloat(-0.3f, 0.3f));
			}
		}

		private void CreateSparks(List<Vector2> mainPath) {
			int sparkCount = Main.rand.Next(5, 10);

			for (int i = 0; i < sparkCount; i++) {
				int pathIndex = Main.rand.Next(1, mainPath.Count - 2);
				Vector2 basePos = mainPath[pathIndex];

				Vector2 offset = new Vector2(
					Main.rand.NextFloat(-10f, 10f),
					Main.rand.NextFloat(-10f, 10f)
				);

				Vector2 sparkPos = basePos + offset;

				Vector2 sparkVel = new Vector2(
					Main.rand.NextFloat(-1.5f, 1.5f),
					Main.rand.NextFloat(-1.5f, 1.5f)
				);

				Color sparkColor = Main.rand.NextBool(3) ?
					new Color(255, 240, 200) :
					new Color(180, 220, 255);

				SparkParticle spark = new SparkParticle {
					Position = sparkPos,
					Velocity = sparkVel,
					Life = Main.rand.NextFloat(0.5f, 0.8f),
					Size = Main.rand.NextFloat(2f, 4f),
					Color = sparkColor
				};

				sparks.Add(spark);
			}
		}

		public override bool PreDraw(ref Color lightColor) {
			if (cachedTexture == null || cachedTexture.IsDisposed)
				return false;

			try {
				Main.spriteBatch.End();

				// 使用 Additive 混合
				Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive,
					SamplerState.LinearClamp, DepthStencilState.None,
					RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

				DrawLightning();
				DrawSparks();

				Main.spriteBatch.End();
				Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
					SamplerState.LinearClamp, DepthStencilState.None,
					RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
			}
			catch (Exception) {
				if (!Main.spriteBatch.IsDisposed) {
					Main.spriteBatch.End();
					Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
						SamplerState.LinearClamp, DepthStencilState.None,
						RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
				}
			}
			return false;
		}

		private void DrawLightning() {
			if (segments == null || segments.Count == 0)
				return;

			var vertices = new List<VertexData>();

			foreach (var segment in segments) {
				if (segment.Life <= 0.05f)
					continue;

				Vector2 dir = segment.End - segment.Start;
				float length = dir.Length();
				if (length < 1f)
					continue;

				dir.Normalize();
				Vector2 perp = new Vector2(-dir.Y, dir.X);

				Color color = segment.Color * (segment.Life * 0.9f);
				float thickness = segment.Thickness * segment.Life;

				Vector2 screenStart = segment.Start - Main.screenPosition;
				Vector2 screenEnd = segment.End - Main.screenPosition;

				Vector2 leftStart = screenStart + perp * thickness * 0.5f;
				Vector2 rightStart = screenStart - perp * thickness * 0.5f;
				Vector2 leftEnd = screenEnd + perp * thickness * 0.5f;
				Vector2 rightEnd = screenEnd - perp * thickness * 0.5f;


				float uMin = 0f;
				float uMax = 1f;
				float vMin = 0f;
				float vMax = 1f;


				vertices.Add(new VertexData(leftStart, new Vector3(uMin, vMin, 0), color));
				vertices.Add(new VertexData(rightStart, new Vector3(uMax, vMin, 0), color));
				vertices.Add(new VertexData(leftEnd, new Vector3(uMin, vMax, 0), color));


				vertices.Add(new VertexData(rightStart, new Vector3(uMax, vMin, 0), color));
				vertices.Add(new VertexData(rightEnd, new Vector3(uMax, vMax, 0), color));
				vertices.Add(new VertexData(leftEnd, new Vector3(uMin, vMax, 0), color));
			}

			if (vertices.Count >= 3) {
				Main.graphics.GraphicsDevice.Textures[0] = cachedTexture;
				Main.graphics.GraphicsDevice.SamplerStates[0] = SamplerState.LinearClamp; // 用Clamp避免重复
				Main.graphics.GraphicsDevice.DrawUserPrimitives(
					PrimitiveType.TriangleList,
					vertices.ToArray(),
					0,
					vertices.Count / 3);
			}
		}

		private void DrawSparks() {
			if (sparks == null || sparks.Count == 0)
				return;


			Texture2D sparkTexture = Terraria.GameContent.TextureAssets.MagicPixel.Value;
			var vertices = new List<VertexData>();

			foreach (var spark in sparks) {
				if (spark.Life <= 0f)
					continue;

				Color color = spark.Color * spark.Life;
				Vector2 screenPos = spark.Position - Main.screenPosition;
				float size = spark.Size * spark.Life;


				Vector2 halfSize = new Vector2(size, size) * 0.5f;

				Vector2 topLeft = screenPos - halfSize;
				Vector2 topRight = screenPos + new Vector2(halfSize.X, -halfSize.Y);
				Vector2 bottomLeft = screenPos + new Vector2(-halfSize.X, halfSize.Y);
				Vector2 bottomRight = screenPos + halfSize;


				vertices.Add(new VertexData(topLeft, new Vector3(0, 0, 0), color));
				vertices.Add(new VertexData(topRight, new Vector3(1, 0, 0), color));
				vertices.Add(new VertexData(bottomLeft, new Vector3(0, 1, 0), color));

				vertices.Add(new VertexData(topRight, new Vector3(1, 0, 0), color));
				vertices.Add(new VertexData(bottomRight, new Vector3(1, 1, 0), color));
				vertices.Add(new VertexData(bottomLeft, new Vector3(0, 1, 0), color));
			}

			if (vertices.Count >= 3) {
				Main.graphics.GraphicsDevice.Textures[0] = sparkTexture;
				Main.graphics.GraphicsDevice.SamplerStates[0] = SamplerState.LinearClamp;
				Main.graphics.GraphicsDevice.DrawUserPrimitives(
					PrimitiveType.TriangleList,
					vertices.ToArray(),
					0,
					vertices.Count / 3);
			}
		}
	}

	public struct VertexData : IVertexType
	{
		public Vector2 Position;
		public Vector3 TexCoord;
		public Color Color;

		public VertexData(Vector2 position, Vector3 texCoord, Color color) {
			Position = position;
			TexCoord = texCoord;
			Color = color;
		}

		public VertexDeclaration VertexDeclaration => _vertexDeclaration;

		private static readonly VertexDeclaration _vertexDeclaration = new VertexDeclaration(
			new VertexElement(0, VertexElementFormat.Vector2, VertexElementUsage.Position, 0),
			new VertexElement(8, VertexElementFormat.Vector3, VertexElementUsage.TextureCoordinate, 0),
			new VertexElement(20, VertexElementFormat.Color, VertexElementUsage.Color, 0)
		);
	}
}