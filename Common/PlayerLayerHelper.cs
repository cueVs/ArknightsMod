using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace ArknightsMod.Common
{
    public static class PlayerLayerHelper
	{
		/// <summary>
		/// 绘制玩家层
		/// partNum: 身体部件，Head(0), Body(1), Legs(2)
		/// </summary>
		/// <param name="drawInfo">绘制坐标偏移</param>
		/// <param name="texture"></param>
		/// <param name="partNum">身体部件，Head(0), Body(1), Legs(2)</param>
		/// <param name="drawOffset"></param>
		public static void AddPlayerDrawLayer(ref PlayerDrawSet drawInfo, Texture2D texture, int partNum, Vector2? drawOffset = null)
		{
			Player player = drawInfo.drawPlayer;
			Vector2 offset = drawOffset ?? Vector2.Zero;
			partNum = Math.Clamp(partNum, 0, 2);

			int drawX = (int)(player.MountedCenter.X + offset.X * player.direction - Main.screenPosition.X);
			int drawY = (int)(player.MountedCenter.Y + offset.Y * player.gravDir - Main.screenPosition.Y);

			// 应用染料
			int dyeShader = player.dye?[partNum].dye ?? 0;

			// 应用行走位移
			float offsetY = 0;
			if (player.bodyFrame.Y >= 7 * player.bodyFrame.Height &&
				player.bodyFrame.Y <= 9 * player.bodyFrame.Height ||
				player.bodyFrame.Y >= 14 * player.bodyFrame.Height &&
				player.bodyFrame.Y <= 16 * player.bodyFrame.Height) {
				offsetY = -2 * player.gravDir;
			}

			SpriteEffects effects = SpriteEffects.None;
			if (player.direction == -1)
				effects |= SpriteEffects.FlipHorizontally;
			if (player.gravDir == -1)
				effects |= SpriteEffects.FlipVertically;

			drawInfo.DrawDataCache.Add(
				new DrawData(texture, new Vector2(drawX, drawY + offsetY + player.gfxOffY),
				null, drawInfo.colorArmorBody, 0f, texture.Size() * 0.5f, 1f, effects, 0) {
					shader = dyeShader
				});
		}
    }
}
