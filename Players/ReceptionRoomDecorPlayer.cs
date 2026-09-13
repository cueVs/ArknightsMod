using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Players
{
	public class ReceptionRoomDecorPlayer : ModPlayer
	{
		private Point16? _sittingAnchor;
		private int _sittingIndex;
		private Point16? _seatTile;
		private bool _wasSitting;

		internal void SetSitting(Point16 anchor, int index, Point16 seatTile)
		{
			_sittingAnchor = anchor;
			_sittingIndex = index;
			_seatTile = seatTile;
		}

		internal bool TryGetSittingInstance(out int anchorX, out int anchorY, out int index)
		{
			if (_sittingAnchor == null) {
				anchorX = anchorY = index = 0;
				return false;
			}
			Point16 p = _sittingAnchor.Value;
			anchorX = p.X;
			anchorY = p.Y;
			index = _sittingIndex;
			return true;
		}

		public override void PostUpdate()
		{
			bool sitting = Player.sitting.isSitting;
			if (_wasSitting && !sitting) {
				ClearSeatTile();
				_sittingAnchor = null;
				_seatTile = null;
			}
			_wasSitting = sitting;
		}

		private void ClearSeatTile()
		{
			if (_seatTile == null)
				return;
			Point16 p = _seatTile.Value;
			if (!WorldGen.InWorld(p.X, p.Y))
				return;
			Tile t = Framing.GetTileSafely(p.X, p.Y);
			int seatType = ModContent.TileType<global::ArknightsMod.Content.Tiles.Infrastructure.ReceptionRoom.ReceptionRoomSeatTile>();
			if (t.HasTile && t.TileType == seatType) {
				t.HasTile = false;
				if (Main.netMode != NetmodeID.SinglePlayer)
					NetMessage.SendTileSquare(-1, p.X, p.Y, 1);
			}
		}
	}
}
