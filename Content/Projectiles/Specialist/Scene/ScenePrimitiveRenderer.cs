using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;

namespace ArknightsMod.Content.Projectiles.Specialist.Scene
{
	// 稀音系特效共用的三角面绘制：在世界空间用 VertexPositionColor 直接绘制，NonPremultiplied 混合。
	// 与 PramanixShockwaveRing 相同的「暂停 spriteBatch → DrawUserPrimitives → 还原」流程。
	internal static class ScenePrimitiveRenderer
	{
		private static BasicEffect effect;

		public static VertexPositionColor Vert(Vector2 p, Color col) => new(new Vector3(p.X, p.Y, 0f), col);

		public static void DrawTriangles(List<VertexPositionColor> verts) {
			if (verts == null || verts.Count < 3 || Main.dedServ)
				return;

			GraphicsDevice gd = Main.graphics?.GraphicsDevice;
			if (gd == null)
				return;

			Ensure(gd);
			if (effect == null)
				return;

			VertexPositionColor[] array = verts.ToArray();

			BlendState oldBlend = gd.BlendState;
			RasterizerState oldRaster = gd.RasterizerState;
			DepthStencilState oldDepth = gd.DepthStencilState;
			SpriteBatch sb = Main.spriteBatch;
			try {
				try { sb.End(); } catch { }
				gd.BlendState = BlendState.NonPremultiplied;
				gd.RasterizerState = RasterizerState.CullNone;
				gd.DepthStencilState = DepthStencilState.None;

				Matrix projection = Matrix.CreateOrthographicOffCenter(0f, Main.screenWidth, Main.screenHeight, 0f, 0f, 1f);
				Matrix model = Matrix.CreateTranslation(new Vector3(-Main.screenPosition.X, -Main.screenPosition.Y, 0f)) * Main.GameViewMatrix.ZoomMatrix;
				effect.World = model;
				effect.View = Matrix.Identity;
				effect.Projection = projection;
				foreach (EffectPass pass in effect.CurrentTechnique.Passes) {
					pass.Apply();
					gd.DrawUserPrimitives(PrimitiveType.TriangleList, array, 0, array.Length / 3);
				}
			}
			finally {
				gd.BlendState = oldBlend;
				gd.RasterizerState = oldRaster;
				gd.DepthStencilState = oldDepth;
				try {
					sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
				}
				catch { }
			}
		}

		private static void Ensure(GraphicsDevice gd) {
			if (effect == null || effect.IsDisposed) {
				effect?.Dispose();
				effect = new BasicEffect(gd) {
					VertexColorEnabled = true,
					TextureEnabled = false,
				};
			}
		}
	}
}
