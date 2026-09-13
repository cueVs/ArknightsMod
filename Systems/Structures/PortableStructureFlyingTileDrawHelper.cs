using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;

namespace ArknightsMod.Systems.Structures
{
	// 画"飞入落位"动画里单个格子的瞬时状态（半透明、带旋转缩放、按光照插值）。
	// 移植自另一个自有项目 PocketStrata 的 PocketFlyingTileDrawHelper
	// （子世界结构出现时的方块飞入效果），改成对接本模组的 StructureCell
	// 而不是那边的 PocketSnapCell，逻辑基本是照搬的——两边字段结构本来就
	// 高度相似，属于同类需求下各自实现出了几乎一样的东西。这里只保留"飞入"
	// 这一半（落点插值 + 渐显），没有移植"飞出"那一半，因为便携建筑只有
	// "凭空出现"，没有"结构消失"的场景，用不上。
	internal static class PortableStructureFlyingTileDrawHelper
	{
		public static void DrawCell(
			SpriteBatch spriteBatch,
			StructureCell cell,
			Vector2 worldCenter,
			float scale,
			float rotation,
			float alpha,
			int tileX,
			int tileY) {
			if (alpha <= 0.01f || scale <= 0.01f || !cell.HasTile)
				return;

			Vector2 screenCenter = worldCenter - Main.screenPosition;
			byte alphaByte = (byte)(255f * Math.Clamp(alpha, 0f, 1f));

			Main.instance.LoadTiles(cell.TileType);
			Texture2D tileTex = TextureAssets.Tile[cell.TileType].Value;
			if (tileTex == null)
				return;

			int sampleX = Utils.Clamp((int)(worldCenter.X / 16f), 1, Main.maxTilesX - 2);
			int sampleY = Utils.Clamp((int)(worldCenter.Y / 16f), 1, Main.maxTilesY - 2);
			Color light = Lighting.GetColor(sampleX, sampleY);
			light.A = alphaByte;

			Rectangle tileFrame = new(cell.TileFrameX, cell.TileFrameY, 16, 16);
			Vector2 origin = tileFrame.Size() * 0.5f;

			spriteBatch.Draw(tileTex, screenCenter, tileFrame, light, rotation, origin, scale, SpriteEffects.None, 0f);
		}
	}
}
