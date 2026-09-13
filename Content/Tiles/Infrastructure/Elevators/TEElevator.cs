using System;
using System.Collections.Generic;
using System.IO;
using ArknightsMod.Content.Tiles.Infrastructure.Elevators;
using ArknightsMod;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.ObjectData;

namespace ArknightsMod.Content.Tiles
{
	public class TEElevator : ModTileEntity
	{
		public enum FloorDetectMode
		{
			ShaftGapOnly = 0,
			ButtonOnly = 1,
			ShaftAndButton = 2
		}

		// 楼层识别模式：每个电梯 TE 独立保存。默认改为“按钮检测”。
		public FloorDetectMode FloorMode = FloorDetectMode.ButtonOnly;

		public static string GetFloorDetectModeLabel(FloorDetectMode mode) => mode switch
		{
			FloorDetectMode.ShaftGapOnly => "井隙",
			FloorDetectMode.ButtonOnly => "按钮",
			_ => "井隙+按钮"
		};

		public void CycleFloorDetectMode() => FloorMode = (FloorDetectMode)(((int)FloorMode + 1) % 3);

		internal static int SuppressKillMultiTileDepth = 0;
		public static bool IsSuppressingKillMultiTile => SuppressKillMultiTileDepth > 0;

		private struct PlatformSnapshot
		{
			public bool HasTile;
			public ushort TileType;
			public short FrameX;
			public short FrameY;
			public SlopeType Slope;
			public bool Half;
			public byte Color;
			public bool HasActuator;
			public bool IsActuated;
			public bool Red;
			public bool Green;
			public bool Blue;
			public bool Yellow;
		}

		private const ushort ShaftWallTileType = 350;
		private const int ElevatorWidth = 4;
		private const int MinOpeningHeight = 3;
		private const int WallSearchRadiusX = 160;
		private const int WallSampleHalfHeight = 6;
		private const int WallSampleMinMatch = 5;
		private const int InsideAirMinMatch = 3;
		// 解决问题期间使用的电梯调试日志，当前不需要输出。
		private const bool ElevatorDebugLogging = false;

		private static readonly SoundStyle ElevatorSfx_PressButton = new SoundStyle("ArknightsMod/Content/Sounds/电梯_按下按钮");
		private static readonly SoundStyle ElevatorSfx_ArriveSoon = new SoundStyle("ArknightsMod/Content/Sounds/电梯_到站_1");
		private static readonly SoundStyle ElevatorSfx_Arrive = new SoundStyle("ArknightsMod/Content/Sounds/电梯_到站");
		private static readonly SoundStyle ElevatorSfx_RunStart = new SoundStyle("ArknightsMod/Content/Sounds/电梯_运行中") { MaxInstances = 1 };
		// 该音效需要“持续播放直到到站”，因此这里用 IsLooped=true。
		private static readonly SoundStyle ElevatorSfx_RunLoop = new SoundStyle("ArknightsMod/Content/Sounds/电梯_运行中_持续") { IsLooped = true, MaxInstances = 1 };

		public readonly List<int> FloorBottomYs = new List<int>();
		public int TargetFloorBottomY = -1;
		public int DebugLastLeftWallX = -1;
		public int DebugLastRightWallX = -1;
		public string DebugWallFailReason = "";
		public int DebugBestLeftCandidateX = -1;
		public int DebugBestLeftWallScore = 0;
		public int DebugBestLeftInsideAirScore = 0;
		public int DebugBestRightCandidateX = -1;
		public int DebugBestRightWallScore = 0;
		public int DebugBestRightInsideAirScore = 0;
		public int DebugDoorCandidateCount = 0;
		public string DebugDoorSample = "";

		private bool _moving = false;
		private int _moveDir = 0;
		private int _accelTicks = 0;
		private float _speedPxPerTick = 0f;
		private float _pixelCarry = 0f;
		private int _desiredTopY = -1;
		private string _lastMoveFailReason = "";
		private float _lastVisualTopLeftYpx = float.NaN;
		public int RenderBaseFrameX = 0;
		public int RenderBaseFrameY = 0;
		private readonly Dictionary<int, float> _riderOffsetY = new Dictionary<int, float>();
		private int _postArriveLockTicks = 0;
		private readonly Dictionary<Point16, PlatformSnapshot> _suppressedPlatforms = new Dictionary<Point16, PlatformSnapshot>();

		// 电梯运行相关音效状态（每次开始搬运时重置）。
		// 由于不同 tModLoader 版本对“停止/追踪声音句柄”的支持不同，这里用 tick 延迟近似 runStart 播放完毕后再启动持续音效。
		private uint _runStartTick = uint.MaxValue;
		private bool _runLoopStarted;
		private bool _aboutToArriveSoundPlayed;
		private bool _arriveSoundPlayed;

		public bool IsMoving => _moving;
		public int MoveDir => _moveDir;
		public float VisualPixelOffset => _moving ? _pixelCarry : 0f;

		public override bool IsLoadingEnabled(Mod mod) => true;

		public override void OnNetPlace()
		{
			ScanFloors();
		}

		public override int Hook_AfterPlacement(int i, int j, int type, int style, int direction, int alternate)
		{
			Point16 topLeft = TileObjectData.TopLeft(i, j);
			if (Main.netMode == NetmodeID.MultiplayerClient)
			{
				NetMessage.SendTileSquare(Main.myPlayer, topLeft.X, topLeft.Y, 3);
				NetMessage.SendData(MessageID.TileEntityPlacement, -1, -1, null, topLeft.X, topLeft.Y, Type, 0f, 0, 0, 0);
				return -1;
			}

			int id = Place(topLeft.X, topLeft.Y);
			((TEElevator)ByID[id]).ScanFloors();
			return id;
		}

		// 客户端 UI / 按钮选层后请求移动；单机与主机直接写 TE，纯客户端走 ModPacket。
		public static void RequestMoveToFloor(int teId, int floorBottomY)
		{
			if (floorBottomY < 0 || teId < 0)
				return;

			if (Main.netMode == NetmodeID.MultiplayerClient)
			{
				ModPacket packet = ModContent.GetInstance<ArknightsMod>().GetPacket();
				packet.Write((short)ArknightsMod.ArkMessageID.ElevatorRequestFloor);
				packet.Write(teId);
				packet.Write(floorBottomY);
				packet.Send();
				return;
			}

			ApplyMoveRequest(teId, floorBottomY);
		}

		internal static void ApplyMoveRequest(int teId, int floorBottomY, int requestingPlayerWhoAmI = -1)
		{
			if (floorBottomY < 0 || teId < 0)
				return;
			if (!TileEntity.ByID.TryGetValue(teId, out TileEntity entity) || entity is not TEElevator te)
				return;

			te.RebindPositionIfMisaligned();
			te.ScanFloors();
			// 客户端只能选择服务端刚扫描到的真实楼层，不能把电梯目标写成任意 Y。
			if (!te.FloorBottomYs.Contains(floorBottomY))
				return;

			if (Main.netMode == NetmodeID.Server)
			{
				if ((uint)requestingPlayerWhoAmI >= Main.maxPlayers)
					return;
				Player requestingPlayer = Main.player[requestingPlayerWhoAmI];
				if (!IsPlayerInElevatorRange(requestingPlayer, te))
					return;
			}

			if (te.IsAlreadyAtFloorBottomY(floorBottomY))
				return;

			te.TargetFloorBottomY = floorBottomY;
			te.SyncToClientsIfServer();
		}

		private void SyncToClientsIfServer()
		{
			if (Main.netMode == NetmodeID.Server)
				NetMessage.SendData(MessageID.TileEntitySharing, -1, -1, null, ID);
		}

		public override bool IsTileValidForEntity(int x, int y)
		{
			Tile tile = Main.tile[x, y];
			// 不强制要求 frame==0：世界加载/成帧异常时，frame 可能不是 0，会导致 TE 被判 invalid 而消失。
			return tile.HasTile && tile.TileType == ModContent.TileType<ElevatorTile>();
		}

		public void ScanFloors()
		{
			DebugLog($"[Elevator] ScanFloors pos=({Position.X},{Position.Y})");
			FloorBottomYs.Clear();
			DebugLastLeftWallX = -1;
			DebugLastRightWallX = -1;
			DebugWallFailReason = "";
			DebugBestLeftCandidateX = -1;
			DebugBestLeftWallScore = 0;
			DebugBestLeftInsideAirScore = 0;
			DebugBestRightCandidateX = -1;
			DebugBestRightWallScore = 0;
			DebugBestRightInsideAirScore = 0;
			DebugDoorCandidateCount = 0;
			DebugDoorSample = "";

			int refX = Position.X + 1;
			int refY = Position.Y + 3;
			if (!TryFindShaftWalls(refX, refY, out int leftWallX, out int rightWallX, out string failReason,
				out int bestLeftX, out int bestLeftWallScore, out int bestLeftAirScore,
				out int bestRightX, out int bestRightWallScore, out int bestRightAirScore))
			{
				DebugWallFailReason = failReason;
				DebugBestLeftCandidateX = bestLeftX;
				DebugBestLeftWallScore = bestLeftWallScore;
				DebugBestLeftInsideAirScore = bestLeftAirScore;
				DebugBestRightCandidateX = bestRightX;
				DebugBestRightWallScore = bestRightWallScore;
				DebugBestRightInsideAirScore = bestRightAirScore;
				DebugLog($"[Elevator] ScanFloors fail={failReason} ref=({refX},{refY}) bestL x={bestLeftX} w={bestLeftWallScore} a={bestLeftAirScore} bestR x={bestRightX} w={bestRightWallScore} a={bestRightAirScore}");
				return;
			}

			DebugLastLeftWallX = leftWallX;
			DebugLastRightWallX = rightWallX;

			TryFindShaftVerticalBounds(leftWallX, rightWallX, refY, out int shaftTopCapY, out int shaftBottomCapY);
			List<int> gapCandidates = new List<int>();
			List<int> buttonCandidates = new List<int>();

			CollectDoorGapFloorCandidates(leftWallX, rightWallX, shaftTopCapY, shaftBottomCapY, gapCandidates);
			CollectButtonFloorCandidates(leftWallX, rightWallX, shaftTopCapY, shaftBottomCapY, buttonCandidates);

			switch (FloorMode)
			{
				case FloorDetectMode.ShaftGapOnly:
					FloorBottomYs.AddRange(gapCandidates);
					break;
				case FloorDetectMode.ButtonOnly:
					FloorBottomYs.AddRange(buttonCandidates);
					break;
				default:
				{
					// “共同检测”：同一层既有井隙候选，也有按钮候选（允许少量 Y 偏差）。
					const int matchToleranceTiles = 3;
					for (int i = 0; i < gapCandidates.Count; i++)
					{
						int gapY = gapCandidates[i];
						bool hasMatchedButton = false;
						for (int j = 0; j < buttonCandidates.Count; j++)
						{
							if (Math.Abs(buttonCandidates[j] - gapY) <= matchToleranceTiles)
							{
								hasMatchedButton = true;
								break;
							}
						}
						if (hasMatchedButton)
							FloorBottomYs.Add(gapY);
					}
					break;
				}
			}

			FloorBottomYs.Sort();
			for (int idx = FloorBottomYs.Count - 1; idx > 0; idx--)
			{
				if (FloorBottomYs[idx] == FloorBottomYs[idx - 1])
					FloorBottomYs.RemoveAt(idx);
			}
			DebugLog($"[Elevator] ScanFloors ok walls=({DebugLastLeftWallX},{DebugLastRightWallX}) floors={FloorBottomYs.Count} doors={DebugDoorCandidateCount} sample={DebugDoorSample}");
		}

		public override void SaveData(TagCompound tag)
		{
			tag["floorMode"] = (int)FloorMode;
		}

		public override void LoadData(TagCompound tag)
		{
			int modeValue = tag.ContainsKey("floorMode")
				? tag.GetInt("floorMode")
				: (int)FloorDetectMode.ButtonOnly;
			if (modeValue < 0 || modeValue > 2)
				modeValue = (int)FloorDetectMode.ButtonOnly;
			FloorMode = (FloorDetectMode)modeValue;
		}

		public override void NetSend(BinaryWriter writer)
		{
			writer.Write((byte)FloorMode);
			writer.Write(TargetFloorBottomY);
			writer.Write(_moving);
			writer.Write(_moveDir);
			writer.Write(_pixelCarry);
			writer.Write(_desiredTopY);
		}

		public override void NetReceive(BinaryReader reader)
		{
			byte modeValue = reader.ReadByte();
			if (modeValue > 2)
				modeValue = (byte)FloorDetectMode.ButtonOnly;
			FloorMode = (FloorDetectMode)modeValue;
			TargetFloorBottomY = reader.ReadInt32();
			_moving = reader.ReadBoolean();
			_moveDir = reader.ReadInt32();
			_pixelCarry = reader.ReadSingle();
			_desiredTopY = reader.ReadInt32();
		}

		public bool IsPointInsideShaft(float worldX, float worldY)
		{
			if (DebugLastLeftWallX < 0 || DebugLastRightWallX < 0)
				ScanFloors();
			if (DebugLastLeftWallX < 0 || DebugLastRightWallX < 0)
				return false;

			float innerLeft = (DebugLastLeftWallX + 1) * 16f;
			float innerRight = DebugLastRightWallX * 16f;
			return worldX >= innerLeft && worldX <= innerRight;
		}

		public int FindNearestFloorBottomY(float worldY)
		{
			if (FloorBottomYs.Count == 0)
				return -1;
			int currentBottomY = (int)Math.Round(worldY / 16f);
			int best = FloorBottomYs[0];
			int bestDist = Math.Abs(best - currentBottomY);
			for (int i = 1; i < FloorBottomYs.Count; i++)
			{
				int y = FloorBottomYs[i];
				int d = Math.Abs(y - currentBottomY);
				if (d < bestDist)
				{
					bestDist = d;
					best = y;
				}
			}
			return best;
		}

		public bool MoveToNearestFloorForPlayer(Player player)
		{
			if (player == null)
				return false;
			ScanFloors();
			if (FloorBottomYs.Count == 0)
				return false;
			int floorBottomY = FindNearestFloorBottomY(player.Bottom.Y);
			if (floorBottomY < 0)
				return false;
			RequestMoveToFloor(ID, floorBottomY);
			return true;
		}

		public static bool IsPlayerInsideCabin(Player player, TEElevator te)
		{
			if (player == null || te == null || !player.active)
				return false;

			float yOffset = te.IsMoving ? te.VisualPixelOffset : 0f;
			Rectangle elevatorRect = new Rectangle(
				te.Position.X * 16,
				(int)(te.Position.Y * 16 + yOffset),
				ElevatorWidth * 16,
				7 * 16);
			Rectangle hitbox = player.Hitbox;
			hitbox.Inflate(2, 2);
			if (hitbox.Intersects(elevatorRect))
				return true;

			return IsPlayerOnElevatorTiles(player, te);
		}

		public static bool IsPlayerOnElevatorTiles(Player player, TEElevator te)
		{
			if (player == null || te == null)
				return false;

			ushort elevatorType = (ushort)ModContent.TileType<ElevatorTile>();
			Rectangle hitbox = player.Hitbox;
			hitbox.Inflate(2, 2);
			int minTileX = hitbox.Left / 16;
			int maxTileX = (hitbox.Right - 1) / 16;
			int minTileY = hitbox.Top / 16;
			int maxTileY = (hitbox.Bottom - 1) / 16;

			for (int tx = minTileX; tx <= maxTileX; tx++)
			{
				for (int ty = minTileY; ty <= maxTileY; ty++)
				{
					if (!TryResolveElevatorTopLeftAt(tx, ty, elevatorType, out Point16 topLeft))
						continue;
					if (topLeft.X == te.Position.X && topLeft.Y == te.Position.Y)
						return true;
				}
			}

			return false;
		}

		public static bool TryFindElevatorFromOccupiedTiles(Player player, out TEElevator result, out int topLeftX, out int topLeftY)
		{
			result = null;
			topLeftX = -1;
			topLeftY = -1;
			if (player == null || !player.active)
				return false;

			ushort elevatorType = (ushort)ModContent.TileType<ElevatorTile>();
			Rectangle hitbox = player.Hitbox;
			hitbox.Inflate(2, 2);
			int minTileX = hitbox.Left / 16;
			int maxTileX = (hitbox.Right - 1) / 16;
			int minTileY = hitbox.Top / 16;
			int maxTileY = (hitbox.Bottom - 1) / 16;

			TEElevator best = null;
			int bestTopX = -1;
			int bestTopY = -1;
			float bestDistSq = float.MaxValue;

			for (int tx = minTileX; tx <= maxTileX; tx++)
			{
				for (int ty = minTileY; ty <= maxTileY; ty++)
				{
					if (!TryResolveElevatorTopLeftAt(tx, ty, elevatorType, out Point16 topLeft))
						continue;

					int id = ModContent.GetInstance<TEElevator>().Find(topLeft.X, topLeft.Y);
					if (id < 0 || !TileEntity.ByID.TryGetValue(id, out TileEntity entity) || entity is not TEElevator te)
						continue;

					Vector2 center = new Vector2((te.Position.X + ElevatorWidth * 0.5f) * 16f, (te.Position.Y + 3.5f) * 16f);
					float distSq = Vector2.DistanceSquared(player.Center, center);
					if (distSq >= bestDistSq)
						continue;

					bestDistSq = distSq;
					best = te;
					bestTopX = topLeft.X;
					bestTopY = topLeft.Y;
				}
			}

			if (best == null)
				return false;

			result = best;
			topLeftX = bestTopX;
			topLeftY = bestTopY;
			return true;
		}

		// 直接按“4×7 footprint 是否包含某个格子”反查 TE，使用 TE 自身权威坐标，
		// 不依赖 topLeft 反推，避免多 Origin 放置时 Find 精确匹配失败。
		public static bool TryFindElevatorContainingTile(int i, int j, out TEElevator te)
		{
			te = null;
			foreach (var kv in ByID)
			{
				if (kv.Value is not TEElevator e)
					continue;
				if (i >= e.Position.X && i < e.Position.X + ElevatorWidth
					&& j >= e.Position.Y && j < e.Position.Y + 7)
				{
					te = e;
					return true;
				}
			}
			return false;
		}

		public static bool IsPlayerInElevatorRange(Player player, TEElevator te, int maxDistanceTiles = 16)
		{
			if (player == null || te == null || !player.active)
				return false;
			if (IsPlayerInsideCabin(player, te))
				return true;

			Vector2 center = new Vector2((te.Position.X + ElevatorWidth * 0.5f) * 16f, (te.Position.Y + 3.5f) * 16f);
			if (Vector2.Distance(player.Center, center) <= maxDistanceTiles * 16f)
				return true;

			te.ScanFloors();
			return te.IsPointInsideShaft(player.Center.X, player.Bottom.Y);
		}

		public static bool TryFindElevatorForPlayer(Player player, out TEElevator result, out int topLeftX, out int topLeftY)
		{
			if (TryFindElevatorFromOccupiedTiles(player, out result, out topLeftX, out topLeftY))
				return true;

			result = null;
			topLeftX = -1;
			topLeftY = -1;
			if (player == null || !player.active)
				return false;

			TEElevator bestCabin = null;
			float bestCabinDistSq = float.MaxValue;
			foreach (var kv in ByID)
			{
				if (kv.Value is not TEElevator te)
					continue;
				if (!IsPlayerInsideCabin(player, te))
					continue;

				Vector2 center = new Vector2((te.Position.X + ElevatorWidth * 0.5f) * 16f, (te.Position.Y + 3.5f) * 16f);
				float distSq = Vector2.DistanceSquared(player.Center, center);
				if (distSq >= bestCabinDistSq)
					continue;

				bestCabinDistSq = distSq;
				bestCabin = te;
			}

			if (bestCabin != null)
			{
				result = bestCabin;
				topLeftX = bestCabin.Position.X;
				topLeftY = bestCabin.Position.Y;
				return true;
			}

			return TryFindNearbyElevatorForPlayer(player, out result, out topLeftX, out topLeftY);
		}

		public static bool TryFindNearbyElevatorForPlayer(Player player, out TEElevator result, out int topLeftX, out int topLeftY)
		{
			result = null;
			topLeftX = -1;
			topLeftY = -1;
			if (player == null || !player.active)
				return false;

			float px = player.Center.X;
			float py = player.Bottom.Y;
			float bestScore = float.MaxValue;

			foreach (var kv in ByID)
			{
				if (kv.Value is not TEElevator te)
					continue;

				te.ScanFloors();
				if (te.FloorBottomYs.Count == 0)
					continue;
				if (!te.IsPointInsideShaft(px, py))
					continue;

				float dx = Math.Abs(px - (te.Position.X + ElevatorWidth * 0.5f) * 16f);
				float dy = Math.Abs(py - (te.Position.Y + 3.5f) * 16f);
				float score = dx + dy * 0.15f;
				if (score < bestScore)
				{
					bestScore = score;
					result = te;
					topLeftX = te.Position.X;
					topLeftY = te.Position.Y;
				}
			}

			return result != null;
		}

		public static bool TryFindNearbyElevatorByWorld(float worldX, float worldY, int maxDistanceTiles, out TEElevator result, out int topLeftX, out int topLeftY)
		{
			result = null;
			topLeftX = -1;
			topLeftY = -1;

			float maxDistSq = maxDistanceTiles * 16f;
			maxDistSq *= maxDistSq;
			float bestDistSq = maxDistSq;

			foreach (var kv in ByID)
			{
				if (kv.Value is not TEElevator te)
					continue;

				Vector2 center = new Vector2((te.Position.X + ElevatorWidth * 0.5f) * 16f, (te.Position.Y + 3.5f) * 16f);
				float distSq = Vector2.DistanceSquared(center, new Vector2(worldX, worldY));
				if (distSq >= bestDistSq)
					continue;

				te.ScanFloors();
				if (te.FloorBottomYs.Count == 0)
					continue;

				bestDistSq = distSq;
				result = te;
				topLeftX = te.Position.X;
				topLeftY = te.Position.Y;
			}

			return result != null;
		}

		private int _lastSimulationGameUpdateCount = -1;

		public int GetStandingSurfaceBottomY() => Position.Y + 6;

		public bool IsAlreadyAtFloorBottomY(int floorBottomY, int toleranceTiles = 1)
			=> Math.Abs(GetStandingSurfaceBottomY() - floorBottomY) <= toleranceTiles;

		public override void Update()
		{
			ProcessSimulation();
		}

		internal void ProcessSimulation()
		{
			if (Main.netMode == NetmodeID.MultiplayerClient)
				return;
			if (_lastSimulationGameUpdateCount == (int)Main.GameUpdateCount)
				return;
			_lastSimulationGameUpdateCount = (int)Main.GameUpdateCount;

			SimulateMovementTick();
		}

		private void SimulateMovementTick()
		{
			// 到站后的短暂“落地缓冲”：保持乘客锁定几帧，避免成帧/碰撞同步导致掉落。
			if (!_moving && _postArriveLockTicks > 0)
			{
				_postArriveLockTicks--;
				float visualTopLeftYpx = Position.Y * 16f;
				UpdateRidersAndCarry(Position.X, Position.Y, 0f, visualTopLeftYpx, snapToBottomSurface: true);
			}

			// 目标楼层 bottomY -> 目标 topY
			if (!_moving)
			{
				if (TargetFloorBottomY < 0)
					return;

				RebindPositionIfMisaligned();
				_desiredTopY = TargetFloorBottomY - 6;
				if (IsAlreadyAtFloorBottomY(TargetFloorBottomY))
				{
					DebugLog($"[Elevator] Target is current floor. pos=({Position.X},{Position.Y}) bottomY={TargetFloorBottomY}");
					TargetFloorBottomY = -1;
					return;
				}

				if (!IsElevatorBodyAt(Position.X, Position.Y))
				{
					TargetFloorBottomY = -1;
					return;
				}

				_moveDir = _desiredTopY > Position.Y ? 1 : -1;
				// 缓存 style 的基准 frame，移动渲染时即便局部 tile 临时缺失也能稳定裁切。
				try
				{
					Tile tl = Framing.GetTileSafely(Position.X, Position.Y);
					if (tl.HasTile && tl.TileType == ModContent.TileType<ElevatorTile>())
					{
						RenderBaseFrameX = tl.TileFrameX;
						RenderBaseFrameY = tl.TileFrameY;
					}
				}
				catch
				{
					RenderBaseFrameX = 0;
					RenderBaseFrameY = 0;
				}
				_moving = true;
				ResetMovementSoundState();
				TryPlayRunStartSound();
				_accelTicks = 0;
				_speedPxPerTick = 0f;
				_pixelCarry = 0f;
				_lastVisualTopLeftYpx = Position.Y * 16f;
				_riderOffsetY.Clear();
				_postArriveLockTicks = 0;
				_lastMoveFailReason = "";
				DebugLog($"[Elevator] StartMove pos=({Position.X},{Position.Y}) targetTopY={_desiredTopY} targetBottomY={TargetFloorBottomY} dir={_moveDir}");
				SyncToClientsIfServer();
			}

			if (_moveDir == 0)
			{
				_moving = false;
				_lastVisualTopLeftYpx = float.NaN;
				TargetFloorBottomY = -1;
				return;
			}

			const int accelDurationTicks = 30; // 0.5s
			const float cruiseSpeedPxPerTick = 8f;
			if (_accelTicks < accelDurationTicks)
			{
				_accelTicks++;
				_speedPxPerTick = cruiseSpeedPxPerTick * _accelTicks / accelDurationTicks;
			}
			else
			{
				_speedPxPerTick = cruiseSpeedPxPerTick;
			}

			bool snapPlayersToBottomThisTick = false;

			float oldVisualTopLeftYpx = Position.Y * 16f + _pixelCarry;
			float stepPx = _speedPxPerTick * _moveDir;
			_pixelCarry += stepPx;

			// 每次真正成功搬运一格 tile 后，再扣除对应的 16px 像素进度；
			// 避免“到站时未执行的 tile-step 仍被预先扣掉”，从而产生一次视觉回退/闪烁。
			int tileStepDir = _moveDir; // 与像素进度方向一致
			while (Math.Abs(_pixelCarry) >= 16f && _moveDir != 0)
			{
				TryPlayArriveSoonSound();
				// 抵达目标：如果下一步会越界，就收尾
				if ((_moveDir > 0 && Position.Y >= _desiredTopY) || (_moveDir < 0 && Position.Y <= _desiredTopY))
				{
					// 到站收尾：视觉偏移归零，并确保本 tick 也把玩家“落在站立面”上，
					// 避免到站瞬间出现一帧悬浮/掉落。
					_pixelCarry = 0f;
					snapPlayersToBottomThisTick = true;
					TryPlayArriveSound();

					_moving = false;
					_moveDir = 0;
					TargetFloorBottomY = -1;
					_postArriveLockTicks = 12;
					DebugLog($"[Elevator] Arrived pos=({Position.X},{Position.Y}) desiredTopY={_desiredTopY}");
					SyncToClientsIfServer();
					break;
				}

				int dyTiles = tileStepDir;
				if (!TryMoveMultiTileVertical(dyTiles, out string failReason))
				{
					_lastMoveFailReason = failReason;
					DebugLog($"[Elevator] MoveAbort pos=({Position.X},{Position.Y}) dy={dyTiles} reason={failReason}");
					_moving = false;
					_moveDir = 0;
					TargetFloorBottomY = -1;
					SyncToClientsIfServer();
					break;
				}
				DebugLog($"[Elevator] StepMove pos=({Position.X},{Position.Y}) dy={dyTiles} desiredTopY={_desiredTopY}", chatThrottleTicks: 10);

				// 成功搬运 1 格：扣除对应的 16px 视觉进度
				_pixelCarry -= 16f * tileStepDir;
			}

			// 玩家平滑跟随：在本 tick 的所有 tile-step 之后再统一按“视觉位置 delta”搬运一次，包含 16px 跳格。
			float newVisualTopLeftYpx = Position.Y * 16f + _pixelCarry;
			if (float.IsNaN(_lastVisualTopLeftYpx))
				_lastVisualTopLeftYpx = oldVisualTopLeftYpx;
			float dyCarry = newVisualTopLeftYpx - _lastVisualTopLeftYpx;
			UpdateRidersAndCarry(Position.X, Position.Y, _pixelCarry, newVisualTopLeftYpx, snapToBottomSurface: snapPlayersToBottomThisTick);

			// 运行中：在电梯与井壁底部两侧产生“摩擦挖掘”粒子，模仿挖掉火星管道镀层时的 dust。
			// WorldGen.KillTile 对 tile type 350（MartianConduitPlating）会使用 dust id=226。
			if (_moving && Main.GameUpdateCount % 2 == 0)
			{
				float visualTopLeftYPx = newVisualTopLeftYpx;
				float bottomYPx = visualTopLeftYPx + 7f * 16f;

				// 粒子锚定在贴图底部两端，避免看起来偏离电梯本体。
				Vector2 leftPos = new Vector2(Position.X * 16f - 3f, bottomYPx - 7f);
				Vector2 rightPos = new Vector2((Position.X + ElevatorWidth) * 16f - 3f, bottomYPx - 7f);

				// 向外喷出一点点速度，并带少量随机竖直抖动。
				float vy = Main.rand.NextFloat(-0.15f, 0.15f);
				Dust.NewDust(leftPos, 6, 6, DustID.t_Martian, -0.25f, vy, 0, default(Color), 1f);
				Dust.NewDust(rightPos, 6, 6, DustID.t_Martian, 0.25f, vy, 0, default(Color), 1f);
			}

			UpdateRunningSounds();
			_lastVisualTopLeftYpx = newVisualTopLeftYpx;
		}

		private void UpdateRidersAndCarry(int topLeftX, int topLeftY, float yOffset, float visualTopLeftYpx, bool snapToBottomSurface)
		{
			Rectangle elevatorRect = new Rectangle(topLeftX * 16, (int)(topLeftY * 16 + yOffset), 4 * 16, 7 * 16);
			float bottomSurfaceYpx = (topLeftY + 6) * 16f;
			for (int p = 0; p < Main.maxPlayers; p++)
			{
				Player plr = Main.player[p];
				if (plr == null || !plr.active || plr.dead)
				{
					_riderOffsetY.Remove(p);
					continue;
				}
				Rectangle hitbox = plr.Hitbox;
				hitbox.Inflate(2, 2);
				bool inElevator = hitbox.Intersects(elevatorRect);
				if (!inElevator)
				{
					_riderOffsetY.Remove(p);
					continue;
				}
				if (!_riderOffsetY.TryGetValue(p, out float offY))
				{
					offY = plr.position.Y - visualTopLeftYpx;
					_riderOffsetY[p] = offY;
				}
				plr.position.Y = visualTopLeftYpx + offY;
				if (snapToBottomSurface)
				{
					float desiredStandY = bottomSurfaceYpx - plr.height;
					// 只在明显穿入平台时才回正，避免到站瞬间出现“玩家被抬一下”。
					const float snapTolerancePx = 8f;
					if (plr.position.Y > desiredStandY + snapTolerancePx)
						plr.position.Y = desiredStandY;
				}
				plr.fallStart = (int)(plr.position.Y / 16f);
				plr.velocity.Y = 0f;
			}
		}

		private void EnsureTopLeft()
		{
			if (TryResolveElevatorTopLeft(Position.X, Position.Y, out Point16 newPos))
				ApplyTePosition(newPos);
		}

		private void RebindPositionIfMisaligned()
		{
			EnsureTopLeft();
			if (IsElevatorBodyAt(Position.X, Position.Y))
				return;

			ushort elevatorType = (ushort)ModContent.TileType<ElevatorTile>();
			int centerX = Position.X;
			int centerY = Position.Y;
			for (int radius = 1; radius <= 32; radius++)
			{
				for (int x = centerX - radius; x <= centerX + radius; x++)
				{
					for (int y = centerY - radius; y <= centerY + radius; y++)
					{
						if (!TryResolveElevatorTopLeftAt(x, y, elevatorType, out Point16 topLeft))
							continue;
						if (!IsElevatorBodyAt(topLeft.X, topLeft.Y))
							continue;
						ApplyTePosition(topLeft);
						DebugLog($"[Elevator] RebindPositionIfMisaligned new=({topLeft.X},{topLeft.Y})");
						return;
					}
				}
			}
		}

		private void ApplyTePosition(Point16 newPos)
		{
			Point16 oldPos = Position;
			if (oldPos == newPos)
				return;

			lock (EntityCreationLock)
			{
				ByPosition.Remove(oldPos);
				Position = newPos;
				ByPosition[newPos] = this;
			}
			DebugLog($"[Elevator] ApplyTePosition old=({oldPos.X},{oldPos.Y}) new=({newPos.X},{newPos.Y})");
		}

		private static bool IsElevatorBodyAt(int topLeftX, int topLeftY)
		{
			ushort elevatorType = (ushort)ModContent.TileType<ElevatorTile>();
			for (int x = 0; x < ElevatorWidth; x++)
			{
				for (int y = 0; y < 7; y++)
				{
					int tx = topLeftX + x;
					int ty = topLeftY + y;
					if (tx < 0 || tx >= Main.maxTilesX || ty < 0 || ty >= Main.maxTilesY)
						return false;
					Tile t = Framing.GetTileSafely(tx, ty);
					if (!t.HasTile || t.TileType != elevatorType)
						return false;
				}
			}
			return true;
		}

		private static bool TryResolveElevatorTopLeft(int startX, int startY, out Point16 topLeft)
		{
			topLeft = Point16.NegativeOne;
			ushort elevatorType = (ushort)ModContent.TileType<ElevatorTile>();

			if (TryResolveElevatorTopLeftAt(startX, startY, elevatorType, out topLeft))
				return true;

			for (int x = startX - 3; x <= startX + 3; x++)
			{
				for (int y = startY - 6; y <= startY + 6; y++)
				{
					if (TryResolveElevatorTopLeftAt(x, y, elevatorType, out topLeft))
						return true;
				}
			}

			return false;
		}

		private static bool TryResolveElevatorTopLeftAt(int x, int y, ushort elevatorType, out Point16 topLeft)
		{
			topLeft = Point16.NegativeOne;
			if (x < 0 || x >= Main.maxTilesX || y < 0 || y >= Main.maxTilesY)
				return false;
			Tile t = Framing.GetTileSafely(x, y);
			if (!t.HasTile || t.TileType != elevatorType)
				return false;

			(int topLeftX, int topLeftY) = ElevatorTile.GetBestTopLeftForRender(x, y);
			if (topLeftX < 0 || topLeftY < 0 || topLeftX >= Main.maxTilesX || topLeftY >= Main.maxTilesY)
				return false;

			Tile tlTile = Framing.GetTileSafely(topLeftX, topLeftY);
			if (!tlTile.HasTile || tlTile.TileType != elevatorType)
				return false;

			topLeft = new Point16(topLeftX, topLeftY);
			return true;
		}

		private Vector2 ElevatorSoundPosition()
		{
			// 在电梯中轴与中段附近播放，尽量让音效随电梯听感“出现在井里”。
			float centerXpx = (Position.X + ElevatorWidth / 2f) * 16f;
			float centerYpx = (Position.Y + 3.5f) * 16f;
			return new Vector2(centerXpx, centerYpx);
		}

		private bool HasRiderInside() => _riderOffsetY.Count > 0;

		private void ResetMovementSoundState()
		{
			StopElevatorRunLoopSound();
			_aboutToArriveSoundPlayed = false;
			_arriveSoundPlayed = false;
			_runStartTick = uint.MaxValue;
			_runLoopStarted = false;
		}

		private const int RunStartApproxTicks = 45;

		private void TryPlayRunStartSound()
		{
			SoundEngine.PlaySound(ElevatorSfx_RunStart, ElevatorSoundPosition());
			_runStartTick = Main.GameUpdateCount;
			_runLoopStarted = false;
		}

		private void StopElevatorRunLoopSound()
		{
			// 只停止“运行中_持续”这段循环，避免把“到站_1/到站”等其它音效一起中断。
			SoundEngine.FindActiveSound(ElevatorSfx_RunLoop)?.Stop();
			_runLoopStarted = false;
		}

		private void TryPlayArriveSoonSound()
		{
			if (_aboutToArriveSoundPlayed)
				return;
			if (!HasRiderInside())
				return;
			if (_desiredTopY < 0)
				return;

			// 1 格提前播放：下一步将抵达 _desiredTopY。
			bool isOneTileAway =
				(_moveDir > 0 && Position.Y == _desiredTopY - 1) ||
				(_moveDir < 0 && Position.Y == _desiredTopY + 1);

			if (!isOneTileAway)
				return;

			SoundEngine.PlaySound(ElevatorSfx_ArriveSoon, ElevatorSoundPosition());
			_aboutToArriveSoundPlayed = true;
		}

		private void TryPlayArriveSound()
		{
			if (_arriveSoundPlayed)
				return;

			StopElevatorRunLoopSound();
			SoundEngine.PlaySound(ElevatorSfx_Arrive, ElevatorSoundPosition());
			_arriveSoundPlayed = true;
		}

		private void UpdateRunningSounds()
		{
			if (!_moving)
			{
				StopElevatorRunLoopSound();
				return;
			}

			if (_runLoopStarted)
				return;

			if (_runStartTick == uint.MaxValue)
				return;

			// 用 tick 延迟近似“运行中.mp3 播放完毕”的时间点。
			uint elapsedTicks = Main.GameUpdateCount - _runStartTick;
			if (elapsedTicks < (uint)RunStartApproxTicks)
				return;

			SoundEngine.PlaySound(ElevatorSfx_RunLoop, ElevatorSoundPosition());
			_runLoopStarted = true;
		}

		private bool TryMoveMultiTileVertical(int dyTiles, out string failReason)
		{
			failReason = "";
			RebindPositionIfMisaligned();
			const int width = 4;
			const int height = 7;
			int oldX = Position.X;
			int oldY = Position.Y;
			int newX = oldX;
			int newY = oldY + dyTiles;
			if (newY < 0 || newY + height - 1 >= Main.maxTilesY)
			{
				failReason = "oob_y";
				return false;
			}
			if (newX < 0 || newX + width - 1 >= Main.maxTilesX)
			{
				failReason = "oob_x";
				return false;
			}

			ushort elevatorTileType = (ushort)ModContent.TileType<ElevatorTile>();
			List<Point16> platformsToSuppress = null;
			// 目标区域必须为空气(允许电梯自身占用的格子；允许平台，会被临时移除后再还原)
			for (int x = 0; x < width; x++)
			{
				for (int y = 0; y < height; y++)
				{
					int tx = newX + x;
					int ty = newY + y;
					bool overlapsOld = (tx >= oldX && tx < oldX + width && ty >= oldY && ty < oldY + height);
					if (overlapsOld)
						continue;
					Tile t = Framing.GetTileSafely(tx, ty);
					if (t.HasTile)
					{
						if (t.TileType == elevatorTileType)
							continue;
						if (TileID.Sets.Platforms[t.TileType])
						{
							if (platformsToSuppress == null)
								platformsToSuppress = new List<Point16>();
							platformsToSuppress.Add(new Point16(tx, ty));
							continue;
						}
						failReason = $"blocked_at({tx},{ty}) type={t.TileType}";
						return false;
					}
				}
			}

			// 旧位置必须真的是电梯本体，否则说明 TE 的 Position 不在真实 topLeft 上。
			// 这种情况下绝不能清空旧位置，否则就会把电梯“凭空删掉”。
			for (int x = 0; x < width; x++)
			{
				for (int y = 0; y < height; y++)
				{
					Tile t = Framing.GetTileSafely(oldX + x, oldY + y);
					if (!t.HasTile || t.TileType != elevatorTileType)
					{
						failReason = $"old_area_mismatch_at({oldX + x},{oldY + y}) has={(t.HasTile ? 1 : 0)} type={t.TileType} expected={elevatorTileType}";
						DebugLog($"[Elevator] MoveAbortPrecheck pos=({oldX},{oldY}) dy={dyTiles} {failReason}");
						return false;
					}
				}
			}

			// 旧 tML 版本里 Tile.Clone()/CopyFrom 的行为可能不稳定（会丢 HasTile/TileType），
			// 这里直接把需要的字段拆出来做快照，保证移动写回可用。
			bool[,] bufHasTile = new bool[width, height];
			ushort[,] bufType = new ushort[width, height];
			short[,] bufFrameX = new short[width, height];
			short[,] bufFrameY = new short[width, height];
			SlopeType[,] bufSlope = new SlopeType[width, height];
			bool[,] bufHalf = new bool[width, height];
			byte[,] bufColor = new byte[width, height];
			bool[,] bufHasActuator = new bool[width, height];
			bool[,] bufIsActuated = new bool[width, height];
			bool[,] bufRed = new bool[width, height];
			bool[,] bufGreen = new bool[width, height];
			bool[,] bufBlue = new bool[width, height];
			bool[,] bufYellow = new bool[width, height];
			for (int x = 0; x < width; x++)
			{
				for (int y = 0; y < height; y++)
				{
					Tile src = Framing.GetTileSafely(oldX + x, oldY + y);
					bufHasTile[x, y] = src.HasTile;
					bufType[x, y] = src.TileType;
					bufFrameX[x, y] = src.TileFrameX;
					bufFrameY[x, y] = src.TileFrameY;
					bufSlope[x, y] = src.Slope;
					bufHalf[x, y] = src.IsHalfBlock;
					bufColor[x, y] = src.TileColor;
					bufHasActuator[x, y] = src.HasActuator;
					bufIsActuated[x, y] = src.IsActuated;
					bufRed[x, y] = src.RedWire;
					bufGreen[x, y] = src.GreenWire;
					bufBlue[x, y] = src.BlueWire;
					bufYellow[x, y] = src.YellowWire;
				}
			}

			// 移动过程中会触发 SquareTileFrame，而 SquareTileFrame 可能触发 KillMultiTile。
			// 若不抑制，KillMultiTile 会把 TE 当作被拆除而 Kill 掉，导致移动中/到站后 TE 丢失、玩家掉落。
			SuppressKillMultiTileDepth++;
			try
			{
				// 允许穿过平台：将即将进入的目标平台先暂存并临时移除。
				if (platformsToSuppress != null)
				{
					for (int k = 0; k < platformsToSuppress.Count; k++)
					{
						Point16 p = platformsToSuppress[k];
						if (_suppressedPlatforms.ContainsKey(p))
							continue;
						Tile t = Framing.GetTileSafely(p.X, p.Y);
						if (!t.HasTile || !TileID.Sets.Platforms[t.TileType])
							continue;
						PlatformSnapshot snap = new PlatformSnapshot
						{
							HasTile = t.HasTile,
							TileType = t.TileType,
							FrameX = t.TileFrameX,
							FrameY = t.TileFrameY,
							Slope = t.Slope,
							Half = t.IsHalfBlock,
							Color = t.TileColor,
							HasActuator = t.HasActuator,
							IsActuated = t.IsActuated,
							Red = t.RedWire,
							Green = t.GreenWire,
							Blue = t.BlueWire,
							Yellow = t.YellowWire,
						};
						_suppressedPlatforms[p] = snap;
						t.HasTile = false;
						t.TileType = 0;
						t.TileFrameX = 0;
						t.TileFrameY = 0;
						t.Slope = 0;
						t.IsHalfBlock = false;
						t.HasActuator = false;
						t.IsActuated = false;
					}
				}

				// 清空旧位置（只清 tile，不清背景墙/液体）
				for (int x = 0; x < width; x++)
				{
					for (int y = 0; y < height; y++)
					{
						Tile dst = Framing.GetTileSafely(oldX + x, oldY + y);
						dst.HasTile = false;
						dst.TileType = 0;
						dst.TileFrameX = 0;
						dst.TileFrameY = 0;
						dst.Slope = 0;
						dst.IsHalfBlock = false;
						dst.HasActuator = false;
						dst.IsActuated = false;
					}
				}

				// 写入新位置
				for (int x = 0; x < width; x++)
				{
					for (int y = 0; y < height; y++)
					{
						Tile dst = Framing.GetTileSafely(newX + x, newY + y);
						// 只移动 tile，不覆盖目标位置原本的背景墙/液体（否则井的背景会被电梯“带走/抹掉”）
						ushort savedWall = dst.WallType;
						byte savedLiquid = dst.LiquidAmount;
						int savedLiquidType = dst.LiquidType;
						// 不使用 CopyFrom/Clone：用快照字段写回。
						dst.HasTile = bufHasTile[x, y];
						dst.TileType = bufType[x, y];
						dst.TileFrameX = bufFrameX[x, y];
						dst.TileFrameY = bufFrameY[x, y];
						dst.Slope = bufSlope[x, y];
						dst.IsHalfBlock = bufHalf[x, y];
						dst.TileColor = bufColor[x, y];
						dst.HasActuator = bufHasActuator[x, y];
						dst.IsActuated = bufIsActuated[x, y];
						dst.RedWire = bufRed[x, y];
						dst.GreenWire = bufGreen[x, y];
						dst.BlueWire = bufBlue[x, y];
						dst.YellowWire = bufYellow[x, y];
						dst.WallType = savedWall;
						dst.LiquidAmount = savedLiquid;
						dst.LiquidType = savedLiquidType;
					}
				}

				// 校验：新位置应该全部是电梯 tile（否则会出现“TE 以为动了，但画面上消失”）
				// 注意：elevatorTileType 在上面已经取过
				for (int x = 0; x < width; x++)
				{
					for (int y = 0; y < height; y++)
					{
						Tile t = Framing.GetTileSafely(newX + x, newY + y);
						if (!t.HasTile || t.TileType != elevatorTileType)
						{
							DebugLog($"[Elevator] TileMissingAfterMove cell=({newX + x},{newY + y}) has={(t.HasTile ? 1 : 0)} type={t.TileType} expected={elevatorTileType} from=({oldX},{oldY})->({newX},{newY})");
							// 不直接 return false：避免日志显示移动成功但实际半截丢失时完全中断（先让你拿到信息）
							// 但记录 failReason，便于后续我们改成硬失败。
							if (string.IsNullOrEmpty(failReason))
								failReason = "tile_missing_after_move";
						}
					}
				}

				// 玩家携带在 Update() 中按像素做，避免这里再按 16px 推一次造成双倍位移。

				Point16 oldPos = Position;
				Point16 newPos = new Point16(newX, newY);
				lock (EntityCreationLock)
				{
					ByPosition.Remove(oldPos);
					Position = newPos;
					ByPosition[newPos] = this;
				}

				// 对整个多格区域成帧，保证 frame/topLeft 计算稳定
				for (int x = oldX - 1; x <= oldX + width; x++)
				{
					for (int y = oldY - 1; y <= oldY + height; y++)
					{
						if (x < 0 || x >= Main.maxTilesX || y < 0 || y >= Main.maxTilesY)
							continue;
						WorldGen.SquareTileFrame(x, y, true);
					}
				}
				for (int x = newX - 1; x <= newX + width; x++)
				{
					for (int y = newY - 1; y <= newY + height; y++)
					{
						if (x < 0 || x >= Main.maxTilesX || y < 0 || y >= Main.maxTilesY)
							continue;
						WorldGen.SquareTileFrame(x, y, true);
					}
				}

				NetMessage.SendTileSquare(-1, newX, newY, width + 2, height + 2);
				NetMessage.SendTileSquare(-1, oldX, oldY, width + 2, height + 2);
				NetMessage.SendData(MessageID.TileEntitySharing, -1, -1, null, ID);

				// 还原电梯已离开的平台格子（旧区域中、且不再被新区域覆盖的部分）
				for (int x = 0; x < width; x++)
				{
					for (int y = 0; y < height; y++)
					{
						int rx = oldX + x;
						int ry = oldY + y;
						bool stillCovered = (rx >= newX && rx < newX + width && ry >= newY && ry < newY + height);
						if (stillCovered)
							continue;
						Point16 key = new Point16(rx, ry);
						if (!_suppressedPlatforms.TryGetValue(key, out PlatformSnapshot snap))
							continue;
						Tile t = Framing.GetTileSafely(rx, ry);
						if (t.HasTile)
						{
							// 已被其他东西占用（或仍是电梯），不强行覆盖；保留快照下次再尝试。
							if (t.TileType == elevatorTileType)
								continue;
							_suppressedPlatforms.Remove(key);
							continue;
						}
						t.HasTile = snap.HasTile;
						t.TileType = snap.TileType;
						t.TileFrameX = snap.FrameX;
						t.TileFrameY = snap.FrameY;
						t.Slope = snap.Slope;
						t.IsHalfBlock = snap.Half;
						t.TileColor = snap.Color;
						t.HasActuator = snap.HasActuator;
						t.IsActuated = snap.IsActuated;
						t.RedWire = snap.Red;
						t.GreenWire = snap.Green;
						t.BlueWire = snap.Blue;
						t.YellowWire = snap.Yellow;
						_suppressedPlatforms.Remove(key);
						WorldGen.SquareTileFrame(rx, ry, true);
					}
				}

				return string.IsNullOrEmpty(failReason);
			}
			finally
			{
				SuppressKillMultiTileDepth--;
			}
		}

		private static void DebugLog(string msg, int chatThrottleTicks = 0)
		{
			if (!ElevatorDebugLogging)
				return;
			try
			{
				ModContent.GetInstance<TEElevator>().Mod.Logger.Info(msg);
			}
			catch
			{
			}
		}

		private static void CarryPlayersWithElevatorPixels(int oldX, int oldY, float yOffset, float dyPx)
		{
			Rectangle elevatorRect = new Rectangle(oldX * 16, (int)(oldY * 16 + yOffset), 4 * 16, 7 * 16);

			for (int p = 0; p < Main.maxPlayers; p++)
			{
				Player plr = Main.player[p];
				if (plr == null || !plr.active || plr.dead)
					continue;
				Rectangle hitbox = plr.Hitbox;
				hitbox.Inflate(2, 2);
				if (!hitbox.Intersects(elevatorRect))
					continue;
				plr.position.Y += dyPx;
				plr.fallStart = (int)(plr.position.Y / 16f);
				plr.velocity.Y = 0f;
			}
		}

		private static bool IsShaftWallTile(int x, int y)
		{
			if (x < 0 || x >= Main.maxTilesX || y < 0 || y >= Main.maxTilesY)
				return false;
			Tile tile = Framing.GetTileSafely(x, y);
			return tile.HasTile && tile.TileType == ShaftWallTileType;
		}

		private static bool IsWallPresent(int x, int y)
		{
			if (x < 0 || x >= Main.maxTilesX || y < 0 || y >= Main.maxTilesY)
				return false;
			Tile tile = Framing.GetTileSafely(x, y);
			return tile.HasTile;
		}

		private static bool IsSolidTile(int x, int y)
		{
			if (x < 0 || x >= Main.maxTilesX || y < 0 || y >= Main.maxTilesY)
				return false;
			Tile tile = Framing.GetTileSafely(x, y);
			if (!tile.HasTile)
				return false;
			if (tile.TileType == ModContent.TileType<ElevatorTile>())
				return false;
			return Main.tileSolid[tile.TileType] && !tile.IsActuated;
		}

		private static bool IsAirLike(int x, int y)
		{
			if (x < 0 || x >= Main.maxTilesX || y < 0 || y >= Main.maxTilesY)
				return false;
			Tile tile = Framing.GetTileSafely(x, y);
			if (!tile.HasTile)
				return true;

			// 电梯井内宽度可能正好等于电梯宽度，导致“井壁内侧一列”被电梯本体占据。
			// 为了识别井壁/开口，这里把电梯 tile 视作空气。
			return tile.TileType == ModContent.TileType<ElevatorTile>();
		}

		private static bool ColumnMatchesWall(int x, int refY)
		{
			int match = 0;
			for (int dy = -WallSampleHalfHeight; dy <= WallSampleHalfHeight; dy++)
			{
				int y = refY + dy;
				if (y < 0 || y >= Main.maxTilesY)
					continue;
				if (IsShaftWallTile(x, y))
					match++;
			}
			return match >= WallSampleMinMatch;
		}

		private static bool ColumnMatchesBoundaryWall(int wallX, int insideX, int refY)
		{
			int wallCount = 0;
			int insideAirCount = 0;
			for (int dy = -WallSampleHalfHeight; dy <= WallSampleHalfHeight; dy++)
			{
				int y = refY + dy;
				if (y < 0 || y >= Main.maxTilesY)
					continue;
				if (IsSolidTile(wallX, y))
					wallCount++;
				if (IsAirLike(insideX, y))
					insideAirCount++;
			}
			return wallCount >= WallSampleMinMatch && insideAirCount >= InsideAirMinMatch;
		}

		private static bool TryFindShaftWalls(
			int refX,
			int refY,
			out int leftWallX,
			out int rightWallX,
			out string failReason,
			out int bestLeftX,
			out int bestLeftWallScore,
			out int bestLeftAirScore,
			out int bestRightX,
			out int bestRightWallScore,
			out int bestRightAirScore)
		{
			leftWallX = -1;
			rightWallX = -1;
			failReason = "";
			bestLeftX = -1;
			bestLeftWallScore = 0;
			bestLeftAirScore = 0;
			bestRightX = -1;
			bestRightWallScore = 0;
			bestRightAirScore = 0;

			int minX = Math.Max(0, refX - WallSearchRadiusX);
			int maxX = Math.Min(Main.maxTilesX - 1, refX + WallSearchRadiusX);

			// 1) 优先按 350 镀层识别井壁
			for (int x = refX; x >= minX; x--)
			{
				if (ColumnMatchesWall(x, refY))
				{
					leftWallX = x;
					break;
				}
			}
			for (int x = refX; x <= maxX; x++)
			{
				if (ColumnMatchesWall(x, refY))
				{
					rightWallX = x;
					break;
				}
			}

			// 2) 若失败，则退化为“边界墙”识别：墙列大多为实心，且内侧一列大多为空气
			if (leftWallX < 0)
			{
				for (int x = refX; x >= minX; x--)
				{
					int insideX = x + 1;
					if (insideX >= Main.maxTilesX)
						continue;
					int wallScore = 0;
					int airScore = 0;
					for (int dy = -WallSampleHalfHeight; dy <= WallSampleHalfHeight; dy++)
					{
						int y = refY + dy;
						if (y < 0 || y >= Main.maxTilesY)
							continue;
						if (IsSolidTile(x, y))
							wallScore++;
						if (IsAirLike(insideX, y))
							airScore++;
					}
					if (wallScore > bestLeftWallScore)
					{
						bestLeftX = x;
						bestLeftWallScore = wallScore;
						bestLeftAirScore = airScore;
					}
					if (wallScore >= WallSampleMinMatch && airScore >= InsideAirMinMatch)
					{
						leftWallX = x;
						break;
					}
				}
			}
			if (rightWallX < 0)
			{
				for (int x = refX; x <= maxX; x++)
				{
					int insideX = x - 1;
					if (insideX < 0)
						continue;
					int wallScore = 0;
					int airScore = 0;
					for (int dy = -WallSampleHalfHeight; dy <= WallSampleHalfHeight; dy++)
					{
						int y = refY + dy;
						if (y < 0 || y >= Main.maxTilesY)
							continue;
						if (IsSolidTile(x, y))
							wallScore++;
						if (IsAirLike(insideX, y))
							airScore++;
					}
					if (wallScore > bestRightWallScore)
					{
						bestRightX = x;
						bestRightWallScore = wallScore;
						bestRightAirScore = airScore;
					}
					if (wallScore >= WallSampleMinMatch && airScore >= InsideAirMinMatch)
					{
						rightWallX = x;
						break;
					}
				}
			}

			if (leftWallX < 0 || rightWallX < 0)
			{
				failReason = "walls_not_found";
				return false;
			}

			// 井宽必须能容纳电梯宽度（墙内宽至少 4）
			if (rightWallX - leftWallX - 1 < ElevatorWidth)
			{
				failReason = "shaft_too_narrow";
				return false;
			}

			return true;
		}

		private static void TryFindShaftVerticalBounds(int leftWallX, int rightWallX, int refY, out int topCapY, out int bottomCapY)
		{
			topCapY = -1;
			bottomCapY = -1;

			int minY = 0;
			int maxY = Main.maxTilesY - 1;

			// 向上找“封顶”：在墙内宽度内，一整行都是火星镀层(350)
			for (int y = refY; y >= minY; y--)
			{
				if (RowIsCappedByMartianPlating(leftWallX, rightWallX, y))
				{
					topCapY = y;
					break;
				}
			}

			// 向下找“封底”
			for (int y = refY; y <= maxY; y++)
			{
				if (RowIsCappedByMartianPlating(leftWallX, rightWallX, y))
				{
					bottomCapY = y;
					break;
				}
			}

			// 如果找不到封口，就允许扫描整个世界（但楼层仍然必须是门）
			if (topCapY < 0)
				topCapY = -1;
			if (bottomCapY < 0)
				bottomCapY = Main.maxTilesY;
		}

		private static bool RowIsCappedByMartianPlating(int leftWallX, int rightWallX, int y)
		{
			if (y < 0 || y >= Main.maxTilesY)
				return false;
			for (int x = leftWallX; x <= rightWallX; x++)
			{
				Tile tile = Framing.GetTileSafely(x, y);
				if (!tile.HasTile || tile.TileType != ShaftWallTileType)
					return false;
			}
			return true;
		}

		private static void AddUniqueFloor(List<int> floors, int bottomY)
		{
			for (int i = 0; i < floors.Count; i++)
			{
				if (floors[i] == bottomY)
					return;
			}
			floors.Add(bottomY);
		}

		private void CollectDoorGapFloorCandidates(int leftWallX, int rightWallX, int topCapY, int bottomCapY, List<int> outFloors)
		{
			int minY = Math.Max(0, topCapY + 1);
			int maxY = Math.Min(Main.maxTilesY - 1, bottomCapY - 1);
			if (minY > maxY)
				return;

			// 左侧：门可能在 wallX(替代镀层) 或 wallX-1(井外) 或 wallX+1(井内装饰/门框)
			ScanDoorBandForFloors(startX: leftWallX - 1, endX: leftWallX + 1, interiorDir: +1, minY, maxY, leftWallX, rightWallX, outFloors);
			// 右侧
			ScanDoorBandForFloors(startX: rightWallX - 1, endX: rightWallX + 1, interiorDir: -1, minY, maxY, leftWallX, rightWallX, outFloors);
		}

		private void ScanDoorBandForFloors(int startX, int endX, int interiorDir, int minY, int maxY, int leftWallX, int rightWallX, List<int> outFloors)
		{
			startX = Math.Max(0, startX);
			endX = Math.Min(Main.maxTilesX - 1, endX);
			if (startX > endX)
				return;

			for (int x = startX; x <= endX; x++)
			{
				int y = minY;
				while (y <= maxY)
				{
					if (!IsDoorCandidateAt(x, y, interiorDir, leftWallX, rightWallX))
					{
						y++;
						continue;
					}

					int topY = y;
					int bottomY = y;
					while (topY - 1 >= minY && IsDoorCandidateAt(x, topY - 1, interiorDir, leftWallX, rightWallX))
						topY--;
					while (bottomY + 1 <= maxY && IsDoorCandidateAt(x, bottomY + 1, interiorDir, leftWallX, rightWallX))
						bottomY++;

					int height = bottomY - topY + 1;
					if (height >= MinOpeningHeight)
					{
						int stopBottomY = bottomY;
						if (stopBottomY >= 0 && stopBottomY < Main.maxTilesY)
							AddUniqueFloor(outFloors, stopBottomY);
					}

					y = bottomY + 1;
				}
			}
		}

		private static bool IsDoorTileType(ushort tileType)
		{
			if (tileType == TileID.ClosedDoor || tileType == TileID.OpenDoor)
				return true;

			// CountsAsDoor 在当前 tML 版本为 int[] 标记数组。
			int[] countsAsDoor = TileID.Sets.RoomNeeds.CountsAsDoor;
			return countsAsDoor != null && tileType < countsAsDoor.Length && countsAsDoor[tileType] != 0;
		}

		private static void CollectButtonFloorCandidates(int leftWallX, int rightWallX, int topCapY, int bottomCapY, List<int> outFloors)
		{
			// 允许按钮放在井壁两侧和井口附近，给建筑留出余量。
			int minX = Math.Max(0, leftWallX - 12);
			int maxX = Math.Min(Main.maxTilesX - 1, rightWallX + 12);

			int minY = Math.Max(0, topCapY + 1);
			int maxY = Math.Min(Main.maxTilesY - 1, bottomCapY - 1);
			if (minY > maxY)
				return;

			ushort buttonType = (ushort)ModContent.TileType<ElevatorButtonTile>();
			for (int x = minX; x <= maxX; x++)
			{
				for (int y = minY; y <= maxY; y++)
				{
					Tile t = Framing.GetTileSafely(x, y);
					if (!t.HasTile || t.TileType != buttonType)
						continue;

					if (!TileObjectData.IsTopLeft(x, y))
						continue;

					TileObjectData data = TileObjectData.GetTileData(t);
					if (data == null)
						continue;

					int bottomY = y + data.Height - 1;
					if (bottomY <= maxY)
						AddUniqueFloor(outFloors, bottomY);
				}
			}
		}

		private bool IsDoorCandidateAt(int x, int y, int interiorDir, int leftWallX, int rightWallX)
		{
			if (x < 0 || x >= Main.maxTilesX || y < 0 || y >= Main.maxTilesY)
				return false;
			int insideX = x + interiorDir;
			if (insideX < 0 || insideX >= Main.maxTilesX)
				return false;

			// 必须在井壁附近（防止扫到其他地方的门）
			if (x < leftWallX - 2 || x > rightWallX + 2)
				return false;

			Tile t = Framing.GetTileSafely(x, y);
			if (!t.HasTile || !IsDoorTileType(t.TileType))
				return false;

			DebugDoorCandidateCount++;
			if (DebugDoorCandidateCount <= 8)
				DebugDoorSample += $"({x},{y}) t={t.TileType} inAir={(IsAirLike(insideX, y) ? 1 : 0)} ";

			// 井内侧必须是空气/电梯，说明这扇门确实朝向井内空间
			if (!IsAirLike(insideX, y))
				return false;

			return true;
		}
	}
}
