using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace ArknightsMod.Common
{
    public static class TileDrawer
	{
		/// <summary>
		/// 绘制物块的常高光遮罩
		/// Texture为默认为传入的路径+"_Glow"，ModTile类里可以直接传入Texture
		/// Type为传入物块的Type，ModTile类里可以直接传入Type
		/// 在物块不以16*16为基准的话可能会出错
		/// </summary>
		/// <param name="spriteBatch"></param>
		/// <param name="i"></param>
		/// <param name="j"></param>
		/// <param name="Texture">默认为传入的路径+"_Glow"，ModTile类里可以直接传入Texture</param>
		/// <param name="Type">传入物块的Type，ModTile类里可以直接传入Type</param>
		public static void DrawTileGlowMask(SpriteBatch spriteBatch, int i, int j, string Texture, int Type, Color? color = null)
		{
			int xFrameOffset = Main.tile[i, j].TileFrameX;
			int yFrameOffset = Main.tile[i, j].TileFrameY;
			Texture2D glowmask = ModContent.Request<Texture2D>($"{Texture}_Glow").Value;
			Vector2 drawOffest = Main.drawToScreen ? Vector2.Zero : new Vector2(Main.offScreenRange);
			var data = TileObjectData.GetTileData(Type, 0);
			Vector2 drawPosition = new Vector2(i * 16, j * 16) - Main.screenPosition + new Vector2(data.DrawXOffset, data.DrawYOffset) + drawOffest;
			Color drawColor = color ?? Color.White;
			Tile trackTile = Main.tile[i, j];

			if (!trackTile.IsHalfBlock && trackTile.Slope == 0)
				spriteBatch.Draw(glowmask, drawPosition, new Rectangle(xFrameOffset, yFrameOffset, 18, 18), drawColor, 0.0f, Vector2.Zero, 1f, SpriteEffects.None, 0.0f);
			else if (trackTile.IsHalfBlock)
				spriteBatch.Draw(glowmask, drawPosition + new Vector2(0f, 8f), new Rectangle(xFrameOffset, yFrameOffset, 18, 8), drawColor, 0.0f, Vector2.Zero, 1f, SpriteEffects.None, 0.0f);
		}


    }
}
