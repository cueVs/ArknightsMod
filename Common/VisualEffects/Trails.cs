using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria;

namespace ArknightsMod.Common.VisualEffects
{
    public static class TrailMaker
	{
        /// <summary>
        /// 弹幕拖尾(弹幕，拖尾贴图，拖尾偏移，拖尾颜色1，拖尾颜色2,拖尾宽度，是否采用拖尾逐渐缩小)
        /// </summary>
        public static void ProjectileDrawTailByConstWidth(Projectile Projectile, Texture2D Tail, Vector2 DrawOrigin, Color TailColor1, Color TailColor2, float Width, bool Lerp)
        {
            Vector2 drawOrigin = DrawOrigin;
            List<CustomVertexInfo> bars = [];
            for (int i = 1; i < Projectile.oldPos.Length; ++i)
            {
                if (Projectile.oldPos[i] == Vector2.Zero) break;
                var normalDir = Projectile.oldPos[i - 1] - Projectile.oldPos[i];
                normalDir = Vector2.Normalize(new Vector2(-normalDir.Y, normalDir.X));
                float scale = Projectile.scale * ((Projectile.oldPos.Length - i) / (float)Projectile.oldPos.Length);
                float width = Width;
                if (Lerp)
                {
                    width = Width * scale;
                }
                var factor = i / (float)Projectile.oldPos.Length;
                var w = MathHelper.Lerp(1f, 0.05f, factor);
                var color = Color.Lerp(TailColor1, TailColor2, factor);
                color.A = 0;
                Vector2 offset = Vector2.Zero;
                bars.Add(new CustomVertexInfo(Projectile.oldPos[i] + normalDir * width + drawOrigin - Main.screenPosition + offset, color, new Vector3(factor, 1, w)));
                bars.Add(new CustomVertexInfo(Projectile.oldPos[i] + normalDir * -width + drawOrigin - Main.screenPosition + offset, color, new Vector3(factor, 0, w)));
            }
            List<CustomVertexInfo> Vx = [];
            if (bars.Count > 2)
            {
                Vx.Add(bars[0]);
                Vx.Add(bars[1]);
                Vx.Add(bars[2]);
                for (int i = 0; i < bars.Count - 2; i += 2)
                {
                    Vx.Add(bars[i]);
                    Vx.Add(bars[i + 2]);
                    Vx.Add(bars[i + 1]);

                    Vx.Add(bars[i + 1]);
                    Vx.Add(bars[i + 2]);
                    Vx.Add(bars[i + 3]);
                }
				Main.graphics.GraphicsDevice.Textures[0] = Tail;
				Main.graphics.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, Vx.ToArray(), 0, Vx.Count / 3);
			}
        }

		/// <summary>
		/// NPC拖尾(NPC，拖尾贴图，拖尾偏移，拖尾颜色1，拖尾颜色2,拖尾宽度，是否采用拖尾逐渐缩小)
		/// </summary>
		public static void NPCDrawTailByConstWidth(NPC NPC, Texture2D Tail, Vector2 DrawOrigin, Color TailColor1, Color TailColor2, float Width, bool Lerp) {
			Vector2 drawOrigin = DrawOrigin;
			List<CustomVertexInfo> bars = [];
			for (int i = 1; i < NPC.oldPos.Length; ++i) {
				if (NPC.oldPos[i] == Vector2.Zero)
					break;
				var normalDir = NPC.oldPos[i - 1] - NPC.oldPos[i];
				normalDir = Vector2.Normalize(new Vector2(-normalDir.Y, normalDir.X));
				float scale = NPC.scale * ((NPC.oldPos.Length - i) / (float)NPC.oldPos.Length);
				float width = Width;
				if (Lerp) {
					width = Width * scale;
				}
				var factor = i / (float)NPC.oldPos.Length;
				var w = MathHelper.Lerp(1f, 0.05f, factor);
				var color = Color.Lerp(TailColor1, TailColor2, factor);
				color.A = 0;
				Vector2 offset = Vector2.Zero;
				bars.Add(new CustomVertexInfo(NPC.oldPos[i] + normalDir * width + drawOrigin - Main.screenPosition + offset, color, new Vector3(factor, 1, w)));
				bars.Add(new CustomVertexInfo(NPC.oldPos[i] + normalDir * -width + drawOrigin - Main.screenPosition + offset, color, new Vector3(factor, 0, w)));
			}
			List<CustomVertexInfo> Vx = [];
			if (bars.Count > 2) {
				Vx.Add(bars[0]);
				Vx.Add(bars[1]);
				Vx.Add(bars[2]);
				for (int i = 0; i < bars.Count - 2; i += 2) {
					Vx.Add(bars[i]);
					Vx.Add(bars[i + 2]);
					Vx.Add(bars[i + 1]);

					Vx.Add(bars[i + 1]);
					Vx.Add(bars[i + 2]);
					Vx.Add(bars[i + 3]);
				}
				Main.graphics.GraphicsDevice.Textures[0] = Tail;
				Main.graphics.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, Vx.ToArray(), 0, Vx.Count / 3);
			}
		}

		/// <summary>
		/// 弹幕拖尾(弹幕，拖尾贴图，拖尾偏移，拖尾颜色1，拖尾颜色2,拖尾宽度函数，是否采用拖尾逐渐缩小)
		/// </summary>
		//public static void ProjectileDrawTailByVariantWidth(Projectile Projectile, Texture2D Tail, Vector2 DrawOrigin, Color TailColor1, Color TailColor2, Func<float, float> widthFunc, bool Lerp) {
		//	Vector2 drawOrigin = DrawOrigin;
		//	List<CustomVertexInfo> bars = new List<CustomVertexInfo>();
		//	for (int i = 1; i < Projectile.oldPos.Length; ++i) {
		//		if (Projectile.oldPos[i] == Vector2.Zero)
		//			break;
		//		var normalDir = Projectile.oldPos[i - 1] - Projectile.oldPos[i];
		//		normalDir = Vector2.Normalize(new Vector2(-normalDir.Y, normalDir.X));
		//		float scale = Projectile.scale * ((Projectile.oldPos.Length - i) / (float)Projectile.oldPos.Length);
		//		float width = widthFunc((i - 1f) / (Projectile.oldPos.Length - 1f));
		//		if (Lerp) {
		//			width = widthFunc((i - 1f) / (Projectile.oldPos.Length - 1f)) * scale;
		//		}
		//		var factor = i / (float)Projectile.oldPos.Length;
		//		var w = MathHelper.Lerp(1f, 0.05f, factor);
		//		var color = Color.Lerp(TailColor1, TailColor2, factor);
		//		color.A = 0;
		//		Vector2 offset = Vector2.Zero;
		//		bars.Add(new CustomVertexInfo(Projectile.oldPos[i] + normalDir * width + drawOrigin - Main.screenPosition + offset, color, new Vector3(factor, 1, w)));
		//		bars.Add(new CustomVertexInfo(Projectile.oldPos[i] + normalDir * -width + drawOrigin - Main.screenPosition + offset, color, new Vector3(factor, 0, w)));
		//	}
		//	List<CustomVertexInfo> Vx = new List<CustomVertexInfo>();
		//	if (bars.Count > 2) {
		//		Vx.Add(bars[0]);
		//		Vx.Add(bars[1]);
		//		Vx.Add(bars[2]);
		//		for (int i = 0; i < bars.Count - 2; i += 2) {
		//			Vx.Add(bars[i]);
		//			Vx.Add(bars[i + 2]);
		//			Vx.Add(bars[i + 1]);

		//			Vx.Add(bars[i + 1]);
		//			Vx.Add(bars[i + 2]);
		//			Vx.Add(bars[i + 3]);
		//		}
		//		Main.graphics.GraphicsDevice.Textures[0] = Tail;
		//		Main.graphics.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, Vx.ToArray(), 0, Vx.Count / 3);
		//	}
		//}

		public struct CustomVertexInfo : IVertexType
        {
            private static VertexDeclaration _vertexDeclaration = new VertexDeclaration(new VertexElement[3]
            {
                new VertexElement(0, VertexElementFormat.Vector2, VertexElementUsage.Position, 0),
                new VertexElement(8, VertexElementFormat.Color, VertexElementUsage.Color, 0),
                new VertexElement(12, VertexElementFormat.Vector3, VertexElementUsage.TextureCoordinate, 0)
            });
            /// <summary>
            /// 绘制位置(世界坐标)
            /// </summary>
            public Vector2 Position;
            /// <summary>
            /// 绘制的颜色
            /// </summary>
            public Color Color;
            /// <summary>
            /// 前两个是纹理坐标，最后一个是自定义的
            /// </summary>
            public Vector3 TexCoord;

            public CustomVertexInfo(Vector2 position, Color color, Vector3 texCoord)
            {
                this.Position = position;
                this.Color = color;
                this.TexCoord = texCoord;
            }

            public VertexDeclaration VertexDeclaration => _vertexDeclaration;
        }

		/// <summary>
		/// 沿圆周采样点，用与弹幕拖尾相同的带状三角网格绘制环形亮线（世界坐标）。边按 0→1→…→segments-1→0 闭合，避免 TriangleList 在首尾留缝。
		/// </summary>
		/// <param name="zeroVertexAlpha">为 true 时顶点 A 置 0（与旧龙卷风拖尾一致，依赖外部已处于加法混合）；为 false 时使用不透明通道并在内部切换为加法混合绘制。</param>
		/// <param name="wobblePhase">若 ≥0，则按该相位叠加半径扭动与带宽变化（秒级时间量即可，如 Main.GlobalTimeWrappedHourly）。若为负则保持正圆。</param>
		public static void DrawCircleWindRibbon(Texture2D tail, Vector2 worldCenter, float radius, int segments, float width, Color tailColor1, Color tailColor2, bool lerpWidth, bool zeroVertexAlpha = true, float wobblePhase = -1f) {
			if (tail == null || segments < 3 || radius <= 0f)
				return;

			float RadiusAt(int idx) {
				float t = idx / (float)segments;
				float ang = MathHelper.TwoPi * t;
				float r = radius;
				if (wobblePhase >= 0f) {
					// 冲击波等场景：略保留动感，半径脉动约为原幅度的 40%
					r += 5.5f * (float)Math.Sin(ang * 5f - wobblePhase * 3.2f);
					r += 3.2f * (float)Math.Sin(ang * 11f + wobblePhase * 4.1f);
					r += 2f * (float)Math.Cos(ang * 17f - wobblePhase * 2f);
				}
				return Math.Max(r, 4f);
			}

			Vector2 PointOnRing(int idx) {
				float t = idx / (float)segments;
				float ang = MathHelper.TwoPi * t;
				if (wobblePhase >= 0f) {
					float bend = 0.015f * (float)Math.Sin(ang * 6f - wobblePhase * 5f) + 0.01f * (float)Math.Cos(ang * 14f + wobblePhase * 3f);
					ang += bend;
				}
				return worldCenter + ang.ToRotationVector2() * RadiusAt(idx);
			}

			List<CustomVertexInfo> bars = [];
			for (int i = 0; i < segments; i++) {
				int j = (i + 1) % segments;
				Vector2 prev = PointOnRing(i);
				Vector2 cur = PointOnRing(j);
				Vector2 segDir = Vector2.Normalize(cur - prev);
				if (segDir == Vector2.Zero)
					continue;
				Vector2 normalDir = Vector2.Normalize(new Vector2(-segDir.Y, segDir.X));
				float scale = (segments - j) / (float)segments;
				float wMul = 1f;
				if (wobblePhase >= 0f) {
					float midT = (i + 0.5f) / segments;
					float midAng = MathHelper.TwoPi * midT;
					wMul = 0.78f + 0.22f * (float)Math.Sin(midAng * 9f + wobblePhase * 7f);
					wMul *= 0.94f + 0.06f * (float)Math.Sin(midAng * 19f - wobblePhase * 11f);
					wMul = MathHelper.Max(0.92f, wMul);
				}
				float w = width * wMul;
				if (lerpWidth)
					w *= scale;
				if (wobblePhase >= 0f)
					w = MathHelper.Max(width * 0.86f, w);
				float factor = j / (float)segments;
				float texW = MathHelper.Lerp(1f, 0.05f, factor);
				Color color = Color.Lerp(tailColor1, tailColor2, factor);
				if (zeroVertexAlpha)
					color.A = 0;
				else
					color.A = (byte)MathHelper.Clamp(90 + (int)(165 * (1f - factor * 0.85f)), 70, 255);
				Vector2 offset = Vector2.Zero;
				bars.Add(new CustomVertexInfo(cur + normalDir * w + offset - Main.screenPosition, color, new Vector3(factor, 1, texW)));
				bars.Add(new CustomVertexInfo(cur + normalDir * -w + offset - Main.screenPosition, color, new Vector3(factor, 0, texW)));
			}
			List<CustomVertexInfo> Vx = [];
			if (bars.Count > 2) {
				Vx.Add(bars[0]);
				Vx.Add(bars[1]);
				Vx.Add(bars[2]);
				for (int i = 0; i < bars.Count - 2; i += 2) {
					Vx.Add(bars[i]);
					Vx.Add(bars[i + 2]);
					Vx.Add(bars[i + 1]);
					Vx.Add(bars[i + 1]);
					Vx.Add(bars[i + 2]);
					Vx.Add(bars[i + 3]);
				}
				SpriteBatch sb = Main.spriteBatch;
				GraphicsDevice gd = Main.graphics.GraphicsDevice;
				if (!zeroVertexAlpha) {
					try { sb.End(); } catch { }
					BlendState oldBlend = gd.BlendState;
					RasterizerState oldRaster = gd.RasterizerState;
					gd.BlendState = BlendState.Additive;
					gd.RasterizerState = RasterizerState.CullNone;
					try {
						gd.Textures[0] = tail;
						gd.DrawUserPrimitives(PrimitiveType.TriangleList, Vx.ToArray(), 0, Vx.Count / 3);
					}
					finally {
						gd.BlendState = oldBlend;
						gd.RasterizerState = oldRaster;
						sb.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
					}
				}
				else {
					gd.Textures[0] = tail;
					gd.DrawUserPrimitives(PrimitiveType.TriangleList, Vx.ToArray(), 0, Vx.Count / 3);
				}
			}
		}
    }
}
