using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Projectiles.Caster.Valarqvin
{
	public class ValarqvinProj : ModProjectile
	{
		// 拖尾更短更细
		public static int TrailLength = 8;

		// 第一层拖尾（内层）– 缩小尺寸，降低亮度
		public static float TrailWidthStart = 5f;
		public static float TrailWidthEnd = 3f;
		public static Color TrailColorHead = new Color(80, 110, 180);
		public static Color TrailColorTail = new Color(60, 80, 140);
		public static float TrailFlowSpeed = 1.5f;
		public static float TrailFadePower = 1.0f;
		public static float TrailBrightness = 0.8f;
		public static float TrailWaveAmplitude = 3f;
		public static float TrailWaveFrequency = 0.6f;

		// 第二层拖尾（外层）– 缩小
		public static float Trail2WidthStart = 10f;
		public static float Trail2WidthEnd = 5f;
		public static Color Trail2ColorHead = new Color(180, 210, 240);
		public static Color Trail2ColorTail = new Color(70, 110, 200);
		public static float Trail2FlowSpeed = 2.5f;
		public static float Trail2FadePower = 1.5f;
		public static float Trail2Brightness = 1.0f;
		public static float Trail2WaveAmplitude = 4f;
		public static float Trail2WaveFrequency = 0.4f;

		// 弹丸本体
		public static float ProjectileSize = 0.8f;
		public static float ProjectileBrightness = 1.5f;
		public static float Velocity = 35f;

		// 光点粒子颜色池（更淡）
		private static readonly Color[] LightColors = new Color[]
		{
			new Color(100, 180, 230),
			new Color(80, 150, 210),
			new Color(60, 120, 190),
		};

		// 光点粒子结构
		private struct LightParticle
		{
			public Vector2 Position;
			public Vector2 Velocity;
			public float Size;
			public float MaxSize;
			public int Life;
			public int MaxLife;
			public Color Color;
			public bool Active => Life > 0;
		}

		// 多边形粒子结构
		private struct PolyParticle
		{
			public Vector2 Position;
			public Vector2 Velocity;
			public float Size;
			public float MaxSize;
			public int Sides;
			public float Rotation;
			public float RotationSpeed;
			public Color Color;
			public int Life;
			public int MaxLife;
			public bool Active => Life > 0;
		}

		// 拖尾用的历史位置队列
		private Queue<Vector2> trailPositions = new Queue<Vector2>();

		// 纹理
		private Texture2D projectileTexture;
		private Texture2D gradientTexture;
		private Texture2D gradientTexture2;
		private Texture2D lightTexture;
		private Vector2 textureSize;

		// 粒子列表
		private List<LightParticle> lightParticles = new List<LightParticle>();
		private List<PolyParticle> polyParticles = new List<PolyParticle>();

		// BasicEffect
		private BasicEffect basicEffect;
		private const int PolyParticleMinLife = 20;
		private const int PolyParticleMaxLife = 35;

		public override void SetDefaults() {
			Projectile.width = 16;
			Projectile.height = 16;
			Projectile.aiStyle = -1;
			Projectile.friendly = true;
			Projectile.hostile = false;
			Projectile.DamageType = DamageClass.Magic;
			Projectile.penetrate = 1;
			Projectile.timeLeft = 300;
			Projectile.ignoreWater = true;
			Projectile.tileCollide = true;
			Projectile.extraUpdates = 0;

			Projectile.velocity = new Vector2(Velocity, 0f);

			projectileTexture = ModContent.Request<Texture2D>("ArknightsMod/Content/Projectiles/Caster/Valarqvin/ValarqvinProj").Value;
			gradientTexture = ModContent.Request<Texture2D>("ArknightsMod/Content/Projectiles/Caster/Valarqvin/Effect/LightningGradient").Value;
			gradientTexture2 = ModContent.Request<Texture2D>("ArknightsMod/Content/Projectiles/Caster/Valarqvin/Effect/LightningGradient_2").Value;
			lightTexture = ModContent.Request<Texture2D>("ArknightsMod/Content/Projectiles/Caster/Valarqvin/Light").Value;

			if (projectileTexture != null && !projectileTexture.IsDisposed)
				textureSize = new Vector2(projectileTexture.Width, projectileTexture.Height);
		}

		public override void AI() {
			Projectile.rotation = Projectile.velocity.ToRotation();
			UpdateTrail();

			// 降低生成频率：每 4 帧生成 1~2 个光点
			if (Projectile.timeLeft % 4 == 0) {
				int count = Main.rand.Next(1, 3);
				for (int i = 0; i < count; i++) {
					LightParticle lp = new LightParticle();
					lp.Position = Projectile.Center + Main.rand.NextVector2Circular(4f, 4f);
					lp.Velocity = Main.rand.NextVector2Circular(1f, 1f);
					lp.MaxSize = Main.rand.NextFloat(0.4f, 0.8f);
					lp.Size = lp.MaxSize;
					lp.MaxLife = Main.rand.Next(15, 25);
					lp.Life = lp.MaxLife;
					lp.Color = Color.White;
					lightParticles.Add(lp);
				}
			}

			// 降低多边形生成频率：每 10 帧生成 1 个
			if (Projectile.timeLeft % 10 == 0) {
				PolyParticle pp = new PolyParticle();
				pp.Position = Projectile.Center + Main.rand.NextVector2Circular(6f, 6f);
				pp.Velocity = Projectile.velocity * 0.15f + Main.rand.NextVector2Circular(1f, 1f);
				pp.MaxSize = Main.rand.NextFloat(5f, 9f);
				pp.Size = pp.MaxSize;
				pp.Sides = Main.rand.Next(3, 6);
				pp.Rotation = Main.rand.NextFloat(MathHelper.TwoPi);
				pp.RotationSpeed = Main.rand.NextFloat(-0.05f, 0.05f);
				pp.Color = LightColors[Main.rand.Next(LightColors.Length)];
				pp.MaxLife = Main.rand.Next(PolyParticleMinLife, PolyParticleMaxLife);
				pp.Life = pp.MaxLife;
				polyParticles.Add(pp);
			}

			// 更新光点粒子
			for (int i = lightParticles.Count - 1; i >= 0; i--) {
				LightParticle lp = lightParticles[i];
				lp.Life--;
				lp.Position += lp.Velocity;
				lp.Velocity *= 0.96f;
				float progress = 1f - (float)lp.Life / lp.MaxLife;
				lp.Size = lp.MaxSize / (1f + progress * 2f);
				lightParticles[i] = lp;
				if (!lp.Active)
					lightParticles.RemoveAt(i);
			}

			// 更新多边形粒子
			for (int i = polyParticles.Count - 1; i >= 0; i--) {
				PolyParticle pp = polyParticles[i];
				pp.Life--;
				pp.Position += pp.Velocity;
				pp.Velocity *= 0.97f;
				pp.Rotation += pp.RotationSpeed;
				float progress = 1f - (float)pp.Life / pp.MaxLife;
				pp.Size = pp.MaxSize / (1f + progress * 1.5f);
				if (pp.Life % 10 == 0 && pp.Sides > 3) {
					pp.Sides--;
				}
				polyParticles[i] = pp;
				if (!pp.Active)
					polyParticles.RemoveAt(i);
			}
		}

		private void UpdateTrail() {
			trailPositions.Enqueue(Projectile.Center);
			while (trailPositions.Count > TrailLength)
				trailPositions.Dequeue();
		}

		public override void OnKill(int timeLeft) {
			if (Projectile.owner != Main.myPlayer)
				return;

			Projectile.NewProjectile(
				Projectile.GetSource_FromThis(),
				Projectile.Center,
				Projectile.velocity * 0.3f,
				ModContent.ProjectileType<ValarqvinProj_Hit>(),
				0, 0f, Projectile.owner);
		}

		public override bool PreDraw(ref Color lightColor) => false;

		public override void PostDraw(Color lightColor) {
			DrawTrail2();
			DrawTrail();
			DrawProjectile();
			DrawLightParticles();
			DrawPolyParticles();
		}

		private void DrawProjectile() {
			if (projectileTexture == null || projectileTexture.IsDisposed)
				return;
			Main.spriteBatch.End();
			Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
			DrawProjectileVertices(Main.graphics.GraphicsDevice, projectileTexture);
			Main.spriteBatch.End();
			Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
		}

		private void DrawProjectileVertices(GraphicsDevice gd, Texture2D texture) {
			float alphaFactor = (float)Projectile.timeLeft / 300f;
			Color drawColor = TrailColorHead * alphaFactor * ProjectileBrightness;

			float textureAspect = textureSize.X / textureSize.Y;
			float baseSize = Projectile.width * Projectile.scale * ProjectileSize;
			float width = baseSize * textureAspect;
			float height = baseSize;

			Vector2 center = Projectile.Center - Main.screenPosition;
			float rot = Projectile.rotation;
			Vector2 offset = new Vector2(width / 2f - 2f, 0).RotatedBy(rot);
			Vector2 adjustedCenter = center - offset;

			Vector2 topLeft = adjustedCenter + new Vector2(-width / 2f, -height / 2f).RotatedBy(rot);
			Vector2 topRight = adjustedCenter + new Vector2(width / 2f, -height / 2f).RotatedBy(rot);
			Vector2 bottomLeft = adjustedCenter + new Vector2(-width / 2f, height / 2f).RotatedBy(rot);
			Vector2 bottomRight = adjustedCenter + new Vector2(width / 2f, height / 2f).RotatedBy(rot);

			VertexData[] vertices = new VertexData[]
			{
				new VertexData(topLeft, new Vector3(0, 0, 1), drawColor),
				new VertexData(topRight, new Vector3(1, 0, 1), drawColor),
				new VertexData(bottomLeft, new Vector3(0, 1, 1), drawColor),
				new VertexData(bottomRight, new Vector3(1, 1, 1), drawColor)
			};
			gd.Textures[0] = texture;
			gd.DrawUserPrimitives(PrimitiveType.TriangleStrip, vertices, 0, 2);
		}

		private void DrawLightParticles() {
			if (lightParticles.Count == 0 || lightTexture == null || lightTexture.IsDisposed)
				return;
			Main.spriteBatch.End();
			Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
			foreach (var lp in lightParticles) {
				float alpha = (float)lp.Life / lp.MaxLife;
				float size = lp.Size * 8f;
				Vector2 pos = lp.Position - Main.screenPosition;
				Color c = lp.Color * alpha;
				Rectangle rect = new Rectangle((int)(pos.X - size * 0.5f), (int)(pos.Y - size * 0.5f), (int)size, (int)size);
				Main.spriteBatch.Draw(lightTexture, rect, null, c, 0f, Vector2.Zero, SpriteEffects.None, 0f);
			}
			Main.spriteBatch.End();
			Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
		}

		private void DrawPolyParticles() {
			if (polyParticles.Count == 0)
				return;
			Main.spriteBatch.End();
			GraphicsDevice gd = Main.graphics.GraphicsDevice;
			BlendState prevBlend = gd.BlendState;
			RasterizerState prevRaster = gd.RasterizerState;
			gd.BlendState = BlendState.Additive;
			gd.RasterizerState = RasterizerState.CullNone;

			if (basicEffect == null) { basicEffect = new BasicEffect(gd); basicEffect.VertexColorEnabled = true; }
			basicEffect.Projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);
			basicEffect.View = Main.GameViewMatrix.TransformationMatrix;
			basicEffect.World = Matrix.Identity;

			foreach (var pp in polyParticles) {
				float alpha = (float)pp.Life / pp.MaxLife;
				Color drawColor = pp.Color * alpha;
				Color centerColor = drawColor * 0.3f;
				Vector2 center = pp.Position - Main.screenPosition;
				float r = pp.Size;
				int sides = Math.Max(pp.Sides, 3);

				VertexPositionColor[] verts = new VertexPositionColor[sides * 3];
				for (int j = 0; j < sides; j++) {
					float a1 = pp.Rotation + j * MathHelper.TwoPi / sides;
					float a2 = pp.Rotation + ((j + 1) % sides) * MathHelper.TwoPi / sides;
					Vector2 o1 = center + new Vector2(MathF.Cos(a1) * r, MathF.Sin(a1) * r);
					Vector2 o2 = center + new Vector2(MathF.Cos(a2) * r, MathF.Sin(a2) * r);
					verts[j * 3 + 0] = new VertexPositionColor(new Vector3(center, 0), centerColor);
					verts[j * 3 + 1] = new VertexPositionColor(new Vector3(o1, 0), drawColor);
					verts[j * 3 + 2] = new VertexPositionColor(new Vector3(o2, 0), drawColor);
				}
				foreach (EffectPass pass in basicEffect.CurrentTechnique.Passes) { pass.Apply(); gd.DrawUserPrimitives(PrimitiveType.TriangleList, verts, 0, sides); }
			}

			gd.BlendState = prevBlend;
			gd.RasterizerState = prevRaster;
			Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
		}

		private void DrawTrail() {
			if (trailPositions.Count < 2 || gradientTexture == null || gradientTexture.IsDisposed)
				return;
			Main.spriteBatch.End();
			Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearWrap, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
			DrawTrailVertices(Main.graphics.GraphicsDevice, gradientTexture, TrailWidthStart, TrailWidthEnd, TrailFlowSpeed, TrailFadePower, TrailColorHead, TrailColorTail, TrailBrightness, TrailWaveAmplitude, TrailWaveFrequency, false);
			Main.spriteBatch.End();
			Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
		}

		private void DrawTrail2() {
			if (trailPositions.Count < 2 || gradientTexture2 == null || gradientTexture2.IsDisposed)
				return;
			Main.spriteBatch.End();
			Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.LinearWrap, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
			DrawTrailVertices(Main.graphics.GraphicsDevice, gradientTexture2, Trail2WidthStart, Trail2WidthEnd, Trail2FlowSpeed, Trail2FadePower, Trail2ColorHead, Trail2ColorTail, Trail2Brightness, Trail2WaveAmplitude, Trail2WaveFrequency, false);
			Main.spriteBatch.End();
			Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
		}

		private void DrawTrailVertices(GraphicsDevice gd, Texture2D texture, float widthStart, float widthEnd, float flowSpeed, float fadePower, Color colorHead, Color colorTail, float brightness, float waveAmplitude, float waveFrequency, bool useAlphaBlend) {
			Vector2[] points = trailPositions.ToArray();
			int count = points.Length;
			if (count < 2)
				return;

			float[] cumLength = new float[count];
			float totalLength = 0f;
			for (int i = 1; i < count; i++) { cumLength[i] = cumLength[i - 1] + Vector2.Distance(points[i], points[i - 1]); totalLength = cumLength[i]; }

			float timeOffset = (float)Main.timeForVisualEffects * 0.02f * flowSpeed;
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
				float waveIntensity = CalculateWaveIntensity(t);
				float wave = CalculateWaveOffset(cumLength[i], totalLength, waveFrequency, waveAmplitude * waveIntensity);
				Vector2 displacedPoint = points[i] + perp * wave;

				Color gradientColor = useAlphaBlend ? colorHead : Color.Lerp(colorTail, colorHead, t);
				float alpha = useAlphaBlend ? brightness * (float)Math.Pow(t, fadePower) : (float)Math.Pow(t, fadePower) * brightness;
				alpha = MathHelper.Clamp(alpha, 0.02f, 1f);

				float width = MathHelper.Lerp(widthEnd, widthStart, t);
				Vector2 left = displacedPoint - perp * width;
				Vector2 right = displacedPoint + perp * width;
				float v = totalLength > 0 ? cumLength[i] / totalLength : 0f;
				v = (v + timeOffset) % 1f;

				Vector2 leftScreen = left - Main.screenPosition;
				Vector2 rightScreen = right - Main.screenPosition;
				Color trailColor = gradientColor * alpha;
				vertices.Add(new VertexData(leftScreen, new Vector3(0, v, 1), trailColor));
				vertices.Add(new VertexData(rightScreen, new Vector3(1, v, 1), trailColor));
			}
			if (vertices.Count >= 4) { gd.Textures[0] = texture; gd.DrawUserPrimitives(PrimitiveType.TriangleStrip, vertices.ToArray(), 0, vertices.Count - 2); }
		}

		private float CalculateWaveIntensity(float t) {
			if (t <= 0.0f)
				return 0.0f;
			if (t <= 0.3f)
				return MathHelper.Lerp(0.0f, 0.3f, t / 0.3f);
			if (t <= 0.5f)
				return MathHelper.Lerp(0.3f, 0.5f, (t - 0.3f) / 0.2f);
			if (t <= 0.8f)
				return MathHelper.Lerp(0.5f, 1.0f, (t - 0.5f) / 0.3f);
			if (t < 1.0f)
				return MathHelper.Lerp(1.0f, 0.0f, (t - 0.8f) / 0.2f);
			return 0.0f;
		}

		private float CalculateWaveOffset(float distanceFromTail, float totalLength, float frequency, float amplitude) {
			float timeFactor = 0.155f;
			float phase = distanceFromTail * frequency * 0.1f + (float)Main.timeForVisualEffects * timeFactor;
			return (float)Math.Sin(phase) * amplitude;
		}
	}
}
