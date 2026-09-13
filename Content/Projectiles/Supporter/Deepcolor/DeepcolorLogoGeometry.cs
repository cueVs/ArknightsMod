using System;
using Microsoft.Xna.Framework;

namespace ArknightsMod.Content.Projectiles.Supporter.Deepcolor
{
	public readonly struct DeepcolorLogoPoints
	{
		public readonly Vector2 A;
		public readonly Vector2 B;
		public readonly Vector2 C;
		public readonly Vector2 D;
		public readonly Vector2 E;

		public DeepcolorLogoPoints(Vector2 a, Vector2 b, Vector2 c, Vector2 d, Vector2 e) {
			A = a;
			B = b;
			C = c;
			D = d;
			E = e;
		}

		public static DeepcolorLogoPoints CreateDefault() {
			const float s = 4f;
			const float tC = 0.7f;
			float h = s * MathF.Sqrt(3f) / 2f;
			float dx = s / 2f;
			float dy = -h;
			var b = new Vector2(0f, h);
			var c = new Vector2(tC * dx, h + tC * dy);
			var a = new Vector2(-s / 2f, 0f);
			var d = new Vector2(0f, c.Y);
			var e = new Vector2(s / 2f, 0f);
			return new DeepcolorLogoPoints(a, b, c, d, e);
		}

		public DeepcolorLogoPoints WithHorizontalScale(float xScale) {
			if (Math.Abs(xScale - 1f) < 0.001f)
				return this;

			return new DeepcolorLogoPoints(
				ScaleX(A, xScale),
				ScaleX(B, xScale),
				ScaleX(C, xScale),
				ScaleX(D, xScale),
				ScaleX(E, xScale));
		}

		private static Vector2 ScaleX(Vector2 v, float xScale) => new(v.X * xScale, v.Y);

		// 蓄力描线顺序：E → A → B → C → D
		public (Vector2 start, Vector2 end)[] GetProgressiveSegments() => [
			(E, A),
			(A, B),
			(B, C),
			(C, D),
		];

		public Vector2 ToWorld(Vector2 local, Vector2 center, float direction, float scale) {
			return center + new Vector2(local.X * direction, -local.Y) * scale;
		}

		public DeepcolorLogoPoints ToWorldPoints(Vector2 center, float direction, float scale) {
			return new DeepcolorLogoPoints(
				ToWorld(A, center, direction, scale),
				ToWorld(B, center, direction, scale),
				ToWorld(C, center, direction, scale),
				ToWorld(D, center, direction, scale),
				ToWorld(E, center, direction, scale));
		}

		public Vector2 GetCentroid() => (A + B + C + D + E) * 0.2f;

		public Vector2 GetTriangleCentroid() => (A + B + E) / 3f;

		// 让三角形几何中心落在 worldAtTriangleCentroid
		public Vector2 GetAnchorForTriangleCentroidAt(Vector2 worldAtTriangleCentroid, float direction, float scale) {
			Vector2 c = GetTriangleCentroid();
			return worldAtTriangleCentroid - new Vector2(c.X * direction, -c.Y) * scale;
		}
	}
}
