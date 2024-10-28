using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
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
            List<CustomVertexInfo> bars = new List<CustomVertexInfo>();
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
            List<CustomVertexInfo> Vx = new List<CustomVertexInfo>();
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
			List<CustomVertexInfo> bars = new List<CustomVertexInfo>();
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
			List<CustomVertexInfo> Vx = new List<CustomVertexInfo>();
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
    }
}
