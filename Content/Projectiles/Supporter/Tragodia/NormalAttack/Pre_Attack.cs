using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace ArknightsMod.Content.Projectiles.Supporter.Tragodia.NormalAttack
{
	public class Pre_Attack : ModProjectile
	{
		public int targetNPCIndex = -1;
		public float offsetY = 20f;
		private NPC targetNPC = null;

		private const int ParticleCount = 4;
		private const int MaxTrailPoints = 30;
		private const float BaseEllipseA = 55f;
		private const float BaseEllipseB = 30f;
		private const float StartRadiusScale = 0.6f;
		private const float EndRadiusScale = 0f;
		private const float AngularSpeed = 0.25f;

		private const float TrailWidth = 25f;
		private const float ScrollSpeed = 5.0f;
		private static readonly Color TrailColor = new Color(180, 120, 255, 240);

		private float[] preAttackAngles;
		private List<Vector2>[] preAttackTrailPositions;
		private Texture2D windTex;
		private BasicEffect effect;
		private float time;

		public override void SetStaticDefaults() {
			Main.projFrames[Projectile.type] = 0;
		}

		public override void SetDefaults() {
			Projectile.width = 0;
			Projectile.height = 0;
			Projectile.friendly = false;
			Projectile.hostile = false;
			Projectile.DamageType = DamageClass.Magic;
			Projectile.penetrate = -1;
			Projectile.tileCollide = false;
			Projectile.timeLeft = 40;
			Projectile.alpha = 255;
			Projectile.ignoreWater = true;
		}

		public override void AI() {
			if (preAttackAngles == null) {
				Random rand = new Random();
				preAttackAngles = new float[ParticleCount];
				preAttackTrailPositions = new List<Vector2>[ParticleCount];

				for (int i = 0; i < ParticleCount; i++) {
					preAttackAngles[i] = (float)(rand.NextDouble() * MathHelper.TwoPi);
					preAttackTrailPositions[i] = new List<Vector2>();
				}

				if (targetNPCIndex >= 0 && targetNPCIndex < Main.maxNPCs) {
					targetNPC = Main.npc[targetNPCIndex];
				}
			}

			if (targetNPC != null && targetNPC.active) {
				Projectile.Center = targetNPC.Center + new Vector2(0, offsetY);
			}

			if (windTex == null)
				windTex = ModContent.Request<Texture2D>("ArknightsMod/Content/Projectiles/Supporter/Tragodia/EffectImage/Tail").Value;

			time++;

			float progress = (float)time / 40f;
			float globalScale = MathHelper.Lerp(StartRadiusScale, EndRadiusScale, progress);
			Vector2 center = Projectile.Center;

			for (int i = 0; i < ParticleCount; i++) {
				preAttackAngles[i] += AngularSpeed;

				float ellipseA = BaseEllipseA * globalScale;
				float ellipseB = BaseEllipseB * globalScale;

				float x = center.X + MathF.Cos(preAttackAngles[i]) * ellipseA;
				float y = center.Y + MathF.Sin(preAttackAngles[i]) * ellipseB;

				preAttackTrailPositions[i].Add(new Vector2(x, y));

				while (preAttackTrailPositions[i].Count > MaxTrailPoints)
					preAttackTrailPositions[i].RemoveAt(0);
			}
		}

		public override bool PreDraw(ref Color lightColor) {
			if (Main.spriteBatch == null || Main.graphics.GraphicsDevice == null || windTex == null)
				return false;

			GraphicsDevice device = Main.graphics.GraphicsDevice;

			float alpha;
			if (time <= 4f)
				alpha = time / 4f;
			else {
				float decay = (time - 4f) / 36f;
				alpha = 1f - (float)Math.Pow(decay, 2f);
			}

			if (effect == null) {
				effect = new BasicEffect(device);
				effect.VertexColorEnabled = true;
				effect.TextureEnabled = true;
			}
			effect.World = Matrix.Identity;
			effect.View = Matrix.Identity;
			effect.Projection = Matrix.CreateOrthographicOffCenter(
				0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

			Matrix transform = Main.GameViewMatrix.TransformationMatrix;
			float zoom = Main.GameViewMatrix.Zoom.X;

			BlendState oldBlend = device.BlendState;
			DepthStencilState oldDepth = device.DepthStencilState;
			RasterizerState oldRaster = device.RasterizerState;
			Main.spriteBatch.End();

			try {
				effect.Texture = windTex;
				device.BlendState = BlendState.Additive;
				device.DepthStencilState = DepthStencilState.None;
				device.RasterizerState = RasterizerState.CullNone;
				device.SamplerStates[0] = SamplerState.PointWrap;

				for (int i = 0; i < ParticleCount; i++) {
					var trail = preAttackTrailPositions[i];
					if (trail.Count < 2)
						continue;

					List<Vector2> screenTrail = new List<Vector2>(trail.Count);
					foreach (var p in trail) {
						Vector2 transformed = Vector2.Transform(p - Main.screenPosition, transform);
						screenTrail.Add(transformed);
					}

					float scaledWidth = TrailWidth * zoom;
					DrawSingleTrail(device, screenTrail, scaledWidth, ScrollSpeed, TrailColor, alpha);
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
				float fade = 1f - trailProgress;

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

		public override bool PreDrawExtras() {
			return false;
		}

		public override bool? CanDamage() {
			return false;
		}
	}
}