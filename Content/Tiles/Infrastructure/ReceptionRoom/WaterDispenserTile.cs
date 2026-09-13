using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.Chat;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent.ObjectInteractions;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;
using ArknightsMod.Systems;

namespace ArknightsMod.Content.Tiles.Infrastructure.ReceptionRoom
{
	public class WaterDispenserTile : ModTile
	{
		private static int DispenserWidthTiles = 5;
		private static int DispenserHeightTiles = 7;
		internal static ulong LastCoffeeTick;
		internal const int CoffeeCooldownTicks = 60 * 60;

		public override string Texture => "ArknightsMod/Content/Items/Placeable/Infrastructure/ReceptionRoom/WaterDispenser_gap1";

		public override void SetStaticDefaults()
		{
			Main.tileFrameImportant[Type] = true;
			Main.tileObsidianKill[Type] = true;

			DustType = DustID.Iron;
			AddMapEntry(new Color(106, 106, 101), CreateMapEntryName());

			int[] heights = new int[DispenserHeightTiles];
			for (int k = 0; k < DispenserHeightTiles; k++)
				heights[k] = 16;

			TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
			TileObjectData.newTile.Width = DispenserWidthTiles;
			TileObjectData.newTile.Height = DispenserHeightTiles;
			int originX = DispenserWidthTiles / 2;
			TileObjectData.newTile.Origin = new Point16(originX, DispenserHeightTiles - 1);
			TileObjectData.newTile.CoordinateWidth = 16;
			TileObjectData.newTile.CoordinatePadding = 1;
			TileObjectData.newTile.CoordinateHeights = heights;
			TileObjectData.newTile.Direction = TileObjectDirection.PlaceRight;
			TileObjectData.newTile.StyleWrapLimit = 2;
			TileObjectData.newTile.StyleMultiplier = 2;
			TileObjectData.newTile.StyleHorizontal = true;
			TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile);
			TileObjectData.newAlternate.Direction = TileObjectDirection.PlaceLeft;
			TileObjectData.newAlternate.Origin = new Point16(originX, DispenserHeightTiles - 1);
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
			return true;
		}

		public override bool RightClick(int i, int j)
		{
			Player player = Main.LocalPlayer;
			if (Main.netMode == NetmodeID.MultiplayerClient) {
				ModPacket p = Mod.GetPacket();
				p.Write((short)ArknightsMod.ArkMessageID.CoffeeMachineRequest);
				p.Send();
				return true;
			}
			TryGiveCoffee(player);
			return true;
		}

		internal static void TryGiveCoffee(Player player)
		{
			ulong now = Main.GameUpdateCount;
			if (LastCoffeeTick != 0 && now - LastCoffeeTick < CoffeeCooldownTicks) {
				if (Main.netMode == NetmodeID.SinglePlayer) {
					Main.NewText("咖啡机还在冷却中。", 200, 200, 200);
				}
				else if (Main.netMode == NetmodeID.Server) {
					ChatHelper.SendChatMessageToClient(NetworkText.FromLiteral("咖啡机还在冷却中。"), new Color(200, 200, 200), player.whoAmI);
				}
				return;
			}
			LastCoffeeTick = now;
			player.QuickSpawnItem(player.GetSource_TileInteraction(Player.tileTargetX, Player.tileTargetY), 5042, 1);
		}

		public override bool PreDrawPlacementPreview(int i, int j, SpriteBatch spriteBatch, ref Rectangle frame, ref Vector2 position, ref Color color, bool validPlacement, ref SpriteEffects spriteEffects)
		{
			const int step = 16 + 1;
			int styleStride = DispenserWidthTiles * step;
			int style = frame.X / styleStride;
			bool flip = style == 0;
			int localX = (frame.X % styleStride) / step;
			frame.X = (flip ? (DispenserWidthTiles - 1 - localX) : localX) * step;
			if (flip) {
				spriteEffects |= SpriteEffects.FlipHorizontally;
			}
			return true;
		}

		public override bool PreDraw(int i, int j, SpriteBatch spriteBatch)
		{
			const int step = 16 + 1;
			Tile tile = Framing.GetTileSafely(i, j);
			int styleStride = DispenserWidthTiles * step;
			int style = tile.TileFrameX / styleStride;
			bool flip = style == 0;
			int localX = (tile.TileFrameX % styleStride) / step;
			int localY = tile.TileFrameY / step;

			int srcX = (flip ? (DispenserWidthTiles - 1 - localX) : localX) * step;
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
