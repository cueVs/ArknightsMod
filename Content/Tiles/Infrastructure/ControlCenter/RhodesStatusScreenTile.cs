using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;
using ArknightsMod.Common;

namespace ArknightsMod.Content.Tiles.Infrastructure.ControlCenter
{
    public class RhodesStatusScreenTile : ModTile
    {
        public override void SetStaticDefaults()
        {
            Main.tileFrameImportant[Type] = true;
            Main.tileLavaDeath[Type] = false;
            TileObjectData.newTile.CopyFrom(TileObjectData.Style3x3Wall);
            TileObjectData.newTile.Width = 32;
            TileObjectData.newTile.Height = 14;
			TileObjectData.newTile.Origin = new Point16(15, 13);
			TileObjectData.newTile.CoordinateHeights = new int[]
			{
				16, 16, 16, 16, 16,
				16, 16, 16, 16, 16,
				16, 16, 16, 16
			};
            TileObjectData.newTile.LavaDeath = false;
            TileObjectData.addTile(Type);
            AddMapEntry(new Color(22, 38, 46));
			AddToArray(ref TileID.Sets.RoomNeeds.CountsAsTorch);
			TileID.Sets.FramesOnKillWall[Type] = true;
            DustType = DustID.Electric;
            Main.tileLighted[Type] = true;
        }
        public override bool CanExplode(int i, int j) => false;
        public override void NumDust(int i, int j, bool fail, ref int num)
        {
            num = fail ? 1 : 3;
        }
        public override void PostDraw(int i, int j, SpriteBatch spriteBatch)
        {
			TileDrawer.DrawTileGlowMask(spriteBatch, i, j, Texture, Type);
		}
        public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
        {
            r = 0.3f;
            g = 0.65f;
            b = 0.65f;
        }
    }
}
