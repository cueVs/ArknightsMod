using ArknightsMod.Players;
using ArknightsMod.Systems;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace ArknightsMod.Content.Tiles.Infrastructure.ReceptionRoom
{
	public class ReceptionRoomSeatTile : ModTile
	{
		public override string Texture => global::ArknightsMod.ArknightsMod.noTexture;

		public override void SetStaticDefaults()
		{
			Main.tileFrameImportant[Type] = true;
			Main.tileNoAttach[Type] = true;
			Main.tileSolid[Type] = false;
			Main.tileSolidTop[Type] = false;
			Main.tileNoFail[Type] = true;
			TileID.Sets.CanBeSatOnForPlayers[Type] = true;
			TileID.Sets.CanBeSatOnForNPCs[Type] = true;

			TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
			TileObjectData.newTile.UsesCustomCanPlace = true;
			TileObjectData.addTile(Type);
		}

		public override bool PreDraw(int i, int j, Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch) => false;

		public override void ModifySittingTargetInfo(int i, int j, ref TileRestingInfo info)
		{
			if (info.RestingEntity is not Player player)
				return;

			ReceptionRoomDecorPlayer mp = player.GetModPlayer<ReceptionRoomDecorPlayer>();
			if (!mp.TryGetSittingInstance(out int anchorX, out int anchorY, out int index))
				return;

			if (!ReceptionRoomDecorSystem.TryGetSittingInfo(new Point16(anchorX, anchorY), index, out int dir, out Vector2 visualOffset))
				return;

			info.AnchorTilePosition.X = i;
			info.AnchorTilePosition.Y = j;
			info.TargetDirection = dir;
			info.VisualOffset += visualOffset;
		}
	}
}
