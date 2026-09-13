using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Projectiles.Medic.Closure
{
	// 可露希尔·扫描枪命中特效（纯代码绘制，三角/线段图元）：
	//   1. 由小到大"爆开"的正方形取景框（正放、不倾斜）：仅右上角、左下角两处 L 形描边，半透明红紫色填充；
	//   2. 中心向随机角度四散的尖头爆炸「角」：偏白的紫色，外缘叠加柔和模糊的紫色辉光；
	//   3. 中心一枚"中子星"：左右两条长"脚"、上下无角的横向纺锤高光点缀；
	//   4. 爆炸中心额外叠加一枚纯白核心。
	// 生成后固定在出现的世界坐标，不跟随任何实体；命中 NPC 或方块都会触发。
	public class ClosureHitBurstProjectile : ModProjectile
	{
		private const int LifeMax = 16; // 整体持续时间缩短

		private static readonly Color FillCol    = new(140, 40, 110);  // 半透明红紫色填充
		private static readonly Color BracketCol = new(235, 200, 255); // 取景框描边：泛白淡紫
		private static readonly Color SpikeCol   = new(245, 225, 255); // 爆炸角：偏白的紫
		private static readonly Color GlowCol    = new(175, 70, 225);  // 紫色外发光
		private static readonly Color StarCol    = new(255, 250, 255); // 中子星核心：近白
		private static readonly Color PureWhite  = new(255, 255, 255);

		private static BasicEffect _basic;

		public override string Texture => ArknightsMod.noTexture;

		public override void Unload() {
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

			// 固定在出生点，不受重力/速度影响；防御性地清零速度，避免被其它系统意外赋值后产生位移
			Projectile.velocity = Vector2.Zero;

			for (int i = 0; i < 6; i++) {
				Vector2 v = Main.rand.NextVector2Circular(4f, 4f);
				Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.PinkTorch, v, 0, default,
					Main.rand.NextFloat(0.9f, 1.5f));
				d.noGravity = true;
			}
		}

		public override void AI() {
			// 保持原地：不受任何外力位移（防御性重置，防止被其它模组/系统的全局速度处理影响）
			Projectile.velocity = Vector2.Zero;
			Lighting.AddLight(Projectile.Center, 0.5f, 0.18f, 0.6f);
		}

		private static void AddTri(List<VertexPositionColor> v, Vector2 a, Vector2 b, Vector2 c,
			Color ca, Color cb, Color cc) {
			v.Add(new VertexPositionColor(new Vector3(a, 0f), ca));
			v.Add(new VertexPositionColor(new Vector3(b, 0f), cb));
			v.Add(new VertexPositionColor(new Vector3(c, 0f), cc));
		}

		// 任意两点间的等宽线段（用于取景框 L 形描边）
		private static void AddThickLine(List<VertexPositionColor> v, Vector2 a, Vector2 b, float thickness, Color col) {
			Vector2 dir = (b - a);
			if (dir.LengthSquared() < 0.0001f)
				return;
			dir.Normalize();
			Vector2 perp = new Vector2(-dir.Y, dir.X) * (thickness * 0.5f);
			Vector2 p0 = a + perp, p1 = a - perp, p2 = b + perp, p3 = b - perp;
			AddTri(v, p0, p1, p2, col, col, col);
			AddTri(v, p1, p3, p2, col, col, col);
		}

		public override bool PreDraw(ref Color lightColor) {
			if (Main.dedServ)
				return false;

			if (_basic == null || _basic.IsDisposed) {
				_basic = new BasicEffect(Main.instance.GraphicsDevice) {
					VertexColorEnabled = true,
					World = Matrix.Identity,
					View  = Matrix.Identity,
				};
			}

			float t = (LifeMax - Projectile.timeLeft) / (float)LifeMax; // 0→1
			var rng = new Random((int)Projectile.ai[0]);

			// 世界坐标 - 屏幕坐标：效果固定锚定在生成时的世界位置，随相机滚动而非跟随玩家
			Vector2 pos = Projectile.Center - Main.screenPosition;

			var verts = new List<VertexPositionColor>(220);

			// ════ 1. 取景框：由小到大展开，正放不倾斜，仅右上角/左下角 L 形描边 + 半透明红紫填充 ════
			float boxGrow = 1f - (float)Math.Pow(1f - MathHelper.Clamp(t / 0.28f, 0f, 1f), 3f); // 快速弹出的缓出曲线
			float boxFade = 1f - MathHelper.Clamp((t - 0.45f) / 0.40f, 0f, 1f);
			if (boxGrow > 0.001f && boxFade > 0.001f) {
				float side = MathHelper.Lerp(4f, 30f, boxGrow);

				Vector2 tl = pos + new Vector2(-side, -side);
				Vector2 tr = pos + new Vector2(side, -side);
				Vector2 bl = pos + new Vector2(-side, side);
				Vector2 br = pos + new Vector2(side, side);

				byte fillA = (byte)(0.38f * boxFade * 255f);
				Color fill = FillCol; fill.A = fillA;
				AddTri(verts, tl, tr, bl, fill, fill, fill);
				AddTri(verts, tr, br, bl, fill, fill, fill);

				byte brA = (byte)(boxFade * 255f);
				Color bracket = BracketCol; bracket.A = brA;
				float bracketLen = side * 0.85f;
				float thick = 2.2f;

				// 右上角：沿顶边向左 + 沿右边向下，两段各自独立（不连成整框）
				AddThickLine(verts, tr, tr + new Vector2(-bracketLen, 0f), thick, bracket);
				AddThickLine(verts, tr, tr + new Vector2(0f, bracketLen), thick, bracket);

				// 左下角：沿底边向右 + 沿左边向上
				AddThickLine(verts, bl, bl + new Vector2(bracketLen, 0f), thick, bracket);
				AddThickLine(verts, bl, bl + new Vector2(0f, -bracketLen), thick, bracket);
			}

			// ════ 2. 辐射状三角「角」：先画宽而淡的多层辉光（模糊感），再画核心（保证不被辉光盖住变淡）════
			float spikeGrow = MathHelper.Clamp(t / 0.14f, 0f, 1f);
			float spikeFade = 1f - MathHelper.Clamp((t - 0.30f) / 0.40f, 0f, 1f);
			if (spikeFade > 0.001f) {
				byte sa = (byte)(spikeFade * 255f);
				Color spikeCore = SpikeCol; spikeCore.A = sa;
				Color transparentCore = spikeCore; transparentCore.A = 0;

				Color glowOuter = GlowCol; glowOuter.A = (byte)(spikeFade * 55f);  // 最外层：极淡、极宽 → 模糊感
				Color glowMid   = GlowCol; glowMid.A   = (byte)(spikeFade * 110f);
				Color transparentOuter = glowOuter; transparentOuter.A = 0;
				Color transparentMid   = glowMid;   transparentMid.A   = 0;

				int spikeCount = 9 + rng.Next(4); // 9~12
				var spikes = new (float ang, float len, float halfW)[spikeCount];
				for (int i = 0; i < spikeCount; i++) {
					spikes[i] = (
						(float)(rng.NextDouble() * MathHelper.TwoPi),
						MathHelper.Lerp(16f, 40f, (float)rng.NextDouble()) * spikeGrow,
						MathHelper.Lerp(2.4f, 5.6f, (float)rng.NextDouble()));
				}

				// 先整体画完所有「最外层模糊辉光」，再画「中层辉光」，最后画「核心」——
				// 保证核心三角不会被后画的辉光半透明层压暗/染色
				foreach (var s in spikes) {
					Vector2 dir  = s.ang.ToRotationVector2();
					Vector2 perp = new(-dir.Y, dir.X);
					Vector2 baseC = pos + dir * (2.0f * spikeGrow);
					Vector2 oL = baseC + perp * (s.halfW * 3.4f);
					Vector2 oR = baseC - perp * (s.halfW * 3.4f);
					Vector2 oApex = pos + dir * (s.len * 1.55f);
					AddTri(verts, oL, oR, oApex, glowOuter, glowOuter, transparentOuter);
				}
				foreach (var s in spikes) {
					Vector2 dir  = s.ang.ToRotationVector2();
					Vector2 perp = new(-dir.Y, dir.X);
					Vector2 baseC = pos + dir * (2.2f * spikeGrow);
					Vector2 mL = baseC + perp * (s.halfW * 2.1f);
					Vector2 mR = baseC - perp * (s.halfW * 2.1f);
					Vector2 mApex = pos + dir * (s.len * 1.30f);
					AddTri(verts, mL, mR, mApex, glowMid, glowMid, transparentMid);
				}
				foreach (var s in spikes) {
					Vector2 dir  = s.ang.ToRotationVector2();
					Vector2 perp = new(-dir.Y, dir.X);
					Vector2 baseC = pos + dir * (2.5f * spikeGrow);
					Vector2 bL = baseC + perp * s.halfW;
					Vector2 bR = baseC - perp * s.halfW;
					Vector2 apex = pos + dir * s.len;
					AddTri(verts, bL, bR, apex, spikeCore, spikeCore, transparentCore);
				}
			}

			// ════ 3. 中子星：左右长脚、上下无角的横向纺锤高光（放大、更醒目）════
			float starFade = 1f - MathHelper.Clamp(t / 0.48f, 0f, 1f);
			if (starFade > 0.001f) {
				byte sa = (byte)(starFade * 255f);
				Color starCol = StarCol; starCol.A = sa;
				Color starEdge = starCol; starEdge.A = 0;

				float legLen = MathHelper.Lerp(3f, 42f, MathHelper.Clamp(t / 0.18f, 0f, 1f)) * (0.75f + 0.25f * starFade);
				float coreHalfH = 3.2f; // 纵向厚度仍很小（上下几乎无角），但比之前更粗一些以更醒目

				Vector2 rightTip = pos + new Vector2(legLen, 0f);
				Vector2 leftTip  = pos + new Vector2(-legLen, 0f);
				Vector2 top      = pos + new Vector2(0f, -coreHalfH);
				Vector2 bottom   = pos + new Vector2(0f, coreHalfH);

				// 横向纺锤：左尖 → 上/下 → 右尖（中心亮、两端淡出）
				AddTri(verts, leftTip, top, pos, starEdge, starCol, starCol);
				AddTri(verts, leftTip, pos, bottom, starEdge, starCol, starCol);
				AddTri(verts, pos, top, rightTip, starCol, starCol, starEdge);
				AddTri(verts, pos, rightTip, bottom, starCol, starEdge, starCol);
			}

			// ════ 4. 爆炸中心纯白核心：全程叠在最上层，保证中心足够"白" ════
			float coreFade = 1f - MathHelper.Clamp((t - 0.10f) / 0.35f, 0f, 1f);
			if (coreFade > 0.001f) {
				byte ca = (byte)(coreFade * 255f);
				Color core = PureWhite; core.A = ca;
				float cs = MathHelper.Lerp(3f, 7.5f, MathHelper.Clamp(t / 0.12f, 0f, 1f)) * coreFade;
				Vector2 e0 = pos + new Vector2(-cs, 0f);
				Vector2 e1 = pos + new Vector2(0f, -cs);
				Vector2 e2 = pos + new Vector2(cs, 0f);
				Vector2 e3 = pos + new Vector2(0f, cs);
				AddTri(verts, e0, e1, e2, core, core, core);
				AddTri(verts, e0, e2, e3, core, core, core);
			}

			if (verts.Count < 3)
				return false;

			GraphicsDevice device = Main.instance.GraphicsDevice;
			Main.spriteBatch.End();

			device.BlendState        = BlendState.NonPremultiplied;
			device.RasterizerState   = RasterizerState.CullNone;
			device.DepthStencilState = DepthStencilState.None;

			// GameViewMatrix.TransformationMatrix 只是像素空间内的缩放+平移，不是裁剪空间投影矩阵，
			// 必须再接上正交投影才是完整变换（否则顶点停在像素空间，会被直接裁剪掉、整体不可见）。
			Matrix projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1f, 1f);
			_basic.Projection = Main.GameViewMatrix.TransformationMatrix * projection;

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
