using ArknightsMod.Content.Tiles.Infrastructure.ReceptionRoom;
using ArknightsMod.Players;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.ObjectData;

namespace ArknightsMod.Systems
{
	public class ReceptionRoomDecorSystem : ModSystem
	{
		private static int _drawProjIndex = -1;
		private static bool _pendingRebuildAnchorIndex;

		public override void OnWorldLoad()
		{
			ReceptionRoomDecorAnchorTE.AnchorByPosition.Clear();
			_pendingRebuildAnchorIndex = true;
		}

		public override void OnWorldUnload()
		{
			ReceptionRoomDecorAnchorTE.AnchorByPosition.Clear();
			_drawProjIndex = -1;
			_pendingRebuildAnchorIndex = false;
		}

		public override void PostUpdateEverything()
		{
			if (Main.dedServ || Main.gameMenu)
				return;
			if (!_pendingRebuildAnchorIndex)
				return;
			_pendingRebuildAnchorIndex = false;
			ReceptionRoomDecorAnchorTE.AnchorByPosition.Clear();
			foreach (var te in TileEntity.ByID.Values) {
				if (te is ReceptionRoomDecorAnchorTE rr)
					ReceptionRoomDecorAnchorTE.AnchorByPosition[rr.Position] = rr;
			}
		}

		public enum DecorKind : byte
		{
			ComputerDesk,
			FileRack,
			OfficeChair,
			OfficeDesk,
			OfficeRecliner,
			SafeBox,
			TrashCan,
			VaseTable,
			WaterDispenser,
		}

		public struct DecorInstance
		{
			public DecorKind Kind;
			public Point16 TopLeft;
			public sbyte Direction;
			public byte Variant;
		}

		private static readonly Dictionary<DecorKind, string> GapTexture = new()
		{
			{ DecorKind.ComputerDesk, "ArknightsMod/Content/Items/Placeable/Infrastructure/ReceptionRoom/ComputerDesk_gap1" },
			{ DecorKind.FileRack, "ArknightsMod/Content/Items/Placeable/Infrastructure/ReceptionRoom/FileRack_gap1" },
			{ DecorKind.OfficeChair, "ArknightsMod/Content/Items/Placeable/Infrastructure/ReceptionRoom/OfficeChair_0_gap1" },
			{ DecorKind.OfficeDesk, "ArknightsMod/Content/Items/Placeable/Infrastructure/ReceptionRoom/OfficeDesk_gap1" },
			{ DecorKind.OfficeRecliner, "ArknightsMod/Content/Items/Placeable/Infrastructure/ReceptionRoom/OfficeRecliner_gap1" },
			{ DecorKind.SafeBox, "ArknightsMod/Content/Items/Placeable/Infrastructure/ReceptionRoom/SafeBox_gap1" },
			{ DecorKind.TrashCan, "ArknightsMod/Content/Items/Placeable/Infrastructure/ReceptionRoom/TrashCan_gap1" },
			{ DecorKind.VaseTable, "ArknightsMod/Content/Items/Placeable/Infrastructure/ReceptionRoom/VaseTable_gap1" },
			{ DecorKind.WaterDispenser, "ArknightsMod/Content/Items/Placeable/Infrastructure/ReceptionRoom/WaterDispenser_gap1" },
		};

		private static readonly Dictionary<DecorKind, string> VariantAltTexture = new()
		{
			{ DecorKind.OfficeChair, "ArknightsMod/Content/Items/Placeable/Infrastructure/ReceptionRoom/OfficeChair_1_gap1" },
		};

		private static readonly Dictionary<string, Asset<Texture2D>> TextureAssetCache = new();
		private static readonly Dictionary<DecorKind, Point16> SizeCache = new();

		public static bool TryMouseOver(int i, int j, Player player)
		{
			if (!TryGetAnchorAt(i, j, out Point16 anchor))
				return false;
			if (!TryGetTopMostHit(anchor, Main.MouseWorld, out int idx, out DecorInstance inst))
				return false;

			player.noThrow = 2;
			return true;
		}

		private static bool TryGetTopMostHitAnywhere(Vector2 mouseWorld, out Point16 anchor, out int index, out DecorInstance inst)
		{
			anchor = default;
			index = -1;
			inst = default;
			foreach (var te in ReceptionRoomDecorAnchorTE.EnumerateAll()) {
				Point16 a = te.Position;
				for (int k = te.Instances.Count - 1; k >= 0; k--) {
					DecorInstance d = te.Instances[k];
					Point16 s = GetSize(d.Kind);
					Rectangle rect = new Rectangle(d.TopLeft.X * 16, d.TopLeft.Y * 16, s.X * 16, s.Y * 16);
					if (rect.Contains(mouseWorld.ToPoint())) {
						anchor = a;
						index = k;
						inst = d;
						return true;
					}
				}
			}
			return false;
		}

		// 两种家具是否允许互相压在一起摆放。同种一直允许（这是这套系统本来就有的
		// 能力）；另外单独放开了"办公桌 + 办公椅"这一对，允许部分重叠——不是泛化成
		// "任意家具都能互相重叠"，只开放这一对明确要求的组合，避免重蹈之前那个
		// "所有家具无差别都能叠"的设计覆盖掉原版住房检测的覆辙。
		private static bool IsOverlapAllowed(DecorKind placing, DecorKind existing)
		{
			if (placing == existing)
				return true;
			return (placing == DecorKind.OfficeDesk && existing == DecorKind.OfficeChair)
				|| (placing == DecorKind.OfficeChair && existing == DecorKind.OfficeDesk);
		}

		// 名字仍叫"SameKind"是历史遗留（外部一堆 Tile 类都在用这个方法名），但
		// 匹配逻辑已经不只是"同种"了，见 IsOverlapAllowed。
		public static bool TryFindSameKindAtTile(int i, int j, int tileType, out Point16 anchor, out DecorInstance inst)
		{
			anchor = default;
			inst = default;
			if (Main.dedServ)
				return false;
			if (!TryMapTileType(tileType, out DecorKind kind))
				return false;

			Point worldTile = new Point(i, j);
			foreach (var te in ReceptionRoomDecorAnchorTE.EnumerateAll()) {
				Point16 a = te.Position;
				for (int k = te.Instances.Count - 1; k >= 0; k--) {
					DecorInstance d = te.Instances[k];
					if (!IsOverlapAllowed(kind, d.Kind))
						continue;
					Point16 s = GetSize(d.Kind);
					Rectangle rect = new Rectangle(d.TopLeft.X, d.TopLeft.Y, s.X, s.Y);
					if (rect.Contains(worldTile)) {
						anchor = a;
						inst = d;
						return true;
					}
				}
			}
			return false;
		}

		public static bool TryRemoveTopMostAt(int i, int j)
		{
			if (!TryGetAnchorAt(i, j, out Point16 anchor))
				return false;
			if (!ReceptionRoomDecorAnchorTE.TryGet(anchor, out ReceptionRoomDecorAnchorTE te))
				return false;
			if (te.Instances.Count <= 0)
				return false;

			DecorInstance inst = te.Instances[te.Instances.Count - 1];
			te.Instances.RemoveAt(te.Instances.Count - 1);
			ModContent.GetInstance<global::ArknightsMod.ArknightsMod>().Logger.Info(
				$"[ReceptionRoomDecor] RemoveTopMost anchor=({anchor.X},{anchor.Y}) removed kind={inst.Kind} topLeft=({inst.TopLeft.X},{inst.TopLeft.Y}) remaining={te.Instances.Count}"
			);

			int itemType = KindToItemType(inst.Kind);
			if (itemType > 0 && Main.netMode != NetmodeID.MultiplayerClient) {
				Rectangle rect = new Rectangle(inst.TopLeft.X * 16, inst.TopLeft.Y * 16, 16, 16);
				Item.NewItem(new EntitySource_TileBreak(anchor.X, anchor.Y), rect, itemType);
			}

			if (te.Instances.Count == 0) {
				ReceptionRoomDecorAnchorTE.AnchorByPosition.Remove(anchor);
				TileEntity.ByPosition.Remove(anchor);
				TileEntity.ByID.Remove(te.ID);
				Tile t = Framing.GetTileSafely(anchor.X, anchor.Y);
				if (t.HasTile && IsAnchorTileType(t.TileType))
					t.HasTile = false;
				if (Main.netMode != NetmodeID.SinglePlayer)
					NetMessage.SendTileSquare(-1, anchor.X, anchor.Y, 1);
				return true;
			}

			// 还剩其它家具：锚点该算桌子还是椅子可能因为这次移除而变了，重新算一次。
			UpdateAnchorTileType(anchor, te);

			// Refresh all remaining instances' covered areas so client tile state stays correct.
			if (Main.netMode != NetmodeID.SinglePlayer) {
				te.SendSync();
				for (int k = 0; k < te.Instances.Count; k++) {
					DecorInstance remain = te.Instances[k];
					Point16 s2 = GetSize(remain.Kind);
					NetMessage.SendTileSquare(-1, remain.TopLeft.X, remain.TopLeft.Y, s2.X, s2.Y);
				}
				return true;
			}

			if (Main.netMode != NetmodeID.SinglePlayer) {
				te.SendSync();
				Point16 s = GetSize(inst.Kind);
				NetMessage.SendTileSquare(-1, inst.TopLeft.X, inst.TopLeft.Y, s.X, s.Y);
			}
			return true;
		}

		public static bool TryRightClick(int i, int j, Player player)
		{
			if (!TryGetAnchorAt(i, j, out Point16 anchor))
				return false;
			if (!TryGetTopMostHit(anchor, Main.MouseWorld, out int idx, out DecorInstance inst))
				return false;

			if (inst.Kind == DecorKind.WaterDispenser) {
				if (Main.netMode == NetmodeID.MultiplayerClient) {
					ModPacket p = ModContent.GetInstance<global::ArknightsMod.ArknightsMod>().GetPacket();
					p.Write((short)global::ArknightsMod.ArknightsMod.ArkMessageID.CoffeeMachineRequest);
					p.Send();
					return true;
				}
				global::ArknightsMod.Content.Tiles.Infrastructure.ReceptionRoom.WaterDispenserTile.TryGiveCoffee(player);
				return true;
			}

			if (inst.Kind == DecorKind.OfficeChair || inst.Kind == DecorKind.OfficeRecliner) {
				Point16 size = GetSize(inst.Kind);
				if (inst.Kind == DecorKind.OfficeChair && inst.Variant == 1)
					return true;
				Point16 seat = inst.Kind == DecorKind.OfficeChair
					? new Point16(inst.TopLeft.X, inst.TopLeft.Y + size.Y - 1)
					: new Point16(inst.TopLeft.X + (size.X / 2), inst.TopLeft.Y + size.Y - 2);
				const int SittingMaxDistance = 40;
				if (!player.IsWithinSnappngRangeToTile(seat.X, seat.Y, SittingMaxDistance))
					return true;
				PlaceSeatTile(seat);
				player.GamepadEnableGrappleCooldown();
				player.sitting.SitDown(player, seat.X, seat.Y);

				ReceptionRoomDecorPlayer mp = player.GetModPlayer<ReceptionRoomDecorPlayer>();
				mp.SetSitting(anchor, idx, seat);
				player.ChangeDir(inst.Direction);
				return true;
			}

			return true;
		}

		private static void PlaceSeatTile(Point16 p)
		{
			if (!WorldGen.InWorld(p.X, p.Y))
				return;
			Tile t = Framing.GetTileSafely(p.X, p.Y);
			int seatType = ModContent.TileType<ReceptionRoomSeatTile>();
			t.HasTile = true;
			t.TileType = (ushort)seatType;
			t.TileFrameX = 0;
			t.TileFrameY = 0;
			if (Main.netMode != NetmodeID.SinglePlayer)
				NetMessage.SendTileSquare(-1, p.X, p.Y, 1);
		}

		public static bool TryGetSittingInfo(Point16 anchor, int index, out int dir, out Vector2 visualOffset)
		{
			dir = 1;
			visualOffset = Vector2.Zero;
			if (!ReceptionRoomDecorAnchorTE.TryGet(anchor, out ReceptionRoomDecorAnchorTE te))
				return false;
			if (index < 0 || index >= te.Instances.Count)
				return false;
			DecorInstance inst = te.Instances[index];
			dir = inst.Direction;
			visualOffset = inst.Kind == DecorKind.OfficeChair ? new Vector2(0f, -6f) : new Vector2(0f, -2f);
			return true;
		}

		internal static void DrawAllDecor(SpriteBatch sb, Vector2 screenPos)
		{
			foreach (var te in ReceptionRoomDecorAnchorTE.EnumerateAll()) {
				for (int k = 0; k < te.Instances.Count; k++) {
					DrawInstance(sb, te.Instances[k], screenPos);
				}
			}
		}

		public override void PostDrawTiles()
		{
			if (Main.dedServ || Main.gameMenu)
				return;
			// Drawing is handled by ReceptionRoomDecorDrawProjectile to guarantee stable ordering.
		}

		public override void PostUpdatePlayers()
		{
			if (Main.dedServ || Main.gameMenu)
				return;
			int owner = Main.myPlayer;
			if (owner < 0 || owner >= Main.maxPlayers)
				return;
			Player p = Main.player[owner];
			if (p == null || !p.active)
				return;
			int type = ModContent.ProjectileType<ReceptionRoomDecorDrawProjectile>();
			bool found = false;
			if (_drawProjIndex >= 0 && _drawProjIndex < Main.maxProjectiles) {
				Projectile pr = Main.projectile[_drawProjIndex];
				if (pr.active && pr.owner == owner && pr.type == type)
					found = true;
			}
			if (!found) {
				for (int i = 0; i < Main.maxProjectiles; i++) {
					Projectile pr = Main.projectile[i];
					if (pr.active && pr.owner == owner && pr.type == type) {
						_drawProjIndex = i;
						found = true;
						break;
					}
				}
			}
			if (!found) {
				int idx = Projectile.NewProjectile(new EntitySource_Misc("ReceptionRoomDecorDraw"), p.Center, Vector2.Zero, type, 0, 0f, owner);
				_drawProjIndex = idx;
			}
		}

		internal static void DrawAllDecorDuringSpriteBatch(SpriteBatch sb, Vector2 screenPos)
		{
			DrawAllDecor(sb, screenPos);
			#if DEBUG
			DrawDebugInstanceRects(sb);
			#endif
		}

		private static void DrawInstance(SpriteBatch sb, DecorInstance inst, Vector2 screenPos)
		{
			Point16 size = GetSize(inst.Kind);
			Texture2D tex = GetTexture(inst);
			if (tex == null || tex.IsDisposed)
				return;
			bool flip = inst.Direction == -1;
			const int step = 16 + 1;
			for (int x = 0; x < size.X; x++) {
				for (int y = 0; y < size.Y; y++) {
					int srcX = (flip ? (size.X - 1 - x) : x) * step;
					int srcY = y * step;
					Rectangle frame = new Rectangle(srcX, srcY, 16, 16);
					Vector2 worldPos = new Vector2((inst.TopLeft.X + x) * 16, (inst.TopLeft.Y + y) * 16);
					Vector2 pos = worldPos - screenPos;
					Color color = Lighting.GetColor(inst.TopLeft.X + x, inst.TopLeft.Y + y);

					SpriteEffects effects = flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
					Vector2 origin = flip ? new Vector2(16, 0) : Vector2.Zero;
					Vector2 drawPos = flip ? pos + new Vector2(16, 0) : pos;
					sb.Draw(tex, drawPos, frame, color, 0f, origin, 1f, effects, 0f);
				}
			}
		}

		private static Texture2D GetTexture(DecorInstance inst)
		{
			Asset<Texture2D> asset = GetTextureAsset(inst);
			Texture2D tex = asset?.Value;
			return tex;
		}

		private static Asset<Texture2D> GetTextureAsset(DecorInstance inst)
		{
			string path = GapTexture[inst.Kind];
			if (inst.Kind == DecorKind.OfficeChair && inst.Variant == 1)
				path = VariantAltTexture[DecorKind.OfficeChair];
			if (TextureAssetCache.TryGetValue(path, out Asset<Texture2D> cached)) {
				Texture2D t = cached?.Value;
				if (t != null && !t.IsDisposed)
					return cached;
			}
			Asset<Texture2D> req = ModContent.Request<Texture2D>(path, AssetRequestMode.ImmediateLoad);
			TextureAssetCache[path] = req;
			return req;
		}

		private static Point16 GetSize(DecorKind kind)
		{
			if (Main.dedServ) {
				return kind switch {
					DecorKind.ComputerDesk => new Point16(5, 4),
					DecorKind.FileRack => new Point16(17, 8),
					DecorKind.OfficeChair => new Point16(2, 3),
					DecorKind.OfficeDesk => new Point16(4, 5),
					DecorKind.OfficeRecliner => new Point16(3, 3),
					DecorKind.SafeBox => new Point16(2, 2),
					DecorKind.TrashCan => new Point16(2, 2),
					DecorKind.VaseTable => new Point16(5, 3),
					DecorKind.WaterDispenser => new Point16(5, 7),
					_ => new Point16(1, 1),
				};
			}
			if (SizeCache.TryGetValue(kind, out Point16 s))
				return s;
			Asset<Texture2D> asset = ModContent.Request<Texture2D>(GapTexture[kind], AssetRequestMode.ImmediateLoad);
			Texture2D tex = asset.Value;
			int w = Math.Max(1, (tex.Width + 1) / 17);
			int h = Math.Max(1, (tex.Height + 1) / 17);
			s = new Point16(w, h);
			SizeCache[kind] = s;
			return s;
		}

		// 原版住房检测（TileID.Sets.RoomNeeds.CountsAsTable/CountsAsChair）只按
		// tile 类型判断，没有"这个格子里具体摆的是什么家具"这种动态钩子——而这套
		// 接待室装饰系统落地后，世界里所有种类的家具（桌子、椅子、垃圾桶……）
		// 统一只留下一个不可见的锚点 tile，真正的外观全靠 DrawAllDecor 自己画。
		// 为了让"算桌子"/"算椅子"的家具能被原版住房检测认出来，锚点 tile 按
		// 家具种类换成对应的子类（ReceptionRoomDecorAnchorTableTile/
		// ReceptionRoomDecorAnchorChairTile），其余种类仍用基类（不参与住房判定）。
		private static int GetAnchorTileType(DecorKind kind) => kind switch {
			DecorKind.OfficeDesk or DecorKind.ComputerDesk or DecorKind.VaseTable
				=> ModContent.TileType<ReceptionRoomDecorAnchorTableTile>(),
			DecorKind.OfficeChair or DecorKind.OfficeRecliner
				=> ModContent.TileType<ReceptionRoomDecorAnchorChairTile>(),
			_ => ModContent.TileType<ReceptionRoomDecorAnchorTile>(),
		};

		// 锚点 tile 现在有三种可能的类型（基类 + 桌子变体 + 椅子变体），凡是判断
		// "这一格是不是锚点"的地方都要认全这三种，不能只认基类那一种。
		internal static bool IsAnchorTileType(int tileType) =>
			tileType == ModContent.TileType<ReceptionRoomDecorAnchorTile>() ||
			tileType == ModContent.TileType<ReceptionRoomDecorAnchorTableTile>() ||
			tileType == ModContent.TileType<ReceptionRoomDecorAnchorChairTile>();

		// 一个锚点格理论上可以叠多件不同种类的家具（同一个 TE 的 Instances 列表），
		// 但 tile 只能是一种类型，只能按优先级二选一：桌子优先于椅子，都没有才用
		// 不参与住房判定的基类。正常玩法里一个锚点通常只对应一件家具，这个优先级
		// 只在故意把两种家具重叠摆在同一格角点时才会用到。
		private static int ComputeAnchorTileType(List<DecorInstance> instances) {
			bool hasTable = false, hasChair = false;
			foreach (DecorInstance inst in instances) {
				int t = GetAnchorTileType(inst.Kind);
				if (t == ModContent.TileType<ReceptionRoomDecorAnchorTableTile>())
					hasTable = true;
				else if (t == ModContent.TileType<ReceptionRoomDecorAnchorChairTile>())
					hasChair = true;
			}
			if (hasTable)
				return ModContent.TileType<ReceptionRoomDecorAnchorTableTile>();
			if (hasChair)
				return ModContent.TileType<ReceptionRoomDecorAnchorChairTile>();
			return ModContent.TileType<ReceptionRoomDecorAnchorTile>();
		}

		// 给 .akstruct 结构格式（AkStructureFormat）导出用：查询某个锚点格当前
		// 挂了哪些家具实例。锚点格本身只是个空壳 tile，真正"这里摆了什么"的数据
		// 全在 TE 的 Instances 里，结构导出如果只存 tile 类型不存这份数据，放到
		// 新位置后就会变成"能挖、有判定，但什么都不画"的空壳——这正是这个方法
		// 存在的原因。
		internal static bool TryGetInstancesAt(Point16 anchorPos, out List<DecorInstance> instances) {
			if (ReceptionRoomDecorAnchorTE.TryGet(anchorPos, out ReceptionRoomDecorAnchorTE te)) {
				instances = te.Instances;
				return true;
			}
			instances = null;
			return false;
		}

		// 给 .akstruct 结构放置用：在目标世界位置重新创建一条家具实例记录
		// （锚点 tile + TE + Instances 项），照抄 ConvertPlacedTileToDecor 里
		// "创建锚点 → 加实例 → 按实例重算锚点类型 → 联机同步"这一套流程，
		// 只是数据来源换成结构文件里存的记录，而不是玩家刚刚放下的物品。
		internal static void RestoreInstanceAtAnchor(Point16 anchorPos, DecorKind kind, Point16 topLeft, sbyte direction, byte variant) {
			EnsureAnchorAndTE(anchorPos, kind, out ReceptionRoomDecorAnchorTE te);
			te.Instances.Add(new DecorInstance {
				Kind = kind,
				TopLeft = topLeft,
				Direction = direction,
				Variant = variant,
			});
			UpdateAnchorTileType(anchorPos, te);
			if (Main.netMode != NetmodeID.SinglePlayer) {
				te.SendSync();
				Point16 s = GetSize(kind);
				NetMessage.SendTileSquare(-1, topLeft.X, topLeft.Y, s.X, s.Y);
			}
		}

		// 按当前锚点上所有家具重新算一次"这一格该是哪种锚点 tile"，类型变了就
		// 原地切换 + 联机同步。放实例/删实例之后都要调用一次。
		private static void UpdateAnchorTileType(Point16 anchorPos, ReceptionRoomDecorAnchorTE te) {
			int desired = ComputeAnchorTileType(te.Instances);
			Tile t = Framing.GetTileSafely(anchorPos.X, anchorPos.Y);
			if (!t.HasTile || t.TileType == desired)
				return;
			t.TileType = (ushort)desired;
			if (Main.netMode != NetmodeID.SinglePlayer)
				NetMessage.SendTileSquare(-1, anchorPos.X, anchorPos.Y, 1);
		}

		private static Point16 GetSeatTile(DecorInstance inst)
		{
			Point16 size = GetSize(inst.Kind);
			int seatX = inst.TopLeft.X + (size.X / 2);
			int seatY = inst.TopLeft.Y + size.Y - 1;
			return new Point16(seatX, seatY);
		}

		private static bool TryGetAnchorAt(int i, int j, out Point16 anchor)
		{
			anchor = new Point16(i, j);
			Tile t = Framing.GetTileSafely(i, j);
			if (!t.HasTile)
				return false;
			return IsAnchorTileType(t.TileType);
		}

		private static bool TryGetTopMostHit(Point16 anchor, Vector2 mouseWorld, out int index, out DecorInstance inst)
		{
			index = -1;
			inst = default;
			if (!ReceptionRoomDecorAnchorTE.TryGet(anchor, out ReceptionRoomDecorAnchorTE te))
				return false;

			for (int k = te.Instances.Count - 1; k >= 0; k--) {
				DecorInstance d = te.Instances[k];
				Point16 s = GetSize(d.Kind);
				Rectangle rect = new Rectangle(d.TopLeft.X * 16, d.TopLeft.Y * 16, s.X * 16, s.Y * 16);
				if (rect.Contains(mouseWorld.ToPoint())) {
					index = k;
					inst = d;
					return true;
				}
			}
			return false;
		}

		public static bool ConvertPlacedTileToDecor(int i, int j, int tileType)
		{
			if (!TryMapTileType(tileType, out DecorKind kind))
				return false;

			// If the player is placing on top of an existing instance of the same kind,
			// stack it into that existing anchor TE to avoid overwriting/clearing anchors.
			Point16 anchorPos;
			if (!TryFindSameKindAtTile(i, j, tileType, out anchorPos, out _))
				anchorPos = new Point16(i, j);

			if (!Main.dedServ) {
				TileObjectData d = TileObjectData.GetTileData(tileType, 0, 0);
				string dataText = d == null
					? "TileObjectData=null"
					: $"W={d.Width} H={d.Height} Origin=({d.Origin.X},{d.Origin.Y}) AnchorTop={d.AnchorTop.type} AnchorBottom={d.AnchorBottom.type} AnchorLeft={d.AnchorLeft.type} AnchorRight={d.AnchorRight.type} UsesCustomCanPlace={d.UsesCustomCanPlace}";
				Tile placed = Framing.GetTileSafely(i, j);
				ModContent.GetInstance<global::ArknightsMod.ArknightsMod>().Logger.Info(
					$"[ReceptionRoomDecor] ConvertPlacedTileToDecor at ({i},{j}) tileType={tileType} kind={kind} frame=({placed.TileFrameX},{placed.TileFrameY}) {dataText}"
				);
			}

			Point16 topLeft = GetTopLeftByFrame(i, j, kind);
			Tile origin = Framing.GetTileSafely(topLeft.X, topLeft.Y);

			(sbyte direction, byte variant) = ExtractState(kind, origin);

			EnsureAnchorAndTE(anchorPos, kind, out ReceptionRoomDecorAnchorTE te);
			DecorInstance inst = new DecorInstance {
				Kind = kind,
				TopLeft = topLeft,
				Direction = direction,
				Variant = variant,
			};
			if (!Main.dedServ) {
				ModContent.GetInstance<global::ArknightsMod.ArknightsMod>().Logger.Info(
					$"[ReceptionRoomDecor] AddInstance kind={kind} placeAt=({i},{j}) anchor=({anchorPos.X},{anchorPos.Y}) topLeft=({topLeft.X},{topLeft.Y}) dir={direction} variant={variant}"
				);
			}
			if (!Main.dedServ) {
				ModContent.GetInstance<global::ArknightsMod.ArknightsMod>().Logger.Info(
					$"[ReceptionRoomDecor] instance kind={kind} anchor=({anchorPos.X},{anchorPos.Y}) topLeft=({topLeft.X},{topLeft.Y}) dir={direction} variant={variant} size=({GetSize(kind).X},{GetSize(kind).Y})"
				);
			}
			te.Instances.Add(inst);
			ClearObjectTiles(topLeft, kind, anchorPos);
			UpdateAnchorTileType(anchorPos, te);
			if (Main.netMode != NetmodeID.SinglePlayer) {
				te.SendSync();
				NetMessage.SendTileSquare(-1, topLeft.X, topLeft.Y, GetSize(kind).X, GetSize(kind).Y);
			}
			return true;
		}

		public override void PostUpdateInput()
		{
			if (Main.dedServ || Main.gameMenu)
				return;
			Player player = Main.LocalPlayer;
			if (player == null)
				return;
			if (player.mouseInterface)
				return;
			if (!TryGetTopMostHitAnywhere(Main.MouseWorld, out Point16 anchor, out int idx, out DecorInstance inst))
				return;

			player.noThrow = 2;

			if (Main.mouseRight && Main.mouseRightRelease)
				TryRightClick(anchor.X, anchor.Y, player);
		}

		private static void EnsureAnchorAndTE(Point16 anchorPos, DecorKind kind, out ReceptionRoomDecorAnchorTE te)
		{
			// 这里先按"这一件家具自己"的种类选一个初始锚点类型；如果是叠加到已有
			// 锚点上（stack 到同类家具），UpdateAnchorTileType 会在加入实例之后
			// 按全部实例重新算一次最终类型，这里的初始选择只是给"全新创建锚点"
			// 那条路径用的。
			int anchorType = GetAnchorTileType(kind);
			Tile t = Framing.GetTileSafely(anchorPos.X, anchorPos.Y);
			if (!t.HasTile || !IsAnchorTileType(t.TileType)) {
				t.HasTile = true;
				t.TileType = (ushort)anchorType;
				t.TileFrameX = 0;
				t.TileFrameY = 0;
			}
			if (!ReceptionRoomDecorAnchorTE.TryGet(anchorPos, out te)) {
				int id = ModContent.GetInstance<ReceptionRoomDecorAnchorTE>().Place(anchorPos.X, anchorPos.Y);
				te = (ReceptionRoomDecorAnchorTE)TileEntity.ByID[id];
				ReceptionRoomDecorAnchorTE.AnchorByPosition[anchorPos] = te;
			}
		}

		private static void ClearObjectTiles(Point16 topLeft, DecorKind kind, Point16 anchorPos)
		{
			Point16 s = GetSize(kind);
			int skipX = anchorPos.X - topLeft.X;
			int skipY = anchorPos.Y - topLeft.Y;
			for (int x = 0; x < s.X; x++)
				for (int y = 0; y < s.Y; y++) {
					if (x == skipX && y == skipY)
						continue;
					Tile t = Framing.GetTileSafely(topLeft.X + x, topLeft.Y + y);
					if (t.HasTile && IsAnchorTileType(t.TileType))
						continue;
					if (t.HasTile)
						t.HasTile = false;
				}
		}

		private static void DrawDebugInstanceRects(SpriteBatch spriteBatch)
		{
			Texture2D px = Terraria.GameContent.TextureAssets.MagicPixel.Value;
			foreach (var te in ReceptionRoomDecorAnchorTE.EnumerateAll()) {
				for (int k = 0; k < te.Instances.Count; k++) {
					DecorInstance inst = te.Instances[k];
					Point16 s = GetSize(inst.Kind);
					Rectangle worldRect = new Rectangle(inst.TopLeft.X * 16, inst.TopLeft.Y * 16, s.X * 16, s.Y * 16);
					Rectangle screen = new Rectangle(worldRect.X - (int)Main.screenPosition.X, worldRect.Y - (int)Main.screenPosition.Y, worldRect.Width, worldRect.Height);
					Color c = k % 3 == 0 ? new Color(255, 64, 64, 120) : k % 3 == 1 ? new Color(64, 255, 64, 120) : new Color(64, 128, 255, 120);
					int t = 2;
					spriteBatch.Draw(px, new Rectangle(screen.X, screen.Y, screen.Width, t), c);
					spriteBatch.Draw(px, new Rectangle(screen.X, screen.Y + screen.Height - t, screen.Width, t), c);
					spriteBatch.Draw(px, new Rectangle(screen.X, screen.Y, t, screen.Height), c);
					spriteBatch.Draw(px, new Rectangle(screen.X + screen.Width - t, screen.Y, t, screen.Height), c);
				}
			}
		}

		private static (sbyte direction, byte variant) ExtractState(DecorKind kind, Tile origin)
		{
			const int step = 16 + 1;
			byte variant = 0;
			sbyte dir = 1;
			Point16 size = GetSize(kind);
			int styleStride = size.X * step;
			int style = origin.TileFrameX / styleStride;
			bool flipStyleIsOne = kind == DecorKind.OfficeChair || kind == DecorKind.OfficeRecliner;
			bool flip = flipStyleIsOne ? (style == 1) : (style == 0);
			dir = (sbyte)(flip ? -1 : 1);
			if (kind == DecorKind.OfficeChair)
				variant = (byte)(origin.TileFrameY >= 51 ? 1 : 0);
			return (dir, variant);
		}

		private static Point16 GetTopLeftByFrame(int i, int j, DecorKind kind)
		{
			const int step = 16 + 1;
			Point16 s = GetSize(kind);
			Tile tile = Framing.GetTileSafely(i, j);
			int styleStride = s.X * step;
			int left = i - (tile.TileFrameX % styleStride) / step;
			int top = j - (tile.TileFrameY % (s.Y * step)) / step;
			return new Point16(left, top);
		}

		private static bool TryMapTileType(int tileType, out DecorKind kind)
		{
			kind = default;
			if (tileType == ModContent.TileType<global::ArknightsMod.Content.Tiles.Infrastructure.ReceptionRoom.ComputerDeskTile>()) { kind = DecorKind.ComputerDesk; return true; }
			if (tileType == ModContent.TileType<global::ArknightsMod.Content.Tiles.Infrastructure.ReceptionRoom.FileRackTile>()) { kind = DecorKind.FileRack; return true; }
			if (tileType == ModContent.TileType<global::ArknightsMod.Content.Tiles.Infrastructure.ReceptionRoom.OfficeChairTile>()) { kind = DecorKind.OfficeChair; return true; }
			if (tileType == ModContent.TileType<global::ArknightsMod.Content.Tiles.Infrastructure.ReceptionRoom.OfficeDeskTile>()) { kind = DecorKind.OfficeDesk; return true; }
			if (tileType == ModContent.TileType<global::ArknightsMod.Content.Tiles.Infrastructure.ReceptionRoom.OfficeReclinerTile>()) { kind = DecorKind.OfficeRecliner; return true; }
			if (tileType == ModContent.TileType<global::ArknightsMod.Content.Tiles.Infrastructure.ReceptionRoom.SafeBoxTile>()) { kind = DecorKind.SafeBox; return true; }
			if (tileType == ModContent.TileType<global::ArknightsMod.Content.Tiles.Infrastructure.ReceptionRoom.TrashCanTile>()) { kind = DecorKind.TrashCan; return true; }
			if (tileType == ModContent.TileType<global::ArknightsMod.Content.Tiles.Infrastructure.ReceptionRoom.VaseTableTile>()) { kind = DecorKind.VaseTable; return true; }
			if (tileType == ModContent.TileType<global::ArknightsMod.Content.Tiles.Infrastructure.ReceptionRoom.WaterDispenserTile>()) { kind = DecorKind.WaterDispenser; return true; }
			return false;
		}

		private static int KindToItemType(DecorKind kind)
		{
			return kind switch {
				DecorKind.ComputerDesk => ModContent.ItemType<global::ArknightsMod.Content.Items.Placeable.Infrastructure.ReceptionRoom.ComputerDesk>(),
				DecorKind.FileRack => ModContent.ItemType<global::ArknightsMod.Content.Items.Placeable.Infrastructure.ReceptionRoom.FileRack>(),
				DecorKind.OfficeChair => ModContent.ItemType<global::ArknightsMod.Content.Items.Placeable.Infrastructure.ReceptionRoom.OfficeChair>(),
				DecorKind.OfficeDesk => ModContent.ItemType<global::ArknightsMod.Content.Items.Placeable.Infrastructure.ReceptionRoom.OfficeDesk>(),
				DecorKind.OfficeRecliner => ModContent.ItemType<global::ArknightsMod.Content.Items.Placeable.Infrastructure.ReceptionRoom.OfficeRecliner>(),
				DecorKind.SafeBox => ModContent.ItemType<global::ArknightsMod.Content.Items.Placeable.Infrastructure.ReceptionRoom.SafeBox>(),
				DecorKind.TrashCan => ModContent.ItemType<global::ArknightsMod.Content.Items.Placeable.Infrastructure.ReceptionRoom.TrashCan>(),
				DecorKind.VaseTable => ModContent.ItemType<global::ArknightsMod.Content.Items.Placeable.Infrastructure.ReceptionRoom.VaseTable>(),
				DecorKind.WaterDispenser => ModContent.ItemType<global::ArknightsMod.Content.Items.Placeable.Infrastructure.ReceptionRoom.WaterDispenser>(),
				_ => 0,
			};
		}
	}

	public class ReceptionRoomDecorDrawProjectile : ModProjectile
	{
		public override string Texture => global::ArknightsMod.ArknightsMod.noTexture;

		public override void SetStaticDefaults()
		{
			ProjectileID.Sets.DrawScreenCheckFluff[Type] = 100000;
		}

		public override void SetDefaults()
		{
			Projectile.width = 2;
			Projectile.height = 2;
			Projectile.friendly = false;
			Projectile.hostile = false;
			Projectile.penetrate = -1;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
			Projectile.timeLeft = 2;
			Projectile.hide = true;
		}

		public override void AI()
		{
			Projectile.timeLeft = 2;
			if (Projectile.owner >= 0 && Projectile.owner < Main.maxPlayers) {
				Player p = Main.player[Projectile.owner];
				if (p != null && p.active)
					Projectile.Center = p.Center;
			}
		}

		public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
		{
			behindNPCs.Add(index);
		}

		public override bool PreDraw(ref Color lightColor)
		{
			if (Main.dedServ || Main.gameMenu)
				return false;
			ReceptionRoomDecorSystem.DrawAllDecorDuringSpriteBatch(Main.spriteBatch, Main.screenPosition);
			return false;
		}
	}

	public class ReceptionRoomDecorAnchorTE : ModTileEntity
	{
		internal static readonly Dictionary<Point16, ReceptionRoomDecorAnchorTE> AnchorByPosition = new();
		public readonly List<ReceptionRoomDecorSystem.DecorInstance> Instances = new();

		internal static IEnumerable<ReceptionRoomDecorAnchorTE> EnumerateAll()
		{
			if (AnchorByPosition.Count > 0) {
				foreach (var te in AnchorByPosition.Values)
					yield return te;
				yield break;
			}
			foreach (var te in TileEntity.ByID.Values) {
				if (te is ReceptionRoomDecorAnchorTE rr)
					yield return rr;
			}
		}

		public override void OnNetPlace() => AnchorByPosition[Position] = this;
		public override void OnKill() => AnchorByPosition.Remove(Position);

		public override bool IsTileValidForEntity(int x, int y)
		{
			Tile t = Framing.GetTileSafely(x, y);
			return t.HasTile && ReceptionRoomDecorSystem.IsAnchorTileType(t.TileType);
		}

		public override int Hook_AfterPlacement(int i, int j, int type, int style, int direction, int alternate)
		{
			if (Main.netMode == NetmodeID.MultiplayerClient) {
				NetMessage.SendTileSquare(Main.myPlayer, i, j, 1);
				NetMessage.SendData(MessageID.TileEntityPlacement, number: i, number2: j, number3: Type);
				return -1;
			}
			int id = ModContent.GetInstance<ReceptionRoomDecorAnchorTE>().Place(i, j);
			AnchorByPosition[new Point16(i, j)] = (ReceptionRoomDecorAnchorTE)TileEntity.ByID[id];
			return id;
		}

		public override void SaveData(TagCompound tag)
		{
			List<TagCompound> list = new();
			foreach (var inst in Instances) {
				list.Add(new TagCompound {
					["k"] = (byte)inst.Kind,
					["x"] = inst.TopLeft.X,
					["y"] = inst.TopLeft.Y,
					["d"] = (int)inst.Direction,
					["v"] = inst.Variant,
				});
			}
			tag["i"] = list;
		}

		public override void LoadData(TagCompound tag)
		{
			Instances.Clear();
			if (!tag.ContainsKey("i"))
				return;
			foreach (TagCompound t in tag.GetList<TagCompound>("i")) {
				int dir = t.ContainsKey("d") ? t.GetInt("d") : 1;
				Instances.Add(new ReceptionRoomDecorSystem.DecorInstance {
					Kind = (ReceptionRoomDecorSystem.DecorKind)t.GetByte("k"),
					TopLeft = new Point16(t.GetShort("x"), t.GetShort("y")),
					Direction = (sbyte)(dir >= 0 ? 1 : -1),
					Variant = t.GetByte("v"),
				});
			}
		}

		public override void NetSend(BinaryWriter writer)
		{
			writer.Write((ushort)Instances.Count);
			foreach (var inst in Instances) {
				writer.Write((byte)inst.Kind);
				writer.Write(inst.TopLeft.X);
				writer.Write(inst.TopLeft.Y);
				writer.Write(inst.Direction);
				writer.Write(inst.Variant);
			}
		}

		public override void NetReceive(BinaryReader reader)
		{
			Instances.Clear();
			int count = reader.ReadUInt16();
			for (int k = 0; k < count; k++) {
				Instances.Add(new ReceptionRoomDecorSystem.DecorInstance {
					Kind = (ReceptionRoomDecorSystem.DecorKind)reader.ReadByte(),
					TopLeft = new Point16(reader.ReadInt16(), reader.ReadInt16()),
					Direction = reader.ReadSByte(),
					Variant = reader.ReadByte(),
				});
			}
		}

		internal static bool TryGet(Point16 pos, out ReceptionRoomDecorAnchorTE te)
		{
			if (AnchorByPosition.TryGetValue(pos, out te))
				return true;
			if (TileEntity.ByPosition.TryGetValue(pos, out TileEntity baseTe) && baseTe is ReceptionRoomDecorAnchorTE rr) {
				te = rr;
				AnchorByPosition[pos] = rr;
				return true;
			}
			te = null;
			return false;
		}

		internal void SendSync()
		{
			if (Main.netMode == NetmodeID.SinglePlayer)
				return;
			NetMessage.SendData(MessageID.TileEntitySharing, number: ID, number2: Position.X, number3: Position.Y);
		}
	}

}
