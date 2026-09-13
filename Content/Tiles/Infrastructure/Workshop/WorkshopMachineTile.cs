using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace ArknightsMod.Content.Tiles.Infrastructure.Workshop
{
	public class WorkshopMachineTile : ModTile
	{
        public override void SetStaticDefaults()
        {
            Main.tileFrameImportant[Type] = true;
            Main.tileObsidianKill[Type] = true;
            
			AddToArray(ref TileID.Sets.RoomNeeds.CountsAsTorch);
			AddToArray(ref TileID.Sets.RoomNeeds.CountsAsTable);
			DustType = DustID.Electric;
			Main.tileLighted[Type] = true;
			AddMapEntry(new Color(106, 106, 101));

			TileObjectData.newTile.CopyFrom(TileObjectData.Style3x3);
			TileObjectData.newTile.Width = 13;
			TileObjectData.newTile.Height = 8;
			TileObjectData.newTile.CoordinateHeights = [16, 16, 16, 16, 16, 16, 16, 16];
			TileObjectData.newTile.Origin = new Point16(5, 7);
			TileObjectData.addTile(Type);
		}
        public override void NumDust(int i, int j, bool fail, ref int num)
        {
            num = (fail ? 1 : 3);
        }
		public override void PostDraw(int i, int j, SpriteBatch spriteBatch)
		{
		}
		public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
		{
			Tile tile = Framing.GetTileSafely(i, j);
			if (tile.TileFrameY > 3 * 18 && tile.TileFrameY < 5 * 18 &&
				tile.TileFrameX < 1 * 18 && tile.TileFrameX < 4 * 18)
			{
				float strength = 0.0014f;
				r = 124 * strength;
				g = 192 * strength;
				b = 255 * strength;
			}
		}
	}
}
