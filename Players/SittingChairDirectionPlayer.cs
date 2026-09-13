using ArknightsMod.Content.Tiles.Infrastructure.ReceptionRoom;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace ArknightsMod.Players
{
	public class SittingChairDirectionPlayer : ModPlayer
	{
		private int _dirSwitchCooldown;

		public override void PostUpdateMiscEffects()
		{
			if (_dirSwitchCooldown > 0)
				_dirSwitchCooldown--;
			if (!Player.sitting.isSitting)
				return;

			Point coords = (Player.Bottom + new Vector2(0f, -2f)).ToTileCoordinates();
			Tile tile = Framing.GetTileSafely(coords.X, coords.Y);

			int chairTileType = ModContent.TileType<OfficeChairTile>();
			int reclinerTileType = ModContent.TileType<OfficeReclinerTile>();
			if (tile.TileType != chairTileType && tile.TileType != reclinerTileType)
				return;

			int? desiredDir = null;
			if (Player.controlLeft) {
				Player.controlLeft = false;
				if (_dirSwitchCooldown == 0)
					desiredDir = -1;
			}
			if (Player.controlRight) {
				Player.controlRight = false;
				if (_dirSwitchCooldown == 0)
					desiredDir = 1;
			}

			if (desiredDir == null)
				return;

			int dir = desiredDir.Value;
			if (tile.TileType == chairTileType)
				OfficeChairTile.TrySetFacing(coords.X, coords.Y, dir);
			else
				OfficeReclinerTile.TrySetFacing(coords.X, coords.Y, dir);

			Player.ChangeDir(dir);
			_dirSwitchCooldown = 10;
		}
	}
}
