using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent;
using Terraria.GameContent.ObjectInteractions;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace ArknightsMod.Content.Tiles.Furniture
{
	public class DareUsa : ModTile
	{
		public const int NextStyleHeight = 60; // Calculated by adding all CoordinateHeights{16, 16, 18} + CoordinatePaddingFix.Y{2} applied to all of them + 2

		public override void SetStaticDefaults()
		{
			// Properties
			Main.tileFrameImportant[Type] = true;
			Main.tileNoAttach[Type] = true;
			Main.tileLavaDeath[Type] = true;
			// TileID.Sets.HasOutlines[Type] = true;
			TileID.Sets.CanBeSatOnForNPCs[Type] = true; // Facilitates calling ModifySittingTargetInfo for NPCs
			TileID.Sets.CanBeSatOnForPlayers[Type] = true; // Facilitates calling ModifySittingTargetInfo for Players
			TileID.Sets.DisableSmartCursor[Type] = true;

			AddToArray(ref TileID.Sets.RoomNeeds.CountsAsChair);

			// DustType = ModContent.DustType<Sparkle>();

			AdjTiles = new int[] { TileID.Chairs };

			// Names
			LocalizedText name = CreateMapEntryName();
			AddMapEntry(new Color(223, 170, 124), name);

			// Placement
			// TileObjectData.newTile.CopyFrom(TileObjectData.Style1x2);
			TileObjectData.newTile.Width = 4;
			TileObjectData.newTile.Height = 3;
			TileObjectData.newTile.Origin = new Point16(1, 2);
			TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile | AnchorType.SolidWithTop | AnchorType.SolidSide, TileObjectData.newTile.Width, 0);
			TileObjectData.newTile.CoordinateHeights = new[] { 16, 16, 18 };
			TileObjectData.newTile.CoordinateWidth = 16;
			TileObjectData.newTile.CoordinatePadding = 2;
			TileObjectData.newTile.CoordinatePaddingFix = new Point16(0, 2);
			TileObjectData.newTile.UsesCustomCanPlace = true;
			// TileObjectData.newTile.Direction = TileObjectDirection.PlaceLeft;

			// The following 3 lines are needed if you decide to add more styles and stack them vertically
			// TileObjectData.newTile.StyleWrapLimit = 2;
			// TileObjectData.newTile.StyleMultiplier = 2;
			// TileObjectData.newTile.StyleHorizontal = true;

			// TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile);
			// TileObjectData.newAlternate.Direction = TileObjectDirection.PlaceRight;
			// TileObjectData.addAlternate(1); // Facing right will use the second texture style
			TileObjectData.addTile(Type);
		}

		//public override void NumDust(int i, int j, bool fail, ref int num) {
		//	num = fail ? 1 : 3;
		//}

		//public override void KillMultiTile(int i, int j, int frameX, int frameY)
		//{
		//	Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 64, 50, ModContent.ItemType<Items.Placeable.Furniture.DareUsa>());
		//}

		public override bool HasSmartInteract(int i, int j, SmartInteractScanSettings settings)
		{
			return settings.player.IsWithinSnappngRangeToTile(i, j, PlayerSittingHelper.ChairSittingMaxDistance); // Avoid being able to trigger it from long range
		}

		public override void ModifySittingTargetInfo(int i, int j, ref TileRestingInfo info)
		{
			// It is very important to know that this is called on both players and NPCs, so do not use Main.LocalPlayer for example, use info.restingEntity
			Tile tile = Framing.GetTileSafely(i, j);

			//info.directionOffset = info.restingEntity is Player ? 6 : 2; // Default to 6 for players, 2 for NPCs
			//info.visualOffset = Vector2.Zero; // Defaults to (0,0)

			info.TargetDirection = 1;
			//if (tile.TileFrameX != 0) {
			//	info.TargetDirection = 1; // Facing right if sat down on the right alternate (added through addAlternate in SetStaticDefaults earlier)
			//}

			// The anchor represents the bottom-most tile of the chair. This is used to align the entity hitbox
			// Since i and j may be from any coordinate of the chair, we need to adjust the anchor based on that
			info.AnchorTilePosition.X = i;
			if (tile.TileFrameX == 0)
			{
				info.AnchorTilePosition.X += 2;
			}
			else if (tile.TileFrameX == 18)
			{
				info.AnchorTilePosition.X++;
			}
			else if (tile.TileFrameX == 54)
			{
				info.AnchorTilePosition.X--;
			}

			info.AnchorTilePosition.Y = j;
			if (tile.TileFrameY == 0)
			{
				info.AnchorTilePosition.Y += 2; // move it 1 down when you click the top-most tile
			}
			else if (tile.TileFrameY == 18)
			{
				info.AnchorTilePosition.Y++; // move it 1 down when you click the top-most tile
			}
		}

		public override bool RightClick(int i, int j)
		{
			Player player = Main.LocalPlayer;

			if (player.IsWithinSnappngRangeToTile(i, j, PlayerSittingHelper.ChairSittingMaxDistance))
			{ // Avoid being able to trigger it from long range
				player.GamepadEnableGrappleCooldown();
				player.sitting.SitDown(player, i, j);
			}

			return true;
		}

		public override void MouseOver(int i, int j)
		{
			Player player = Main.LocalPlayer;

			if (!player.IsWithinSnappngRangeToTile(i, j, PlayerSittingHelper.ChairSittingMaxDistance))
			{ // Match condition in RightClick. Interaction should only show if clicking it does something
				return;
			}

			player.noThrow = 2;
			player.cursorItemIconEnabled = true;
			player.cursorItemIconID = ModContent.ItemType<Items.Placeable.Furniture.DareUsa>();

			//if (Main.tile[i, j].TileFrameX / 18 < 1) {
			//	player.cursorItemIconReversed = true;
			//}
		}
	}
}
