using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent.ObjectInteractions;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using ArknightsMod.Systems;

namespace ArknightsMod.Content.Tiles.Infrastructure.ReceptionRoom
{
	public class OfficeReclinerTile : ModTile
	{
		private static int ReclinerWidthTiles = 3;
		private static int ReclinerHeightTiles = 3;
		private const int SittingMaxDistance = 40;

		public override string Texture => "ArknightsMod/Content/Items/Placeable/Infrastructure/ReceptionRoom/OfficeRecliner_gap1";

		public override void SetStaticDefaults()
		{
			Main.tileFrameImportant[Type] = true;
			Main.tileObsidianKill[Type] = true;
			TileID.Sets.CanBeSatOnForNPCs[Type] = true;
			TileID.Sets.CanBeSatOnForPlayers[Type] = true;
			AddToArray(ref TileID.Sets.RoomNeeds.CountsAsChair);

			DustType = DustID.Iron;
			AddMapEntry(new Color(106, 106, 101), CreateMapEntryName());

			int[] heights = new int[ReclinerHeightTiles];
			for (int k = 0; k < ReclinerHeightTiles; k++)
				heights[k] = 16;

			TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
			TileObjectData.newTile.Width = ReclinerWidthTiles;
			TileObjectData.newTile.Height = ReclinerHeightTiles;
			int originX = ReclinerWidthTiles / 2;
			TileObjectData.newTile.Origin = new Point16(originX, ReclinerHeightTiles - 1);
			TileObjectData.newTile.CoordinateWidth = 16;
			TileObjectData.newTile.CoordinatePadding = 1;
			TileObjectData.newTile.CoordinateHeights = heights;
			TileObjectData.newTile.Direction = TileObjectDirection.PlaceRight;
			TileObjectData.newTile.StyleWrapLimit = 2;
			TileObjectData.newTile.StyleMultiplier = 2;
			TileObjectData.newTile.StyleHorizontal = true;
			TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile);
			TileObjectData.newAlternate.Direction = TileObjectDirection.PlaceLeft;
			TileObjectData.newAlternate.Origin = new Point16(originX, ReclinerHeightTiles - 1);
			TileObjectData.addAlternate(1);
			TileObjectData.addTile(Type);
		}

		public override void PlaceInWorld(int i, int j, Item item)
		{
			if (Main.netMode == NetmodeID.MultiplayerClient)
				return;
			ReceptionRoomDecorSystem.ConvertPlacedTileToDecor(i, j, Type);
		}

		public override bool CanPlace(int i, int j)
		{
			bool result = base.CanPlace(i, j);
			if (!result && ReceptionRoomDecorSystem.TryFindSameKindAtTile(i, j, Type, out Point16 anchor, out ReceptionRoomDecorSystem.DecorInstance inst)) {
				if (!Main.dedServ) {
					ModContent.GetInstance<global::ArknightsMod.ArknightsMod>().Logger.Info(
						$"[ReceptionRoomDecor] CanPlace overlap allowed {Name} at ({i},{j}) overlapAnchor=({anchor.X},{anchor.Y}) overlapTopLeft=({inst.TopLeft.X},{inst.TopLeft.Y})"
					);
				}
				return true;
			}
			if (!Main.dedServ) {
				Item held = Main.LocalPlayer?.HeldItem;
				TileObjectData d = TileObjectData.GetTileData(Type, 0, 0);
				string dataText = d == null
					? "TileObjectData=null"
					: $"W={d.Width} H={d.Height} Origin=({d.Origin.X},{d.Origin.Y}) AnchorTop={d.AnchorTop.type} AnchorBottom={d.AnchorBottom.type} AnchorLeft={d.AnchorLeft.type} AnchorRight={d.AnchorRight.type} UsesCustomCanPlace={d.UsesCustomCanPlace}";
				ModContent.GetInstance<global::ArknightsMod.ArknightsMod>().Logger.Info(
					$"[ReceptionRoomDecor] CanPlace {Name} at ({i},{j}) => {result} heldType={held?.type ?? -1} createTile={held?.createTile ?? -1} placeStyle={held?.placeStyle ?? -1} {dataText}"
				);
			}
			return result;
		}

		public override bool HasSmartInteract(int i, int j, SmartInteractScanSettings settings)
		{
			return settings.player.IsWithinSnappngRangeToTile(i, j, SittingMaxDistance);
		}

		private static Point16 GetTopLeft(int i, int j)
		{
			const int rowStep = 16 + 1;
			const int colStep = 16 + 1;
			Tile tile = Framing.GetTileSafely(i, j);
			int styleStride = ReclinerWidthTiles * colStep;
			int left = i - (tile.TileFrameX % styleStride) / colStep;
			int top = j - tile.TileFrameY / rowStep;
			return new Point16(left, top);
		}

		public override void ModifySittingTargetInfo(int i, int j, ref TileRestingInfo info)
		{
			const int rowStep = 16 + 1;
			const int colStep = 16 + 1;
			Tile tile = Framing.GetTileSafely(i, j);
			int styleStride = ReclinerWidthTiles * colStep;
			int style = tile.TileFrameX / styleStride;
			info.AnchorTilePosition.X = i;
			info.AnchorTilePosition.Y = j;
			info.TargetDirection = info.RestingEntity is Player player ? player.direction : (style == 0 ? 1 : -1);
			info.VisualOffset.Y -= 2f;
		}

		internal static bool TrySetFacing(int i, int j, int targetDirection)
		{
			if (targetDirection != 1 && targetDirection != -1)
				return false;
			const int step = 16 + 1;
			int styleStride = ReclinerWidthTiles * step;
			Point16 topLeft = GetTopLeft(i, j);
			Tile origin = Framing.GetTileSafely(topLeft.X, topLeft.Y);
			int currentStyle = origin.TileFrameX / styleStride;
			int desiredStyle = targetDirection == 1 ? 0 : 1;
			int deltaStyle = desiredStyle - currentStyle;
			if (deltaStyle == 0)
				return false;
			short deltaX = (short)(deltaStyle * styleStride);
			for (int x = 0; x < ReclinerWidthTiles; x++)
				for (int y = 0; y < ReclinerHeightTiles; y++) {
					Tile t = Framing.GetTileSafely(topLeft.X + x, topLeft.Y + y);
					t.TileFrameX += deltaX;
				}
			if (Main.netMode != NetmodeID.SinglePlayer)
				NetMessage.SendTileSquare(-1, topLeft.X, topLeft.Y, ReclinerWidthTiles, ReclinerHeightTiles);
			return true;
		}

		public override bool RightClick(int i, int j)
		{
			Player player = Main.LocalPlayer;
			Point16 topLeft = GetTopLeft(i, j);
			Point16 seat = new Point16(topLeft.X + (ReclinerWidthTiles / 2), topLeft.Y + ReclinerHeightTiles - 2);
			if (player.IsWithinSnappngRangeToTile(seat.X, seat.Y, SittingMaxDistance)) {
				player.GamepadEnableGrappleCooldown();
				player.sitting.SitDown(player, seat.X, seat.Y);
			}
			return true;
		}

		public override bool PreDrawPlacementPreview(int i, int j, SpriteBatch spriteBatch, ref Rectangle frame, ref Vector2 position, ref Color color, bool validPlacement, ref SpriteEffects spriteEffects)
		{
			const int step = 16 + 1;
			int styleStride = ReclinerWidthTiles * step;
			int style = frame.X / styleStride;
			bool flip = style == 1;
			int localX = (frame.X % styleStride) / step;
			frame.X = (flip ? (ReclinerWidthTiles - 1 - localX) : localX) * step;
			if (flip) {
				spriteEffects |= SpriteEffects.FlipHorizontally;
			}
			return true;
		}

		public override bool PreDraw(int i, int j, SpriteBatch spriteBatch)
		{
			const int step = 16 + 1;
			Tile tile = Framing.GetTileSafely(i, j);
			int styleStride = ReclinerWidthTiles * step;
			int style = tile.TileFrameX / styleStride;
			bool flip = style == 1;
			int localX = (tile.TileFrameX % styleStride) / step;
			int localY = tile.TileFrameY / step;

			int srcX = (flip ? (ReclinerWidthTiles - 1 - localX) : localX) * step;
			int srcY = localY * step;
			Rectangle frame = new Rectangle(srcX, srcY, 16, 16);

			Vector2 zero = Main.drawToScreen ? Vector2.Zero : new Vector2(Main.offScreenRange);
			Vector2 pos = new Vector2(i * 16 - (int)Main.screenPosition.X, j * 16 - (int)Main.screenPosition.Y) + zero;
			Color color = Lighting.GetColor(i, j);

			Texture2D texture = ModContent.Request<Texture2D>(Texture, AssetRequestMode.ImmediateLoad).Value;
			SpriteEffects effects = flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
			Vector2 origin = flip ? new Vector2(16, 0) : Vector2.Zero;
			Vector2 drawPos = flip ? pos + new Vector2(16, 0) : pos;
			spriteBatch.Draw(texture, drawPos, frame, color, 0f, origin, 1f, effects, 0f);
			return false;
		}
	}
}
