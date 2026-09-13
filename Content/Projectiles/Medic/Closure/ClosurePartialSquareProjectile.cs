using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Projectiles.Medic.Closure
{
	// 可露希尔·二技能激光"次级"命中特效：红色、无填充的正方形描边，且描边并非完整一圈——
	// 只显示周长的 30%~50%（起始位置随机，可能跨过一个角，也可能就只剩一个角露出来）。
	// 描边粗细与扫描枪命中特效（ClosureHitBurstProjectile）的取景框白色描边一致。
	// 每三次激光命中里，有两次显示这个（另一次显示完整的 ClosureHitBurstProjectile 取景框）。
	// 生成位置不跟随目标移动（一次性定格在命中附近的随机偏移点，播完即销毁）。
	public class ClosurePartialSquareProjectile : ModProjectile
	{
		private const int LifeMax = 16;
		private const float HalfSide = 16f;
		private const float Thickness = 2.2f; // 与 ClosureHitBurstProjectile 取景框描边粗细一致

		private static readonly Color LineCol = new(220, 40, 40);

		private static BasicEffect _basic;

		private float _startFrac;
		private float _lengthFrac;

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
			Projectile.velocity = Vector2.Zero;
			_startFrac  = Main.rand.NextFloat();
			_lengthFrac = Main.rand.NextFloat(0.30f, 0.50f);
		}

		public override void AI() {
			Projectile.velocity = Vector2.Zero;
			Lighting.AddLight(Projectile.Center, 0.35f, 0.06f, 0.06f);
		}

		private static void AddTri(List<VertexPositionColor> v, Vector2 a, Vector2 b, Vector2 c, Color col) {
			v.Add(new VertexPositionColor(new Vector3(a, 0f), col));
			v.Add(new VertexPositionColor(new Vector3(b, 0f), col));
			v.Add(new VertexPositionColor(new Vector3(c, 0f), col));
		}

		private static void AddThickLine(List<VertexPositionColor> v, Vector2 a, Vector2 b, float thickness, Color col) {
			Vector2 dir = b - a;
			if (dir.LengthSquared() < 0.0001f)
				return;
			dir.Normalize();
			Vector2 perp = new Vector2(-dir.Y, dir.X) * (thickness * 0.5f);
			AddTri(v, a + perp, a - perp, b + perp, col);
			AddTri(v, a - perp, b - perp, b + perp, col);
		}

		// 只画正方形周长上 [from, to) 这一段范围（单位与 corners 一致，沿 4 条边依次展开）
		private static void DrawPerimeterRange(List<VertexPositionColor> v, Vector2[] corners, float edgeLen,
			float from, float to, float thickness, Color col) {
			for (int e = 0; e < 4; e++) {
				float edgeStart = e * edgeLen;
				float edgeEnd = edgeStart + edgeLen;
				float clipStart = MathHelper.Max(from, edgeStart);
				float clipEnd   = MathHelper.Min(to, edgeEnd);
				if (clipEnd <= clipStart)
					continue;
				float t0 = (clipStart - edgeStart) / edgeLen;
				float t1 = (clipEnd - edgeStart) / edgeLen;
				Vector2 p0 = Vector2.Lerp(corners[e], corners[e + 1], t0);
				Vector2 p1 = Vector2.Lerp(corners[e], corners[e + 1], t1);
				AddThickLine(v, p0, p1, thickness, col);
			}
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
			float grow = 1f - (float)System.Math.Pow(1f - MathHelper.Clamp(t / 0.25f, 0f, 1f), 3f);
			float fade = 1f - MathHelper.Clamp((t - 0.35f) / 0.55f, 0f, 1f);
			if (fade <= 0.01f)
				return false;

			Vector2 pos = Projectile.Center - Main.screenPosition;
			float halfSide = HalfSide * MathHelper.Lerp(0.6f, 1f, grow);

			Vector2[] corners = {
				pos + new Vector2(-halfSide, -halfSide),
				pos + new Vector2( halfSide, -halfSide),
				pos + new Vector2( halfSide,  halfSide),
				pos + new Vector2(-halfSide,  halfSide),
			};
			corners = new[] { corners[0], corners[1], corners[2], corners[3], corners[0] };

			float edgeLen = halfSide * 2f;
			float totalLen = edgeLen * 4f;
			float s0 = _startFrac * totalLen;
			float segLen = _lengthFrac * totalLen;

			Color col = LineCol * fade;

			var verts = new List<VertexPositionColor>(24);
			DrawPerimeterRange(verts, corners, edgeLen, s0, System.Math.Min(s0 + segLen, totalLen), Thickness, col);
			if (s0 + segLen > totalLen)
				DrawPerimeterRange(verts, corners, edgeLen, 0f, s0 + segLen - totalLen, Thickness, col);

			if (verts.Count < 3)
				return false;

			GraphicsDevice device = Main.instance.GraphicsDevice;
			Main.spriteBatch.End();

			device.BlendState        = BlendState.NonPremultiplied;
			device.RasterizerState   = RasterizerState.CullNone;
			device.DepthStencilState = DepthStencilState.None;

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
