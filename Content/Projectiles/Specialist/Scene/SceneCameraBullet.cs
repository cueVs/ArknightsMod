using System.Collections.Generic;
using ArknightsMod.Common.Particle;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Projectiles.Specialist.Scene
{
	// 稀音的摄像机左键弹幕：接近白色的黄绿色小球 + 一体式拖尾（黄绿→深绿并淡出），
	// 抛物线下坠弹道；命中敌人或撞墙/消失时爆发多角散开命中特效，并向四周炸出带物理的绿色溅射。
	// 承担左键伤害（魔法）。
	public class SceneCameraBullet : ModProjectile
	{
		private const int TrailLength = 12;
		private const float BallRadius = 4.5f;
		private const float TrailHalfWidth = 7f;        // 拖尾头部半宽（已加宽）
		private const float Gravity = 0.18f;
		private const float MaxFallSpeed = 24f;

		// 弹道求解参数：按到目标距离推飞行时间，保证落点正好在光标。
		private const float LaunchSpeed = 13f;
		private const float MinFlightTime = 12f;
		private const float MaxFlightTime = 55f;

		private static readonly Color BallCenter = new(240, 255, 215);  // 接近白色的黄绿
		private static readonly Color BallEdge = new(190, 255, 70);     // 黄绿
		private static readonly Color TrailHead = new(190, 255, 70);    // 拖尾头：黄绿
		private static readonly Color TrailTail = new(18, 80, 28);      // 拖尾尾：深绿
		private static readonly Color SplatterColor = new(120, 240, 80);// 命中溅射：绿

		public override string Texture => "ArknightsMod/Assets/null";

		public override void SetStaticDefaults() {
			ProjectileID.Sets.TrailCacheLength[Type] = TrailLength;
			ProjectileID.Sets.TrailingMode[Type] = 3; // 逐帧记录 oldPos
			ProjectileID.Sets.DrawScreenCheckFluff[Type] = 600;
		}

		public override void SetDefaults() {
			Projectile.width = 8;
			Projectile.height = 8;
			Projectile.friendly = true;
			Projectile.DamageType = DamageClass.Magic; // 左键改为魔法伤害
			Projectile.penetrate = 1;       // 命中一次即破
			Projectile.tileCollide = true;
			Projectile.timeLeft = 240;
			Projectile.aiStyle = -1;
		}

		public override void AI() {
			// 物理下坠弹道
			Projectile.velocity.Y += Gravity;
			if (Projectile.velocity.Y > MaxFallSpeed)
				Projectile.velocity.Y = MaxFallSpeed;
			Projectile.rotation = Projectile.velocity.ToRotation();
		}

		// 计算从 start 发射、在重力作用下恰好经过 target 的初速度（固定飞行时间法）。
		// 保证"光标点哪就打到哪"，同时保留抛物线下坠手感。
		public static Vector2 ComputeLaunchVelocity(Vector2 start, Vector2 target) {
			Vector2 to = target - start;
			float dist = to.Length();
			float t = MathHelper.Clamp(dist / LaunchSpeed, MinFlightTime, MaxFlightTime);
			float vx = to.X / t;
			float vy = to.Y / t - 0.5f * Gravity * t;
			return new Vector2(vx, vy);
		}

		public override bool OnTileCollide(Vector2 oldVelocity) => true; // 撞墙即破，由 OnKill 触发命中特效

		public override void OnKill(int timeLeft) {
			// 多角散开的命中特效（owner 生成并联机同步）
			if (Main.myPlayer == Projectile.owner)
				Projectile.NewProjectile(Projectile.GetSource_Death(), Projectile.Center, Vector2.Zero,
					ModContent.ProjectileType<SceneCameraHitBurst>(), 0, 0f, Projectile.owner);

			// 向四周独立炸开、带物理下坠的绿色溅射
			if (!Main.dedServ) {
				int count = Main.rand.Next(8, 13);
				for (int i = 0; i < count; i++) {
					Vector2 vel = Main.rand.NextVector2Circular(5.5f, 5.5f);
					if (vel == Vector2.Zero)
						vel = new Vector2(0f, -1f);
					Color c = SplatterColor;
					c.R = (byte)Utils.Clamp(c.R + Main.rand.Next(-30, 30), 0, 255);
					c.G = (byte)Utils.Clamp(c.G + Main.rand.Next(-30, 25), 0, 255);
					new DefaultParticle(Projectile.Center, vel, Main.rand.Next(18, 32),
						Main.rand.NextFloat(0.5f, 1.0f), c, noGravity: false).Spawn();
				}
			}
		}

		public override bool PreDraw(ref Color lightColor) {
			if (Main.dedServ)
				return false;

			var verts = new List<VertexPositionColor>(128);
			AppendTrail(verts);
			AppendBall(verts);
			ScenePrimitiveRenderer.DrawTriangles(verts);
			return false;
		}

		// 一体式锥形拖尾：沿 oldPos 连成带状，头宽尾尖；颜色黄绿→深绿，alpha 头实尾透。
		private void AppendTrail(List<VertexPositionColor> verts) {
			Vector2 half = Projectile.Size * 0.5f;
			List<Vector2> pts = new() { Projectile.Center };
			for (int i = 0; i < Projectile.oldPos.Length; i++) {
				if (Projectile.oldPos[i] == Vector2.Zero)
					break;
				pts.Add(Projectile.oldPos[i] + half);
			}
			if (pts.Count < 2)
				return;

			int last = pts.Count - 1;
			for (int i = 0; i < last; i++) {
				float t0 = i / (float)last;
				float t1 = (i + 1) / (float)last;
				float w0 = MathHelper.Lerp(TrailHalfWidth, 0f, t0);
				float w1 = MathHelper.Lerp(TrailHalfWidth, 0f, t1);
				Color c0 = Color.Lerp(TrailHead, TrailTail, t0) * (1f - t0);
				Color c1 = Color.Lerp(TrailHead, TrailTail, t1) * (1f - t1);

				Vector2 dir = pts[i] - pts[i + 1];
				if (dir == Vector2.Zero)
					dir = Projectile.velocity;
				Vector2 perp = new Vector2(-dir.Y, dir.X).SafeNormalize(Vector2.UnitY);

				Vector2 a = pts[i] + perp * w0;
				Vector2 b = pts[i] - perp * w0;
				Vector2 c = pts[i + 1] + perp * w1;
				Vector2 d = pts[i + 1] - perp * w1;

				verts.Add(ScenePrimitiveRenderer.Vert(a, c0));
				verts.Add(ScenePrimitiveRenderer.Vert(b, c0));
				verts.Add(ScenePrimitiveRenderer.Vert(c, c1));
				verts.Add(ScenePrimitiveRenderer.Vert(b, c0));
				verts.Add(ScenePrimitiveRenderer.Vert(d, c1));
				verts.Add(ScenePrimitiveRenderer.Vert(c, c1));
			}
		}

		// 头部小球：三角扇，中心接近白色、边缘黄绿。
		private void AppendBall(List<VertexPositionColor> verts) {
			const int seg = 14;
			Vector2 center = Projectile.Center;
			for (int i = 0; i < seg; i++) {
				float a0 = MathHelper.TwoPi * i / seg;
				float a1 = MathHelper.TwoPi * (i + 1) / seg;
				Vector2 p0 = center + a0.ToRotationVector2() * BallRadius;
				Vector2 p1 = center + a1.ToRotationVector2() * BallRadius;
				verts.Add(ScenePrimitiveRenderer.Vert(center, BallCenter));
				verts.Add(ScenePrimitiveRenderer.Vert(p0, BallEdge));
				verts.Add(ScenePrimitiveRenderer.Vert(p1, BallEdge));
			}
		}
	}
}
