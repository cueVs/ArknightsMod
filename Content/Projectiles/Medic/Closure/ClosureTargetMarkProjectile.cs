using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Projectiles.Medic.Closure
{
	// 可露希尔·二技能「模型扩展」目标标记（纯代码绘制，三角图元）：
	//   悬浮在被锁定目标固定左上角、朝向目标呈立体透视效果的等边三角形图案——
	//   1. 中心一个小的白色三角形细线 + 外侧一道较大的三角形细线；
	//   2. 出现时淡入，同时有一个更大的三角形由大到小淡入收缩，最终停在外侧细线再外一点的位置；
	//   3. 每道三角形细线都带红紫色外发光；
	//   4. 图案中心有一大块红紫色→紫色渐变、半透明、边缘模糊的圆形打底；
	//   5. 中心叠加一枚扁平化的"中子星"：左右更长、上下更窄，独立叠加红色外发光，
	//      渲染顺序在三角图案之上；
	//   6. 攻击期间不断向四周飞出大小不一的白色/红紫色小三角形（均带红紫发光边缘）；
	//   7. 消失（离场）时，三条三角形的线条不是整体淡出，而是沿线段碎成一颗颗颜色相近的
	//      小三角形向外飘散淡出；打底圆/中子星/飞出粒子仍是普通淡出，不受影响。
	// ai[0] = 锁定目标的 NPC 下标。攻击（Volley）会续命；停火或目标失效后淡出销毁。
	public class ClosureTargetMarkProjectile : ModProjectile
	{
		private const int KeepAliveTicks = 55;  // 每次攻击续命时长，需大于武器攻击间隔
		private const int FadeInTicks    = 10;
		private const int ShrinkDelayTicks = 8;  // 第三三角延迟出现的时长（不与前两道三角同时开始）
		private const int ShrinkTicks    = 22;  // 第三三角自身由大到小的收缩动画时长（延长）
		private const int FadeOutTicks   = 12;

		// 图案整体朝向角：按参考图校准的固定角度，非逐帧动态瞄准目标中心——
		// 目标中心方向随目标碰撞箱大小浮动较大，直接瞄准会显得"转过头"而不像参考图那种小幅倾斜）。
		private const float FaceAngle  = -1.31f; // 在此基础上再逆时针转一点
		private const float FaceSquash = 0.68f;  // 立体透视压缩系数：垂直于朝向的分量按此收窄，
		                                          // 数值加大=两侧收窄得更少，显示更宽
		private const float InnerR    = 9f;     // 第一三角（内侧，最小）
		private const float OuterR    = 15f;    // 第二三角（外侧细线）
		private const float SettledR  = 21f;    // 第三三角最终停留半径（与第二三角留一点间距）
		private const float ShrinkFromR = 48f;  // 第三三角起始半径
		private const float BackdropR = 30f;    // 中心打底半径

		private static readonly Color White     = new(255, 255, 255);
		private static readonly Color RedPurple = new(215, 45, 130);
		private static readonly Color Purple    = new(150, 60, 210);
		private static readonly Color GlowCol   = new(170, 40, 160);
		private static readonly Color NeutronGlowCol = new(235, 60, 60); // 中子星独立的红色外发光

		private static BasicEffect _basic;

		private int _age;
		private int _keepAlive = KeepAliveTicks;
		private bool _fadingOut;
		private int _fadeAge;

		private struct FlyTri {
			public Vector2 Offset;   // 相对图案中心
			public Vector2 Vel;
			public float Size;
			public float Rot;
			public float RotVel;
			public int Life;
			public int MaxLife;
			public bool IsWhite;
		}
		private readonly List<FlyTri> _flyTris = new();

		// 三角线条"离场"动画用的碎片：淡出开始时把三条三角形的每条边切成若干小段，
		// 每段各自化作一个与该段颜色相近的小三角形，随后向外飘散、随主体一起淡出。
		private struct ExitShard {
			public Vector2 Offset;
			public Vector2 Vel;
			public float Size;
			public float Rot;
			public float RotVel;
			public Color Col;
		}
		private readonly List<ExitShard> _exitShards = new();
		private bool _shardsBuilt;

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
			Projectile.timeLeft    = 99999; // 生命周期由 _keepAlive/_fadingOut 手动管理
		}

		private NPC TargetNPC {
			get {
				int idx = (int)Projectile.ai[0];
				if (idx < 0 || idx >= Main.maxNPCs)
					return null;
				NPC npc = Main.npc[idx];
				return npc.active && npc.CanBeChasedBy() ? npc : null;
			}
		}

		public override void OnSpawn(IEntitySource source) {
			NPC target = TargetNPC;
			if (target != null)
				Projectile.Center = MarkerPos(target);
		}

		private static Vector2 MarkerPos(NPC target) =>
			new(target.Hitbox.Left - 30f, target.Hitbox.Top - 36f); // 固定左上角

		private int _laserWhoAmI = -1; // 追踪本标记持续存在的激光实例，避免每次攻击都重新生成一条新的

		/// <summary>武器每次攻击时调用：续命 + 续命/生成激光 + 喷出一批小三角形。
		/// 激光本身在攻击持续期间是同一个实例持续显示（每次调用只续命+重播"射出"过渡动画），
		/// 而不是每次攻击都重新生成一条独立的新激光。</summary>
		public void Volley(int damage, float knockback) {
			_keepAlive = KeepAliveTicks;
			_fadingOut = false;
			_fadeAge = 0;
			if (_shardsBuilt) {
				_shardsBuilt = false;
				_exitShards.Clear();
			}

			int idx = (int)Projectile.ai[0];
			if (Projectile.owner == Main.myPlayer && TargetNPC != null) {
				bool hasLaser = _laserWhoAmI >= 0 && _laserWhoAmI < Main.maxProjectiles
					&& Main.projectile[_laserWhoAmI].active
					&& Main.projectile[_laserWhoAmI].type == ModContent.ProjectileType<ClosureMarkLaserProjectile>();

				if (hasLaser) {
					(Main.projectile[_laserWhoAmI].ModProjectile as ClosureMarkLaserProjectile)?.Refresh(damage, knockback, idx);
				}
				else {
					_laserWhoAmI = Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero,
						ModContent.ProjectileType<ClosureMarkLaserProjectile>(), damage, knockback, Projectile.owner, idx);
				}
			}

			SpawnFlyTri();
		}

		private void SpawnFlyTri() {
			float ang = Main.rand.NextFloat(MathHelper.TwoPi);
			_flyTris.Add(new FlyTri {
				Offset  = Vector2.Zero,
				Vel     = ang.ToRotationVector2() * Main.rand.NextFloat(1.0f, 3.2f),
				Size    = Main.rand.NextFloat(3f, 8f),
				Rot     = Main.rand.NextFloat(MathHelper.TwoPi),
				RotVel  = Main.rand.NextFloat(-0.2f, 0.2f),
				Life    = 0,
				MaxLife = Main.rand.Next(18, 32),
				IsWhite = Main.rand.NextBool(),
			});
		}

		public override void AI() {
			Projectile.velocity = Vector2.Zero;
			_age++;

			NPC target = TargetNPC;
			if (target != null && !_fadingOut)
				Projectile.Center = MarkerPos(target);

			// 攻击间隙持续少量喷出小三角形，维持"不断飞出"的观感（数量已减少）
			if (!_fadingOut && _keepAlive > 0 && Main.rand.NextBool(20))
				SpawnFlyTri();

			// 粒子更新
			for (int i = _flyTris.Count - 1; i >= 0; i--) {
				FlyTri t = _flyTris[i];
				t.Offset += t.Vel;
				t.Rot += t.RotVel;
				t.Life++;
				if (t.Life >= t.MaxLife) {
					_flyTris.RemoveAt(i);
					continue;
				}
				_flyTris[i] = t;
			}

			if (target == null)
				_fadingOut = true;

			if (_keepAlive > 0)
				_keepAlive--;
			else
				_fadingOut = true;

			if (_fadingOut) {
				if (!_shardsBuilt) {
					BuildExitShards();
					_shardsBuilt = true;
				}
				for (int i = 0; i < _exitShards.Count; i++) {
					ExitShard s = _exitShards[i];
					s.Offset += s.Vel;
					s.Rot += s.RotVel;
					_exitShards[i] = s;
				}

				_fadeAge++;
				if (_fadeAge > FadeOutTicks)
					Projectile.Kill();
			}

			Lighting.AddLight(Projectile.Center, 0.45f, 0.12f, 0.4f);
		}

		// 淡出开始时调用一次：把三条三角形（第一/第二/第三三角）的每条边切成若干小段，
		// 每段各自生成一个颜色相近的碎片三角形，随后向外飘散、随主体一起淡出。
		private void BuildExitShards() {
			_exitShards.Clear();
			int veinSeed = (int)Projectile.ai[0];
			BuildShardsForEdges(SettledR, RedPurple, -1);
			BuildShardsForEdges(OuterR, White, veinSeed * 2 + 1);
			BuildShardsForEdges(InnerR, White, veinSeed * 2);
		}

		private void BuildShardsForEdges(float r, Color baseCol, int seed) {
			Span<Vector2> p = stackalloc Vector2[3];
			TriCorners(Vector2.Zero, r, p);

			const int segPerEdge = 5;
			for (int k = 0; k < 3; k++) {
				Vector2 a = p[k], b = p[(k + 1) % 3];
				for (int s = 0; s < segPerEdge; s++) {
					float t0 = s / (float)segPerEdge;
					float t1 = (s + 1) / (float)segPerEdge;
					Vector2 mid = Vector2.Lerp(a, b, (t0 + t1) * 0.5f);

					Color col = baseCol;
					if (seed >= 0) {
						float n = Hash01(seed, k * 31 + s, 17);
						col = n < 0.42f ? White : RedPurple;
					}

					Vector2 outward = mid.LengthSquared() > 0.01f
						? Vector2.Normalize(mid)
						: Main.rand.NextFloat(MathHelper.TwoPi).ToRotationVector2();

					_exitShards.Add(new ExitShard {
						Offset  = mid,
						Vel     = outward * Main.rand.NextFloat(0.5f, 1.6f) + Main.rand.NextVector2Circular(0.3f, 0.3f),
						Size    = Main.rand.NextFloat(2.0f, 3.8f),
						Rot     = Main.rand.NextFloat(MathHelper.TwoPi),
						RotVel  = Main.rand.NextFloat(-0.25f, 0.25f),
						Col     = col,
					});
				}
			}
		}

		private static void AddTri(List<VertexPositionColor> v, Vector2 a, Vector2 b, Vector2 c, Color col) {
			v.Add(new VertexPositionColor(new Vector3(a, 0f), col));
			v.Add(new VertexPositionColor(new Vector3(b, 0f), col));
			v.Add(new VertexPositionColor(new Vector3(c, 0f), col));
		}

		private static void AddThickLine(List<VertexPositionColor> v, Vector2 a, Vector2 b, float thickness, Color col) {
			Vector2 d = b - a;
			if (d.LengthSquared() < 0.0001f)
				return;
			d.Normalize();
			Vector2 n = new Vector2(-d.Y, d.X) * (thickness * 0.5f);
			AddTri(v, a + n, a - n, b + n, col);
			AddTri(v, a - n, b - n, b + n, col);
		}

		// 等边三角形三个顶点：以固定的 FaceAngle 为局部前轴（forward），0 号顶点沿该方向，
		// 垂直于该方向的分量（right）按 FaceSquash 收窄——不是把整块图案在平面内刚性旋转，
		// 而是让它看起来像一块在 3D 空间中侧转过去的立体图案，又被投影回二维平面。
		private static void TriCorners(Vector2 center, float r, Span<Vector2> pts) {
			Vector2 forward = FaceAngle.ToRotationVector2();
			Vector2 right = new(-forward.Y, forward.X);
			for (int k = 0; k < 3; k++) {
				float ang = k * MathHelper.TwoPi / 3f;
				Vector2 local = ang.ToRotationVector2(); // (cos, sin)：x 沿 forward，y 沿 right
				pts[k] = center + forward * (local.X * r) + right * (local.Y * r * FaceSquash);
			}
		}

		// 带红紫外发光的三角形描线：先画厚而淡的发光层，再画细的本色线
		private static void AddGlowTriOutline(List<VertexPositionColor> v, Vector2 center, float r,
			float thickness, Color lineCol, float glowAlphaMul) {
			Span<Vector2> p = stackalloc Vector2[3];
			TriCorners(center, r, p);

			Color glow = GlowCol; glow.A = 0;
			Color glowMid = GlowCol * glowAlphaMul;
			for (int k = 0; k < 3; k++) {
				Vector2 a = p[k], b = p[(k + 1) % 3];
				AddThickLine(v, a, b, thickness * 3.4f, Color.Lerp(glow, glowMid, 0.5f));
			}
			for (int k = 0; k < 3; k++) {
				Vector2 a = p[k], b = p[(k + 1) % 3];
				AddThickLine(v, a, b, thickness, lineCol);
			}
		}

		// 简单确定性哈希，返回 0~1：给"第一/第二三角"生成固定不变的不规则红紫纹路
		// （同一个 seed 每次算出来的图案都一样，不会每帧闪烁重新随机）。
		private static float Hash01(int a, int b, int c) {
			unchecked {
				int h = a * 374761393 + b * 668265263 + c * unchecked((int)2246822519);
				h = (h ^ (h >> 13)) * 1274126177;
				h ^= h >> 16;
				return (h & 0x7fffffff) / (float)int.MaxValue;
			}
		}

		// 带红紫外发光的三角形描线，主体为白色但沿线段叠加不规则的红紫色纹路斑块
		// （第一三角、第二三角用这个，而不是纯色实线）。seed 保证纹路图案固定不变。
		private static void AddVeinedTriOutline(List<VertexPositionColor> v, Vector2 center, float r,
			float thickness, float fade, float glowAlphaMul, int seed) {
			Span<Vector2> p = stackalloc Vector2[3];
			TriCorners(center, r, p);

			// 外层红紫发光（同 AddGlowTriOutline）
			Color glow = GlowCol; glow.A = 0;
			Color glowMid = GlowCol * glowAlphaMul;
			for (int k = 0; k < 3; k++) {
				Vector2 a = p[k], b = p[(k + 1) % 3];
				AddThickLine(v, a, b, thickness * 3.4f, Color.Lerp(glow, glowMid, 0.5f));
			}

			// 白色基础细线
			Color whiteCol = White * fade;
			for (int k = 0; k < 3; k++) {
				Vector2 a = p[k], b = p[(k + 1) % 3];
				AddThickLine(v, a, b, thickness, whiteCol);
			}

			// 不规则红紫纹路：每条边细分成若干段，用哈希决定哪些段落显现红紫色斑块、
			// 以及斑块的浓淡，段与段之间浓度不同、长短不一，形成"纹路"而非规则条纹。
			const int segPerEdge = 7;
			Color veinCol = RedPurple * fade;
			for (int k = 0; k < 3; k++) {
				Vector2 a = p[k], b = p[(k + 1) % 3];
				for (int s = 0; s < segPerEdge; s++) {
					float n = Hash01(seed, k * 31 + s, 17);
					if (n < 0.42f) // 大约一半左右的段落显现纹路，其余保持纯白
						continue;
					float t0 = s / (float)segPerEdge;
					float t1 = (s + 1) / (float)segPerEdge;
					Vector2 sa = Vector2.Lerp(a, b, t0);
					Vector2 sb = Vector2.Lerp(a, b, t1);
					float intensity = MathHelper.Lerp(0.35f, 0.9f, (n - 0.42f) / 0.58f);
					AddThickLine(v, sa, sb, thickness * 1.35f, veinCol * intensity);
				}
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

			float fadeIn  = MathHelper.Clamp(_age / (float)FadeInTicks, 0f, 1f);
			float fadeOut = _fadingOut ? MathHelper.Clamp(1f - _fadeAge / (float)FadeOutTicks, 0f, 1f) : 1f;
			float fade = fadeIn * fadeOut;
			if (fade <= 0.001f)
				return false;

			Vector2 pos = Projectile.Center - Main.screenPosition;
			var verts = new List<VertexPositionColor>(600);

			// ── 1. 中心打底：红紫→紫渐变、半透明、边缘模糊的大圆 ──
			{
				const int seg = 20;
				Color c0 = RedPurple * (0.5f * fade);
				Color c1 = Purple * fade; c1.A = 0;
				for (int i = 0; i < seg; i++) {
					float a0 = MathHelper.TwoPi * i / seg;
					float a1 = MathHelper.TwoPi * (i + 1) / seg;
					AddTri(verts, pos,
						pos + a0.ToRotationVector2() * BackdropR,
						pos + a1.ToRotationVector2() * BackdropR, c0);
					// 用顶点色直接过渡：中心 c0、边缘 c1
					int n = verts.Count;
					verts[n - 2] = new VertexPositionColor(verts[n - 2].Position, c1);
					verts[n - 1] = new VertexPositionColor(verts[n - 1].Position, c1);
				}
			}

			if (!_fadingOut) {
				// ── 2. 第三三角：延迟一小段时间才开始出现，随后由大到小收缩（持续时间更长），
				//      收缩完成后停留在第二三角外侧留一点间距的位置 ──
				int localAge = _age - ShrinkDelayTicks;
				if (localAge >= 0) {
					float shrinkT = MathHelper.Clamp(localAge / (float)ShrinkTicks, 0f, 1f);
					shrinkT = 1f - (1f - shrinkT) * (1f - shrinkT); // 缓出
					float r = MathHelper.Lerp(ShrinkFromR, SettledR, shrinkT);
					float alpha = fade * MathHelper.Lerp(0.25f, 0.9f, shrinkT); // 收缩过程中淡入
					AddGlowTriOutline(verts, pos, r, 1.4f, RedPurple * alpha, 0.35f * alpha);
				}

				// ── 3. 第一三角（内侧）+ 第二三角（外侧）：主体白色，叠加不规则红紫纹路，均带外发光 ──
				int veinSeed = (int)Projectile.ai[0];
				AddVeinedTriOutline(verts, pos, OuterR, 1.4f, fade, 0.4f * fade, veinSeed * 2 + 1);
				AddVeinedTriOutline(verts, pos, InnerR, 1.2f, fade, 0.4f * fade, veinSeed * 2);
			}
			else {
				// ── 2+3 的"离场"版本：三角线条不再整体淡出，而是碎成一颗颗小三角形向外飘散淡出 ──
				Span<Vector2> tri = stackalloc Vector2[3];
				foreach (var s in _exitShards) {
					Color c = s.Col * fade;
					if (c.A <= 2)
						continue;
					Vector2 c0 = pos + s.Offset;
					for (int k = 0; k < 3; k++) {
						float ang = s.Rot + k * MathHelper.TwoPi / 3f;
						tri[k] = c0 + ang.ToRotationVector2() * s.Size;
					}
					AddTri(verts, tri[0], tri[1], tri[2], c);
				}
			}

			// ── 4. 中子星：独立于三角图案之上渲染——扁平化（左右更长、上下更窄的椭圆核心+描边），
			//      并单独叠加一层柔和的红色外发光（与三角图案本身的红紫发光是两套独立颜色/形状）──
			{
				const int seg = 16;
				const float coreRx = 6.4f, coreRy = 2.6f;  // 压扁：左右拉长、上下收窄
				const float ringRx = 8.8f, ringRy = 3.6f;
				const float glowRx = 16f,  glowRy = 6.5f;  // 独立红色外发光范围，更宽更柔和
				Color coreCol = White * fade;
				Color ringCol = RedPurple * fade;
				Color glowInner = NeutronGlowCol * (fade * 0.6f);
				Color glowOuter = glowInner; glowOuter.A = 0;

				// 独立红色外发光：柔和扁圆，中心稍浓、边缘透明淡出
				for (int i = 0; i < seg; i++) {
					float a0 = MathHelper.TwoPi * i / seg;
					float a1 = MathHelper.TwoPi * (i + 1) / seg;
					Vector2 g0 = pos + new Vector2(a0.ToRotationVector2().X * glowRx, a0.ToRotationVector2().Y * glowRy);
					Vector2 g1 = pos + new Vector2(a1.ToRotationVector2().X * glowRx, a1.ToRotationVector2().Y * glowRy);
					AddTri(verts, pos, g0, g1, glowInner, glowOuter, glowOuter);
				}

				for (int i = 0; i < seg; i++) {
					float a0 = MathHelper.TwoPi * i / seg;
					float a1 = MathHelper.TwoPi * (i + 1) / seg;
					Vector2 dir0 = a0.ToRotationVector2();
					Vector2 dir1 = a1.ToRotationVector2();
					Vector2 p0 = pos + new Vector2(dir0.X * coreRx, dir0.Y * coreRy);
					Vector2 p1 = pos + new Vector2(dir1.X * coreRx, dir1.Y * coreRy);
					AddTri(verts, pos, p0, p1, coreCol);
					// 红紫描边环
					Vector2 q0 = pos + new Vector2(dir0.X * ringRx, dir0.Y * ringRy);
					Vector2 q1 = pos + new Vector2(dir1.X * ringRx, dir1.Y * ringRy);
					AddTri(verts, p0, q0, q1, ringCol);
					AddTri(verts, p0, q1, p1, ringCol);
				}

				// 左右伸出的长角（横向纺锤，上下无角，加长）
				const float legLen = 26f;
				const float legHalfH = 2.4f;
				Color legEdge = coreCol; legEdge.A = 0;
				Vector2 lTip = pos + new Vector2(-legLen, 0f);
				Vector2 rTip = pos + new Vector2(legLen, 0f);
				Vector2 top = pos + new Vector2(0f, -legHalfH);
				Vector2 bot = pos + new Vector2(0f, legHalfH);
				AddTri(verts, lTip, top, pos, legEdge, coreCol, coreCol);
				AddTri(verts, lTip, pos, bot, legEdge, coreCol, coreCol);
				AddTri(verts, pos, top, rTip, coreCol, coreCol, legEdge);
				AddTri(verts, pos, rTip, bot, coreCol, legEdge, coreCol);
			}

			// ── 5. 向四周飞出的小三角形（白/红紫，均带红紫发光模糊边缘）──
			Span<Vector2> p = stackalloc Vector2[3];
			Span<Vector2> g = stackalloc Vector2[3];
			foreach (var t in _flyTris) {
				float lifeT = t.Life / (float)t.MaxLife;
				float alpha = fade * MathHelper.Clamp((1f - lifeT) * 2f, 0f, 1f) * MathHelper.Clamp(lifeT * 6f, 0f, 1f);
				if (alpha <= 0.01f)
					continue;

				Vector2 c = pos + t.Offset;
				for (int k = 0; k < 3; k++) {
					float ang = t.Rot + k * MathHelper.TwoPi / 3f;
					p[k] = c + ang.ToRotationVector2() * t.Size;
				}
				// 发光模糊边缘：先画放大 1.8 倍的低透明度紫色三角
				for (int k = 0; k < 3; k++)
					g[k] = c + (p[k] - c) * 1.8f;
				Color glowC = GlowCol * (alpha * 0.45f); glowC.A = 0;
				AddTri(verts, g[0], g[1], g[2], GlowCol * (alpha * 0.4f), glowC, glowC);

				Color bodyC = (t.IsWhite ? White : RedPurple) * alpha;
				AddTri(verts, p[0], p[1], p[2], bodyC);
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

		private static void AddTri(List<VertexPositionColor> v, Vector2 a, Vector2 b, Vector2 c,
			Color ca, Color cb, Color cc) {
			v.Add(new VertexPositionColor(new Vector3(a, 0f), ca));
			v.Add(new VertexPositionColor(new Vector3(b, 0f), cb));
			v.Add(new VertexPositionColor(new Vector3(c, 0f), cc));
		}
	}
}
