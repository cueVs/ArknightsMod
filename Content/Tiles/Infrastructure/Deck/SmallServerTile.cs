using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.DataStructures;
using Terraria.ModLoader;
using Terraria.ObjectData;
using Microsoft.Xna.Framework.Graphics;
using ArknightsMod.Common;
using Terraria.Enums;

namespace ArknightsMod.Content.Tiles.Infrastructure.Deck
{
	public class SmallServerTile : ModTile
	{
        public override void SetStaticDefaults()
        {
            Main.tileFrameImportant[Type] = true;
            Main.tileObsidianKill[Type] = true;
            
			DustType = DustID.Electric;
			Main.tileLighted[Type] = true;
			AddMapEntry(new Color(94, 117, 143));

			TileObjectData.newTile.CopyFrom(TileObjectData.Style5x4);
			TileObjectData.newTile.Origin = new Point16(2, 5);
			TileObjectData.newTile.Width = 5;
			TileObjectData.newTile.Height = 6;
			TileObjectData.newTile.CoordinateHeights = [16, 16, 16, 16, 16, 16];
			TileObjectData.newTile.Direction = TileObjectDirection.PlaceRight;
			TileObjectData.newTile.StyleWrapLimit = 2;
			TileObjectData.newTile.StyleMultiplier = 2;
			TileObjectData.newTile.StyleHorizontal = true;
			TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile);
			TileObjectData.newAlternate.Direction = TileObjectDirection.PlaceLeft;
			TileObjectData.addAlternate(1);
			TileObjectData.addTile(Type);
		}
        public override void NumDust(int i, int j, bool fail, ref int num)
        {
            num = (fail ? 1 : 3);
        }
		public override void PostDraw(int i, int j, SpriteBatch spriteBatch)
		{
			TileDrawer.DrawTileGlowMask(spriteBatch, i, j, Texture, Type);
		}
	}
}
