using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using ArknightsMod.Content.Projectiles.Supporter.Tragodia;
using ArknightsMod.Content.Projectiles.Supporter.Tragodia.SP2;
using ArknightsMod.Content.Projectiles.Supporter.Tragodia.SP1;
using ArknightsMod.Content.Projectiles.Supporter.Tragodia.EffectImage;
using ArknightsMod.Content.Projectiles.Supporter.Tragodia.NormalAttack;
namespace ArknightsMod.Content.Projectiles.Supporter.Tragodia.SP1
{
	public class SP1Extract : ModProjectile
	{

		private const int FLIGHT_FRAMES = 18;
		private const int TRACK_FRAMES = 20;
		private const float FLIGHT_SPEED_MULT = 1.05f;

		//状态 
		private bool IsTracking => Projectile.localAI[0] > 0;
		private bool HasTarget => _target != null && _target.active;

		private NPC _target;
		private Vector2 _startPos;
		private Vector2 _controlPoint;
		private float _progress;
		private int _damage;
		public bool isMain = false;

	
		private Queue<Vector2> trailPositions = new Queue<Vector2>();
		private const int TrailLength = 10;

		// 第一
		private const float TrailWidthStart = 8f;
		private const float TrailWidthEnd = 5f;
		private static readonly Color TrailColorHead = new Color(140, 30, 220);
		private static readonly Color TrailColorTail = new Color(170, 80, 255);
		private const float TrailFlowSpeed = 2f;
		private const float TrailFadePower = 1.2f;
		private const float TrailBrightness = 1f;
		private const float TrailWaveAmplitude = 6f;
		private const float TrailWaveFrequency = 0.8f;

		// 第二
		private const float Trail2WidthStart = 18f;
		private const float Trail2WidthEnd = 7f;
		private static readonly Color Trail2ColorHead = new Color(220, 150, 255);
		private static readonly Color Trail2ColorTail = new Color(120, 40, 230);
		private const float Trail2FlowSpeed = 4f;
		private const float Trail2FadePower = 2f;
		private const float Trail2Brightness = 1.7f;
		private const float Trail2WaveAmplitude = 8f;
		private const float Trail2WaveFrequency = 0.6f;

		// 第三
		private const float Trail3WidthStart = 16f;
		private const float Trail3WidthEnd = 12f;
		private static readonly Color Trail3ColorHead = new Color(30, 10, 60);
		private static readonly Color Trail3ColorTail = new Color(15, 5, 35);
		private const float Trail3FlowSpeed = 2f;
		private const float Trail3FadePower = 1.2f;
		private const float Trail3Brightness = 13f;
		private const float Trail3WaveAmplitude = 14f;
		private const float Trail3WaveFrequency = 0.5f;

		// 纹理
		private Texture2D projectileTexture;
		private Texture2D gradientTexture1;
		private Texture2D gradientTexture2;
		private Texture2D gradientTexture3;

		// 顶点结构（我找不到mod里的，干脆注册一个）
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

		public override void SetDefaults() {
			Projectile.width = 8;
			Projectile.height = 12;
			Projectile.friendly = false;
			Projectile.hostile = false;
			Projectile.penetrate = -1;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
			Projectile.timeLeft = 300;
			Projectile.alpha = 0;
			Projectile.scale = 0.8f;

			TryLoadTexture(ref gradientTexture1, "ArknightsMod/Content/Projectiles/Caster/Valarqvin/Effect/LightningGradient_2");
			TryLoadTexture(ref gradientTexture2, "ArknightsMod/Content/Projectiles/Caster/Valarqvin/Effect/LightningGradient_3");
			TryLoadTexture(ref gradientTexture3, "ArknightsMod/Content/Projectiles/Caster/Valarqvin/Effect/LightningGradient_4");
		}

		private void TryLoadTexture(ref Texture2D texture, string path) {
			if (ModContent.HasAsset(path))
				texture = ModContent.Request<Texture2D>(path).Value;
			
		}

		public override void AI() {

			trailPositions.Enqueue(Projectile.Center);
			while (trailPositions.Count > TrailLength)
				trailPositions.Dequeue();

			if (!IsTracking)
				DoFlight();
			else
				DoTrack();

			Lighting.AddLight(Projectile.Center, new Vector3(0.5f, 0, 1) * 0.4f);
		}

		private void DoFlight() {
			Projectile.localAI[1]++;
			if (Projectile.velocity.LengthSquared() > 0.01f) {
				Projectile.velocity *= FLIGHT_SPEED_MULT;
				Projectile.rotation = Projectile.velocity.ToRotation() - MathHelper.PiOver2;
			}
			float pulse = 0.7f + 0.3f * (float)Math.Sin(Projectile.localAI[1] * 0.2f);
			Projectile.scale = pulse;

			if (Projectile.localAI[1] >= FLIGHT_FRAMES)
				EnterTracking();
		}

		private void EnterTracking() {
			if (_target == null || !_target.active)
				_target = FindTarget();
			if (_target == null) {
				Projectile.Kill();
				return;
			}

			_startPos = Projectile.Center;
			Vector2 endPos = _target.Center;
			Vector2 mid = (_startPos + endPos) / 2f;
			Vector2 dir = endPos - _startPos;
			Vector2 perp = new Vector2(-dir.Y, dir.X);
			perp.Normalize();
			float arc = MathHelper.Clamp(dir.Length() * 0.3f, 30f, 120f);
			float side = (Projectile.identity % 2 == 0) ? 1f : -1f;
			_controlPoint = mid + perp * arc * side;

			_progress = 0f;
			Projectile.localAI[0] = 1f;
			Projectile.velocity = Vector2.Zero;
		}

		private void DoTrack() {
			if (!HasTarget) {
				Projectile.Kill();
				return;
			}

			_progress += 1f / TRACK_FRAMES;
			float rawT = MathHelper.Clamp(_progress, 0f, 1f);
			float t = EaseInOutQuad(rawT);

			Vector2 endPos = _target.Center;
			float u = 1f - t;
			Vector2 pos = u * u * _startPos + 2f * u * t * _controlPoint + t * t * endPos;
			Projectile.Center = pos;

			float nextRawT = MathHelper.Clamp(rawT + 0.02f, 0f, 1f);
			float nextT = EaseInOutQuad(nextRawT);
			float nextU = 1f - nextT;
			Vector2 nextPos = nextU * nextU * _startPos + 2f * nextU * nextT * _controlPoint + nextT * nextT * endPos;
			Vector2 lookDir = nextPos - pos;
			if (lookDir.LengthSquared() > 0.01f)
				Projectile.rotation = lookDir.ToRotation() - MathHelper.PiOver2;

			Projectile.scale = 1f;

			if (rawT >= 1f) {
				if (isMain)
					SpawnWaveSet();
				Projectile.Kill();
			}
		}

		private float EaseInOutQuad(float t) {
			return t < 0.5f ? 2f * t * t : 1f - (float)Math.Pow(-2f * t + 2f, 2f) / 2f;
		}

		private NPC FindTarget() {
			Player player = Main.player[Projectile.owner];
			if (player == null)
				return null;
			NPC closest = null;
			float minDist = float.MaxValue;
			Vector2 mouse = Main.MouseWorld;
			foreach (NPC npc in Main.ActiveNPCs) {
				if (!npc.CanBeChasedBy(player) || npc.friendly)
					continue;
				float dist = Vector2.DistanceSquared(npc.Center, mouse);
				if (dist < minDist) {
					minDist = dist;
					closest = npc;
				}
			}
			return closest;
		}

		private void SpawnWaveSet() {
			Player player = Main.player[Projectile.owner];
			if (player == null || _target == null || !_target.active)
				return;

	
			SoundEngine.PlaySound(new SoundStyle("ArknightsMod/Assets/Sound/Tragodia/SP1_Hit") {
				Volume = 1.5f,
				MaxInstances = 5,
			}, _target.Center);

			Vector2 spawnPos = _target.Center + new Vector2(0, 20f);
			var source = Projectile.GetSource_FromThis();

			int normalType = ModContent.ProjectileType<TragodiaNormalAttack>();
			Projectile.NewProjectile(source, spawnPos, Vector2.Zero,
				normalType, _damage, Projectile.knockBack, player.whoAmI);

			int waveType = ModContent.ProjectileType<WaveProjectile>();
			Projectile.NewProjectile(source, spawnPos, Vector2.Zero,
				waveType, 0, 0, player.whoAmI);

			int lightType = ModContent.ProjectileType<Light>();
			Projectile.NewProjectile(source, spawnPos, Vector2.Zero,
				lightType, 0, 0, player.whoAmI);

			int ribbonType = ModContent.ProjectileType<RibbonProjectile>();
			Projectile.NewProjectile(source, spawnPos, Vector2.Zero,
				ribbonType, 0, 0, player.whoAmI);
		}


		public static void SpawnPair(Player player, NPC target, int damage) {
			if (player == null || target == null || !target.active)
				return;


			Vector2 spawnPos = target.Center;

			int dustType = ModContent.ProjectileType<Pre_Attack>();
			Projectile dust = Main.projectile[Projectile.NewProjectile(
				player.GetSource_FromThis(),
				target.Center,
				Vector2.Zero,
				dustType, 0, 0, player.whoAmI)];
			if (dust.ModProjectile is Pre_Attack windDust) {
				windDust.targetNPCIndex = target.whoAmI;
				windDust.offsetY = 0f;
			}

			// 随机方向
			float baseAngle = (target.Center - player.Center).ToRotation();
			float angle1 = baseAngle + Main.rand.NextFloat(-1.2f, 1.2f);
			float angle2 = angle1 + Main.rand.NextFloat(2.0f, 3.5f);
			float initialSpeed = 3f;

			for (int i = 0; i < 2; i++) {
				float angle = (i == 0) ? angle1 : angle2;
				Vector2 vel = angle.ToRotationVector2() * initialSpeed;

				int idx = Projectile.NewProjectile(
					player.GetSource_FromThis(),
					spawnPos,
					vel,
					ModContent.ProjectileType<SP1Extract>(),
					0, 0f, player.whoAmI);

				if (idx >= 0 && idx < Main.maxProjectiles) {
					Projectile p = Main.projectile[idx];
					if (p.ModProjectile is SP1Extract extract) {
						extract._target = target;
						extract._damage = damage;
						extract.isMain = (i == 0);
						// 初始化拖尾队列，避免空引用
						for (int j = 0; j < TrailLength; j++)
							extract.trailPositions.Enqueue(p.Center);
					}
					p.timeLeft = 300;
				}
			}
		}


		public override bool PreDraw(ref Color lightColor) => false;

		public override void PostDraw(Color lightColor) {
			DrawTrail3();
			DrawTrail2();
			DrawTrail1();
			DrawProjectile();
		}
		//三层拖尾，吓哭了
		private void DrawTrail1() {
			if (trailPositions.Count < 2 || gradientTexture1 == null)
				return;
			Main.spriteBatch.End();
			Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearWrap, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
			DrawTrailVertices(gradientTexture1, TrailWidthStart, TrailWidthEnd, TrailFlowSpeed, TrailFadePower, TrailColorHead, TrailColorTail, TrailBrightness, TrailWaveAmplitude, TrailWaveFrequency);
			Main.spriteBatch.End();
			Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
		}

		private void DrawTrail2() {
			if (trailPositions.Count < 2 || gradientTexture2 == null)
				return;
			Main.spriteBatch.End();
			Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.LinearWrap, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
			DrawTrailVertices(gradientTexture2, Trail2WidthStart, Trail2WidthEnd, Trail2FlowSpeed, Trail2FadePower, Trail2ColorHead, Trail2ColorTail, Trail2Brightness, Trail2WaveAmplitude, Trail2WaveFrequency);
			Main.spriteBatch.End();
			Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
		}

		private void DrawTrail3() {
			if (trailPositions.Count < 2 || gradientTexture3 == null)
				return;
			Main.spriteBatch.End();
			Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearWrap, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
			DrawTrailVertices(gradientTexture3, Trail3WidthStart, Trail3WidthEnd, Trail3FlowSpeed, Trail3FadePower, Trail3ColorHead, Trail3ColorTail, Trail3Brightness, Trail3WaveAmplitude, Trail3WaveFrequency);
			Main.spriteBatch.End();
			Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
		}

		private void DrawTrailVertices(Texture2D texture, float widthStart, float widthEnd, float flowSpeed, float fadePower, Color colorHead, Color colorTail, float brightness, float waveAmplitude, float waveFrequency) {
			Vector2[] points = trailPositions.ToArray();
			int count = points.Length;
			if (count < 2)
				return;

			float[] cumLength = new float[count];
			float totalLength = 0f;
			for (int i = 1; i < count; i++) {
				cumLength[i] = cumLength[i - 1] + Vector2.Distance(points[i], points[i - 1]);
				totalLength = cumLength[i];
			}

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

				Color gradientColor = Color.Lerp(colorTail, colorHead, t);
				float alpha = (float)Math.Pow(t, fadePower) * brightness;
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
				Main.graphics.GraphicsDevice.Textures[0] = texture;
				Main.graphics.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleStrip, vertices.ToArray(), 0, vertices.Count - 2);
			}
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

		private void DrawProjectile() {
			if (projectileTexture == null)
				return;
			Main.spriteBatch.End();
			Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
			DrawProjectileVertices(projectileTexture);
			Main.spriteBatch.End();
			Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
		}

		private void DrawProjectileVertices(Texture2D texture) {
			float alphaFactor = (float)Projectile.timeLeft / 300f;
			Color drawColor = TrailColorHead * alphaFactor * 1.5f;
			Vector2 textureSize = texture.Size();
			float textureAspect = textureSize.X / textureSize.Y;
			float baseSize = Projectile.width * Projectile.scale * 1.2f;
			float width = baseSize * textureAspect;
			float height = baseSize;

			Vector2 center = Projectile.Center - Main.screenPosition;
			float rot = Projectile.rotation + MathHelper.Pi; // 尖端向后
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

			Main.graphics.GraphicsDevice.Textures[0] = texture;
			Main.graphics.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleStrip, vertices, 0, 2);
		}
	}
}