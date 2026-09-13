using System;
using System.Collections.Generic;
using ArknightsMod.Players;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Projectiles.Medic.Closure
{
	// 可露希尔·二/三技能视觉特效（纯代码绘制，无贴图）：
	//   在玩家身后绘制巨大的方形特效 + 蓝色焰气飘逸效果。
	//   · 方形：填充从上到下 30% 不透明度的红→蓝渐变；描边只保留四个角（半红半白随机纹理）。
	//   · 点阵：方形中心有规律的点阵图案（点用加深不透明度的对应背景渐变颜色），
	//           不断向上移动淡出、下方同时向上移动淡入新的点阵。
	//   · 四角：四个角的图案不断向外扩大淡出，同时新的四个角由小到大淡入出现。
	//   · 焰气：玩家身后蓝色不透明度随机的焰气飘逸效果，用代码绘制（非粒子）。
	public class ClosureSkillSquareEffect : ModProjectile
	{
		private static readonly Color TopColor    = new(255, 80, 100);   // 顶部：偏红
		private static readonly Color BottomColor = new(90, 150, 255);   // 底部：蓝色
		private static readonly Color FlameBlue   = new(140, 200, 255);  // 焰气：蓝色

		private static BasicEffect _basic;

		private int _age;
		private readonly List<FlameWisp> _wisps = new();

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
			Projectile.netImportant = true;
			Projectile.timeLeft    = 99999; // 由二/三技能开启状态手动管理
		}

		/// <summary>若该玩家当前没有方形特效弹幕，则生成一个（避免重复）。</summary>
		public static void EnsureFor(Player player) {
			if (Main.netMode == Terraria.ID.NetmodeID.MultiplayerClient && player.whoAmI != Main.myPlayer)
				return;
			int type = ModContent.ProjectileType<ClosureSkillSquareEffect>();
			for (int i = 0; i < Main.maxProjectiles; i++) {
				Projectile p = Main.projectile[i];
				if (p.active && p.type == type && p.owner == player.whoAmI)
					return;
			}
			Projectile.NewProjectile(player.GetSource_Misc("ClosureSkillSquare"), player.Center, Vector2.Zero,
				type, 0, 0f, player.whoAmI);
		}

		private bool OwnerSkillActive {
			get {
				Player owner = Main.player[Projectile.owner];
				if (owner == null || !owner.active || owner.dead)
					return false;
				var mp = owner.GetModPlayer<WeaponPlayer>();
				return (mp.Skill == 1 || mp.Skill == 2) && mp.SkillActive; // 二或三技能开启期间
			}
		}

		public override void AI() {
			Projectile.velocity = Vector2.Zero;
			_age++;

			Player owner = Main.player[Projectile.owner];
			if (owner != null && owner.active)
				Projectile.Center = owner.Center;

			if (!OwnerSkillActive) {
				Projectile.Kill();
				return;
			}

			// 每 2~5 帧生成一个新的焰气线条
			if (Main.rand.NextBool(3))
				SpawnWisp(owner);

			// 更新焰气
			for (int i = _wisps.Count - 1; i >= 0; i--) {
				_wisps[i].Update();
				if (_wisps[i].Dead)
					_wisps.RemoveAt(i);
			}
		}

		private void SpawnWisp(Player owner) {
			if (owner == null)
				return;
			// 从玩家身后偏下方生成，向斜后上方飘逸（不受玩家朝向影响，统一向后）
			Vector2 start = owner.Center + new Vector2(0f, Main.rand.Next(5, 25));
			_wisps.Add(new FlameWisp(start));
		}

		private class FlameWisp {
			public Vector2 Pos;
			public Vector2 Vel;
			public float Life;
			public float MaxLife;
			public float Width;
			public float Alpha;
			public bool Dead => Life <= 0f;

			public FlameWisp(Vector2 start) {
				Pos = start;
				// 向斜后上方飘逸（x 随机左右，y 向上），模拟气焰飘散
				Vel = new Vector2(Main.rand.NextFloat(-1.5f, 1.5f), Main.rand.NextFloat(-3.0f, -1.5f));
				MaxLife = Main.rand.Next(50, 100);
				Life = MaxLife;
				Width = Main.rand.NextFloat(4f, 10f);
				Alpha = Main.rand.NextFloat(0.3f, 0.7f);
			}

			public void Update() {
				Life--;
				Pos += Vel;
				Vel.X *= 0.96f;
				Vel.Y *= 0.94f;
				// 越向上越向两侧扩散（模拟气焰自然飘散）
				Vel.X += Main.rand.NextFloat(-0.1f, 0.1f);
			}

			public float FadeAlpha => MathHelper.Clamp(Life / MaxLife, 0f, 1f) * Alpha;
		}

		private static void AddTri(List<VertexPositionColor> v, Vector2 a, Vector2 b, Vector2 c,
			Color ca, Color cb, Color cc) {
			v.Add(new VertexPositionColor(new Vector3(a, 0f), ca));
			v.Add(new VertexPositionColor(new Vector3(b, 0f), cb));
			v.Add(new VertexPositionColor(new Vector3(c, 0f), cc));
		}

		// 画巨大方形特效：填充渐变 + 四角描边 + 点阵图案
		private void AddSquare(List<VertexPositionColor> verts, Vector2 center, float size, float alpha) {
			float half = size * 0.5f;
			Vector2 tl = center + new Vector2(-half, -half);
			Vector2 tr = center + new Vector2( half, -half);
			Vector2 br = center + new Vector2( half,  half);
			Vector2 bl = center + new Vector2(-half,  half);

			// 1. 填充：从上到下 红→蓝渐变，30% 不透明度
			Color topCol    = TopColor * (alpha * 0.30f);
			Color bottomCol = BottomColor * (alpha * 0.30f);
			AddTri(verts, tl, tr, br, topCol, topCol, bottomCol);
			AddTri(verts, tl, br, bl, topCol, bottomCol, bottomCol);

			// 2. 四角描边：每个角只保留一小段 L 形描边（半红半白随机纹理颜色）
			float cornerLen = size * 0.15f;
			float cornerThick = 2f;
			AddCornerBorder(verts, tl, cornerLen, cornerThick, alpha, true,  true);  // 左上
			AddCornerBorder(verts, tr, cornerLen, cornerThick, alpha, false, true);  // 右上
			AddCornerBorder(verts, br, cornerLen, cornerThick, alpha, false, false); // 右下
			AddCornerBorder(verts, bl, cornerLen, cornerThick, alpha, true,  false); // 左下

			// 3. 点阵图案：方形中心有规律的点阵，不断向上移动淡出、下方同时向上移动淡入新的点阵
			float dotSpacing = 18f;
			float dotSize = 3f;
			float scrollOffset = (_age * 1.2f) % dotSpacing; // 向上滚动
			for (float x = -half + dotSpacing; x < half; x += dotSpacing) {
				for (float y = -half + dotSpacing; y < half + dotSpacing; y += dotSpacing) {
					float yy = y - scrollOffset;
					if (yy < -half || yy > half)
						continue;
					// 点的颜色：根据 y 位置在渐变背景色中插值，然后加深不透明度
					float t = (yy + half) / size; // 0(top)→1(bottom)
					Color dotCol = Color.Lerp(TopColor, BottomColor, t) * (alpha * 0.7f);
					// 上下两端淡入淡出
					float fadeY = 1f - Math.Abs(yy) / half;
					fadeY = MathHelper.Clamp(fadeY * 2f, 0f, 1f);
					dotCol *= fadeY;
					Vector2 dotPos = center + new Vector2(x, yy);
					AddDot(verts, dotPos, dotSize, dotCol);
				}
			}

			// 4. 四角扩散动画：四个角不断向外扩大淡出，同时新的四个角由小到大淡入出现
			float cornerAnimCycle = 60f;
			float cornerPhase = (_age % cornerAnimCycle) / cornerAnimCycle;
			float cornerScale = MathHelper.Lerp(1f, 2.4f, cornerPhase);
			float cornerAlpha = (1f - cornerPhase) * alpha;
			AddCornerGlow(verts, tl, cornerLen * cornerScale, cornerAlpha, true,  true);
			AddCornerGlow(verts, tr, cornerLen * cornerScale, cornerAlpha, false, true);
			AddCornerGlow(verts, br, cornerLen * cornerScale, cornerAlpha, false, false);
			AddCornerGlow(verts, bl, cornerLen * cornerScale, cornerAlpha, true,  false);
		}

		// 四角描边：L 形连续渐变（红→白→红，柔和过渡，不是生硬的像素块）
		private static void AddCornerBorder(List<VertexPositionColor> verts, Vector2 corner, float len, float thick,
			float alpha, bool left, bool top) {
			int hSign = left ? 1 : -1;
			int vSign = top ? 1 : -1;
			int segs = (int)(len / 3f); // 分段数，确保连续
			// 横向描边：渐变色从红→白→红
			for (int i = 0; i < segs; i++) {
				float t = i / (float)segs;
				float d = t * len;
				// 正弦波控制渐变：0→1→0，红→白→红
				float colorT = (float)Math.Sin(t * MathHelper.Pi);
				Color col = Color.Lerp(new Color(255, 80, 100), Color.White, colorT) * (alpha * 0.85f);
				Vector2 p = corner + new Vector2(hSign * d, 0f);
				AddDot(verts, p, thick, col);
			}
			// 纵向描边：同样渐变
			for (int i = 0; i < segs; i++) {
				float t = i / (float)segs;
				float d = t * len;
				float colorT = (float)Math.Sin(t * MathHelper.Pi);
				Color col = Color.Lerp(new Color(255, 80, 100), Color.White, colorT) * (alpha * 0.85f);
				Vector2 p = corner + new Vector2(0f, vSign * d);
				AddDot(verts, p, thick, col);
			}
		}

		// 四角扩散发光
		private static void AddCornerGlow(List<VertexPositionColor> verts, Vector2 corner, float len, float alpha,
			bool left, bool top) {
			int hSign = left ? 1 : -1;
			int vSign = top ? 1 : -1;
			Color glowCol = Color.Lerp(Color.White, Color.Cyan, 0.5f) * alpha;
			Color clear = glowCol; clear.A = 0;
			// 横向发光
			Vector2 h0 = corner;
			Vector2 h1 = corner + new Vector2(hSign * len, 0f);
			AddTri(verts, h0, h1, h1 + new Vector2(0f, vSign * 4f), glowCol, clear, clear);
			// 纵向发光
			Vector2 v0 = corner;
			Vector2 v1 = corner + new Vector2(0f, vSign * len);
			AddTri(verts, v0, v1, v1 + new Vector2(hSign * 4f, 0f), glowCol, clear, clear);
		}

		// 画一个小正方形点
		private static void AddDot(List<VertexPositionColor> verts, Vector2 pos, float size, Color col) {
			float h = size * 0.5f;
			Vector2 tl = pos + new Vector2(-h, -h);
			Vector2 tr = pos + new Vector2( h, -h);
			Vector2 br = pos + new Vector2( h,  h);
			Vector2 bl = pos + new Vector2(-h,  h);
			AddTri(verts, tl, tr, br, col, col, col);
			AddTri(verts, tl, br, bl, col, col, col);
		}

		// 画一条焰气线条（用柔和的椭圆形 + 径向渐变透明，模拟气焰飘散的柔和边缘）
		private static void AddFlameWisp(List<VertexPositionColor> verts, FlameWisp wisp) {
			float alpha = wisp.FadeAlpha;
			if (alpha <= 0.01f)
				return;
			float w = wisp.Width;
			float h = w * 3.5f; // 高度比宽度长
			Color col = FlameBlue * alpha;
			Color clear = col; clear.A = 0;

			// 用多个三角形扇形模拟椭圆，从中心向边缘径向渐变透明
			const int seg = 12;
			Vector2 center = wisp.Pos;
			for (int i = 0; i < seg; i++) {
				float a0 = MathHelper.TwoPi * i / seg;
				float a1 = MathHelper.TwoPi * (i + 1) / seg;
				// 椭圆：x 轴用 w，y 轴用 h
				Vector2 p0 = center + new Vector2((float)Math.Cos(a0) * w, (float)Math.Sin(a0) * h);
				Vector2 p1 = center + new Vector2((float)Math.Cos(a1) * w, (float)Math.Sin(a1) * h);
				// 中心有色，边缘透明
				AddTri(verts, center, p0, p1, col, clear, clear);
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

			Player owner = Main.player[Projectile.owner];
			if (owner == null || !owner.active)
				return false;

			float alpha = 0.85f;
			var verts = new List<VertexPositionColor>(1024);

			// 方形固定在玩家正身后（不受朝向影响），尺寸缩小，玩家位置在方形中间偏下
			Vector2 squarePos = owner.Center - Main.screenPosition + new Vector2(0f, -20f); // 玩家位置在方形中间偏下
			float squareSize = 100f; // 从 140 缩小到 100
			AddSquare(verts, squarePos, squareSize, alpha);

			// 焰气在玩家身后飘逸
			foreach (var wisp in _wisps) {
				Vector2 wispScreenPos = wisp.Pos - Main.screenPosition;
				AddFlameWisp(verts, new FlameWisp(wispScreenPos) {
					Life = wisp.Life, MaxLife = wisp.MaxLife, Width = wisp.Width, Alpha = wisp.Alpha
				});
			}

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
