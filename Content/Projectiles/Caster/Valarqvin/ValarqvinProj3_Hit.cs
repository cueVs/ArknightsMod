using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Projectiles.Caster.Valarqvin
{
	public class ValarqvinProj3_Hit : ModProjectile
	{
		private const int TotalFrames = 25;
		private const int PeakFrame = 12;
		private const int ShrinkEndFrame = 16;

		private const float MainStartWidth = 36f;
		private const float MainStartHeight = 5f;
		private const float MainPeakWidth = 110f;
		private const float MainPeakHeight = 10f;
		private const float MainShrinkWidth = 75f;
		private const float MainShrinkHeight = 6f;
		private const float MainEndWidth = 12f;
		private const float MainEndHeight = 2f;

		private const float OuterScaleStart = 1.8f;
		private const float OuterScalePeak = 1.6f;
		private const float OuterScaleShrink = 1.4f;
		private const float OuterScaleEnd = 1.1f;

		private static readonly Color MainColor = new Color(150, 180, 255);
		private static readonly Color OuterColor = new Color(68, 90, 172);

		private const float LargerCrossScale = 1.8f;
		private const float LargerCrossAlpha = 0.25f;

		private const int CoreGlowTotalFrames = 20;
		private const int CoreGlowPeakFrame = 7;
		private const float CoreGlowStartSize = 12f;
		private const float CoreGlowPeakSize = 45f;
		private const float CoreGlowEndSize = 4f;
		private const float CoreGlowOuterScale = 2.0f;
		private static readonly Color CoreGlowMainColor = new Color(200, 230, 255);
		private static readonly Color CoreGlowOuterColor = new Color(80, 110, 200);

		private const int MidGlowTotalFrames = 20;
		private const int MidGlowPeakFrame = 8;
		private const float MidGlowStartSize = 20f;
		private const float MidGlowPeakSize = 75f;
		private const float MidGlowEndSize = 8f;
		private const float MidGlowOuterScale = 1.8f;
		private static readonly Color MidGlowMainColor = new Color(140, 180, 240);
		private static readonly Color MidGlowOuterColor = new Color(60, 90, 170);

		private const int BgGlowTotalFrames = 16;
		private const float BgGlowStartSize = 35f;
		private const float BgGlowPeakSize = 130f;
		private const float BgGlowEndSize = 15f;
		private const float BgGlowOuterScale = 1.5f;
		private static readonly Color BgGlowMainColor = new Color(100, 140, 200);
		private static readonly Color BgGlowOuterColor = new Color(40, 60, 120);

		private const int DarkCrossStartFrame = 3;
		private const int DarkCrossDuration = 14;
		private const float DarkCrossStartWidth = 75f;
		private const float DarkCrossStartHeight = 10f;
		private const float DarkCrossEndWidth = 0f;
		private const float DarkCrossEndHeight = 0f;
		private const float DarkCrossAlpha = 0.55f;
		private static readonly Color DarkCrossColor = Color.Black;

		private const int LightParticleCount = 12;
		private const int HitParticleCount = 15;
		private const int PolyParticleCount = 18;
		private const int PolyParticleMinLife = 30;
		private const int PolyParticleMaxLife = 50;

		private const int RandomParticleCount = 15;
		private const int RandomParticleMinLife = 18;
		private const int RandomParticleMaxLife = 35;

		private const int ExplosionSpikeCount = 45;
		private const float ExplosionMaxRadius = 50f;
		private const float ExplosionSpikeMinLength = 0.25f;
		private const float ExplosionSpikeMaxLength = 1.0f;
		private const float ExplosionDirectionWeight = 0.2f;

		private const int MaskStartFrame = 3;
		private const float MaskExpandSpeed = 2.5f;
		private const float MaskFeatherWidth = 0.6f;

		private static readonly Color ExplosionFillColor = new Color(12, 20, 40);
		private static readonly Color ExplosionEdgeColor = new Color(30, 45, 80);
		private const float ExplosionAlpha = 0.65f;

		private static readonly Color[] PolyColors = new Color[]
		{
			new Color(0, 200, 255),
			new Color(0, 150, 255),
			new Color(30, 80, 220),
			new Color(0, 100, 200),
			new Color(20, 60, 180),
			new Color(0, 180, 230),
		};

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

		private struct RandomParticle
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

		private struct ExplosionData
		{
			public Vector2[] SpikeEnds;
			public float[] SpikeLengths;
		}

		private Texture2D _lightTexture;
		private Texture2D _crossTexture;
		private Vector2 _spawnPosition;
		private List<LightParticle> lightParticles = new List<LightParticle>();
		private List<PolyParticle> polyParticles = new List<PolyParticle>();
		private List<RandomParticle> randomParticles = new List<RandomParticle>();
		private ExplosionData _explosionData;
		private BasicEffect _basicEffect;
		private bool spawnedParticles = false;

		public override void SetDefaults() {
			Projectile.width = 2;
			Projectile.height = 2;
			Projectile.friendly = false;
			Projectile.hostile = false;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
			Projectile.timeLeft = TotalFrames;
			Projectile.penetrate = -1;
			Projectile.aiStyle = -1;
		}

		public override void AI() {
			if (!spawnedParticles) {
				spawnedParticles = true;
				_spawnPosition = Projectile.Center;
				InitializeExplosion();
				SpawnParticles();
			}

			for (int i = lightParticles.Count - 1; i >= 0; i--) {
				LightParticle lp = lightParticles[i];
				lp.Life--;
				lp.Position += lp.Velocity;
				lp.Velocity *= 0.94f;
				float p = 1f - (float)lp.Life / lp.MaxLife;
				lp.Size = lp.MaxSize / (1f + p * 3f);
				lightParticles[i] = lp;
				if (!lp.Active)
					lightParticles.RemoveAt(i);
			}

			for (int i = polyParticles.Count - 1; i >= 0; i--) {
				PolyParticle pp = polyParticles[i];
				pp.Life--;
				pp.Position += pp.Velocity;
				pp.Velocity *= 0.96f;
				pp.Rotation += pp.RotationSpeed;
				float p = 1f - (float)pp.Life / pp.MaxLife;
				pp.Size = pp.MaxSize / (1f + p * 2f);
				if (pp.Life % 8 == 0 && pp.Sides > 3) {
					pp.Sides--;
					pp.Color = PolyColors[Main.rand.Next(PolyColors.Length)];
				}
				pp.Position += new Vector2((float)Math.Sin(pp.Life * 0.3f) * 0.4f, (float)Math.Cos(pp.Life * 0.5f) * 0.4f);
				polyParticles[i] = pp;
				if (!pp.Active)
					polyParticles.RemoveAt(i);
			}

			for (int i = randomParticles.Count - 1; i >= 0; i--) {
				RandomParticle rp = randomParticles[i];
				rp.Life--;
				rp.Position += rp.Velocity;
				rp.Velocity *= 0.92f;
				float p = 1f - (float)rp.Life / rp.MaxLife;
				rp.Size = rp.MaxSize * (1f - p);
				randomParticles[i] = rp;
				if (!rp.Active)
					randomParticles.RemoveAt(i);
			}
		}

		private void InitializeExplosion() {
			_explosionData = new ExplosionData();
			_explosionData.SpikeEnds = new Vector2[ExplosionSpikeCount];
			_explosionData.SpikeLengths = new float[ExplosionSpikeCount];

			float step = MathHelper.TwoPi / ExplosionSpikeCount;
			Vector2 dir = Projectile.velocity.SafeNormalize(Vector2.UnitX);
			float baseAngle = dir.ToRotation();

			for (int i = 0; i < ExplosionSpikeCount; i++) {
				float angle = i * step;
				float noise = MathF.Sin(i * 1.7f + 0.5f) * 0.35f
							+ MathF.Sin(i * 4.1f + 2.3f) * 0.25f
							+ MathF.Cos(i * 2.5f + 1.1f) * 0.2f
							+ MathF.Sin(i * 6.7f + 3.8f) * 0.15f;
				if (Main.rand.NextFloat() < 0.3f)
					noise += Main.rand.NextFloat(-0.3f, 0.4f);
				float height = MathHelper.Clamp(0.5f + noise, 0.1f, 1.0f);
				float ratio = MathHelper.Lerp(ExplosionSpikeMinLength, ExplosionSpikeMaxLength, height);
				float diff = angle - baseAngle;
				float dirFactor = 1f + ExplosionDirectionWeight * MathF.Cos(diff);
				ratio *= MathHelper.Clamp(dirFactor, 0.6f, 1.4f);
				_explosionData.SpikeLengths[i] = ratio;
				float r = ExplosionMaxRadius * ratio;
				_explosionData.SpikeEnds[i] = new Vector2(MathF.Cos(angle) * r, MathF.Sin(angle) * r);
			}
		}

		private void SpawnParticles() {
			Vector2 hitPos = _spawnPosition;
			Vector2 dir = Projectile.velocity.SafeNormalize(Vector2.UnitX);
			Vector2 backDir = -dir;
			float baseSpeed = Projectile.velocity.Length();

			for (int i = 0; i < HitParticleCount; i++) {
				float angle = dir.ToRotation() + Main.rand.NextFloat(-1.2f, 1.2f);
				float speed = baseSpeed * Main.rand.NextFloat(0.4f, 0.8f);
				Vector2 vel = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * speed;
				vel += Main.rand.NextVector2Circular(2f, 2f);
				int life = Main.rand.Next(35, 55);
				float size = Main.rand.NextFloat(14f, 24f);
				Color col = new Color(68, 90, 172).MultiplyRGB(Color.White * Main.rand.NextFloat(0.8f, 1f));

				Projectile p = Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), hitPos, vel,
					ModContent.ProjectileType<ValarqvinHitParticle>(), 0, 0f, Projectile.owner);
				if (p.ModProjectile is ValarqvinHitParticle particle)
					particle.Initialize(life, size, 2f, vel, hitPos, col);

				if (i % 3 == 0) {
					Vector2 backVel = backDir * speed * 0.7f + Main.rand.NextVector2Circular(1.5f, 1.5f);
					Projectile bp = Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), hitPos, backVel,
						ModContent.ProjectileType<ValarqvinHitParticle>(), 0, 0f, Projectile.owner);
					if (bp.ModProjectile is ValarqvinHitParticle bpParticle)
						bpParticle.Initialize(life, size * 0.9f, 2f, backVel, hitPos, col);
				}
			}

			for (int i = 0; i < LightParticleCount; i++) {
				LightParticle lp = new LightParticle();
				lp.Position = hitPos + Main.rand.NextVector2Circular(8f, 8f);
				lp.Velocity = Main.rand.NextVector2Circular(1f, 2f) + dir * Main.rand.NextFloat(0.5f, 1.5f);
				lp.MaxSize = Main.rand.NextFloat(1f, 2f);
				lp.Size = lp.MaxSize;
				lp.MaxLife = Main.rand.Next(30, 50);
				lp.Life = lp.MaxLife;
				lp.Color = Color.White;
				lightParticles.Add(lp);

				if (i % 2 == 0) {
					LightParticle blp = new LightParticle();
					blp.Position = hitPos + Main.rand.NextVector2Circular(6f, 6f);
					blp.Velocity = Main.rand.NextVector2Circular(0.5f, 1.5f) + backDir * Main.rand.NextFloat(0.5f, 1.5f);
					blp.MaxSize = Main.rand.NextFloat(0.8f, 1.8f);
					blp.Size = blp.MaxSize;
					blp.MaxLife = Main.rand.Next(25, 40);
					blp.Life = blp.MaxLife;
					blp.Color = Color.White;
					lightParticles.Add(blp);
				}
			}

			for (int i = 0; i < PolyParticleCount; i++) {
				PolyParticle pp = new PolyParticle();
				pp.Position = hitPos + Main.rand.NextVector2Circular(6f, 6f);
				pp.Velocity = Main.rand.NextVector2Circular(2f, 3.5f) + dir * Main.rand.NextFloat(1f, 2.5f);
				pp.MaxSize = Main.rand.NextFloat(10f, 18f);
				pp.Size = pp.MaxSize;
				pp.Sides = Main.rand.Next(3, 7);
				pp.Rotation = Main.rand.NextFloat(MathHelper.TwoPi);
				pp.RotationSpeed = Main.rand.NextFloat(-0.12f, 0.12f);
				pp.Color = PolyColors[Main.rand.Next(PolyColors.Length)];
				pp.MaxLife = Main.rand.Next(PolyParticleMinLife, PolyParticleMaxLife);
				pp.Life = pp.MaxLife;
				polyParticles.Add(pp);

				if (i % 3 == 0) {
					PolyParticle bpp = new PolyParticle();
					bpp.Position = hitPos + Main.rand.NextVector2Circular(4f, 4f);
					bpp.Velocity = Main.rand.NextVector2Circular(1.5f, 3f) + backDir * Main.rand.NextFloat(1f, 2f);
					bpp.MaxSize = Main.rand.NextFloat(8f, 14f);
					bpp.Size = bpp.MaxSize;
					bpp.Sides = Main.rand.Next(3, 7);
					bpp.Rotation = Main.rand.NextFloat(MathHelper.TwoPi);
					bpp.RotationSpeed = Main.rand.NextFloat(-0.1f, 0.1f);
					bpp.Color = PolyColors[Main.rand.Next(PolyColors.Length)];
					bpp.MaxLife = Main.rand.Next(PolyParticleMinLife, PolyParticleMaxLife);
					bpp.Life = bpp.MaxLife;
					polyParticles.Add(bpp);
				}
			}

			for (int i = 0; i < RandomParticleCount; i++) {
				RandomParticle rp = new RandomParticle();
				rp.Position = hitPos + Main.rand.NextVector2Circular(5f, 5f);
				rp.Velocity = Main.rand.NextVector2Circular(2.5f, 4f);
				rp.MaxSize = Main.rand.NextFloat(0.7f, 1.5f);
				rp.Size = rp.MaxSize;
				rp.MaxLife = Main.rand.Next(RandomParticleMinLife, RandomParticleMaxLife);
				rp.Life = rp.MaxLife;
				rp.Color = PolyColors[Main.rand.Next(PolyColors.Length)];
				randomParticles.Add(rp);

				if (i % 3 == 0) {
					RandomParticle brp = new RandomParticle();
					brp.Position = hitPos + Main.rand.NextVector2Circular(4f, 4f);
					brp.Velocity = Main.rand.NextVector2Circular(1.5f, 3f) + backDir * Main.rand.NextFloat(0.5f, 1.5f);
					brp.MaxSize = Main.rand.NextFloat(0.5f, 1.2f);
					brp.Size = brp.MaxSize;
					brp.MaxLife = Main.rand.Next(RandomParticleMinLife, RandomParticleMaxLife);
					brp.Life = brp.MaxLife;
					brp.Color = PolyColors[Main.rand.Next(PolyColors.Length)];
					randomParticles.Add(brp);
				}
			}
		}

		public override bool PreDraw(ref Color lightColor) {
			if (_lightTexture == null)
				_lightTexture = ModContent.Request<Texture2D>("ArknightsMod/Content/Projectiles/Caster/Valarqvin/Light").Value;
			if (_crossTexture == null)
				_crossTexture = ModContent.Request<Texture2D>("ArknightsMod/Content/Projectiles/Rogue/Dedication/Light_horizontal").Value;
			if (_lightTexture == null || _crossTexture == null)
				return false;

			int frame = TotalFrames - Projectile.timeLeft;
			if (frame < 0 || frame >= TotalFrames)
				return false;

			DrawExplosion(frame);
			DrawBgGlow(frame);
			DrawMidGlow(frame);
			DrawCoreGlow(frame);
			DrawLightParticles();
			DrawPolyParticles();
			DrawCrossEffect(frame);
			DrawRandomParticles();
			DrawDarkCross(frame);
			return false;
		}

		private void DrawExplosion(int frame) {
			if (_explosionData.SpikeEnds == null)
				return;
			Main.spriteBatch.End();
			GraphicsDevice gd = Main.graphics.GraphicsDevice;
			BlendState prevBlend = gd.BlendState;
			RasterizerState prevRaster = gd.RasterizerState;
			gd.BlendState = BlendState.AlphaBlend;
			gd.RasterizerState = RasterizerState.CullNone;

			if (_basicEffect == null) { _basicEffect = new BasicEffect(gd); _basicEffect.VertexColorEnabled = true; }
			_basicEffect.Projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);
			_basicEffect.View = Main.GameViewMatrix.TransformationMatrix;
			_basicEffect.World = Matrix.Identity;

			float progress = (float)frame / TotalFrames;
			float globalAlpha = ExplosionAlpha * (1f - progress * progress);
			float scaleT = MathHelper.Clamp(progress / 0.35f, 0f, 1f);
			float scale = 1f - (1f - scaleT) * (1f - scaleT);

			Vector2 screenCenter = _spawnPosition - Main.screenPosition;
			float maskRadius = 0f;
			if (frame >= MaskStartFrame)
				maskRadius = (frame - MaskStartFrame) * MaskExpandSpeed;
			float featherOuter = maskRadius;
			float featherInner = maskRadius * (1f - MaskFeatherWidth);

			Vector2[] screenVerts = new Vector2[ExplosionSpikeCount];
			float[] dists = new float[ExplosionSpikeCount];
			for (int i = 0; i < ExplosionSpikeCount; i++) {
				screenVerts[i] = screenCenter + _explosionData.SpikeEnds[i] * scale;
				dists[i] = Vector2.Distance(screenVerts[i], screenCenter);
			}

			List<VertexPositionColor> triangles = new List<VertexPositionColor>();
			for (int i = 0; i < ExplosionSpikeCount; i++) {
				int next = (i + 1) % ExplosionSpikeCount;
				float a1 = GetMaskAlpha(dists[i], featherOuter, featherInner, globalAlpha);
				float a2 = GetMaskAlpha(dists[next], featherOuter, featherInner, globalAlpha);
				float aC = GetMaskAlpha(0f, featherOuter, featherInner, globalAlpha);
				if (aC < 0.01f && a1 < 0.01f && a2 < 0.01f)
					continue;
				triangles.Add(new VertexPositionColor(new Vector3(screenCenter, 0), ExplosionFillColor * aC));
				triangles.Add(new VertexPositionColor(new Vector3(screenVerts[i], 0), ExplosionEdgeColor * a1));
				triangles.Add(new VertexPositionColor(new Vector3(screenVerts[next], 0), ExplosionEdgeColor * a2));
			}

			if (triangles.Count >= 3)
				foreach (EffectPass pass in _basicEffect.CurrentTechnique.Passes) { pass.Apply(); gd.DrawUserPrimitives(PrimitiveType.TriangleList, triangles.ToArray(), 0, triangles.Count / 3); }

			gd.BlendState = prevBlend;
			gd.RasterizerState = prevRaster;
			Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
		}

		private float GetMaskAlpha(float dist, float maskOuter, float maskInner, float baseAlpha) {
			if (maskOuter <= 0f)
				return baseAlpha;
			if (dist <= maskInner)
				return 0f;
			if (dist >= maskOuter)
				return baseAlpha;
			float t = (dist - maskInner) / (maskOuter - maskInner);
			return baseAlpha * t;
		}

		private void DrawCoreGlow(int frame) { DrawGlow(frame, CoreGlowTotalFrames, CoreGlowPeakFrame, CoreGlowStartSize, CoreGlowPeakSize, CoreGlowEndSize, CoreGlowOuterScale, CoreGlowMainColor, CoreGlowOuterColor, 1f); }
		private void DrawMidGlow(int frame) { DrawGlow(frame, MidGlowTotalFrames, MidGlowPeakFrame, MidGlowStartSize, MidGlowPeakSize, MidGlowEndSize, MidGlowOuterScale, MidGlowMainColor, MidGlowOuterColor, 0.9f); }

		private void DrawBgGlow(int frame) {
			float progress = (float)frame / BgGlowTotalFrames;
			if (progress >= 1f)
				return;
			float alpha = 1f - progress * progress * progress;
			DrawGlow(frame, BgGlowTotalFrames, MidGlowPeakFrame, BgGlowStartSize, BgGlowPeakSize, BgGlowEndSize, BgGlowOuterScale, BgGlowMainColor, BgGlowOuterColor, alpha);
		}

		private void DrawGlow(int frame, int totalFrames, int peakFrame, float startSize, float peakSize, float endSize, float outerScale, Color mainColor, Color outerColor, float baseAlpha) {
			if (frame >= totalFrames)
				return;
			if (_lightTexture == null || _lightTexture.IsDisposed)
				return;

			float progress = (float)frame / totalFrames;
			float size;
			if (frame <= peakFrame) { float t = frame / (float)peakFrame; float e = 1f - (1f - t) * (1f - t); size = startSize + (peakSize - startSize) * e; }
			else { float t = (frame - peakFrame) / (float)(totalFrames - 1 - peakFrame); float e = t * t; size = peakSize - (peakSize - endSize) * e; }

			float alpha = baseAlpha * (1f - progress * progress);
			Vector2 sp = _spawnPosition - Main.screenPosition;

			Main.spriteBatch.End();
			Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
			Rectangle mr = new Rectangle((int)(sp.X - size * 0.5f), (int)(sp.Y - size * 0.5f), (int)size, (int)size);
			Main.spriteBatch.Draw(_lightTexture, mr, null, mainColor * alpha);
			float os = size * outerScale;
			Rectangle or = new Rectangle((int)(sp.X - os * 0.5f), (int)(sp.Y - os * 0.5f), (int)os, (int)os);
			Main.spriteBatch.Draw(_lightTexture, or, null, outerColor * alpha * 0.7f);
			Main.spriteBatch.End();
			Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
		}

		private void DrawLightParticles() {
			if (lightParticles.Count == 0)
				return;
			Main.spriteBatch.End();
			Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
			foreach (var lp in lightParticles) {
				float alpha = (float)lp.Life / lp.MaxLife;
				float size = lp.Size * 14f;
				Vector2 pos = lp.Position - Main.screenPosition;
				Rectangle rect = new Rectangle((int)(pos.X - size * 0.5f), (int)(pos.Y - size * 0.5f), (int)size, (int)size);
				Main.spriteBatch.Draw(_lightTexture, rect, null, lp.Color * alpha, 0f, Vector2.Zero, SpriteEffects.None, 0f);
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
			if (_basicEffect == null) { _basicEffect = new BasicEffect(gd); _basicEffect.VertexColorEnabled = true; }
			_basicEffect.Projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);
			_basicEffect.View = Main.GameViewMatrix.TransformationMatrix;
			_basicEffect.World = Matrix.Identity;

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
				foreach (EffectPass pass in _basicEffect.CurrentTechnique.Passes) { pass.Apply(); gd.DrawUserPrimitives(PrimitiveType.TriangleList, verts, 0, sides); }
			}
			gd.BlendState = prevBlend;
			gd.RasterizerState = prevRaster;
			Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
		}

		private void DrawCrossEffect(int frame) {
			GetCrossSize(frame, out float mW, out float mH, out float oW, out float oH);
			float alpha = CalculateAlpha(frame);
			Vector2 sp = _spawnPosition - Main.screenPosition;
			Main.spriteBatch.End();
			Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
			DrawRotatedCrossPart(sp, mW, mH, oW, oH, alpha, MathHelper.PiOver4);
			DrawRotatedCrossPart(sp, mW, mH, oW, oH, alpha, -MathHelper.PiOver4);
			Main.spriteBatch.End();
			Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
		}

		private void DrawRotatedCrossPart(Vector2 c, float mW, float mH, float oW, float oH, float a, float r) {
			Vector2 o = _crossTexture.Size() / 2f;
			Main.spriteBatch.Draw(_crossTexture, c, null, MainColor * a, r, o, new Vector2(mW / _crossTexture.Width, mH / _crossTexture.Height), SpriteEffects.None, 0f);
			Main.spriteBatch.Draw(_crossTexture, c, null, OuterColor * a, r, o, new Vector2(oW / _crossTexture.Width, oH / _crossTexture.Height), SpriteEffects.None, 0f);
		}

		private void DrawDarkCross(int frame) {
			int lf = frame - DarkCrossStartFrame;
			if (lf < 0 || lf >= DarkCrossDuration)
				return;
			float p = (float)lf / (DarkCrossDuration - 1);
			float w = DarkCrossStartWidth + (DarkCrossEndWidth - DarkCrossStartWidth) * p;
			float h = DarkCrossStartHeight + (DarkCrossEndHeight - DarkCrossStartWidth) * p;
			float alpha = DarkCrossAlpha * (1f - p * p);
			if (alpha <= 0f)
				return;
			Vector2 sp = _spawnPosition - Main.screenPosition;
			Main.spriteBatch.End();
			Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
			Color dc = DarkCrossColor * alpha;
			Vector2 o = _crossTexture.Size() / 2f;
			float sx = w / _crossTexture.Width, sy = h / _crossTexture.Height;
			Main.spriteBatch.Draw(_crossTexture, sp, null, dc, MathHelper.PiOver4, o, new Vector2(sx, sy), SpriteEffects.None, 0f);
			Main.spriteBatch.Draw(_crossTexture, sp, null, dc, -MathHelper.PiOver4, o, new Vector2(sx, sy), SpriteEffects.None, 0f);
			Main.spriteBatch.End();
			Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
		}

		private void DrawRandomParticles() {
			if (randomParticles.Count == 0)
				return;
			Main.spriteBatch.End();
			Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
			foreach (var rp in randomParticles) {
				float alpha = (float)rp.Life / rp.MaxLife;
				float size = rp.Size * 10f;
				Vector2 pos = rp.Position - Main.screenPosition;
				Rectangle rect = new Rectangle((int)(pos.X - size * 0.5f), (int)(pos.Y - size * 0.5f), (int)size, (int)size);
				Main.spriteBatch.Draw(_lightTexture, rect, null, rp.Color * alpha, 0f, Vector2.Zero, SpriteEffects.None, 0f);
			}
			Main.spriteBatch.End();
			Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
		}

		private void GetCrossSize(int f, out float mW, out float mH, out float oW, out float oH) {
			float mw, mh;
			if (f <= PeakFrame) { float t = f / (float)PeakFrame; float e = 1f - (1f - t) * (1f - t); mw = MainStartWidth + (MainPeakWidth - MainStartWidth) * e; mh = MainStartHeight + (MainPeakHeight - MainStartHeight) * e; }
			else if (f <= ShrinkEndFrame) { float t = (f - PeakFrame) / (float)(ShrinkEndFrame - PeakFrame); float e = t * t; mw = MainPeakWidth + (MainShrinkWidth - MainPeakWidth) * e; mh = MainPeakHeight + (MainShrinkHeight - MainPeakHeight) * e; }
			else { float t = (f - ShrinkEndFrame) / (float)(TotalFrames - 1 - ShrinkEndFrame); float e = 1f - (1f - t) * (1f - t); mw = MainShrinkWidth + (MainEndWidth - MainShrinkWidth) * e; mh = MainShrinkHeight + (MainEndHeight - MainShrinkHeight) * e; }
			mW = Math.Max(1, mw);
			mH = Math.Max(1, mh);
			float s;
			if (f <= PeakFrame) { float t = f / (float)PeakFrame; float e = 1f - (1f - t) * (1f - t); s = OuterScaleStart + (OuterScalePeak - OuterScaleStart) * e; }
			else if (f <= ShrinkEndFrame) { float t = (f - PeakFrame) / (float)(ShrinkEndFrame - PeakFrame); float e = t * t; s = OuterScalePeak + (OuterScaleShrink - OuterScalePeak) * e; }
			else { float t = (f - ShrinkEndFrame) / (float)(TotalFrames - 1 - ShrinkEndFrame); float e = 1f - (1f - t) * (1f - t); s = OuterScaleShrink + (OuterScaleEnd - OuterScaleShrink) * e; }
			oW = mW * s;
			oH = mH * s;
		}

		private float CalculateAlpha(int f) { float t = f / (float)(TotalFrames - 1); return 1f - t * t; }
	}
}