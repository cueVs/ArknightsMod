using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Projectiles.Specialist.Scene
{
	// 稀音弹幕命中/破坏时的命中特效：多角散开的星形爆发，
	// 中心接近白色的亮黄绿、四周绿色渐变；快速放大并淡出。纯视觉。
	public class SceneCameraHitBurst : ModProjectile
	{
		private const int Lifetime = 16;
		private const int Spikes = 9;             // 尖角数量
		private const float MaxRadius = 26f;      // 较小范围
		private const float InnerRatio = 0.42f;   // 谷底/尖角半径比

		private static readonly Color BurstCenter = new(245, 255, 220); // 接近白色的亮黄绿
		private static readonly Color BurstEdge = new(40, 200, 60);     // 四周绿

		public override string Texture => "ArknightsMod/Assets/null";

		public override void SetStaticDefaults() {
			ProjectileID.Sets.DrawScreenCheckFluff[Type] = 600;
		}

		public override void SetDefaults() {
			Projectile.width = 2;
			Projectile.height = 2;
			Projectile.friendly = false;
			Projectile.hostile = false;
			Projectile.penetrate = -1;
			Projectile.tileCollide = false;
			Projectile.timeLeft = Lifetime;
			Projectile.aiStyle = -1;
		}

		public override void OnSpawn(IEntitySource source) {
			Projectile.rotation = Main.rand.NextFloat(MathHelper.TwoPi); // 随机朝向，避免雷同
		}

		public override void AI() {
			Projectile.velocity = Vector2.Zero;
		}

		private float Progress => 1f - Projectile.timeLeft / (float)Lifetime;

		public override bool PreDraw(ref Color lightColor) {
			if (Main.dedServ)
				return false;

			float p = Progress;
			float ease = 1f - (1f - p) * (1f - p); // easeOut 扩张
			float radius = MathHelper.Lerp(MaxRadius * 0.35f, MaxRadius, ease);
			float alpha = p < 0.15f ? p / 0.15f : MathHelper.Clamp(1f - (p - 0.15f) / 0.85f, 0f, 1f);
			if (alpha <= 0.01f)
				return false;

			Color center = BurstCenter * alpha;
			Color edge = BurstEdge * (alpha * 0.55f); // 尖端略透，柔化散开

			var verts = new List<VertexPositionColor>(Spikes * 6);
			int pts = Spikes * 2;
			Vector2 c = Projectile.Center;
			float baseRot = Projectile.rotation;

			for (int i = 0; i < pts; i++) {
				float ang0 = baseRot + MathHelper.TwoPi * i / pts;
				float ang1 = baseRot + MathHelper.TwoPi * (i + 1) / pts;
				float r0 = (i % 2 == 0) ? radius : radius * InnerRatio;
				float r1 = ((i + 1) % 2 == 0) ? radius : radius * InnerRatio;
				// 尖角端更绿更透，谷底偏中心色，做中心→四周渐变
				Color col0 = (i % 2 == 0) ? edge : Color.Lerp(center, edge, 0.4f);
				Color col1 = ((i + 1) % 2 == 0) ? edge : Color.Lerp(center, edge, 0.4f);

				Vector2 p0 = c + ang0.ToRotationVector2() * r0;
				Vector2 p1 = c + ang1.ToRotationVector2() * r1;

				verts.Add(ScenePrimitiveRenderer.Vert(c, center));
				verts.Add(ScenePrimitiveRenderer.Vert(p0, col0));
				verts.Add(ScenePrimitiveRenderer.Vert(p1, col1));
			}

			ScenePrimitiveRenderer.DrawTriangles(verts);
			return false;
		}
	}
}
