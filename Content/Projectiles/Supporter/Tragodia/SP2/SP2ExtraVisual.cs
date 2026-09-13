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

namespace ArknightsMod.Content.Projectiles.Supporter.Tragodia.SP2
{
	public class SP2ExtraVisual : ModProjectile
	{

		private const float BaseRadius = 90f;
		private const float EllipseRatio = 0.6f;
		private const float MaxRiseHeight = 150f;
		private const float SpiralSpeed = 0.12f;
		private const float RiseSpeedStart = 1.8f;
		private const float RiseSpeedEnd = 0.3f;
		private const int MaxLife = 90;


		private const float ProjectileBaseSize = 28f;


		private const int TrailLength = 30;
		private Queue<Vector2> trailPositions = new Queue<Vector2>();

		private const float TrailWidthStart = 17f;
		private const float TrailWidthEnd = 3f;
		private static readonly Color TrailColorHead = new Color(200, 140, 255);
		private static readonly Color TrailColorTail = new Color(80, 20, 180);
		private const float TrailFlowSpeed = 2f;
		private const float TrailFadePower = 3f;
		private const float TrailBrightness = 1.3f;

		// 纹理
		private Texture2D particleTexture;
		private Texture2D gradientTexture;
		private Texture2D glowTexture; 

		//懒得引用了，虽然我知道这不好
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

		//运行
		private float spiralAngle;
		private Vector2 bottomCenter;
		private Vector2 currentPos;
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
			currentHeight = 0f;
			riseSpeed = RiseSpeedStart;
			spiralAngle = Main.rand.NextFloat(MathHelper.TwoPi);

			randomRadiusOffset = Main.rand.NextFloat(-30f, 30f);
			randomHeightOffset = Main.rand.NextFloat(-20f, 20f);
			randomAngleOffset = Main.rand.NextFloat(-0.5f, 0.5f);

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

			currentPos = bottomCenter + new Vector2(x, y - currentHeight + randomHeightOffset * (1f - heightRatio));
			Projectile.Center = currentPos;

			float nextAngle = spiralAngle + SpiralSpeed;
			float nextBaseRadius = BaseRadius + randomRadiusOffset * (1f - (currentHeight + riseSpeed) / MaxRiseHeight);
			float nextRadius = nextBaseRadius * (1f - (currentHeight + riseSpeed) / MaxRiseHeight * 0.8f);
			float nextX = MathF.Cos(nextAngle) * nextRadius;
			float nextY = MathF.Sin(nextAngle) * nextRadius * EllipseRatio;
			Vector2 nextPos = bottomCenter + new Vector2(nextX, nextY - (currentHeight + riseSpeed));

			velocity = nextPos - currentPos;
			if (velocity.LengthSquared() > 0.01f)
				velocity.Normalize();
			else
				velocity = new Vector2(0, -1);

			if (currentHeight >= MaxRiseHeight)
				Projectile.Kill();
		}

		public override bool PreDraw(ref Color lightColor) => false;

		public override void PostDraw(Color lightColor) {
			DrawTrail();
			DrawProjectileGlow();
			DrawProjectile();
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

		// 圆形光晕
		private void DrawProjectileGlow() {
			if (glowTexture == null)
				return;

			float alpha = (float)Projectile.timeLeft / MaxLife;
			Vector2 drawPos = Projectile.Center - Main.screenPosition;

			Main.spriteBatch.End();
			Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive,
				SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone,
				null, Main.GameViewMatrix.TransformationMatrix);

			// 外层光晕
			float outerSize = ProjectileBaseSize * 5f;
			Rectangle outerRect = new Rectangle(
				(int)(drawPos.X - outerSize * 0.5f), (int)(drawPos.Y - outerSize * 0.5f),
				(int)outerSize, (int)outerSize);
			Main.spriteBatch.Draw(glowTexture, outerRect, null, new Color(100, 30, 200) * alpha * 0.3f);

			// 内层光晕
			float innerSize = ProjectileBaseSize * 2.5f;
			Rectangle innerRect = new Rectangle(
				(int)(drawPos.X - innerSize * 0.5f), (int)(drawPos.Y - innerSize * 0.5f),
				(int)innerSize, (int)innerSize);
			Main.spriteBatch.Draw(glowTexture, innerRect, null, new Color(160, 60, 255) * alpha * 0.5f);

			Main.spriteBatch.End();
			Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
				Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer,
				null, Main.GameViewMatrix.TransformationMatrix);
		}

		private void DrawProjectile() {
			if (particleTexture == null)
				return;

			float alpha = (float)Projectile.timeLeft / MaxLife;

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
			float textureAspect = textureSize.X / textureSize.Y;
			float baseSize = ProjectileBaseSize;
			float width = baseSize * textureAspect;
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