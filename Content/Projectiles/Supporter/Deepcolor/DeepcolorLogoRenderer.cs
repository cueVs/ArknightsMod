using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;

namespace ArknightsMod.Content.Projectiles.Supporter.Deepcolor
{
	public static class DeepcolorLogoRenderer
	{
		public const float DefaultWorldScale = 5.5f;
		public const float StrikeWorldScaleBonus = 1.34f;
		public const float ChargePerspectiveXScale = 0.8f;

		private const float ChargeMinThickness = 3.4f;
		private const float ChargeMaxThickness = 5.5f;
		private const float StrikeMinThickness = 3.9f;
		private const float StrikeMaxThickness = 6.1f;
		private const float MiterLimit = 3.5f;
		private const float EndCapExtendFactor = 0.5f;

		public static Color LogoColor => CoreColor;

		private static readonly Color CoreColor = new(120, 255, 140);
		private static readonly Vector3 LightColor = new(120f / 255f, 1f, 140f / 255f);

		private static readonly List<VertexPositionColor> MeshBuffer = new(256);
		private static readonly List<BevelInfo> BevelInfos = new(8);
		private static readonly Dictionary<int, BevelInfo> BevelAt = new(8);
		private static BasicEffect _meshEffect;
		private static GraphicsDevice _meshDevice;

		public static float GetChargeDrawDirection(Player owner) {
			int facing = owner.direction;
			if (facing == 0)
				facing = 1;
			return facing >= 0 ? -1f : 1f;
		}

		public static void DrawLogo(
			SpriteBatch spriteBatch,
			DeepcolorLogoPoints points,
			float drawProgress,
			float scale,
			float alpha) {
			if (alpha <= 0f || drawProgress <= 0f)
				return;

			var segments = points.GetProgressiveSegments();
			float thickness = GetChargeLineThickness(scale);
			float totalLength = GetTotalSegmentLength(segments);
			if (totalLength <= 0f)
				return;

			float budget = totalLength * MathHelper.Clamp(drawProgress, 0f, 1f);
			var drawn = new List<(Vector2 start, Vector2 end)>(segments.Length);
			foreach (var seg in segments) {
				if (budget <= 0f)
					break;

				float segLen = seg.start.Distance(seg.end);
				if (segLen <= 0f)
					continue;

				if (budget >= segLen - 0.001f) {
					drawn.Add(seg);
					budget -= segLen;
				}
				else {
					Vector2 dir = seg.end - seg.start;
					drawn.Add((seg.start, seg.start + dir / segLen * budget));
					budget = 0f;
				}
			}

			if (drawn.Count > 0)
				DrawPolyline(spriteBatch, drawn, thickness, CoreColor * alpha, segments);
		}

		public static void DrawChargeLogoFull(
			SpriteBatch spriteBatch,
			DeepcolorLogoPoints points,
			float scale,
			float alpha) {
			if (alpha <= 0f)
				return;

			var segments = points.GetProgressiveSegments();
			DrawPolyline(spriteBatch, segments, GetChargeLineThickness(scale), CoreColor * alpha, segments);
		}

		public static void DrawStrikeLogoFull(
			SpriteBatch spriteBatch,
			DeepcolorLogoPoints points,
			float scale,
			float alpha) {
			if (alpha <= 0f)
				return;

			var segments = points.GetProgressiveSegments();
			DrawPolyline(spriteBatch, segments, GetStrikeLineThickness(scale), CoreColor * alpha, segments);
		}

		public static float GetChargeLineThickness(float logoScale) {
			float t = MathHelper.Clamp((logoScale - 1f) / 0.55f, 0f, 1f);
			return MathHelper.Lerp(ChargeMinThickness, ChargeMaxThickness, t);
		}

		public static float GetStrikeLineThickness(float logoScale) {
			float t = MathHelper.Clamp((logoScale - 1f) / 0.55f, 0f, 1f);
			return MathHelper.Lerp(StrikeMinThickness, StrikeMaxThickness, t);
		}

		public static void EmitLogoLight(Vector2 center, float alpha, float intensityScale = 1f) {
			if (alpha <= 0.01f)
				return;

			Lighting.AddLight(center, LightColor * (0.68f * alpha * intensityScale));
		}

		public static void EmitLogoLight(DeepcolorLogoPoints worldPoints, float alpha, float intensityScale = 1f) {
			if (alpha <= 0.01f)
				return;

			float corner = alpha * 0.42f * intensityScale;
			EmitLogoLight(worldPoints.GetCentroid(), alpha, intensityScale);
			EmitLogoLight(worldPoints.A, corner, 1f);
			EmitLogoLight(worldPoints.B, corner, 1f);
			EmitLogoLight(worldPoints.C, corner, 1f);
			EmitLogoLight(worldPoints.D, corner, 1f);
			EmitLogoLight(worldPoints.E, corner, 1f);
		}

		private static void DrawPolyline(
			SpriteBatch spriteBatch,
			IReadOnlyList<(Vector2 start, Vector2 end)> segments,
			float thickness,
			Color color,
			(Vector2 start, Vector2 end)[] progressiveSegments) {
			if (segments.Count == 0 || color.A <= 0)
				return;

			float halfWidth = thickness * 0.5f;
			Vector2 strokeOrigin = progressiveSegments[0].start;

			MeshBuffer.Clear();

			foreach (var chain in BuildStrokeChains(segments)) {
				if (chain.Count >= 2)
					BuildChainMesh(chain, halfWidth, color, MeshBuffer, strokeOrigin, progressiveSegments);
			}

			if (MeshBuffer.Count >= 3)
				FlushMesh(spriteBatch);
			else
				MeshBuffer.Clear();
		}

		// 叶节点出发走链，分叉处断开 → D-C-B-A 与 E-A
		private static List<List<Vector2>> BuildStrokeChains(IReadOnlyList<(Vector2 start, Vector2 end)> segments) {
			var adj = new Dictionary<long, List<(long key, Vector2 pos)>>();

			void AddEdge(Vector2 a, Vector2 b) {
				long ka = PosKey(a);
				long kb = PosKey(b);
				if (!adj.ContainsKey(ka))
					adj[ka] = new List<(long, Vector2)>();
				if (!adj.ContainsKey(kb))
					adj[kb] = new List<(long, Vector2)>();
				adj[ka].Add((kb, b));
				adj[kb].Add((ka, a));
			}

			foreach (var (s, e) in segments) {
				if (Vector2.DistanceSquared(s, e) < 0.0001f)
					continue;
				AddEdge(s, e);
			}

			var usedEdges = new HashSet<(long, long)>();
			var chains = new List<List<Vector2>>();

			foreach (var (leafKey, leafNeighbors) in adj) {
				if (leafNeighbors.Count != 1)
					continue;

				long prevKey = -1;
				long curKey = leafKey;
				var chain = new List<Vector2> { GetPosFromKey(leafKey, segments) };

				while (true) {
					if (!adj.TryGetValue(curKey, out var neighbors))
						break;

					long nextKey = -1;
					Vector2 nextPos = default;
					foreach (var (nk, np) in neighbors) {
						if (nk == prevKey)
							continue;
						nextKey = nk;
						nextPos = np;
						break;
					}

					if (nextKey < 0)
						break;

					long edgeA = Math.Min(curKey, nextKey);
					long edgeB = Math.Max(curKey, nextKey);
					if (!usedEdges.Add((edgeA, edgeB)))
						break;

					chain.Add(nextPos);

					if (adj[nextKey].Count != 2)
						break;

					prevKey = curKey;
					curKey = nextKey;
				}

				if (chain.Count >= 2)
					chains.Add(chain);
			}

			if (chains.Count > 0)
				return chains;

			foreach (var (s, e) in segments) {
				if (Vector2.DistanceSquared(s, e) >= 0.0001f)
					chains.Add(new List<Vector2> { s, e });
			}
			return chains;
		}

		private static Vector2 GetPosFromKey(long key, IReadOnlyList<(Vector2 start, Vector2 end)> segments) {
			foreach (var (s, e) in segments) {
				if (PosKey(s) == key)
					return s;
				if (PosKey(e) == key)
					return e;
			}
			return Vector2.Zero;
		}

		private static Vector2? GetStrokeTurnDirectionDraw((Vector2 start, Vector2 end)[] progressiveSegments) {
			if (progressiveSegments.Length < 2)
				return null;

			Vector2 d = ToDrawPos(progressiveSegments[1].end) - ToDrawPos(progressiveSegments[1].start);
			float lenSq = d.LengthSquared();
			return lenSq < 1e-4f ? null : d / MathF.Sqrt(lenSq);
		}

		private static float GetDrawHalfWidth(List<Vector2> pts, float halfWidth) {
			if (pts.Count < 2)
				return halfWidth;

			Vector2 worldSeg = pts[1] - pts[0];
			float worldSegLen = worldSeg.Length();
			if (worldSegLen < 0.001f)
				return halfWidth;

			Vector2 worldNorm = new Vector2(-worldSeg.Y, worldSeg.X) / worldSegLen;
			float hwDraw = Vector2.Distance(ToDrawPos(pts[0]), ToDrawPos(pts[0] + worldNorm * halfWidth));
			return hwDraw < 0.5f ? halfWidth : hwDraw;
		}

		private static void BuildChainMesh(
			List<Vector2> pts,
			float halfWidth,
			Color color,
			List<VertexPositionColor> verts,
			Vector2 strokeOrigin,
			(Vector2 start, Vector2 end)[] progressiveSegments) {
			int n = pts.Count;
			if (n < 2)
				return;

			var dp = new Vector2[n];
			for (int i = 0; i < n; i++)
				dp[i] = ToDrawPos(pts[i]);

			var dirs = new Vector2[n - 1];
			var norms = new Vector2[n - 1];
			for (int i = 0; i < n - 1; i++) {
				Vector2 d = dp[i + 1] - dp[i];
				float len = d.Length();
				dirs[i] = len < 0.001f ? Vector2.UnitX : d / len;
				norms[i] = new Vector2(-dirs[i].Y, dirs[i].X);
			}

			float hwDraw = GetDrawHalfWidth(pts, halfWidth);
			Vector2? turnDirDraw = GetStrokeTurnDirectionDraw(progressiveSegments);
			bool isStrokeStart = Vector2.DistanceSquared(pts[0], strokeOrigin) < 0.25f;

			var left = new Vector2[n];
			var right = new Vector2[n];
			BevelInfos.Clear();
			BevelAt.Clear();

			for (int i = 0; i < n; i++) {
				if (i == 0) {
					Vector2? turnDir = n >= 3 ? dirs[1] : turnDirDraw;
					if (isStrokeStart && turnDir.HasValue) {
						ApplyRightTriangleStartCap(dp[0], dirs[0], turnDir.Value, hwDraw, out left[i], out right[i]);
					}
					else {
						Vector2 ext = -dirs[0] * (hwDraw * EndCapExtendFactor);
						left[i] = dp[0] + norms[0] * hwDraw + ext;
						right[i] = dp[0] - norms[0] * hwDraw + ext;
					}
				}
				else if (i == n - 1) {
					Vector2 ext = dirs[n - 2] * (hwDraw * EndCapExtendFactor);
					left[i] = dp[n - 1] + norms[n - 2] * hwDraw + ext;
					right[i] = dp[n - 1] - norms[n - 2] * hwDraw + ext;
				}
				else {
					ComputeMiter(
						dp[i], norms[i - 1], norms[i], hwDraw,
						out left[i], out right[i],
						out bool bevel, out BevelInfo bevelInfo);

					if (bevel) {
						bevelInfo.Index = i;
						BevelInfos.Add(bevelInfo);
						BevelAt[i] = bevelInfo;
					}
				}
			}

			for (int i = 0; i < n - 1; i++) {
				Vector2 l1 = left[i + 1];
				Vector2 r1 = right[i + 1];

				if (BevelAt.TryGetValue(i + 1, out BevelInfo bevel)) {
					float cross = Cross(dirs[i], i + 1 < n - 1 ? dirs[i + 1] : dirs[i]);
					if (cross >= 0f)
						l1 = bevel.OuterA;
					else
						r1 = bevel.OuterA;
				}

				AddQuadRaw(verts, left[i], right[i], l1, r1, color);
			}

			foreach (var bevel in BevelInfos) {
				int i = bevel.Index;
				float cross = Cross(dirs[i - 1], i < n - 1 ? dirs[i] : dirs[i - 1]);

				if (cross >= 0f)
					AddTriRaw(verts, dp[i], bevel.OuterA, bevel.OuterB, color);
				else
					AddTriRaw(verts, dp[i], bevel.OuterB, bevel.OuterA, color);
			}

			BevelAt.Clear();
		}

		private struct BevelInfo
		{
			public int Index;
			public Vector2 OuterA;
			public Vector2 OuterB;
		}

		private static float Cross(Vector2 a, Vector2 b) => a.X * b.Y - a.Y * b.X;

		// E 点直角三角端帽，斜边由首段 quad 对角线承担
		private static void ApplyRightTriangleStartCap(
			Vector2 vertex,
			Vector2 dirFirst,
			Vector2 dirNext,
			float halfWidth,
			out Vector2 capLeft,
			out Vector2 capRight) {
			Vector2 normFirst = new(-dirFirst.Y, dirFirst.X);
			float cross01 = Cross(dirFirst, dirNext);
			float extLen = ComputeTurnMiterExtension(dirFirst, dirNext, halfWidth);

			if (cross01 < 0f) {
				capLeft = vertex + normFirst * halfWidth - dirFirst * extLen;
				capRight = vertex - normFirst * halfWidth;
			}
			else {
				capLeft = vertex + normFirst * halfWidth;
				capRight = vertex - normFirst * halfWidth - dirFirst * extLen;
			}
		}

		// extLen = 2·hw·|cot θ|，使倒角斜面与 dirB（下一段/三角边）平行
		private static float ComputeTurnMiterExtension(Vector2 dirA, Vector2 dirB, float halfWidth) {
			float absCross = MathF.Abs(Cross(dirA, dirB));
			float absDot = MathF.Abs(Vector2.Dot(dirA, dirB));

			if (absCross < 0.001f)
				return halfWidth * MiterLimit * 2f;

			return MathHelper.Min(
				2f * halfWidth * absDot / absCross,
				halfWidth * MiterLimit * 2f);
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

		private static void AddQuadRaw(
			List<VertexPositionColor> verts,
			Vector2 l0, Vector2 r0,
			Vector2 l1, Vector2 r1,
			Color color) {
			verts.Add(MakeVertex(l0, color));
			verts.Add(MakeVertex(l1, color));
			verts.Add(MakeVertex(r0, color));

			verts.Add(MakeVertex(r0, color));
			verts.Add(MakeVertex(l1, color));
			verts.Add(MakeVertex(r1, color));
		}

		private static void AddTriRaw(
			List<VertexPositionColor> verts,
			Vector2 a, Vector2 b, Vector2 c,
			Color color) {
			verts.Add(MakeVertex(a, color));
			verts.Add(MakeVertex(b, color));
			verts.Add(MakeVertex(c, color));
		}

		private static VertexPositionColor MakeVertex(Vector2 p, Color color) {
			return new VertexPositionColor(new Vector3(p.X, p.Y, 0f), color);
		}

		private static Vector2 ToDrawPos(Vector2 world) {
			Vector2 screen = world - Main.screenPosition;
			return Vector2.Transform(screen, Main.GameViewMatrix.TransformationMatrix);
		}

		private static void EnsureMeshEffect(GraphicsDevice device) {
			if (_meshEffect != null && _meshDevice == device)
				return;

			_meshEffect?.Dispose();
			_meshDevice = device;
			_meshEffect = new BasicEffect(device) {
				VertexColorEnabled = true,
				TextureEnabled = false,
				Alpha = 1f,
			};
		}

		private static void FlushMesh(SpriteBatch spriteBatch) {
			GraphicsDevice gd = Main.graphics.GraphicsDevice;
			EnsureMeshEffect(gd);

			try {
				spriteBatch.End();
			}
			catch {
			}

			BlendState oldBlend = gd.BlendState;
			RasterizerState oldRaster = gd.RasterizerState;
			DepthStencilState oldDepth = gd.DepthStencilState;

			gd.BlendState = BlendState.AlphaBlend;
			gd.RasterizerState = RasterizerState.CullNone;
			gd.DepthStencilState = DepthStencilState.None;

			_meshEffect.View = Matrix.Identity;
			_meshEffect.World = Matrix.Identity;
			_meshEffect.Projection = Matrix.CreateOrthographicOffCenter(
				0f, Main.screenWidth, Main.screenHeight, 0f, 0f, 1f);

			VertexPositionColor[] arr = MeshBuffer.ToArray();
			foreach (EffectPass pass in _meshEffect.CurrentTechnique.Passes) {
				pass.Apply();
				gd.DrawUserPrimitives(PrimitiveType.TriangleList, arr, 0, arr.Length / 3);
			}

			gd.BlendState = oldBlend;
			gd.RasterizerState = oldRaster;
			gd.DepthStencilState = oldDepth;

			spriteBatch.Begin(
				SpriteSortMode.Deferred,
				BlendState.AlphaBlend,
				Main.DefaultSamplerState,
				DepthStencilState.None,
				Main.Rasterizer,
				null,
				Main.GameViewMatrix.TransformationMatrix);
		}

		private static long PosKey(Vector2 p) {
			const float quant = 2f;
			int x = (int)MathF.Round(p.X / quant);
			int y = (int)MathF.Round(p.Y / quant);
			return ((long)x << 32) | (uint)y;
		}

		private static float GetTotalSegmentLength((Vector2 start, Vector2 end)[] segments) {
			float total = 0f;
			foreach (var seg in segments)
				total += seg.start.Distance(seg.end);
			return total;
		}

		public static void DrawRingMesh(
			SpriteBatch spriteBatch,
			Vector2 worldCenter,
			float radius,
			float thickness,
			Color edgeColor) {
			const int segments = 64;
			float innerR = MathF.Max(0f, radius - thickness);
			Color innerColor = Color.Transparent;
			Vector2 screenC = worldCenter - Main.screenPosition;

			var verts = new VertexPositionColor[segments * 6];
			int vi = 0;

			for (int i = 0; i < segments; i++) {
				float a0 = MathHelper.TwoPi * i / segments;
				float a1 = MathHelper.TwoPi * (i + 1) / segments;

				Vector2 o0 = Vector2.Transform(screenC + new Vector2(MathF.Cos(a0), MathF.Sin(a0)) * radius, Main.GameViewMatrix.TransformationMatrix);
				Vector2 o1 = Vector2.Transform(screenC + new Vector2(MathF.Cos(a1), MathF.Sin(a1)) * radius, Main.GameViewMatrix.TransformationMatrix);
				Vector2 i0 = Vector2.Transform(screenC + new Vector2(MathF.Cos(a0), MathF.Sin(a0)) * innerR, Main.GameViewMatrix.TransformationMatrix);
				Vector2 i1 = Vector2.Transform(screenC + new Vector2(MathF.Cos(a1), MathF.Sin(a1)) * innerR, Main.GameViewMatrix.TransformationMatrix);

				verts[vi++] = MakeVertex(o0, edgeColor);
				verts[vi++] = MakeVertex(o1, edgeColor);
				verts[vi++] = MakeVertex(i0, innerColor);

				verts[vi++] = MakeVertex(o1, edgeColor);
				verts[vi++] = MakeVertex(i1, innerColor);
				verts[vi++] = MakeVertex(i0, innerColor);
			}

			DrawVertices(spriteBatch, verts);
		}

		private static void DrawVertices(SpriteBatch spriteBatch, VertexPositionColor[] verts) {
			GraphicsDevice gd = Main.graphics.GraphicsDevice;
			EnsureMeshEffect(gd);

			try {
				spriteBatch.End();
			}
			catch {
			}

			BlendState oldBlend = gd.BlendState;
			RasterizerState oldRaster = gd.RasterizerState;
			DepthStencilState oldDepth = gd.DepthStencilState;

			gd.BlendState = BlendState.Additive;
			gd.RasterizerState = RasterizerState.CullNone;
			gd.DepthStencilState = DepthStencilState.None;

			_meshEffect.View = Matrix.Identity;
			_meshEffect.World = Matrix.Identity;
			_meshEffect.Projection = Matrix.CreateOrthographicOffCenter(
				0f, Main.screenWidth, Main.screenHeight, 0f, 0f, 1f);

			foreach (EffectPass pass in _meshEffect.CurrentTechnique.Passes) {
				pass.Apply();
				gd.DrawUserPrimitives(PrimitiveType.TriangleList, verts, 0, verts.Length / 3);
			}

			gd.BlendState = oldBlend;
			gd.RasterizerState = oldRaster;
			gd.DepthStencilState = oldDepth;

			spriteBatch.Begin(
				SpriteSortMode.Deferred,
				BlendState.AlphaBlend,
				Main.DefaultSamplerState,
				DepthStencilState.None,
				Main.Rasterizer,
				null,
				Main.GameViewMatrix.TransformationMatrix);
		}
	}
}
