using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using ArknightsMod.Common.Particle;

namespace ArknightsMod.Content.Projectiles.Specialist.Scene
{
	// 稀音的摄像机左键命中特效（纯视觉，不造成伤害——伤害由 SceneCameraBullet 弹幕承担）：
	// 朝光标方向射出双层椭圆环冲击波（前后错开、黄→绿、淡入淡出），
	// 伴随向前扇形溅射的绿色粒子，发射点生成大小不一、代码绘制的柳叶状叶片并掉落。
	public class SceneCameraShot : ModProjectile
	{
		// ---- 可调参数 ----
		private const int LifetimeTotal = 80;       // 弹幕总寿命（叶片掉落 + 淡出）
		private const int RingLifetime = 28;        // 圆环扩张/消失窗口（攻击频率由武器 useTime 决定，互不影响）
		private const float RingStartRadius = 22f;  // 大环初始半径
		private const float RingMaxRadius = 80f;    // 大环最大半径（放大倍数收敛）
		private const float RingSmallScale = 0.6f;  // 较小（后方）环相对大环半径
		private const float RingSeparation = 0.5f;  // 两环沿射向的分离距离（× 大环半径，已缩短）
		private const float RingBandWidth = 16f;    // 环带宽度（略微加粗）
		private const float EllipseAspect = 0.35f;  // 沿射向的压扁系数（越小越扁：射向更短、垂直射向更长）
		private const int RingSegments = 48;
		private const float ForwardDrag = 0.92f;    // 环中心前冲速度衰减

		private const int LeafCount = 8;            // 叶片数量
		private const float LeafLength = 18f;       // 叶片长度（柳叶状：长而瘦）
		private const float LeafMaxWidth = 2.6f;    // 叶片最大半宽
		private const float LeafMaxOpacity = 0.5f;  // 叶片不透明度上限
		private const float LeafGravity = 0.16f;
		private const float LeafAirDrag = 0.97f;

		private static readonly Color RingColorStart = new(255, 221, 40);  // 黄（刚出现）
		private static readonly Color RingColorEnd = new(36, 210, 70);     // 绿（快速过渡到）
		private static readonly Color LeafColorBase = new(70, 190, 60);
		private static readonly Color ParticleColor = new(120, 240, 80);

		private struct Leaf
		{
			public Vector2 Position;
			public Vector2 Velocity;
			public float Rotation;
			public float AngularVelocity;
			public float Scale;
			public Color Tint;
			public int Age;
			public int Life;
		}

		private readonly List<Leaf> leaves = new();
		private bool initialized;
		private float aimAngle;

		public override string Texture => "ArknightsMod/Assets/null";

		public override void SetStaticDefaults() {
			ProjectileID.Sets.DrawScreenCheckFluff[Type] = 1600;
		}

		public override void SetDefaults() {
			Projectile.width = 2;
			Projectile.height = 2;
			Projectile.friendly = false;   // 纯视觉，不造成伤害
			Projectile.hostile = false;
			Projectile.penetrate = -1;
			Projectile.tileCollide = false;
			Projectile.timeLeft = LifetimeTotal;
		}

		// 寿命已流逝刻数：0 → LifetimeTotal
		private int Elapsed => LifetimeTotal - Projectile.timeLeft;
		// 圆环阶段进度 0→1（28 刻后维持 1）
		private float RingProgress => MathHelper.Clamp(Elapsed / (float)RingLifetime, 0f, 1f);
		private bool RingsActive => Elapsed <= RingLifetime;

		private static float EaseOutQuad(float t) => 1f - (1f - t) * (1f - t);

		private float OuterRadius => MathHelper.Lerp(RingStartRadius, RingMaxRadius, EaseOutQuad(RingProgress));

		// 快速淡入(前 12%)后淡出 —— 淡入足够快，黄色阶段才能被看到
		private float RingAlpha {
			get {
				float p = RingProgress;
				if (p <= 0.12f)
					return p / 0.12f;
				return MathHelper.Clamp(1f - (p - 0.12f) / 0.88f, 0f, 1f);
			}
		}

		public override void OnSpawn(IEntitySource source) {
			aimAngle = Projectile.velocity.ToRotation();
			Projectile.rotation = aimAngle;
		}

		public override void AI() {
			if (!initialized) {
				initialized = true;
				aimAngle = Projectile.velocity == Vector2.Zero ? aimAngle : Projectile.velocity.ToRotation();
				SpawnLeaves();
				SpawnSplatter();
			}
			else if (Elapsed < 6) {
				SpawnSplatter();
			}

			// 环中心朝射向前冲、逐渐减速（位移由引擎按 velocity 自动施加）。
			Projectile.velocity *= ForwardDrag;

			UpdateLeaves();
		}

		private void SpawnLeaves() {
			if (Main.dedServ)
				return;

			Vector2 origin = Projectile.Center;
			for (int i = 0; i < LeafCount; i++) {
				Vector2 vel = new Vector2(Main.rand.NextFloat(-2.4f, 2.4f), Main.rand.NextFloat(-3.6f, -0.6f));
				vel += aimAngle.ToRotationVector2() * Main.rand.NextFloat(0f, 1.6f);
				Color tint = LeafColorBase;
				tint.R = (byte)MathHelper.Clamp(tint.R + Main.rand.Next(-25, 40), 0, 255);
				tint.G = (byte)MathHelper.Clamp(tint.G + Main.rand.Next(-30, 45), 0, 255);
				tint.B = (byte)MathHelper.Clamp(tint.B + Main.rand.Next(-20, 30), 0, 255);

				leaves.Add(new Leaf {
					Position = origin + Main.rand.NextVector2Circular(8f, 8f),
					Velocity = vel,
					Rotation = Main.rand.NextFloat(MathHelper.TwoPi),
					AngularVelocity = Main.rand.NextFloat(-0.18f, 0.18f),
					Scale = Main.rand.NextFloat(0.55f, 1.5f),
					Tint = tint,
					Age = 0,
					Life = Main.rand.Next(50, LifetimeTotal),
				});
			}
		}

		private void SpawnSplatter() {
			if (Main.dedServ)
				return;

			Vector2 dir = aimAngle.ToRotationVector2();
			int count = Main.rand.Next(2, 4);
			// 加性粒子用「降亮度」近似限制不透明度上限（≤50%）。
			Color splatter = ParticleColor * 0.5f;
			for (int i = 0; i < count; i++) {
				// 朝攻击方向的扇形区域内随机角度溅射；直线发射、无重力下坠。
				Vector2 vel = dir.RotatedByRandom(MathHelper.ToRadians(28f)) * Main.rand.NextFloat(3.5f, 9f);
				new DefaultParticle(Projectile.Center, vel, Main.rand.Next(16, 28),
					Main.rand.NextFloat(0.5f, 1.1f), splatter, noGravity: true).Spawn();
			}
		}

		private void UpdateLeaves() {
			for (int i = 0; i < leaves.Count; i++) {
				Leaf leaf = leaves[i];
				leaf.Velocity.X *= LeafAirDrag;
				leaf.Velocity.Y = leaf.Velocity.Y * LeafAirDrag + LeafGravity;
				leaf.Position += leaf.Velocity;
				leaf.Rotation += leaf.AngularVelocity;
				// 飘摆：让旋转随速度方向轻微摆动
				leaf.AngularVelocity = MathHelper.Lerp(leaf.AngularVelocity, leaf.Velocity.X * 0.012f, 0.05f);
				leaf.Age++;
				leaves[i] = leaf;
			}
		}

		public override bool PreDraw(ref Color lightColor) {
			if (Main.dedServ)
				return false;

			var verts = new List<VertexPositionColor>(512);

			if (RingsActive && RingAlpha > 0.01f) {
				float alpha = RingAlpha;
				Color ringColor = Color.Lerp(RingColorStart, RingColorEnd, MathHelper.Clamp(RingProgress * 2.2f, 0f, 1f));
				float largeR = OuterRadius;
				float smallR = largeR * RingSmallScale;
				Vector2 aimDir = aimAngle.ToRotationVector2();
				float gap = largeR * RingSeparation;
				// 两环沿射向前后错开（非内/外同心）：较小者在后（靠玩家），较大者在前。
				Vector2 smallCenter = Projectile.Center - aimDir * gap * 0.5f;
				Vector2 largeCenter = Projectile.Center + aimDir * gap * 0.5f;
				AppendEllipseRing(verts, smallCenter, smallR, ringColor, alpha * 0.9f);
				AppendEllipseRing(verts, largeCenter, largeR, ringColor, alpha);
			}

			AppendLeaves(verts);

			ScenePrimitiveRenderer.DrawTriangles(verts);

			return false;
		}

		// 椭圆环带：局部坐标先沿"射向轴"压扁(EllipseAspect)、垂直射向保持原长，
		// 再整体旋转到射向 —— 于是朝某方向发射时，环呈"射向短、垂直射向长"的椭圆，
		// 像从玩家面前正对射出去一样。
		private void AppendEllipseRing(List<VertexPositionColor> verts, Vector2 center, float radius, Color color, float alpha) {
			float cos = MathF.Cos(aimAngle);
			float sin = MathF.Sin(aimAngle);
			Color edge = color * (alpha * 0f);   // 透明边
			Color core = color * alpha;

			float rOuter = radius;
			float rInner = Math.Max(radius - RingBandWidth, 2f);
			float rMid = MathHelper.Lerp(rInner, rOuter, 0.5f);

			Vector2 Map(float ang, float r) {
				// 局部 x = 沿射向（压扁），局部 y = 垂直射向（保持）
				float lx = MathF.Cos(ang) * r * EllipseAspect;
				float ly = MathF.Sin(ang) * r;
				// 旋转到射向
				return center + new Vector2(lx * cos - ly * sin, lx * sin + ly * cos);
			}

			for (int i = 0; i < RingSegments; i++) {
				float a0 = MathHelper.TwoPi * i / RingSegments;
				float a1 = MathHelper.TwoPi * (i + 1) / RingSegments;

				Vector2 i0 = Map(a0, rInner), i1 = Map(a1, rInner);
				Vector2 m0 = Map(a0, rMid), m1 = Map(a1, rMid);
				Vector2 o0 = Map(a0, rOuter), o1 = Map(a1, rOuter);

				// 内半带：inner(透明) → mid(实)
				verts.Add(Vpc(i0, edge)); verts.Add(Vpc(m0, core)); verts.Add(Vpc(m1, core));
				verts.Add(Vpc(i0, edge)); verts.Add(Vpc(m1, core)); verts.Add(Vpc(i1, edge));
				// 外半带：mid(实) → outer(透明)
				verts.Add(Vpc(m0, core)); verts.Add(Vpc(o0, edge)); verts.Add(Vpc(o1, edge));
				verts.Add(Vpc(m0, core)); verts.Add(Vpc(o1, edge)); verts.Add(Vpc(m1, core));
			}
		}

		private void AppendLeaves(List<VertexPositionColor> verts) {
			const int slices = 7; // 沿叶脉采样段数
			foreach (Leaf leaf in leaves) {
				float lifeRatio = leaf.Life <= 0 ? 1f : leaf.Age / (float)leaf.Life;
				if (lifeRatio >= 1f)
					continue;
				// 末段淡出，并整体限制不透明度上限。
				float fade = lifeRatio < 0.7f ? 1f : MathHelper.Clamp(1f - (lifeRatio - 0.7f) / 0.3f, 0f, 1f);
				float a = fade * LeafMaxOpacity;
				Color col = leaf.Tint * a;
				Color vein = Color.Lerp(leaf.Tint, Color.Black, 0.45f) * a;

				float cos = MathF.Cos(leaf.Rotation);
				float sin = MathF.Sin(leaf.Rotation);
				float len = LeafLength * leaf.Scale;
				float maxW = LeafMaxWidth * leaf.Scale;

				Vector2 ToWorld(float along, float across) {
					// 叶片局部：along 沿叶脉(0=基部,len=尖端)，across 横向
					float lx = across;
					float ly = -along; // 朝上为尖
					return leaf.Position + new Vector2(lx * cos - ly * sin, lx * sin + ly * cos);
				}

				// 柳叶状不对称轮廓：基部圆、约 35% 处最宽、尖端细长。
				float Width(float t) => maxW * MathF.Pow(t, 0.6f) * MathF.Pow(1f - t, 1.1f) / 0.337f;

				for (int s = 0; s < slices; s++) {
					float t0 = s / (float)slices;
					float t1 = (s + 1) / (float)slices;
					float a0 = t0 * len, a1 = t1 * len;
					float w0 = Width(t0), w1 = Width(t1);

					Vector2 l0 = ToWorld(a0, -w0), r0 = ToWorld(a0, w0);
					Vector2 l1 = ToWorld(a1, -w1), r1 = ToWorld(a1, w1);
					Vector2 c0 = ToWorld(a0, 0f), c1 = ToWorld(a1, 0f);

					// 左半片
					verts.Add(Vpc(l0, col)); verts.Add(Vpc(c0, col)); verts.Add(Vpc(c1, col));
					verts.Add(Vpc(l0, col)); verts.Add(Vpc(c1, col)); verts.Add(Vpc(l1, col));
					// 右半片
					verts.Add(Vpc(c0, col)); verts.Add(Vpc(r0, col)); verts.Add(Vpc(r1, col));
					verts.Add(Vpc(c0, col)); verts.Add(Vpc(r1, col)); verts.Add(Vpc(c1, col));
				}

				// 叶脉（沿中线一条细带，止于近尖端）
				float vw = 0.35f * leaf.Scale;
				float veinTip = len * 0.85f;
				Vector2 vb0 = ToWorld(0f, -vw), vb1 = ToWorld(0f, vw);
				Vector2 vt0 = ToWorld(veinTip, -vw), vt1 = ToWorld(veinTip, vw);
				verts.Add(Vpc(vb0, vein)); verts.Add(Vpc(vt0, vein)); verts.Add(Vpc(vt1, vein));
				verts.Add(Vpc(vb0, vein)); verts.Add(Vpc(vt1, vein)); verts.Add(Vpc(vb1, vein));
			}
		}

		private static VertexPositionColor Vpc(Vector2 p, Color col) => ScenePrimitiveRenderer.Vert(p, col);
	}
}
