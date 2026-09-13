using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Localization;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent;

namespace ArknightsMod.Content.Tiles.Infrastructure.Deck
{
	public class YellowRailingTile : ModTile
	{
        public override void SetStaticDefaults()
        {
            Main.tileFrameImportant[Type] = true;
            Main.tileObsidianKill[Type] = true;
			TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
			TileObjectData.newTile.Width = 1;
			TileObjectData.newTile.Height = 1;
			TileObjectData.newTile.CoordinateHeights = [16];
			TileObjectData.addTile(Type);
            DustType = DustID.Copper;
			LocalizedText name = CreateMapEntryName();
			AddMapEntry(new Color(133, 64, 24), name);
		}
        public override void NumDust(int i, int j, bool fail, ref int num)
        {
            num = (fail ? 1 : 3);
        }
		public override bool PreDrawPlacementPreview(int i, int j, SpriteBatch spriteBatch,
			ref Rectangle frame, ref Vector2 position, ref Color color, bool validPlacement,
			ref SpriteEffects spriteEffects) {
			Texture2D texture = TextureAssets.Tile[Type].Value;

			Vector2 zero = Main.drawToScreen ? Vector2.Zero : new Vector2(Main.offScreenRange);

			Main.EntitySpriteDraw(
				texture,
				new Vector2(i * 16 - (int)Main.screenPosition.X, (j - 1) * 16 - (int)Main.screenPosition.Y) + zero,
				null,
				Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0);
			return false;
		}
		public override bool PreDraw(int i, int j, SpriteBatch spriteBatch) {
			Texture2D texture = TextureAssets.Tile[Type].Value;

			Vector2 zero = Main.drawToScreen ? Vector2.Zero : new Vector2(Main.offScreenRange);

			Main.EntitySpriteDraw(
				texture,
				new Vector2(i * 16 - (int)Main.screenPosition.X, (j - 1) * 16 - (int)Main.screenPosition.Y) + zero,
				null,
				Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0);
			return false;
		}
	}
}
