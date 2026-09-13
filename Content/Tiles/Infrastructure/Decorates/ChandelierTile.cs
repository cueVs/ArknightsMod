using ArknightsMod.Common;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace ArknightsMod.Content.Tiles.Infrastructure.Decorates
{
	public class ChandelierTile : ModTile
	{
		public override void SetStaticDefaults()
		{
			Main.tileFrameImportant[Type] = true;
			Main.tileLavaDeath[Type] = true;
			Main.tileLighted[Type] = true;
			AddToArray(ref TileID.Sets.RoomNeeds.CountsAsTorch);

			DustType = DustID.Electric;

			TileObjectData.newTile.CopyFrom(TileObjectData.Style1x2Top);
			TileObjectData.newTile.Width = 2;
			TileObjectData.newTile.Height = 2;
			TileObjectData.newTile.Origin = new Point16(0, 0);
			TileObjectData.newTile.CoordinateHeights = [16, 16];
			TileObjectData.addTile(Type);

			AddToArray(ref TileID.Sets.RoomNeeds.CountsAsTorch);
			AddMapEntry(new Color(166, 157, 157));
		}

		public override void NumDust(int i, int j, bool fail, ref int num) {
			num = 1;
		}
		public override void PostDraw(int i, int j, SpriteBatch spriteBatch)
		{
			TileDrawer.DrawTileGlowMask(spriteBatch, i, j, Texture, Type);
		}
		public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
		{
			Tile tile = Framing.GetTileSafely(i, j);
			if (tile.TileFrameY >= 1 * 18 && tile.TileFrameY < 2 * 18)
			{
				float strength = 1.5f;
				r = 0.65f * strength;
				g = 0.61f * strength;
				b = 0.61f * strength;
			}
		}
	}
}
