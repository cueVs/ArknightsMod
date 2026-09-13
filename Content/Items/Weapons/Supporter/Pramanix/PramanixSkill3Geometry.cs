using System;
using Microsoft.Xna.Framework;
using Terraria;

namespace ArknightsMod.Content.Items.Weapons.Supporter.Pramanix
{
	// 三技能函数路径、弹幕、装饰标志的几何定义
	public static class PramanixSkill3Geometry
	{
		// 路径
		public const float PathB = 0.5f;
		public const float PathL = 2.2f;
		public const int PathCount = 6;
		public const float PathAngleStep = MathHelper.Pi / 3f;
		public const float PathPixelScale = 118f;

		// 弹幕（局部坐标，尖端指向 +Y）
		public const float BoltShapeScale = 3.2f;
		private const float A = 2f;
		private const float H = 1.6f;
		private const float D = 0.45f;
		private const float S = 0.5f;

		public static readonly Vector2[] BoltOuter = [
			new(0f, 0f),
			new(A, -A * H),
			new(0f, -2f * A * H),
			new(-A, -A * H),
		];

		public static readonly Vector2[] BoltInner = [
			new(0f, -D),
			new(A - D, -A * H),
			new(0f, -2f * A * H + D),
			new(-A + D, -A * H),
		];

		public static readonly Vector2[] BoltSpike = [
			new(-A * S, -2f * A * H - A * H * S),
			new(0f, -2f * A * H),
			new(A * S, -2f * A * H - A * H * S),
			new(0f, -2f * A * H - 2f * A * H * S),
		];

		// 轨道标志
		public const int EmblemCount = 8;
		public const float EmblemWidthScale = 0.82f;
		public const float EmblemDrawScale = 6.2f;
		public const float EmblemOrbitRadius = 168f;
		public const float EmblemLineBoost = 1.45f;
		public const float EmblemMaxDisplayAlpha = 0.5f;
		public const float EmblemEmissiveScale = 0.2f;
		// 弹幕淡出后标志仍保持全亮的刻数
		public const int EmblemFadeDelayTicks = 8;
		// 标志淡出时长（长于弹幕 FadeTicks）
		public const int EmblemFadeTicks = 24;
		// 弹幕消失后标志继续淡出的刻数
		public const int EmblemLingerTicks = 12;
		public const float Skill3AreaRangeMultiplier = 1.4f;
		public const float Skill3BoltHitRadius = 52f;

		public static readonly Vector2[] EmblemPoints = [
			new(-6f, 0f),
			new(-3f, 6f),
			new(0f, 0f),
			new(-2f, 0f),
			new(2f, 8f),
			new(6f, 0f),
			new(4f, 0f),
			new(7f, 6f),
			new(10f, 0f),
			new(2f, 1.2f),
			new(3.25f, 3.6f),
			new(2f, 6f),
			new(0.8f, 3.6f),
		];

		// 描边折线顺序影响 miter 拐角
		public static readonly int[][] EmblemStrokeChains = [
			[0, 1, 2, 3, 4, 5, 6, 7, 8],
			[9, 10, 11, 12],
		];

		public static readonly bool[] EmblemChainClosed = [false, true];

		// 四角标志（q1 外框 + q2 内框）
		public const int CornerEmblemCount = 4;
		public const float CornerEmblemOrbitRadius = 302f;
		public const float CornerEmblemDrawScale = 7.8f;
		private const float CornerA = 2f;
		private const float CornerH = 1.6f;
		private const float CornerD = 0.7f;

		public static readonly float[] CornerEmblemAngles = [
			-MathHelper.Pi * 0.75f,
			-MathHelper.PiOver4,
			MathHelper.Pi * 0.75f,
			MathHelper.PiOver4,
		];

		public static readonly Vector2[] CornerEmblemPoints = [
			new(0f, 0f),
			new(CornerA, -CornerA * CornerH),
			new(0f, -2f * CornerA * CornerH),
			new(-CornerA, -CornerA * CornerH),
			new(0f, -CornerD),
			new(CornerA - CornerD, -CornerA * CornerH),
			new(0f, -2f * CornerA * CornerH + CornerD),
			new(-CornerA + CornerD, -CornerA * CornerH),
		];

		public static readonly int[][] CornerEmblemStrokeChains = [
			[0, 1, 2, 3],
			[4, 5, 6, 7],
		];

		public static readonly bool[] CornerEmblemChainClosed = [true, true];

		public static int FacingSign(int direction) => direction >= 0 ? 1 : -1;

		public static Vector2 SamplePath(int pathIndex, float t, int direction = 1) {
			float offset = pathIndex * PathAngleStep;
			float spiral = MathF.Exp(PathB * t) - 1f;
			float ang = t + offset;
			var point = new Vector2(
				-spiral * MathF.Cos(ang),
				spiral * MathF.Sin(ang));
			if (direction < 0)
				point.X = -point.X;
			return point;
		}

		public static Vector2 SamplePathWorld(int pathIndex, float t, Vector2 origin, int direction = 1) =>
			origin + SamplePath(pathIndex, t, direction) * PathPixelScale;

		public static Vector2 SamplePathTangent(int pathIndex, float t, int direction = 1, float dt = 0.025f) {
			t = MathHelper.Clamp(t, 0f, PathL);
			float tBack = Math.Max(0f, t - dt);
			float tFwd = Math.Min(PathL, t + dt);

			Vector2 a;
			Vector2 b;
			if (tFwd - t < 0.0001f) {
				// 路径终点用反向差分，避免切线归零后弹尖朝下
				a = SamplePath(pathIndex, tBack, direction);
				b = SamplePath(pathIndex, t, direction);
			}
			else if (t - tBack < 0.0001f) {
				a = SamplePath(pathIndex, t, direction);
				b = SamplePath(pathIndex, tFwd, direction);
			}
			else {
				a = SamplePath(pathIndex, tBack, direction);
				b = SamplePath(pathIndex, tFwd, direction);
			}

			Vector2 delta = b - a;
			if (delta.LengthSquared() < 0.0001f)
				delta = SamplePath(pathIndex, Math.Min(PathL, dt), direction);
			if (delta.LengthSquared() < 0.0001f)
				delta = new Vector2(FacingSign(direction), 0f);
			return Vector2.Normalize(delta);
		}

		public static float PathRotation(int pathIndex, float t, int direction = 1) {
			Vector2 tangent = SamplePathTangent(pathIndex, t, direction);
			// 局部 -Y 为弹尖，减 PiOver2 使弹尖沿路径前进方向
			return tangent.ToRotation() - MathHelper.PiOver2;
		}

		// 装饰标志尖端背对玩家、朝轨道外侧
		public static float EmblemRotation(float orbitAngle) =>
			orbitAngle - MathHelper.PiOver2;

		public static float GetSkill3AreaRadius(int direction = 1) =>
			GetPathOuterRadius(direction) * Skill3AreaRangeMultiplier;

		public static float GetPathOuterRadius(int direction = 1) {
			float maxDist = 0f;
			for (int i = 0; i < PathCount; i++) {
				float dist = SamplePath(i, PathL, direction).Length() * PathPixelScale;
				if (dist > maxDist)
					maxDist = dist;
			}
			return maxDist;
		}

		public static Vector2 TransformBoltPoint(Vector2 local, Vector2 worldPos, float rotation, float scale, int direction = 1) {
			local.X *= FacingSign(direction);
			float cos = MathF.Cos(rotation);
			float sin = MathF.Sin(rotation);
			Vector2 scaled = local * (scale * BoltShapeScale);
			return worldPos + new Vector2(
				scaled.X * cos - scaled.Y * sin,
				scaled.X * sin + scaled.Y * cos);
		}

		public static Vector2 TransformEmblemPoint(Vector2 local, Vector2 worldPos, float rotation, float scale, int direction = 1) {
			local.X *= EmblemWidthScale * FacingSign(direction);
			float cos = MathF.Cos(rotation);
			float sin = MathF.Sin(rotation);
			Vector2 scaled = local * scale;
			return worldPos + new Vector2(
				scaled.X * cos - scaled.Y * sin,
				scaled.X * sin + scaled.Y * cos);
		}
	}
}
