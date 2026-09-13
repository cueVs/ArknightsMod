using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Chat;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using ArknightsMod.Content.Items.Consumables.PortableStructures;

namespace ArknightsMod.Systems.Structures
{
	// 「便携建筑」类物品（PortableStructureItemBase 的所有子类共用）
	// 的落地光标预览 + 联机部署流程。
	//
	// 联机流程（沿用「客户端请求 / 服务器权威」这个标准模式，不是本次新发明的套路；
	// 这套流程最早是在已被本系统取代、现已移除的「便携自建安全屋」上验证可用的）：
	//   1. 客户端左键 → 发送 RequestDeploy 请求包（物品类型 + 落点），
	//      不做任何本地预测、不消耗物品。
	//   2. 服务器收到请求（ReceiveDeployRequest）→ 校验持有物品/位置 →
	//      TryDeployLocally 真正写入方块 →
	//      NetMessage.SendTileSquare 把方块数据同步给所有客户端 →
	//      手动扣除该玩家物品栏一个（这个物品的 UseItem 在服务器上从没被真正
	//      调用过，不会有原版的自动消耗）。
	//   3. 服务器再广播一条"播放出场特效"的包给所有客户端（包括请求者
	//      自己）——这是新加的部分，专门为了让特效在所有人的屏幕上同步播放，
	//      而不是只有放置者能看到。
	public sealed class AkStructureDeploySystem : ModSystem
	{
		// ══════════════════ 光标预览：光标在建筑水平居中、贴底部 ══════════════════

		// 记录"玩家刚刚看到的幽灵投影"落在哪个 topLeft——点击时不再重新读一次
		// 鼠标位置，而是直接复用这个值。原因：UseItem 在 Update 阶段执行，
		// 而玩家点击那一刻实际"看到"的画面来自上一帧 Draw（PostDrawTiles）时
		// 采样的鼠标位置；如果点击时重新采样，两次采样之间哪怕只错开几像素的
		// 鼠标移动，也可能让"看到的投影位置"和"实际落地位置"差了一整格，
		// 就是这个 bug 的来源。改成直接复用投影当帧算出来的 topLeft，
		// 保证"所见即所得"，不会再有这种因采样时机不同导致的偏差。
		private static int _lastPreviewItemType = -1;
		private static Point _lastPreviewTopLeft;

		public static bool TryGetLastPreviewTopLeft(int itemType, out Point topLeft) {
			if (_lastPreviewItemType == itemType) {
				topLeft = _lastPreviewTopLeft;
				return true;
			}
			topLeft = default;
			return false;
		}

		public override void PostDrawTiles() {
			if (Main.dedServ || Main.gameMenu)
				return;

			Player player = Main.LocalPlayer;
			if (player?.HeldItem?.ModItem is not PortableStructureItemBase structureItem)
				return;

			StructureData structure = structureItem.GetStructure();
			if (structure == null)
				return;

			Point topLeft = structureItem.GetDeploymentTopLeft();
			_lastPreviewItemType = structureItem.Type;
			_lastPreviewTopLeft = topLeft;

			Rectangle area = new(topLeft.X, topLeft.Y, structure.Width, structure.Height);
			bool inWorld = WorldGen.InWorld(area.Left, area.Top, 10) && WorldGen.InWorld(area.Right - 1, area.Bottom - 1, 10);

			// 同一个钩子里画两层：半透明真实贴图"投影" + 越界时的红色警示外框。
			// PostDrawTiles 触发时 spriteBatch 未 Begin，只能自己 Begin/End，
			// 不要照抄"先 End 再 Begin"的写法（那个坑已经在扳手那边踩过一次了）。
			Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
				SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone,
				null, Main.GameViewMatrix.TransformationMatrix);

			DrawGhostStructure(structure, topLeft);

			if (!inWorld)
				DrawOutOfBoundsWarning(area);

			Main.spriteBatch.End();
		}

		private static void DrawGhostStructure(StructureData structure, Point topLeft) {
			const float opacity = 0.6f;

			for (int y = 0; y < structure.Height; y++) {
				for (int x = 0; x < structure.Width; x++) {
					StructureCell cell = structure[x, y];
					Vector2 pos = new Vector2((topLeft.X + x) * 16, (topLeft.Y + y) * 16) - Main.screenPosition;

					if (cell.HasWall)
						DrawGhostWall(cell, pos, opacity);

					if (cell.HasTile)
						DrawGhostTile(cell, pos, opacity);
				}
			}
		}

		// 墙先画（在下层），墙没有单独存 FrameX/FrameY 的旧版 .akstruct（版本 1）会用
		// 默认帧 0/0，拼接看起来可能不太连贯，但只影响预览，真正落地后墙的帧由
		// StructureData.PlaceAt 里的 SquareWallFrame 重新算过，不受这里影响。
		private static void DrawGhostWall(StructureCell cell, Vector2 pos, float opacity) {
			Main.instance.LoadWall(cell.WallType);
			Texture2D tex = TextureAssets.Wall[cell.WallType].Value;
			if (tex == null)
				return;

			Rectangle source = new(cell.WallFrameX, cell.WallFrameY, 32, 32);
			Color color = WorldGen.paintColor(cell.WallColor) * opacity;
			Main.spriteBatch.Draw(tex, pos - new Vector2(8f), source, color);
		}

		private static void DrawGhostTile(StructureCell cell, Vector2 pos, float opacity) {
			Main.instance.LoadTiles(cell.TileType);
			Texture2D tex = TextureAssets.Tile[cell.TileType].Value;
			if (tex == null)
				return;

			Rectangle source = new(cell.TileFrameX, cell.TileFrameY, 16, 16);
			Color color = WorldGen.paintColor(cell.TileColor) * opacity;
			Main.spriteBatch.Draw(tex, pos, source, color);
		}

		private static void DrawOutOfBoundsWarning(Rectangle area) {
			Texture2D pixel = TextureAssets.MagicPixel.Value;
			Color color = new Color(255, 80, 80, 180);
			Rectangle screenArea = new(
				area.Left * 16 - (int)Main.screenPosition.X,
				area.Top * 16 - (int)Main.screenPosition.Y,
				area.Width * 16,
				area.Height * 16);

			const int border = 2;
			Main.spriteBatch.Draw(pixel, new Rectangle(screenArea.X, screenArea.Y, screenArea.Width, border), color);
			Main.spriteBatch.Draw(pixel, new Rectangle(screenArea.X, screenArea.Bottom - border, screenArea.Width, border), color);
			Main.spriteBatch.Draw(pixel, new Rectangle(screenArea.X, screenArea.Y, border, screenArea.Height), color);
			Main.spriteBatch.Draw(pixel, new Rectangle(screenArea.Right - border, screenArea.Y, border, screenArea.Height), color);
		}

		// ══════════════════ 联机：客户端请求 ══════════════════

		public static void RequestDeploy(int itemType, Point topLeft) {
			ModPacket packet = ModContent.GetInstance<ArknightsMod>().GetPacket();
			packet.Write((short)ArknightsMod.ArkMessageID.AkStructureRequestDeploy);
			packet.Write((short)itemType);
			packet.Write((short)topLeft.X);
			packet.Write((short)topLeft.Y);
			packet.Send();
		}

		internal static void ReceiveDeployRequest(BinaryReader reader, int whoAmI) {
			short itemType = reader.ReadInt16();
			Point topLeft = new(reader.ReadInt16(), reader.ReadInt16());

			if (Main.netMode != NetmodeID.Server)
				return;
			if (whoAmI < 0 || whoAmI >= Main.maxPlayers)
				return;

			Player player = Main.player[whoAmI];
			if (player == null || !player.active || player.HeldItem.type != itemType || player.HeldItem.stack <= 0)
				return;

			if (ModContent.GetModItem(itemType) is not PortableStructureItemBase structureItem)
				return;

			StructureData structure = structureItem.GetStructure();
			if (structure == null)
				return;

			if (!TryDeployLocally(structure, topLeft, out Rectangle area, out string failureKey)) {
				if (failureKey != null)
					ChatHelper.SendChatMessageToClient(NetworkText.FromKey(failureKey), Color.OrangeRed, whoAmI);
				return;
			}

			// 这个物品的 UseItem 在服务器上从未被真正调用过（客户端那边直接 return false
			// 跳过了原版消耗流程），必须手动扣物品栏 + 手动同步回该客户端，
			// 照抄便携自建安全屋已经验证过的做法。
			player.HeldItem.stack--;
			if (player.HeldItem.stack <= 0)
				player.HeldItem.TurnToAir();
			NetMessage.SendData(MessageID.SyncEquipment, -1, -1, null, player.whoAmI, player.selectedItem);

			BroadcastPlacedEffect(itemType, area);
		}

		// ══════════════════ 联机：服务器广播"播放特效" ══════════════════

		private static void BroadcastPlacedEffect(int itemType, Rectangle area) {
			ModPacket packet = ModContent.GetInstance<ArknightsMod>().GetPacket();
			packet.Write((short)ArknightsMod.ArkMessageID.AkStructurePlacedEffect);
			packet.Write((short)itemType);
			packet.Write((short)area.X);
			packet.Write((short)area.Y);
			packet.Write((short)area.Width);
			packet.Write((short)area.Height);
			packet.Send(); // toClient 默认 -1 = 广播给所有客户端，包括请求部署的那一个

			// 服务器自己是专用服务器时没有画面，不需要（也不能）在这里播放特效；
			// 但如果是"作为主机的玩家"（非专用服务器，Main.netMode == Server 且
			// 本机也在渲染），下面这条广播 Send() 并不会发给"自己"（服务器不会把
			// 自己当成一个 client），所以主机玩家本地也要单独触发一次。
			if (!Main.dedServ)
				TriggerLocalEffect(itemType, area);
		}

		// ══════════════════ 所有客户端（含单人本地路径）：收到广播后触发本地特效 ══════════════════

		internal static void ReceivePlacedEffect(BinaryReader reader) {
			short itemType = reader.ReadInt16();
			Rectangle area = new(reader.ReadInt16(), reader.ReadInt16(), reader.ReadInt16(), reader.ReadInt16());
			TriggerLocalEffect(itemType, area);
		}

		private static void TriggerLocalEffect(int itemType, Rectangle area) {
			if (ModContent.GetModItem(itemType) is not PortableStructureItemBase structureItem)
				return;

			StructureData structure = structureItem.GetStructure();
			if (structure == null)
				return;

			PortableStructureEntryEffect.TryBeginEntryEffect(area, structure);
		}

		// ══════════════════ 实际落地（校验 + 写入 + 联机同步瓦片数据）══════════════════

		// 在单人/服务器权威的那一侧真正把结构写进世界。不要在纯客户端调用这个——
		// 客户端没有权限直接改世界方块，必须走 RequestDeploy 那条网络路径。
		public static bool TryDeployLocally(StructureData structure, Point topLeft, out Rectangle area, out string failureKey) {
			area = new Rectangle(topLeft.X, topLeft.Y, structure.Width, structure.Height);
			failureKey = null;

			if (!WorldGen.InWorld(area.Left, area.Top, 10) || !WorldGen.InWorld(area.Right - 1, area.Bottom - 1, 10)) {
				failureKey = "Mods.ArknightsMod.Items.PortableStructure.Deploy.Failure.OutOfBounds";
				return false;
			}

			structure.PlaceAt(new Terraria.DataStructures.Point16(topLeft.X, topLeft.Y));

			if (Main.netMode != NetmodeID.SinglePlayer)
				NetMessage.SendTileSquare(-1, area.X + area.Width / 2, area.Y + area.Height / 2, System.Math.Max(area.Width, area.Height));

			return true;
		}
	}
}
