using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ArknightsMod.Content.Items.Weapons.Supporter.Pramanix
{
	// 折线描边网格（miter/bevel 拐角）
	internal static class PramanixSkill3StrokeMesh
	{
		private const float MiterLimit = 3.5f;
		private const float EndCapExtendFactor = 0.5f;

		private struct BevelInfo
		{
			public int Index;
			public Vector2 OuterA;
			public Vector2 OuterB;
		}

		internal static void AppendChain(
			IReadOnlyList<Vector2> points,
			bool closed,
			float halfWidth,
			Color color,
			ICollection<VertexPositionColor> verts) {
			if (points == null || points.Count < 2 || color.A <= 0)
				return;

			int n = points.Count;
			var dp = new Vector2[n];
			for (int i = 0; i < n; i++)
				dp[i] = points[i];

			int segCount = closed ? n : n - 1;
			var dirs = new Vector2[segCount];
			var norms = new Vector2[segCount];
			for (int i = 0; i < segCount; i++) {
				int j = (i + 1) % n;
				Vector2 d = dp[j] - dp[i];
				float len = d.Length();
				dirs[i] = len < 0.001f ? Vector2.UnitX : d / len;
				norms[i] = new Vector2(-dirs[i].Y, dirs[i].X);
			}

			var left = new Vector2[n];
			var right = new Vector2[n];
			var bevelInfos = new List<BevelInfo>();
			var bevelAt = new Dictionary<int, BevelInfo>();

			for (int i = 0; i < n; i++) {
				if (!closed && i == 0) {
					Vector2 ext = -dirs[0] * (halfWidth * EndCapExtendFactor);
					left[i] = dp[0] + norms[0] * halfWidth + ext;
					right[i] = dp[0] - norms[0] * halfWidth + ext;
				}
				else if (!closed && i == n - 1) {
					Vector2 ext = dirs[n - 2] * (halfWidth * EndCapExtendFactor);
					left[i] = dp[n - 1] + norms[n - 2] * halfWidth + ext;
					right[i] = dp[n - 1] - norms[n - 2] * halfWidth + ext;
				}
				else {
					int prevSeg = (i - 1 + segCount) % segCount;
					int nextSeg = i % segCount;
					ComputeMiter(
						dp[i], norms[prevSeg], norms[nextSeg], halfWidth,
						out left[i], out right[i],
						out bool bevel, out BevelInfo bevelInfo);

					if (bevel) {
						bevelInfo.Index = i;
						bevelInfos.Add(bevelInfo);
						bevelAt[i] = bevelInfo;
					}
				}
			}

			int quadCount = closed ? n : n - 1;
			for (int i = 0; i < quadCount; i++) {
				int next = (i + 1) % n;
				Vector2 l1 = left[next];
				Vector2 r1 = right[next];

				if (bevelAt.TryGetValue(next, out BevelInfo bevel)) {
					int dirIndex = closed ? i : Math.Min(i, segCount - 1);
					int nextDirIndex = closed ? (i + 1) % segCount : Math.Min(i + 1, segCount - 1);
					float cross = Cross(dirs[dirIndex], dirs[nextDirIndex]);
					if (cross >= 0f)
						l1 = bevel.OuterA;
					else
						r1 = bevel.OuterA;
				}

				AddQuad(verts, left[i], right[i], l1, r1, color);
			}

			foreach (BevelInfo bevel in bevelInfos) {
				int i = bevel.Index;
				int prevSeg = (i - 1 + segCount) % segCount;
				int nextSeg = i % segCount;
				float cross = Cross(dirs[prevSeg], dirs[nextSeg]);
				if (cross >= 0f)
					AddTri(verts, dp[i], bevel.OuterA, bevel.OuterB, color);
				else
					AddTri(verts, dp[i], bevel.OuterB, bevel.OuterA, color);
			}
		}

		private static void ComputeMiter(
			Vector2 vertex,
			Vector2 normPrev,
			Vector2 normNext,
			float halfWidth,
			out Vector2 miterLeft,
			out Vector2 miterRight,
			out bool bevel,
			out BevelInfo bevelInfo) {
			bevel = false;
			bevelInfo = default;

			Vector2 miterDir = normPrev + normNext;
			float miterLenSq = miterDir.LengthSquared();

			if (miterLenSq < 1e-5f) {
				miterLeft = vertex + normPrev * halfWidth;
				miterRight = vertex - normPrev * halfWidth;
				return;
			}

			miterDir = Vector2.Normalize(miterDir);
			float dot = Vector2.Dot(miterDir, normPrev);
			if (Math.Abs(dot) < 1e-4f)
				dot = 1e-4f;

			float miterScale = halfWidth / dot;
			if (miterScale <= halfWidth * MiterLimit) {
				miterLeft = vertex + miterDir * miterScale;
				miterRight = vertex - miterDir * miterScale;
				return;
			}

			bevel = true;
			float clamped = halfWidth * MiterLimit;
			miterLeft = vertex + miterDir * clamped;
			miterRight = vertex - miterDir * clamped;
			bevelInfo = new BevelInfo {
				OuterA = vertex + normPrev * halfWidth,
				OuterB = vertex + normNext * halfWidth,
			};
		}

		private static float Cross(Vector2 a, Vector2 b) => a.X * b.Y - a.Y * b.X;

		private static void AddQuad(
			ICollection<VertexPositionColor> verts,
			Vector2 l0, Vector2 r0, Vector2 l1, Vector2 r1,
			Color color) {
			verts.Add(Vpc(l0, color));
			verts.Add(Vpc(l1, color));
			verts.Add(Vpc(r0, color));

			verts.Add(Vpc(r0, color));
			verts.Add(Vpc(l1, color));
			verts.Add(Vpc(r1, color));
		}

		private static void AddTri(
			ICollection<VertexPositionColor> verts,
			Vector2 a, Vector2 b, Vector2 c,
			Color color) {
			verts.Add(Vpc(a, color));
			verts.Add(Vpc(b, color));
			verts.Add(Vpc(c, color));
		}

		private static VertexPositionColor Vpc(Vector2 p, Color color) =>
			new(new Vector3(p.X, p.Y, 0f), color);
	}
}
