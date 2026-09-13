using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using ArknightsMod.Content.Projectiles.Supporter.Tragodia;
using ArknightsMod.Content.Projectiles.Supporter.Tragodia.SP2;
using ArknightsMod.Content.Projectiles.Supporter.Tragodia.SP1;
using ArknightsMod.Content.Projectiles.Supporter.Tragodia.EffectImage;
using ArknightsMod.Content.Projectiles.Supporter.Tragodia.NormalAttack;

namespace ArknightsMod.Content.Projectiles.Supporter.Tragodia.SP3
{
	public class SP3ExtraVisual : ModProjectile
	{

		private const float BaseRadius = 60f;
		private const float EllipseRatio = 0.6f;
		private const float MaxRiseHeight = 800f;
		private const float SpiralSpeed = 0.12f;
		private const float RiseSpeedStart = 5f;
		private const float RiseSpeedEnd = 0.8f;
		private const int MaxLife = 90;


		private const float ProjectileBaseSize = 20f;


		private const int TrailLength = 30;
		private Queue<Vector2> trailPositions = new Queue<Vector2>();
		private const float TrailWidthStart = 14f;
		private const float TrailWidthEnd = 2f;
		private static readonly Color TrailColorHead = new Color(200, 140, 255);
		private static readonly Color TrailColorTail = new Color(80, 20, 180);
		private const float TrailFlowSpeed = 2f;
		private const float TrailFadePower = 3f;
		private const float TrailBrightness = 1.3f;


		private const float BeamLength = 800f;
		private const float BeamWidth = 24f;
		private const float BeamDissolveTime = 90f;
		private const int BeamFlowSpeed = 2;
		private static readonly Color BeamColorCenter = new Color(200, 100, 255);
		private static readonly Color BeamColorEdge = new Color(60, 20, 160);

		// 纹理
		private Texture2D particleTexture;
		private Texture2D gradientTexture;
		private Texture2D glowTexture;


		private static readonly float[] AngleOffsets = { 0f, MathHelper.TwoPi / 3f, MathHelper.TwoPi * 2f / 3f };
		private int subIndex = 0;
		private Vector2 beamCenter;

		// 依旧懒，明明可以直接引用SP2下的VertexData
		private struct VertexData : IVertexType
		{
			public Vector3 Position;
			public Vector3 TextureCoordinate;
			public Color Color;
			public static readonly VertexDeclaration VertexDeclaration = new VertexDeclaration(
				new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
				new VertexElement(12, VertexElementFormat.Vector3, VertexElementUsage.TextureCoordinate, 0),
				new VertexElement(24, VertexElementFormat.Color, VertexElementUsage.Color, 0)
			);
			VertexDeclaration IVertexType.VertexDeclaration => VertexDeclaration;

			public VertexData(Vector2 position, Vector3 texCoord, Color color) {
				Position = new Vector3(position.X, position.Y, 0f);
				TextureCoordinate = texCoord;
				Color = color;
			}
		}


		private float spiralAngle;
		private Vector2 bottomCenter;
		private float currentHeight;
		private float riseSpeed;
		private Vector2 velocity;
		private float randomRadiusOffset;
		private float randomHeightOffset;
		private float randomAngleOffset;

		public override void SetDefaults() {
			Projectile.width = 12;
			Projectile.height = 12;
			Projectile.friendly = false;
			Projectile.hostile = false;
			Projectile.penetrate = -1;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
			Projectile.timeLeft = MaxLife;
			Projectile.alpha = 255;
			Projectile.scale = 1f;
		}

		public override void AI() {

			int delayFrames = (int)Projectile.ai[2] * 2;
			if (Projectile.localAI[1] < delayFrames) {
				Projectile.localAI[1]++;
				// 初始化拖尾队列
				if (trailPositions.Count == 0) {
					for (int i = 0; i < TrailLength; i++)
						trailPositions.Enqueue(Projectile.Center);
				}
				return;
			}

			if (Projectile.localAI[0] == 0) {
				Projectile.localAI[0] = 1;
				Initialize();
			}

			trailPositions.Enqueue(Projectile.Center);
			while (trailPositions.Count > TrailLength)
				trailPositions.Dequeue();

			UpdateSpiralMotion();

			float alpha = (float)Projectile.timeLeft / MaxLife;
			Lighting.AddLight(Projectile.Center, new Vector3(0.5f, 0, 1) * alpha * 0.4f);
		}

		private void Initialize() {
			bottomCenter = Projectile.Center;
			beamCenter = Projectile.Center;
			currentHeight = 0f;
			riseSpeed = RiseSpeedStart;

			subIndex = (int)Projectile.ai[2];
			if (subIndex < 0 || subIndex > 2)
				subIndex = 0;
			spiralAngle = AngleOffsets[subIndex] + Main.rand.NextFloat(-0.3f, 0.3f);

			randomRadiusOffset = Main.rand.NextFloat(-25f, 25f);
			randomHeightOffset = Main.rand.NextFloat(-15f, 15f);
			randomAngleOffset = Main.rand.NextFloat(-0.4f, 0.4f);

			TryLoadTexture(ref particleTexture, "ArknightsMod/Content/Projectiles/Supporter/Tragodia/SP2/SP2ExtraVisual");
			TryLoadTexture(ref gradientTexture, "ArknightsMod/Content/Projectiles/Caster/Valarqvin/Effect/LightningGradient");
			TryLoadTexture(ref glowTexture, "ArknightsMod/Content/Projectiles/Supporter/Tragodia/EffectImage/ProjLightCore");

			for (int i = 0; i < TrailLength; i++)
				trailPositions.Enqueue(Projectile.Center);
		}

		private void TryLoadTexture(ref Texture2D texture, string path) {
			if (ModContent.HasAsset(path))
				texture = ModContent.Request<Texture2D>(path).Value;
		}

		private void UpdateSpiralMotion() {
			float progress = (float)(MaxLife - Projectile.timeLeft) / MaxLife;

			riseSpeed = MathHelper.Lerp(RiseSpeedStart, RiseSpeedEnd, (float)Math.Pow(progress, 0.7f));
			currentHeight += riseSpeed;

			spiralAngle += SpiralSpeed * (1f + randomAngleOffset * 0.3f);

			float heightRatio = currentHeight / MaxRiseHeight;
			float baseRadius = BaseRadius + randomRadiusOffset * (1f - heightRatio);
			float currentRadius = baseRadius * (1f - heightRatio * 0.8f);

			float x = MathF.Cos(spiralAngle) * currentRadius;
			float y = MathF.Sin(spiralAngle) * currentRadius * EllipseRatio;

			Projectile.Center = bottomCenter + new Vector2(x, y - currentHeight + randomHeightOffset * (1f - heightRatio));

			float nextAngle = spiralAngle + SpiralSpeed;
			float nextRadius = baseRadius * (1f - (currentHeight + riseSpeed) / MaxRiseHeight * 0.8f);
			float nextX = MathF.Cos(nextAngle) * nextRadius;
			float nextY = MathF.Sin(nextAngle) * nextRadius * EllipseRatio;
			Vector2 nextPos = bottomCenter + new Vector2(nextX, nextY - (currentHeight + riseSpeed));

			velocity = nextPos - Projectile.Center;
			if (velocity.LengthSquared() > 0.01f)
				velocity.Normalize();
			else
				velocity = new Vector2(0, -1);

			if (currentHeight >= MaxRiseHeight)
				Projectile.Kill();
		}


		public override bool PreDraw(ref Color lightColor) => false;

		public override void PostDraw(Color lightColor) {
			DrawBeam();
			DrawTrail();
			DrawProjectileGlow();
			DrawProjectile();
		}

		
		private void DrawBeam() {
			if (gradientTexture == null)
				return;

			float globalAlpha = (float)Projectile.timeLeft / MaxLife;
			float beamProgress = Math.Min((float)(MaxLife - Projectile.timeLeft) / BeamDissolveTime, 1f);

			Vector2 beamTop = beamCenter + new Vector2(0, -BeamLength);
			Vector2 beamBottom = beamCenter;
			Vector2 dir = beamBottom - beamTop;
			if (dir.Length() < 1f)
				return;
			dir.Normalize();
			Vector2 right = new Vector2(-dir.Y, dir.X);
			float halfWidth = BeamWidth * 0.5f;

			Main.spriteBatch.End();
			Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.LinearWrap,
				DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

			float timeOffset = (float)Main.timeForVisualEffects * 0.02f * BeamFlowSpeed;
			timeOffset -= (float)Math.Floor(timeOffset);

			List<VertexData> vertices = new List<VertexData>();
			int segments = 60;

			for (int i = 0; i <= segments; i++) {
				float t = (float)i / segments;
				Vector2 point = Vector2.Lerp(beamTop, beamBottom, t);

				float v = t + timeOffset;
				v -= (float)Math.Floor(v);

				float edgeAlpha = MathHelper.Clamp(1f - beamProgress * 1.5f, 0f, 1f);
				float centerAlpha = MathHelper.Clamp(1f - beamProgress * 0.4f, 0f, 1f);

				Color centerColor = BeamColorCenter * centerAlpha * globalAlpha;
				Color edgeColor = BeamColorEdge * edgeAlpha * globalAlpha;

				Vector2 leftPos = point - right * halfWidth;
				Vector2 rightPos = point + right * halfWidth;
				Vector2 centerPos = point;

				Vector2 leftScreen = leftPos - Main.screenPosition;
				Vector2 centerScreen = centerPos - Main.screenPosition;
				Vector2 rightScreen = rightPos - Main.screenPosition;

				vertices.Add(new VertexData(leftScreen, new Vector3(0, v, 1), edgeColor));
				vertices.Add(new VertexData(centerScreen, new Vector3(0.5f, v, 1), centerColor));
				vertices.Add(new VertexData(rightScreen, new Vector3(1, v, 1), edgeColor));
			}

			if (vertices.Count >= 6) {
				List<short> indices = new List<short>();
				for (int i = 0; i < segments; i++) {
					short i0 = (short)(i * 3);
					short i1 = (short)(i * 3 + 1);
					short i2 = (short)(i * 3 + 2);
					short j0 = (short)((i + 1) * 3);
					short j1 = (short)((i + 1) * 3 + 1);
					short j2 = (short)((i + 1) * 3 + 2);

					indices.Add(i0);
					indices.Add(i1);
					indices.Add(j0);
					indices.Add(i1);
					indices.Add(j1);
					indices.Add(j0);
					indices.Add(i1);
					indices.Add(i2);
					indices.Add(j1);
					indices.Add(i2);
					indices.Add(j2);
					indices.Add(j1);
				}

				Main.graphics.GraphicsDevice.Textures[0] = gradientTexture;
				Main.graphics.GraphicsDevice.DrawUserIndexedPrimitives(
					PrimitiveType.TriangleList,
					vertices.ToArray(), 0, vertices.Count,
					indices.ToArray(), 0, indices.Count / 3);
			}

			Main.spriteBatch.End();
			Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
				Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer,
				null, Main.GameViewMatrix.TransformationMatrix);
		}


		private void DrawTrail() {
			if (trailPositions.Count < 2 || gradientTexture == null)
				return;
			Main.spriteBatch.End();
			Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearWrap,
				DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
			DrawTrailVertices();
			Main.spriteBatch.End();
			Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
				Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer,
				null, Main.GameViewMatrix.TransformationMatrix);
		}

		private void DrawTrailVertices() {
			Vector2[] points = trailPositions.ToArray();
			int count = points.Length;
			if (count < 2)
				return;

			float globalAlpha = (float)Projectile.timeLeft / MaxLife;
			float[] cumLength = new float[count];
			float totalLength = 0f;
			for (int i = 1; i < count; i++) {
				cumLength[i] = cumLength[i - 1] + Vector2.Distance(points[i], points[i - 1]);
				totalLength = cumLength[i];
			}

			float timeOffset = (float)Main.timeForVisualEffects * 0.02f * TrailFlowSpeed;
			timeOffset -= (float)Math.Floor(timeOffset);

			List<VertexData> vertices = new List<VertexData>();
			for (int i = 0; i < count; i++) {
				Vector2 dir;
				if (i == 0)
					dir = points[i + 1] - points[i];
				else if (i == count - 1)
					dir = points[i] - points[i - 1];
				else
					dir = points[i + 1] - points[i - 1];
				if (dir.LengthSquared() < 0.001f)
					dir = Vector2.UnitX;
				else
					dir.Normalize();

				Vector2 perp = new Vector2(-dir.Y, dir.X);
				float t = (float)i / (count - 1);

				Color gradientColor = Color.Lerp(TrailColorTail, TrailColorHead, t);
				float alpha = (float)Math.Pow(t, TrailFadePower) * TrailBrightness * globalAlpha;
				alpha = MathHelper.Clamp(alpha, 0.02f, 1f);

				float width = MathHelper.Lerp(TrailWidthEnd, TrailWidthStart, t);
				Vector2 left = points[i] - perp * width;
				Vector2 right = points[i] + perp * width;
				float v = totalLength > 0 ? cumLength[i] / totalLength : 0f;
				v = (v + timeOffset) % 1f;

				Vector2 leftScreen = left - Main.screenPosition;
				Vector2 rightScreen = right - Main.screenPosition;
				Color trailColor = gradientColor * alpha;

				vertices.Add(new VertexData(leftScreen, new Vector3(0, v, 1), trailColor));
				vertices.Add(new VertexData(rightScreen, new Vector3(1, v, 1), trailColor));
			}

			if (vertices.Count >= 4) {
				Main.graphics.GraphicsDevice.Textures[0] = gradientTexture;
				Main.graphics.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleStrip,
					vertices.ToArray(), 0, vertices.Count - 2);
			}
		}


		private void DrawProjectileGlow() {
			if (glowTexture == null)
				return;

			float alpha = (float)Projectile.timeLeft / MaxLife;
			Vector2 drawPos = Projectile.Center - Main.screenPosition;

			Main.spriteBatch.End();
			Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive,
				SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone,
				null, Main.GameViewMatrix.TransformationMatrix);

			float outerSize = ProjectileBaseSize * 4f;
			Main.spriteBatch.Draw(glowTexture,
				new Rectangle((int)(drawPos.X - outerSize * 0.5f), (int)(drawPos.Y - outerSize * 0.5f), (int)outerSize, (int)outerSize),
				null, new Color(100, 30, 200) * alpha * 0.3f);

			float innerSize = ProjectileBaseSize * 2f;
			Main.spriteBatch.Draw(glowTexture,
				new Rectangle((int)(drawPos.X - innerSize * 0.5f), (int)(drawPos.Y - innerSize * 0.5f), (int)innerSize, (int)innerSize),
				null, new Color(160, 60, 255) * alpha * 0.5f);

			Main.spriteBatch.End();
			Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
				Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer,
				null, Main.GameViewMatrix.TransformationMatrix);
		}


		private void DrawProjectile() {
			if (particleTexture == null)
				return;

			Main.spriteBatch.End();
			Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive,
				SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone,
				null, Main.GameViewMatrix.TransformationMatrix);

			DrawProjectileVertices();

			Main.spriteBatch.End();
			Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
				Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer,
				null, Main.GameViewMatrix.TransformationMatrix);
		}

		private void DrawProjectileVertices() {
			float alphaFactor = (float)Projectile.timeLeft / MaxLife;
			Color drawColor = new Color(200, 140, 255) * alphaFactor * 1.5f;

			Vector2 textureSize = particleTexture.Size();
			float aspect = textureSize.X / textureSize.Y;
			float baseSize = ProjectileBaseSize;
			float width = baseSize * aspect;
			float height = baseSize;

			Vector2 center = Projectile.Center - Main.screenPosition;
			float rot = velocity.ToRotation() + MathHelper.PiOver2;

			Vector2 topLeft = center + new Vector2(-width / 2f, -height / 2f).RotatedBy(rot);
			Vector2 topRight = center + new Vector2(width / 2f, -height / 2f).RotatedBy(rot);
			Vector2 bottomLeft = center + new Vector2(-width / 2f, height / 2f).RotatedBy(rot);
			Vector2 bottomRight = center + new Vector2(width / 2f, height / 2f).RotatedBy(rot);

			VertexData[] vertices = new VertexData[]
			{
				new VertexData(topLeft, new Vector3(0, 0, 1), drawColor),
				new VertexData(topRight, new Vector3(1, 0, 1), drawColor),
				new VertexData(bottomLeft, new Vector3(0, 1, 1), drawColor),
				new VertexData(bottomRight, new Vector3(1, 1, 1), drawColor)
			};

			Main.graphics.GraphicsDevice.Textures[0] = particleTexture;
			Main.graphics.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleStrip, vertices, 0, 2);
		}
	}
}