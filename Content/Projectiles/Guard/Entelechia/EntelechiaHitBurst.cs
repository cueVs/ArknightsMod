using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Projectiles.Guard.Entelechia
{
	// 隐德来希·命中爆开特效（纯代码绘制，三角图元）：
	//   向四周随机角度/方向/大小辐射的「三角角」爆发；颜色在极短时间内由外向内快速
	//   过渡到 红(#b4232d)→深红(#1c0405) 再淡出；整体白色很少，仅爆炸中心保持白色；
	//   伴随红色粒子与少量出现即逝的白色辐射线。
	public class EntelechiaHitBurst : ModProjectile
	{
		private const int LifeMax = 24;

		private static readonly Color White   = new(255, 255, 255);
		private static readonly Color Red     = new(180, 35, 45);  // #b4232d
		private static readonly Color DarkRed = new(28, 4, 5);     // #1c0405

		private static BasicEffect _basic;

		public override string Texture => ArknightsMod.noTexture;

		public override void Unload() {
			// Dispose 也必须在主线程执行（FNA3D 图形对象约束）
			Main.QueueMainThreadAction(() => {
				_basic?.Dispose();
				_basic = null;
			});
		}

		public override void SetDefaults() {
			Projectile.width = Projectile.height = 8;
			Projectile.friendly    = false;
			Projectile.hostile     = false;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
			Projectile.penetrate   = -1;
			Projectile.timeLeft    = LifeMax;
		}

		public override void OnSpawn(IEntitySource source) {
			if (Projectile.ai[0] <= 0f)
				Projectile.ai[0] = Main.rand.Next(1, int.MaxValue);

			for (int i = 0; i < 8; i++) {
				Vector2 v = Main.rand.NextVector2Circular(4.5f, 4.5f);
				Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.RedTorch, v, 0, default,
					Main.rand.NextFloat(1.0f, 1.7f));
				d.noGravity = true;
			}
		}

		public override void AI() {
			Lighting.AddLight(Projectile.Center, 0.6f, 0.2f, 0.15f);
		}

		// 颜色随生命快速过渡（白只在极早期，随后红→深红）。delay 让内段滞后于外段。
		private static Color Grad(float t, float delay) {
			float u = t - delay;
			if (u < 0.06f)
				return White;
			return Color.Lerp(Red, DarkRed, MathHelper.Clamp((u - 0.06f) / 0.26f, 0f, 1f));
		}

		private static void AddTri(List<VertexPositionColor> v, Vector2 a, Vector2 b, Vector2 c,
			Color ca, Color cb, Color cc) {
			v.Add(new VertexPositionColor(new Vector3(a, 0f), ca));
			v.Add(new VertexPositionColor(new Vector3(b, 0f), cb));
			v.Add(new VertexPositionColor(new Vector3(c, 0f), cc));
		}

		// 以 center 为基点、沿 dir 的细长四边形（两段三角），用于白色辐射线
		private static void AddQuadRay(List<VertexPositionColor> v, Vector2 center, Vector2 dir,
			float len, float halfW, Color col) {
			Vector2 perp = new(-dir.Y, dir.X);
			Vector2 a = center + perp * halfW;
			Vector2 b = center - perp * halfW;
			Vector2 c = center + dir * len + perp * halfW;
			Vector2 d = center + dir * len - perp * halfW;
			AddTri(v, a, b, c, col, col, col);
			AddTri(v, b, d, c, col, col, col);
		}

		public override bool PreDraw(ref Color lightColor) {
			if (Main.dedServ)
				return false;

			// 主线程内惰性创建 BasicEffect（不能在 Load 里建，那是非主线程）
			if (_basic == null || _basic.IsDisposed) {
				_basic = new BasicEffect(Main.instance.GraphicsDevice) {
					VertexColorEnabled = true,
					World = Matrix.Identity,
					View  = Matrix.Identity,
				};
			}

			float t = (LifeMax - Projectile.timeLeft) / (float)LifeMax; // 0→1
			float grow = MathHelper.Clamp(t / 0.18f, 0f, 1f);            // 初期快速向外扩张
			float spikeFade = 1f - MathHelper.Clamp((t - 0.40f) / 0.45f, 0f, 1f);
			if (spikeFade <= 0.001f && t > 0.9f)
				return false;

			Vector2 pos = Projectile.Center - Main.screenPosition;
			var rng = new Random((int)Projectile.ai[0]);

			byte sa = (byte)(MathHelper.Clamp(spikeFade, 0f, 1f) * 255f);
			Color baseCol = Grad(t, 0.14f); baseCol.A = sa; // 内段（靠中心，滞后）
			Color tipCol  = Grad(t, 0.00f); tipCol.A  = sa; // 外段（领先变深红）

			var verts = new List<VertexPositionColor>(128);

			// ── 辐射状三角「角」：随机角度/方向/大小，apex 朝外 ──
			int spikeCount = 11 + rng.Next(5); // 11~15
			for (int i = 0; i < spikeCount; i++) {
				float ang   = (float)(rng.NextDouble() * MathHelper.TwoPi);
				float len   = MathHelper.Lerp(18f, 46f, (float)rng.NextDouble()) * grow;
				float halfW = MathHelper.Lerp(3.0f, 7.0f, (float)rng.NextDouble());
				Vector2 dir  = ang.ToRotationVector2();
				Vector2 perp = new(-dir.Y, dir.X);

				Vector2 baseC = pos + dir * (3f * grow);     // 底边靠近中心
				Vector2 bL = baseC + perp * halfW;
				Vector2 bR = baseC - perp * halfW;
				Vector2 apex = pos + dir * len;              // 尖端朝外

				// 底边用内段色、尖端用外段色 → 颜色由外向内过渡
				AddTri(verts, bL, bR, apex, baseCol, baseCol, tipCol);
			}

			// ── 爆炸中心白色核心（小四边形，保持白色）──
			float coreFade = 1f - MathHelper.Clamp((t - 0.12f) / 0.55f, 0f, 1f);
			if (coreFade > 0.001f) {
				byte ca = (byte)(coreFade * 255f);
				Color core = White; core.A = ca;
				float cs = MathHelper.Lerp(9f, 3f, t) * (0.4f + 0.6f * grow);
				Vector2 e0 = pos + new Vector2(-cs, 0f);
				Vector2 e1 = pos + new Vector2(0f, -cs);
				Vector2 e2 = pos + new Vector2(cs, 0f);
				Vector2 e3 = pos + new Vector2(0f, cs);
				AddTri(verts, e0, e1, e2, core, core, core); // 菱形
				AddTri(verts, e0, e2, e3, core, core, core);
			}

			// ── 少量白色辐射线：仅初期出现并快速消失 ──
			float lineFade = 1f - MathHelper.Clamp(t / 0.30f, 0f, 1f);
			if (lineFade > 0.01f) {
				byte la = (byte)(lineFade * 255f);
				Color lineCol = White; lineCol.A = la;
				int lineCount = 3 + rng.Next(2); // 3~4 条
				for (int i = 0; i < lineCount; i++) {
					float ang  = (float)(rng.NextDouble() * MathHelper.TwoPi);
					float llen = MathHelper.Lerp(36f, 64f, (float)rng.NextDouble()) * grow;
					AddQuadRay(verts, pos, ang.ToRotationVector2(), llen, 1.2f, lineCol);
				}
			}

			if (verts.Count < 3)
				return false;

			GraphicsDevice device = Main.instance.GraphicsDevice;
			Main.spriteBatch.End();

			device.BlendState        = BlendState.Additive;
			device.RasterizerState   = RasterizerState.CullNone;
			device.DepthStencilState = DepthStencilState.None;

			_basic.Projection = Matrix.CreateOrthographicOffCenter(
				0, Main.screenWidth, Main.screenHeight, 0, -1f, 1f);

			var arr = verts.ToArray();
			foreach (EffectPass pass in _basic.CurrentTechnique.Passes) {
				pass.Apply();
				device.DrawUserPrimitives(PrimitiveType.TriangleList, arr, 0, arr.Length / 3);
			}

			Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
				Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone,
				null, Main.GameViewMatrix.TransformationMatrix);

			return false;
		}
	}
}
