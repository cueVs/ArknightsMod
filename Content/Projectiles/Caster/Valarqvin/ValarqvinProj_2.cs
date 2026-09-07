using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Projectiles.Caster.Valarqvin
{
	public class ValarqvinProj_2 : ModProjectile
	{
		// 拖尾长度，也就是用多少个历史位置来画拖尾
		public static int TrailLength = 13;

		// 第一层拖尾，最细的那条，颜色偏深蓝
		public static float TrailWidthStart = 8f;
		public static float TrailWidthEnd = 5f;
		public static Color TrailColorHead = new Color(68, 90, 172);
		public static Color TrailColorTail = new Color(73, 113, 223);
		public static float TrailFlowSpeed = 2f;
		public static float TrailFadePower = 1.2f;
		public static float TrailBrightness = 1f;
		public static float TrailWaveAmplitude = 6f;
		public static float TrailWaveFrequency = 0.8f;

		// 第二层拖尾，中等粗细，颜色偏亮蓝白，用 Additive 混合发光
		public static float Trail2WidthStart = 18f;
		public static float Trail2WidthEnd = 7f;
		public static Color Trail2ColorHead = new Color(217, 252, 255);
		public static Color Trail2ColorTail = new Color(86, 127, 251);
		public static float Trail2FlowSpeed = 4f;
		public static float Trail2FadePower = 2f;
		public static float Trail2Brightness = 1.7f;
		public static float Trail2WaveAmplitude = 8f;
		public static float Trail2WaveFrequency = 0.6f;

		// 第三层拖尾，最宽的一层，暗蓝黑色，放在最底下作为轮廓阴影
		public static float Trail3WidthStart = 16f;
		public static float Trail3WidthEnd = 12f;
		public static Color Trail3ColorHead = new Color(20, 30, 60);
		public static Color Trail3ColorTail = new Color(10, 20, 40);
		public static float Trail3FlowSpeed = 2f;
		public static float Trail3FadePower = 1.2f;
		public static float Trail3Brightness = 13f;
		public static float Trail3WaveAmplitude = 14f;
		public static float Trail3WaveFrequency = 0.5f;

		// 弹丸本体的大小和亮度
		public static float ProjectileSize = 1.4f;
		public static float ProjectileBrightness = 2.3f;
		public static float Velocity = 35f;

		// 科技风多边形粒子可用的颜色池，青蓝到深蓝之间随机挑选
		private static readonly Color[] LightColors = new Color[]
		{
			new Color(0, 200, 255),
			new Color(0, 150, 255),
			new Color(30, 80, 220),
			new Color(0, 100, 200),
			new Color(20, 60, 180),
			new Color(0, 180, 230),
		};

		// 光点粒子，就是弹丸周围飘散的那些白色小光点
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

		// 科技感多边形粒子，会随机变边数和颜色，带抖动
		private struct TechParticle
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

		// 各种纹理
		private Texture2D projectileTexture;
		private Texture2D gradientTexture;
		private Texture2D gradientTexture2;
		private Texture2D gradientTexture3;
		private Texture2D lightTexture;
		private Vector2 textureSize;

		// 粒子列表
		private List<LightParticle> lightParticles = new List<LightParticle>();
		private List<TechParticle> techParticles = new List<TechParticle>();

		// 画多边形粒子用的 BasicEffect，得手动管理矩阵才能跟游戏画面缩放匹配
		private BasicEffect basicEffect;
		private const int TechParticleMinLife = 30;
		private const int TechParticleMaxLife = 50;

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

			// 加载所有需要的贴图
			projectileTexture = ModContent.Request<Texture2D>("ArknightsMod/Content/Projectiles/Caster/Valarqvin/ValarqvinProj").Value;
			gradientTexture = ModContent.Request<Texture2D>("ArknightsMod/Content/Projectiles/Caster/Valarqvin/Effect/LightningGradient_2").Value;
			gradientTexture2 = ModContent.Request<Texture2D>("ArknightsMod/Content/Projectiles/Caster/Valarqvin/Effect/LightningGradient_3").Value;
			gradientTexture3 = ModContent.Request<Texture2D>("ArknightsMod/Content/Projectiles/Caster/Valarqvin/Effect/LightningGradient_4").Value;
			lightTexture = ModContent.Request<Texture2D>("ArknightsMod/Content/Projectiles/Caster/Valarqvin/Light").Value;

			if (projectileTexture != null && !projectileTexture.IsDisposed)
				textureSize = new Vector2(projectileTexture.Width, projectileTexture.Height);
		}

		public override void AI() {
			Projectile.rotation = Projectile.velocity.ToRotation();
			UpdateTrail();

			// 每 2 帧蹦出 2~4 个白色小光点
			if (Projectile.timeLeft % 2 == 0) {
				int count = Main.rand.Next(2, 5);
				for (int i = 0; i < count; i++) {
					LightParticle lp = new LightParticle();
					lp.Position = Projectile.Center + Main.rand.NextVector2Circular(6f, 6f);
					lp.Velocity = Main.rand.NextVector2Circular(1.5f, 1.5f);
					lp.MaxSize = Main.rand.NextFloat(0.6f, 1.2f);
					lp.Size = lp.MaxSize;
					lp.MaxLife = Main.rand.Next(20, 35);
					lp.Life = lp.MaxLife;
					lp.Color = Color.White;
					lightParticles.Add(lp);
				}
			}

			// 每 6 帧生成 1~2 个多边形粒子
			if (Projectile.timeLeft % 6 == 0) {
				int count = Main.rand.Next(1, 3);
				for (int i = 0; i < count; i++) {
					TechParticle tp = new TechParticle();
					tp.Position = Projectile.Center + Main.rand.NextVector2Circular(10f, 10f);
					tp.Velocity = Projectile.velocity * 0.2f + Main.rand.NextVector2Circular(1.5f, 1.5f);
					tp.MaxSize = Main.rand.NextFloat(8f, 14f);
					tp.Size = tp.MaxSize;
					tp.Sides = Main.rand.Next(3, 7);
					tp.Rotation = Main.rand.NextFloat(MathHelper.TwoPi);
					tp.RotationSpeed = Main.rand.NextFloat(-0.08f, 0.08f);
					tp.Color = LightColors[Main.rand.Next(LightColors.Length)];
					tp.MaxLife = Main.rand.Next(TechParticleMinLife, TechParticleMaxLife);
					tp.Life = tp.MaxLife;
					techParticles.Add(tp);
				}
			}

			// 更新所有光点粒子：移动、减速、缩小
			for (int i = lightParticles.Count - 1; i >= 0; i--) {
				LightParticle lp = lightParticles[i];
				lp.Life--;
				lp.Position += lp.Velocity;
				lp.Velocity *= 0.95f;
				float progress = 1f - (float)lp.Life / lp.MaxLife;
				lp.Size = lp.MaxSize / (1f + progress * 3f);
				lightParticles[i] = lp;
				if (!lp.Active)
					lightParticles.RemoveAt(i);
			}

			// 更新多边形粒子：移动、减速、旋转、边数减少、抖动
			for (int i = techParticles.Count - 1; i >= 0; i--) {
				TechParticle tp = techParticles[i];
				tp.Life--;
				tp.Position += tp.Velocity;
				tp.Velocity *= 0.96f;
				tp.Rotation += tp.RotationSpeed;

				float progress = 1f - (float)tp.Life / tp.MaxLife;
				tp.Size = tp.MaxSize / (1f + progress * 2f);

				if (tp.Life % 8 == 0 && tp.Sides > 3) {
					tp.Sides--;
					tp.Color = LightColors[Main.rand.Next(LightColors.Length)];
				}

				// 给粒子加一个微小的正弦抖动，让它看起来更生动
				tp.Position += new Vector2(
					(float)Math.Sin(tp.Life * 0.3f) * 0.4f,
					(float)Math.Cos(tp.Life * 0.5f) * 0.4f
				);

				techParticles[i] = tp;
				if (!tp.Active)
					techParticles.RemoveAt(i);
			}
		}

		private void UpdateTrail() {
			trailPositions.Enqueue(Projectile.Center);
			while (trailPositions.Count > TrailLength)
				trailPositions.Dequeue();
		}

		// 击中敌人时生成爆炸特效
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			SpawnHitEffect();
		}

		// 弹丸消失时（撞墙或时间到）也生成爆炸特效
		public override void OnKill(int timeLeft) {
			if (Projectile.owner != Main.myPlayer)
				return;

			SpawnHitEffect();
		}

		private void SpawnHitEffect() {
			Projectile.NewProjectile(
				Projectile.GetSource_FromThis(),
				Projectile.Center,
				Projectile.velocity * 0.5f,
				ModContent.ProjectileType<ValarqvinProj3_Hit>(),
				0, 0f, Projectile.owner);
		}

		// 不让原版画我们的弹丸，我们自己来
		public override bool PreDraw(ref Color lightColor) => false;

		public override void PostDraw(Color lightColor) {
			// 从最底层画到最上层：第三层拖尾 → 第二层 → 第一层 → 弹丸本体 → 粒子
			DrawTrail3();
			DrawTrail2();
			DrawTrail();
			DrawProjectile();
			DrawLightParticles();
			DrawTechParticles();
		}

		// 画那些飘散的小光点，用 Additive 混合让它们发光
		private void DrawLightParticles() {
			if (lightParticles.Count == 0 || lightTexture == null || lightTexture.IsDisposed)
				return;

			Main.spriteBatch.End();
			Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive,
				SamplerState.PointClamp, DepthStencilState.None,
				RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

			foreach (var lp in lightParticles) {
				float alpha = (float)lp.Life / lp.MaxLife;
				float size = lp.Size * 12f;
				Vector2 pos = lp.Position - Main.screenPosition;
				Color c = lp.Color * alpha;

				Rectangle rect = new Rectangle(
					(int)(pos.X - size * 0.5f),
					(int)(pos.Y - size * 0.5f),
					(int)size, (int)size);

				Main.spriteBatch.Draw(lightTexture, rect, null, c, 0f, Vector2.Zero, SpriteEffects.None, 0f);
			}

			Main.spriteBatch.End();
			Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
				Main.DefaultSamplerState, DepthStencilState.None,
				Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
		}

		// 画科多边形粒子，用 BasicEffect 手动提交三角形
		// 注意这里设置了 View 矩阵为 Main.GameViewMatrix.TransformationMatrix，
		// 这样才能在 UI 缩放或窗口大小改变时仍然对齐
		private void DrawTechParticles() {
			if (techParticles.Count == 0)
				return;

			Main.spriteBatch.End();

			GraphicsDevice gd = Main.graphics.GraphicsDevice;
			BlendState prevBlend = gd.BlendState;
			RasterizerState prevRaster = gd.RasterizerState;

			gd.BlendState = BlendState.Additive;
			gd.RasterizerState = RasterizerState.CullNone;

			if (basicEffect == null) {
				basicEffect = new BasicEffect(gd);
				basicEffect.VertexColorEnabled = true;
			}
			basicEffect.Projection = Matrix.CreateOrthographicOffCenter(
				0, Main.screenWidth, Main.screenHeight, 0, -1, 1);
			basicEffect.View = Main.GameViewMatrix.TransformationMatrix;
			basicEffect.World = Matrix.Identity;

			List<VertexPositionColor> allVertices = new List<VertexPositionColor>();

			foreach (var tp in techParticles) {
				float alpha = (float)tp.Life / tp.MaxLife;
				Color drawColor = tp.Color * alpha;
				Color centerColor = drawColor * 0.3f;

				Vector2 center = tp.Position - Main.screenPosition;
				float r = tp.Size;
				int sides = tp.Sides;
				if (sides < 3)
					sides = 3;

				int centerIndex = allVertices.Count;
				allVertices.Add(new VertexPositionColor(new Vector3(center, 0), centerColor));

				int firstOuter = allVertices.Count;
				for (int j = 0; j < sides; j++) {
					float angle = tp.Rotation + j * MathHelper.TwoPi / sides;
					Vector2 outer = center + new Vector2((float)Math.Cos(angle) * r, (float)Math.Sin(angle) * r);
					allVertices.Add(new VertexPositionColor(new Vector3(outer, 0), drawColor));
				}

				for (int j = 0; j < sides; j++) {
					int next = (j == sides - 1) ? firstOuter : firstOuter + j + 1;
					allVertices.Add(allVertices[centerIndex]);
					allVertices.Add(allVertices[firstOuter + j]);
					allVertices.Add(allVertices[next]);
				}
			}

			if (allVertices.Count >= 3) {
				foreach (EffectPass pass in basicEffect.CurrentTechnique.Passes) {
					pass.Apply();
					gd.DrawUserPrimitives(PrimitiveType.TriangleList,
						allVertices.ToArray(), 0, allVertices.Count / 3);
				}
			}

			gd.BlendState = prevBlend;
			gd.RasterizerState = prevRaster;

			Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
				Main.DefaultSamplerState, DepthStencilState.None,
				Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
		}

		// 画弹丸本体，用三角条带把贴图贴上去，Additive 混合让它发亮
		private void DrawProjectile() {
			if (projectileTexture == null || projectileTexture.IsDisposed)
				return;

			Main.spriteBatch.End();
			Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive,
				SamplerState.LinearClamp, DepthStencilState.None,
				RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

			DrawProjectileVertices(Main.graphics.GraphicsDevice, projectileTexture);

			Main.spriteBatch.End();
			Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
				Main.DefaultSamplerState, DepthStencilState.None,
				Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
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

			// 让弹丸稍微向后偏移一点，视觉中心对齐碰撞箱
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

		// 下面三个是三层拖尾的绘制，各自设置好混合模式后调用同一个核心方法
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

		private void DrawTrail3() {
			if (trailPositions.Count < 2 || gradientTexture3 == null || gradientTexture3.IsDisposed)
				return;
			Main.spriteBatch.End();
			Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearWrap, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
			DrawTrailVertices(Main.graphics.GraphicsDevice, gradientTexture3, Trail3WidthStart, Trail3WidthEnd, Trail3FlowSpeed, Trail3FadePower, Trail3ColorHead, Trail3ColorTail, Trail3Brightness, Trail3WaveAmplitude, Trail3WaveFrequency, false);
			Main.spriteBatch.End();
			Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
		}

		// 拖尾的核心绘制逻辑：根据历史位置拉一条三角条带，纹理沿条带流动，加上波动偏移
		private void DrawTrailVertices(GraphicsDevice gd, Texture2D texture, float widthStart, float widthEnd, float flowSpeed, float fadePower, Color colorHead, Color colorTail, float brightness, float waveAmplitude, float waveFrequency, bool useAlphaBlend) {
			Vector2[] points = trailPositions.ToArray();
			int count = points.Length;
			if (count < 2)
				return;

			// 计算每个点到起点的累积距离，用来做纹理 V 坐标和波动相位
			float[] cumLength = new float[count];
			float totalLength = 0f;
			for (int i = 1; i < count; i++) {
				cumLength[i] = cumLength[i - 1] + Vector2.Distance(points[i], points[i - 1]);
				totalLength = cumLength[i];
			}

			// 纹理流动的偏移，随时间变化
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
				float alpha = useAlphaBlend
					? brightness * (float)Math.Pow(t, fadePower)
					: (float)Math.Pow(t, fadePower) * brightness;
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

			if (vertices.Count >= 4) {
				gd.Textures[0] = texture;
				gd.DrawUserPrimitives(PrimitiveType.TriangleStrip, vertices.ToArray(), 0, vertices.Count - 2);
			}
		}

		// 波动强度曲线：尾部不动 → 逐渐增强 → 中间最强 → 头部衰减到零
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

		// 计算波动的实际偏移量，用正弦波让拖尾左右摆动
		private float CalculateWaveOffset(float distanceFromTail, float totalLength, float frequency, float amplitude) {
			float timeFactor = 0.155f;
			float phase = distanceFromTail * frequency * 0.1f + (float)Main.timeForVisualEffects * timeFactor;
			return (float)Math.Sin(phase) * amplitude;
		}
	}
}
