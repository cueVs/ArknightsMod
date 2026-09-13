using Terraria.DataStructures;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using ArknightsMod.Content.Items.Developer;

namespace ArknightsMod.Systems.Structures
{
	// 手持「折叠式建筑」时，在光标处画出结构的半透明预览——
	// 直接用方块本身的真实贴图按半透明画一遍（而不是纯色方框），
	// 这样"投影"和"完整图纸"这两个需求点是同一份绘制逻辑，
	// 玩家在放置前就能看到和实际生成后几乎一样的画面。
	public class FoldableBuildingPreviewSystem : ModSystem
	{
		private const float PreviewOpacity = 0.6f;

		public override void PostDrawTiles() {
			if (Main.dedServ || Main.gameMenu)
				return;

			Player player = Main.LocalPlayer;
			if (player?.HeldItem?.ModItem is not FoldableBuildingItem)
				return;

			if (!FoldableBuildingItem.TryGetStructure(out StructureData structure))
				return;

			Point16 anchor = FoldableBuildingItem.GetCursorAnchorTile();

			// 同 DevWrenchSelectionSystem 里的说明：PostDrawTiles 触发时
			// spriteBatch 并未 Begin，这里只能自己 Begin、自己 End，不要在前面
			// 多余地先 End()，也不要结束时再多 Begin() 一次去"复原"。
			Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
				SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone,
				null, Main.GameViewMatrix.TransformationMatrix);

			for (int y = 0; y < structure.Height; y++) {
				for (int x = 0; x < structure.Width; x++) {
					StructureCell cell = structure[x, y];
					int worldTileX = anchor.X + x;
					int worldTileY = anchor.Y + y;

					if (cell.HasWall)
						DrawGhostWall(cell, worldTileX, worldTileY);

					if (cell.HasTile)
						DrawGhostTile(cell, worldTileX, worldTileY);
				}
			}

			Main.spriteBatch.End();
		}

		// 墙同样只用保存下来的帧坐标直接取图（旧版 .akstruct 没存墙帧的话
		// 就是默认 0/0），只影响预览拼接是否好看，真正落地后墙帧由
		// StructureData.PlaceAt 里的 SquareWallFrame 重新算过。
		private static void DrawGhostWall(StructureCell cell, int worldTileX, int worldTileY) {
			Main.instance.LoadWall(cell.WallType);
			Texture2D tex = TextureAssets.Wall[cell.WallType].Value;
			if (tex == null)
				return;

			Rectangle source = new(cell.WallFrameX, cell.WallFrameY, 32, 32);
			Vector2 pos = new Vector2(worldTileX * 16 - 8, worldTileY * 16 - 8) - Main.screenPosition;
			Color color = WorldGen.paintColor(cell.WallColor) * PreviewOpacity;

			Main.spriteBatch.Draw(tex, pos, source, color);
		}

		private static void DrawGhostTile(StructureCell cell, int worldTileX, int worldTileY) {
			Main.instance.LoadTiles(cell.TileType);
			Texture2D tex = TextureAssets.Tile[cell.TileType].Value;
			if (tex == null)
				return;

			// 用保存下来的原始帧坐标直接取图。对于"帧不重要"的简单方块（泥土、石头这类），
			// frameX/frameY 在保存时基本是 0，取到的就是它的默认外观，足够当预览用；
			// 对于"帧重要"的大型物块（家具等），frameX/frameY 保存的是它在原来那个位置上
			// 的帧偏移，直接照抄能大致还原外观，但如果新落点导致相邻关系变化，
			// 单靠这一份预览逻辑不会自动重新计算帧——这属于类文件顶部提到的
			// "已知待完善"里"多格家具锚点"问题的一部分，等有真实结构文件测试时再处理。
			Rectangle source = new(cell.TileFrameX, cell.TileFrameY, 16, 16);
			Vector2 pos = new Vector2(worldTileX * 16, worldTileY * 16) - Main.screenPosition;
			Color color = WorldGen.paintColor(cell.TileColor) * PreviewOpacity;

			Main.spriteBatch.Draw(tex, pos, source, color);
		}
	}
}
