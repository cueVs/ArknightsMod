using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Projectiles.Medic.Closure
{
	// 可露希尔·扫描枪枪口特效：每次开火都重新生成一个由小到大出现的椭圆形闪光（不再跨发持续），
	// 能量镖弹幕从中飞出。出现时整体是白色，快速过渡为内圆淡紫色、外圈红紫色的硬边缘椭圆，
	// 整体不透明度封顶在 60% 左右（不会变成完全不透明的实心块）；边缘外侧依次是：一道很细的
	// 蓝色描边、蓝色与红紫色交织的短距离外发光、以及更外侧一圈明显偏蓝、带一定间距的环绕弧线。
	// 位置每帧都跟随玩家当前位置平移（不会在停火后停留在地图上的绝对坐标不动），但朝向角度
	// 固定为开火那一刻的方向，不会跟着之后的鼠标移动转动，直到下一发开火才重新定向。
	public class ClosureMuzzleFlashProjectile : ModProjectile
	{
		private const int GrowTicks            = 6;   // 出现时由小到大的弹出动画时长
		private const int ColorTransitionTicks = 6;   // 完全展开后，白→双色的颜色过渡时长（很快）
		private const int FadeOutTicks         = 8;   // 生命末尾的淡出时长
		private const int LifeMax              = 26;  // 单发闪光的总生命（每次开火都会重新生成一个新的）
		private const float MaxBodyAlpha       = 0.75f; // 椭圆本体不透明度封顶，避免变成实心块
		private const float RingSpeed          = 0.11f; // 环绕弧线每 tick 转动的角度

		private static readonly Color WhiteCol       = new(255, 255, 255); // 起始：白色
		private static readonly Color InnerTargetCol = new(205, 150, 245); // 内圆最终颜色：淡紫色
		private static readonly Color OuterTargetCol = new(170, 30, 120);  // 外圈最终颜色：红紫色
		private static readonly Color OutlineCol     = new(90, 190, 255);  // 椭圆边缘的细蓝色描边
		private static readonly Color GlowBlueCol    = new(80, 175, 255);  // 外发光内侧：蓝色
		private static readonly Color GlowPurpleCol  = new(180, 40, 150);  // 外发光外侧：红紫色
		private static readonly Color RingCol        = new(70, 165, 255);  // 外援环绕线：更明显的蓝色

		private static BasicEffect _basic;

		// 环绕弧线的角度是全局静态的、只增不减：每次开火重新生成新实例时不会归零重来，
		// 而是接着上一发结束时的角度继续转，视觉上像是同一圈光始终在绕转，不会瞬间跳回起点。
		private static float _globalRingAngle;

		private Vector2 _frozenDir = Vector2.UnitX; // 开火那一刻的朝向，固定不变直到下一发

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
			// 用生成时传入的 velocity 方向记录朝向（面朝光标/发射方向），此后固定不再随光标转动
			_frozenDir = Projectile.velocity.SafeNormalize(Vector2.UnitX);
			Projectile.rotation = _frozenDir.ToRotation();
			Projectile.velocity = Vector2.Zero;
		}

		public override void AI() {
			// 位置每帧都跟随玩家当前位置平移，只是朝向固定不变——这样停火淡出的过程中，
			// 特效仍会跟着玩家移动，不会停留在地图上的某个绝对坐标不动。
			Player owner = Main.player[Projectile.owner];
			if (owner.active && !owner.dead) {
				Projectile.Center = owner.itemLocation + _frozenDir * 48f;
			}

			_globalRingAngle += RingSpeed;

			Lighting.AddLight(Projectile.Center, 0.5f, 0.18f, 0.55f);
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

			int age = LifeMax - Projectile.timeLeft;

			// 弹出动画：由小到大
			float grow = MathHelper.Clamp(age / (float)GrowTicks, 0f, 1f);
			grow = 1f - (1f - grow) * (1f - grow); // 缓出

			// 生命末尾淡出
			float fade = MathHelper.Clamp(Projectile.timeLeft / (float)FadeOutTicks, 0f, 1f);
			if (fade <= 0.001f)
				return false;

			Vector2 pos = Projectile.Center - Main.screenPosition;
			Vector2 dir  = Projectile.rotation.ToRotationVector2();
			Vector2 perp = new(-dir.Y, dir.X);

			// 椭圆朝向：短轴（较小的那一头）朝着光标/射击方向，长轴（较大的一侧）垂直于射击方向——
			// 即枪口正面看到的是一个"横向摊开"的椭圆截面，而不是沿枪管方向拉长的水滴形。
			float alongHalf  = MathHelper.Lerp(1.2f, 4.5f, grow); // 沿 dir（朝光标）的半轴：短
			float acrossHalf = MathHelper.Lerp(1.6f, 9f, grow);   // 沿 perp（垂直于 dir）的半轴：长

			const int segments = 24;
			var verts = new List<VertexPositionColor>(segments * 6 * 4 + 16 * 6);

			// 颜色：椭圆完全展开（age ≥ GrowTicks）之前绝大部分时候都保持白色，只有展开之后才
			// 快速（ColorTransitionTicks）过渡成两段固定色——内圆（中心→60%半径）淡紫色，
			// 外圈（60%→100%半径）红紫色；过渡完成后保持不变直到淡出。
			// 整体乘 MaxBodyAlpha 封顶不透明度，避免看起来像一坨实心色块。
			// 边界是纯粹的几何硬边缘（不画超出椭圆范围的像素），不做透明度渐隐。
			float colorT = MathHelper.Clamp((age - GrowTicks) / (float)ColorTransitionTicks, 0f, 1f);
			Color innerCol = Color.Lerp(WhiteCol, InnerTargetCol, colorT) * (fade * MaxBodyAlpha);
			Color outerCol = Color.Lerp(WhiteCol, OuterTargetCol, colorT) * (fade * MaxBodyAlpha);

			const float midFrac = 0.60f; // 内圆（淡紫）与外圈（红紫）的分界半径

			Vector2 center = pos + dir * (alongHalf * 0.4f); // 椭圆略朝发射方向前移，衔接弹幕出膛点
			for (int i = 0; i < segments; i++) {
				float a0 = MathHelper.TwoPi * i / segments;
				float a1 = MathHelper.TwoPi * (i + 1) / segments;
				float c0 = System.MathF.Cos(a0), s0 = System.MathF.Sin(a0);
				float c1 = System.MathF.Cos(a1), s1 = System.MathF.Sin(a1);

				Vector2 mid0 = center + dir * (c0 * alongHalf * midFrac) + perp * (s0 * acrossHalf * midFrac);
				Vector2 mid1 = center + dir * (c1 * alongHalf * midFrac) + perp * (s1 * acrossHalf * midFrac);
				Vector2 out0 = center + dir * (c0 * alongHalf) + perp * (s0 * acrossHalf);
				Vector2 out1 = center + dir * (c1 * alongHalf) + perp * (s1 * acrossHalf);

				// 内圆扇形：淡紫色
				verts.Add(new VertexPositionColor(new Vector3(center, 0f), innerCol));
				verts.Add(new VertexPositionColor(new Vector3(mid0, 0f), innerCol));
				verts.Add(new VertexPositionColor(new Vector3(mid1, 0f), innerCol));

				// 外圈环带：红紫色，直接到椭圆几何边界为止（硬边缘）
				verts.Add(new VertexPositionColor(new Vector3(mid0, 0f), outerCol));
				verts.Add(new VertexPositionColor(new Vector3(out0, 0f), outerCol));
				verts.Add(new VertexPositionColor(new Vector3(out1, 0f), outerCol));
				verts.Add(new VertexPositionColor(new Vector3(mid0, 0f), outerCol));
				verts.Add(new VertexPositionColor(new Vector3(out1, 0f), outerCol));
				verts.Add(new VertexPositionColor(new Vector3(mid1, 0f), outerCol));
			}

			// 椭圆边缘的细蓝色描边 + 外发光：这两层用"固定像素外扩量"而不是按比例缩放的半径——
			// 椭圆本体本来就很小（半轴最大只有 6~12px），若外扩量按比例算（比如 ×1.05），
			// 换算下来只有一两个像素宽，几乎看不见。改成固定像素外扩后，无论椭圆本身多大，
			// 描边和外发光的实际粗细/延伸距离都是恒定的，稳定可见。
			const float outlineThickness = 0.8f;  // 蓝色描边厚度（像素，进一步变细）
			const float glowExtra        = 13.0f; // 外发光在描边基础上再向外延伸的距离（像素）
			const byte  glowInnerAlpha   = 150;    // 外发光内侧不透明度：固定值，不随角度变化

			Color outlineCol = OutlineCol * fade;
			Color glowOuterCol = GlowPurpleCol * fade; glowOuterCol.A = 0; // 外侧渐隐透明

			for (int i = 0; i < segments; i++) {
				float a0 = MathHelper.TwoPi * i / segments;
				float a1 = MathHelper.TwoPi * (i + 1) / segments;
				float c0 = System.MathF.Cos(a0), s0 = System.MathF.Sin(a0);
				float c1 = System.MathF.Cos(a1), s1 = System.MathF.Sin(a1);

				Vector2 EllipsePt(float c, float s, float extraAlong, float extraAcross) =>
					center + dir * (c * (alongHalf + extraAlong)) + perp * (s * (acrossHalf + extraAcross));

				Vector2 edge0 = EllipsePt(c0, s0, 0f, 0f);
				Vector2 edge1 = EllipsePt(c1, s1, 0f, 0f);
				Vector2 out0  = EllipsePt(c0, s0, outlineThickness, outlineThickness);
				Vector2 out1  = EllipsePt(c1, s1, outlineThickness, outlineThickness);

				// 细蓝色描边：紧贴几何边界外侧，硬边缘、不做透明度渐隐
				verts.Add(new VertexPositionColor(new Vector3(edge0, 0f), outlineCol));
				verts.Add(new VertexPositionColor(new Vector3(out0, 0f), outlineCol));
				verts.Add(new VertexPositionColor(new Vector3(out1, 0f), outlineCol));
				verts.Add(new VertexPositionColor(new Vector3(edge0, 0f), outlineCol));
				verts.Add(new VertexPositionColor(new Vector3(out1, 0f), outlineCol));
				verts.Add(new VertexPositionColor(new Vector3(edge1, 0f), outlineCol));

				// 短距离外发光：蓝紫交织。之前用"随角度+时间变化的正弦"同时调制了颜色和透明度，
				// 内侧不透明度被压低的地方叠加向外淡出，看起来就像一颗颗小球绕着椭圆转——
				// 现在改成只调制色相（蓝↔紫），不透明度固定为 glowInnerAlpha，且不含时间项
				// （椭圆朝向本来就固定不转，静态的蓝紫交织纹理更自然，也不会有旋转错觉）。
				float weave0 = System.MathF.Sin(a0 * 3f) * 0.5f + 0.5f;
				float weave1 = System.MathF.Sin(a1 * 3f) * 0.5f + 0.5f;
				Color hue0 = Color.Lerp(GlowBlueCol, GlowPurpleCol, weave0 * 0.5f) * fade; hue0.A = (byte)(glowInnerAlpha * fade);
				Color hue1 = Color.Lerp(GlowBlueCol, GlowPurpleCol, weave1 * 0.5f) * fade; hue1.A = (byte)(glowInnerAlpha * fade);

				Vector2 gOut0 = EllipsePt(c0, s0, outlineThickness + glowExtra, outlineThickness + glowExtra);
				Vector2 gOut1 = EllipsePt(c1, s1, outlineThickness + glowExtra, outlineThickness + glowExtra);

				verts.Add(new VertexPositionColor(new Vector3(out0, 0f), hue0));
				verts.Add(new VertexPositionColor(new Vector3(gOut0, 0f), glowOuterCol));
				verts.Add(new VertexPositionColor(new Vector3(gOut1, 0f), glowOuterCol));
				verts.Add(new VertexPositionColor(new Vector3(out0, 0f), hue0));
				verts.Add(new VertexPositionColor(new Vector3(gOut1, 0f), glowOuterCol));
				verts.Add(new VertexPositionColor(new Vector3(out1, 0f), hue1));
			}

			// 外援环绕线：与椭圆边缘保持一定间距的蓝色圆弧，随时间不断绕椭圆转动，营造扫描/充能感。
			// 用两条同心弧线（内窄外宽，两端各自向透明淡出）画出一条更粗的短弧。
			// 角度用 _globalRingAngle（跨实例持续累加，见字段注释），不会在每次开火重新生成时归零。
			// 弧线长度（ringSpan）在移动的同时也随时间平滑地忽长忽短——叠加两组不同频率、
			// 不成整数倍关系的正弦波，避免出现规律性的周期重复，看起来更接近"随机"波动。
			const float ringSpanMin = 1.1f;
			const float ringSpanMax = 2.3f;
			float spanNoise = System.MathF.Sin(_globalRingAngle * 0.37f) * 0.6f
			                + System.MathF.Sin(_globalRingAngle * 0.91f + 1.7f) * 0.4f;
			spanNoise = spanNoise * 0.5f + 0.5f; // 归一化到 0~1
			float ringSpan = MathHelper.Lerp(ringSpanMin, ringSpanMax, spanNoise);
			const float ringDist   = 1.60f;  // 距椭圆边界的外扩比例（留出明显间距）
			const float ringInner  = 0.86f;  // 弧线厚度：内边界相对 ringDist 的比例（更粗）
			const int   ringArcSeg = 16;
			float ringAngle = _globalRingAngle;
			Color ringColOpaque = RingCol * fade;
			Color ringColClear  = RingCol * fade; ringColClear.A = 0;
			for (int i = 0; i < ringArcSeg; i++) {
				float u0 = i / (float)ringArcSeg;
				float u1 = (i + 1) / (float)ringArcSeg;
				float a0 = ringAngle - ringSpan * 0.5f + ringSpan * u0;
				float a1 = ringAngle - ringSpan * 0.5f + ringSpan * u1;

				// 弧线两端做柔和淡出（首尾 20% 范围内衰减到透明），中段保持不透明
				float edgeFade0 = MathHelper.Clamp(System.MathF.Min(u0, 1f - u0) / 0.2f, 0f, 1f);
				float edgeFade1 = MathHelper.Clamp(System.MathF.Min(u1, 1f - u1) / 0.2f, 0f, 1f);
				Color col0 = Color.Lerp(ringColClear, ringColOpaque, edgeFade0);
				Color col1 = Color.Lerp(ringColClear, ringColOpaque, edgeFade1);

				float c0 = System.MathF.Cos(a0), s0 = System.MathF.Sin(a0);
				float c1 = System.MathF.Cos(a1), s1 = System.MathF.Sin(a1);

				Vector2 rIn0  = center + dir * (c0 * alongHalf * ringDist * ringInner) + perp * (s0 * acrossHalf * ringDist * ringInner);
				Vector2 rIn1  = center + dir * (c1 * alongHalf * ringDist * ringInner) + perp * (s1 * acrossHalf * ringDist * ringInner);
				Vector2 rOut0 = center + dir * (c0 * alongHalf * ringDist) + perp * (s0 * acrossHalf * ringDist);
				Vector2 rOut1 = center + dir * (c1 * alongHalf * ringDist) + perp * (s1 * acrossHalf * ringDist);

				verts.Add(new VertexPositionColor(new Vector3(rIn0, 0f), col0));
				verts.Add(new VertexPositionColor(new Vector3(rOut0, 0f), col0));
				verts.Add(new VertexPositionColor(new Vector3(rOut1, 0f), col1));
				verts.Add(new VertexPositionColor(new Vector3(rIn0, 0f), col0));
				verts.Add(new VertexPositionColor(new Vector3(rOut1, 0f), col1));
				verts.Add(new VertexPositionColor(new Vector3(rIn1, 0f), col1));
			}

			GraphicsDevice device = Main.instance.GraphicsDevice;
			Main.spriteBatch.End();

			device.BlendState        = BlendState.NonPremultiplied;
			device.RasterizerState   = RasterizerState.CullNone;
			device.DepthStencilState = DepthStencilState.None;

			// 必须叠加 GameViewMatrix（游戏内缩放）再接正交投影，否则缩放不为默认值时会错位/不可见
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
