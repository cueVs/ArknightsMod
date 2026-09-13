using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.DataStructures;
using Terraria.ModLoader;
using Terraria.ObjectData;
using Microsoft.Xna.Framework.Graphics;

namespace ArknightsMod.Content.Tiles.Infrastructure.Medical
{
	public class MedicalDeskTile : ModTile
	{
        public override void SetStaticDefaults()
        {
            Main.tileFrameImportant[Type] = true;
            Main.tileObsidianKill[Type] = true;
            TileObjectData.newTile.CopyFrom(TileObjectData.Style5x4);
			TileObjectData.newTile.Origin = new Point16(2, 2);
			TileObjectData.newTile.Width = 5;
			TileObjectData.newTile.Height = 3;
			TileObjectData.newTile.CoordinateHeights = [16, 16, 16];
			TileObjectData.addTile(Type);
			AddToArray(ref TileID.Sets.RoomNeeds.CountsAsTorch);
			AddToArray(ref TileID.Sets.RoomNeeds.CountsAsTable);
			DustType = DustID.Electric;
			Main.tileLighted[Type] = true;
			AddMapEntry(new Color(151, 197, 159));
		}
        public override void NumDust(int i, int j, bool fail, ref int num)
        {
            num = (fail ? 1 : 3);
        }
		public override void PostDraw(int i, int j, SpriteBatch spriteBatch)
		{
			//TileDrawer.DrawTileGlowMask(spriteBatch, i, j, Texture, Type);
		}
		public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
		{
			Tile tile = Framing.GetTileSafely(i, j);
			if (tile.TileFrameY < 1 * 18 && tile.TileFrameX < 2 * 18)
			{
				r = 0.38f;
				g = 0.85f;
				b = 1f;
			}
		}
	}
}
