using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.DataStructures;
using Terraria.ModLoader;
using Terraria.ObjectData;
using Microsoft.Xna.Framework.Graphics;
using ArknightsMod.Common;
using Terraria.Enums;

namespace ArknightsMod.Content.Tiles.Infrastructure
{
	public class SmallDisplayTile : ModTile
	{
        public override void SetStaticDefaults()
        {
            Main.tileFrameImportant[Type] = true;
            Main.tileObsidianKill[Type] = true;
			DustType = DustID.Electric;
			Main.tileLighted[Type] = true;
			AddMapEntry(Color.Olive);

			TileObjectData.newTile.CopyFrom(TileObjectData.Style2x2);
			TileObjectData.newTile.Origin = new Point16(0, 1);
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
		public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b) {
			r = 0.38f;
			g = 0.85f;
			b = 1f;
		}
		public override void PostDraw(int i, int j, SpriteBatch spriteBatch)
		{
			TileDrawer.DrawTileGlowMask(spriteBatch, i, j, Texture, Type);
		}
	}
}
