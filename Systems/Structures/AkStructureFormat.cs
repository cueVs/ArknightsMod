using Terraria.DataStructures;
using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using ArknightsMod.Systems;

namespace ArknightsMod.Systems.Structures
{
	// 本模组自定义的建筑结构二进制格式（扩展名 .akstruct）。
	// 由「开发测试扳手」（ArknightsMod.Content.Items.Developer.DevTestWrench）
	// 框选并导出，由「折叠式建筑」（FoldableBuildingItem）读取并还原。
	//
	// 只保存"方块数据"：Tile（类型/帧/坡面/染色）、Wall（类型/染色）、
	// 液体、四色导线。不保存物块实体（箱子内容物、告示牌文字、
	// 训练假人姿态等 TileEntity 数据）——原始需求只提到"方块数据"，
	// 没有要求实体，实体的序列化涉及更复杂的每类型专属逻辑，
	// 目前不在范围内。如果后续要支持箱子等，需要在这个格式里
	// 单独加一段 TileEntity 区块，并对应扩展 Save/Load。
	public static class AkStructureFormat
	{
		// 文件头魔数，用来快速识别文件类型 / 避免读到无关文件。
		public const string Magic = "AKST";

		// 格式版本号。以后如果改变二进制布局（比如加入 TileEntity 区块），
		// 递增这个数字，并在 Load 里按版本号分支兼容旧文件，
		// 不要直接改布局又不加版本判断——那样旧文件会读出乱码而不是明确报错。
		//
		// 版本 2：HasWall 分支新增 WallFrameX/WallFrameY（供预览用真实贴图帧），
		// 版本 1 的旧文件没有这两个字段，读取时按 0/0 补齐（真正落地时墙的帧
		// 反正会被 SquareWallFrame 重新算过，这两个字段只影响预览像不像）。
		//
		// 版本 3：新增 HasDecorInstances 分支，补存会客室家具系统（见
		// ReceptionRoomDecorSystem）挂在锚点 tile 上的"这里具体摆了什么家具"数据
		// （种类/朝向/变体/相对锚点的偏移）。这类家具落地后世界里真实的 tile 只是
		// 一个空壳锚点，图全靠系统按 TileEntity 里的数据自己画——之前只存 tile
		// 类型不存这份数据，办公桌/办公椅这类家具录进结构再放出来就会变成"能挖、
		// 有判定，但什么都不画"的空壳。版本 1/2 的旧文件没有这个分支，读到这类
		// 家具的锚点 tile 时只能重现空壳（老问题依旧存在，无法追溯修复，只能
		// 重新导出一份新文件）。
		//
		// 版本 4：文件头新增"模组方块/模组墙 ID 映射表"，解决一个会让旧结构随时间
		// 自己烂掉的严重问题——
		//   模组方块的数字 ID 不是固定的，是加载时由 TileLoader.ReserveTileID() 这个
		//   自增计数器按注册顺序临时分配的。版本 1~3 直接把 tile.TileType 当数字存进
		//   文件，只要之后往模组里新增/删除任何一个 ModTile，排在它后面的方块 ID 就会
		//   整体移位，老文件里存的数字便会指向完全不同的方块，表现为"建筑放出来缺一块，
		//   还冒出些没做过的奇怪方块"。（原版方块 ID 是固定常量，不受影响。）
		//   版本 4 起改为：文件里仍然按数字存，但额外记一张
		//   「这个文件里用到的模组方块数字 → "模组名/内部名"」的对照表，读取时按名字
		//   查回当前这次运行的真实 ID 再替换，从此增删方块都不会再错位。
		//   查不回来的（方块被删了/改名了/来自没装的模组）当作空气跳过，并打一条警告，
		//   总比默默摆错方块强。
		// ⚠ 版本 1~3 的旧文件没有这张表，里面的模组方块数字已经无从考证当时指的是什么，
		//   无法自动修复，只能重新导出。
		public const ushort FormatVersion = 4;

		public const string FileExtension = ".akstruct";

		// 单元格标志位（写在每个格子最前面的一个 byte）。
		// 用位标志而不是每项都单独写一个 bool，是因为空气格/无液体格非常多，
		// 这样可以让"什么都没有"的格子只占 1 字节，而不是好几个字段各占 1 字节。
		[Flags]
		private enum CellFlags : byte
		{
			None = 0,
			HasTile = 1 << 0,
			HasWall = 1 << 1,
			HasLiquid = 1 << 2,
			IsHalfBlock = 1 << 3,
			HasActuator = 1 << 4,
			IsActuated = 1 << 5,
			HasWire = 1 << 6,
			// CellFlags 是 byte，这是最后一个空位了（bit 7）——以后再要加新的
			// "有没有 XXX"标志位，得先把 CellFlags 换成 ushort，不能直接接着往下加。
			HasDecorInstances = 1 << 7,
		}

		[Flags]
		private enum WireFlags : byte
		{
			None = 0,
			Red = 1 << 0,
			Blue = 1 << 1,
			Green = 1 << 2,
			Yellow = 1 << 3,
		}

		// 把世界坐标矩形区域（tile 坐标，闭区间）内的方块数据写入 .akstruct 文件。
		// topLeft：框选区域左上角（tile 坐标）。
		// bottomRight：框选区域右下角（tile 坐标，含）。
		// filePath：完整输出路径，调用方负责决定文件名/目录已存在。
		public static void Save(Point16 topLeft, Point16 bottomRight, string filePath) {
			int minX = Math.Min(topLeft.X, bottomRight.X);
			int maxX = Math.Max(topLeft.X, bottomRight.X);
			int minY = Math.Min(topLeft.Y, bottomRight.Y);
			int maxY = Math.Max(topLeft.Y, bottomRight.Y);

			int width = maxX - minX + 1;
			int height = maxY - minY + 1;

			if (width <= 0 || height <= 0)
				throw new ArgumentException("选区宽高必须大于 0。");
			if ((long)width * height > 4_000_000)
				throw new ArgumentException($"选区过大（{width}x{height}），为避免生成超大文件已中止，请缩小选区。");

			using FileStream fs = new(filePath, FileMode.Create, FileAccess.Write);
			using BinaryWriter w = new(fs);

			w.Write(Magic.ToCharArray());
			w.Write(FormatVersion);
			w.Write((ushort)width);
			w.Write((ushort)height);

			WriteModTypeMaps(w, minX, maxX, minY, maxY);

			for (int y = minY; y <= maxY; y++) {
				for (int x = minX; x <= maxX; x++) {
					WriteCell(w, Main.tile[x, y], x, y);
				}
			}
		}

		// 扫一遍选区，把用到的所有"模组方块/模组墙"的数字 ID 和它的稳定身份
		// （"模组名/内部名"）配对写进文件头。原版方块不进表——它们的 ID 是固定常量，
		// 不会因为装了/改了什么模组而变化，存数字就够了。
		private static void WriteModTypeMaps(BinaryWriter w, int minX, int maxX, int minY, int maxY) {
			var modTiles = new Dictionary<ushort, string>();
			var modWalls = new Dictionary<ushort, string>();

			for (int y = minY; y <= maxY; y++) {
				for (int x = minX; x <= maxX; x++) {
					Tile tile = Main.tile[x, y];

					if (tile.HasTile && tile.TileType >= TileID.Count && !modTiles.ContainsKey(tile.TileType)) {
						ModTile mt = ModContent.GetModTile(tile.TileType);
						if (mt != null)
							modTiles[tile.TileType] = mt.Mod.Name + "/" + mt.Name;
					}

					if (tile.WallType != WallID.None && tile.WallType >= WallID.Count && !modWalls.ContainsKey(tile.WallType)) {
						ModWall mw = ModContent.GetModWall(tile.WallType);
						if (mw != null)
							modWalls[tile.WallType] = mw.Mod.Name + "/" + mw.Name;
					}
				}
			}

			WriteTypeMap(w, modTiles);
			WriteTypeMap(w, modWalls);
		}

		private static void WriteTypeMap(BinaryWriter w, Dictionary<ushort, string> map) {
			w.Write((ushort)map.Count);
			foreach (KeyValuePair<ushort, string> pair in map) {
				w.Write(pair.Key);
				w.Write(pair.Value);
			}
		}

		private static void WriteCell(BinaryWriter w, Tile tile, int x, int y) {
			CellFlags flags = CellFlags.None;
			if (tile.HasTile) flags |= CellFlags.HasTile;
			if (tile.WallType != WallID.None) flags |= CellFlags.HasWall;
			if (tile.LiquidAmount > 0) flags |= CellFlags.HasLiquid;
			if (tile.IsHalfBlock) flags |= CellFlags.IsHalfBlock;
			if (tile.HasActuator) flags |= CellFlags.HasActuator;
			if (tile.IsActuated) flags |= CellFlags.IsActuated;

			WireFlags wire = WireFlags.None;
			if (tile.RedWire) wire |= WireFlags.Red;
			if (tile.BlueWire) wire |= WireFlags.Blue;
			if (tile.GreenWire) wire |= WireFlags.Green;
			if (tile.YellowWire) wire |= WireFlags.Yellow;
			if (wire != WireFlags.None) flags |= CellFlags.HasWire;

			List<ReceptionRoomDecorSystem.DecorInstance> decorInstances = null;
			if (tile.HasTile && ReceptionRoomDecorSystem.IsAnchorTileType(tile.TileType)
				&& ReceptionRoomDecorSystem.TryGetInstancesAt(new Point16(x, y), out decorInstances)
				&& decorInstances.Count > 0) {
				flags |= CellFlags.HasDecorInstances;
			}

			w.Write((byte)flags);

			if (flags.HasFlag(CellFlags.HasTile)) {
				w.Write(tile.TileType);
				w.Write(tile.TileFrameX);
				w.Write(tile.TileFrameY);
				w.Write(tile.TileColor);
				w.Write((byte)tile.Slope);
			}

			if (flags.HasFlag(CellFlags.HasWall)) {
				w.Write(tile.WallType);
				w.Write(tile.WallColor);
				w.Write((short)tile.WallFrameX);
				w.Write((short)tile.WallFrameY);
			}

			if (flags.HasFlag(CellFlags.HasLiquid)) {
				w.Write((byte)tile.LiquidType);
				w.Write(tile.LiquidAmount);
			}

			if (flags.HasFlag(CellFlags.HasWire))
				w.Write((byte)wire);

			if (flags.HasFlag(CellFlags.HasDecorInstances)) {
				w.Write((byte)decorInstances.Count);
				foreach (ReceptionRoomDecorSystem.DecorInstance inst in decorInstances) {
					w.Write((byte)inst.Kind);
					w.Write(inst.Direction);
					w.Write(inst.Variant);
					// 存相对这个锚点格的偏移，而不是绝对世界坐标——结构放到新位置时
					// 只要把偏移加回新的锚点坐标就行，不用管原来导出时具体在哪。
					w.Write((short)(inst.TopLeft.X - x));
					w.Write((short)(inst.TopLeft.Y - y));
				}
			}
		}

		// 读取一个本机磁盘上的 .akstruct 文件（比如开发测试扳手刚导出、还躺在
		// GetDefaultDirectory 里的那种）。只对导出者本机有效——
		// 要做成能发给所有玩家使用的成品建筑，应该把文件复制进模组源码目录，
		// 走下面的 LoadFromMod，让它随 .tmod 一起打包分发。
		public static StructureData Load(string filePath) {
			using FileStream fs = new(filePath, FileMode.Open, FileAccess.Read);
			return Load(fs, filePath);
		}

		// 从模组自带资源里读取一份 .akstruct（随 .tmod 一起打包，每个装了这个模组的
		// 玩家、包括联机时的其他客户端，读到的都是同一份数据）。
		// mod：通常传 Mod 属性（ModItem/ModSystem 上都有）。
		// assetPath：相对模组根目录的路径，例如
		// "Assets/Structures/PortableRhodesIslandSafehouse.akstruct"，
		// 大小写、斜杠方向要跟磁盘上实际文件一致。
		public static StructureData LoadFromMod(Mod mod, string assetPath) {
			using Stream s = mod.GetFileStream(assetPath);
			return Load(s, assetPath);
		}

		private static StructureData Load(Stream stream, string sourceDescription) {
			using BinaryReader r = new(stream);

			char[] magic = r.ReadChars(4);
			if (new string(magic) != Magic)
				throw new InvalidDataException($"不是有效的 .akstruct 文件（文件头不匹配）：{sourceDescription}");

			ushort version = r.ReadUInt16();
			if (version < 1 || version > FormatVersion) {
				throw new InvalidDataException(
					$"不支持的 .akstruct 版本号 {version}（当前代码只认识版本 1~{FormatVersion}）：{sourceDescription}");
			}

			ushort width = r.ReadUInt16();
			ushort height = r.ReadUInt16();

			// 版本 4 起：文件头带着"模组方块数字 → 模组名/内部名"的对照表，
			// 这里按名字查回本次运行的真实 ID，得到「文件里的旧数字 → 现在的数字」映射。
			// 版本 1~3 没有这张表，只能原样使用文件里的数字（那些文件本来就有 ID 错位问题，
			// 见 FormatVersion 的注释）。
			Dictionary<ushort, ushort> tileRemap = null;
			Dictionary<ushort, ushort> wallRemap = null;
			if (version >= 4) {
				tileRemap = ReadTypeMap<ModTile>(r, sourceDescription, "方块");
				wallRemap = ReadTypeMap<ModWall>(r, sourceDescription, "墙");
			}

			var cells = new StructureCell[width, height];
			for (int y = 0; y < height; y++) {
				for (int x = 0; x < width; x++) {
					cells[x, y] = ReadCell(r, version, tileRemap, wallRemap);
				}
			}

			return new StructureData(width, height, cells);
		}

		// 读一张类型对照表，返回「文件里的旧数字 → 当前运行时的真实数字」。
		// 查不到的（方块被删了/改名了/来自没装的模组）映射到 UnresolvedType，
		// 调用方会把这种格子当空气跳过——宁可缺一块，也不要摆上一个完全无关的方块。
		private const ushort UnresolvedType = ushort.MaxValue;

		private static Dictionary<ushort, ushort> ReadTypeMap<T>(BinaryReader r, string sourceDescription, string kindLabel)
			where T : class, IModType {
			ushort count = r.ReadUInt16();
			var remap = new Dictionary<ushort, ushort>(count);

			for (int i = 0; i < count; i++) {
				ushort savedId = r.ReadUInt16();
				string identity = r.ReadString();

				if (ModContent.TryFind(identity, out T found)) {
					remap[savedId] = found switch {
						ModTile mt => (ushort)mt.Type,
						ModWall mw => (ushort)mw.Type,
						_ => UnresolvedType,
					};
				}
				else {
					remap[savedId] = UnresolvedType;
					ModContent.GetInstance<ArknightsMod>()?.Logger.Warn(
						$"[.akstruct] {sourceDescription}：找不到{kindLabel} \"{identity}\"（可能已被删除/改名，或来自未安装的模组），该处将留空。");
				}
			}

			return remap;
		}

		// 把文件里存的数字翻译成当前运行时的真实类型。没有映射表（旧版本文件）时原样返回。
		private static ushort ResolveType(ushort savedType, Dictionary<ushort, ushort> remap) =>
			remap != null && remap.TryGetValue(savedType, out ushort actual) ? actual : savedType;

		private static StructureCell ReadCell(BinaryReader r, ushort version,
			Dictionary<ushort, ushort> tileRemap, Dictionary<ushort, ushort> wallRemap) {
			var flags = (CellFlags)r.ReadByte();
			StructureCell cell = default;

			if (flags.HasFlag(CellFlags.HasTile)) {
				cell.HasTile = true;
				cell.TileType = ResolveType(r.ReadUInt16(), tileRemap);
				cell.TileFrameX = r.ReadInt16();
				cell.TileFrameY = r.ReadInt16();
				cell.TileColor = r.ReadByte();
				cell.Slope = r.ReadByte();

				// 查不回来的模组方块：整格当空气，别摆上一个无关的方块。
				if (cell.TileType == UnresolvedType)
					cell.HasTile = false;
			}

			if (flags.HasFlag(CellFlags.HasWall)) {
				cell.HasWall = true;
				cell.WallType = ResolveType(r.ReadUInt16(), wallRemap);
				cell.WallColor = r.ReadByte();
				if (version >= 2) {
					cell.WallFrameX = r.ReadInt16();
					cell.WallFrameY = r.ReadInt16();
				}

				// 同上：查不回来的模组墙留空，不要摆成别的墙。
				if (cell.WallType == UnresolvedType) {
					cell.HasWall = false;
					cell.WallType = WallID.None;
				}
			}

			if (flags.HasFlag(CellFlags.HasLiquid)) {
				cell.LiquidType = r.ReadByte();
				cell.LiquidAmount = r.ReadByte();
			}

			if (flags.HasFlag(CellFlags.HasWire)) {
				var wire = (WireFlags)r.ReadByte();
				cell.RedWire = wire.HasFlag(WireFlags.Red);
				cell.BlueWire = wire.HasFlag(WireFlags.Blue);
				cell.GreenWire = wire.HasFlag(WireFlags.Green);
				cell.YellowWire = wire.HasFlag(WireFlags.Yellow);
			}

			cell.IsHalfBlock = flags.HasFlag(CellFlags.IsHalfBlock);
			cell.HasActuator = flags.HasFlag(CellFlags.HasActuator);
			cell.IsActuated = flags.HasFlag(CellFlags.IsActuated);

			if (version >= 3 && flags.HasFlag(CellFlags.HasDecorInstances)) {
				byte count = r.ReadByte();
				cell.DecorInstances = new List<DecorInstanceRecord>(count);
				for (int k = 0; k < count; k++) {
					cell.DecorInstances.Add(new DecorInstanceRecord {
						Kind = (ReceptionRoomDecorSystem.DecorKind)r.ReadByte(),
						Direction = r.ReadSByte(),
						Variant = r.ReadByte(),
						OffsetX = r.ReadInt16(),
						OffsetY = r.ReadInt16(),
					});
				}
			}

			return cell;
		}

		// 结构数据存放的默认目录：%UserProfile%/Documents/My Games/Terraria/tModLoader/ArknightsModStructures/
		// （即 Main.SavePath 下的子目录，和存档同一个位置，本机不同用户互不干扰）。
		// 目录不存在会自动创建。
		public static string GetDefaultDirectory() {
			string dir = Path.Combine(Main.SavePath, "ArknightsModStructures");
			Directory.CreateDirectory(dir);
			return dir;
		}
	}

	// 挂在一个锚点格上的"这里具体摆了什么会客室家具"记录，对应
	// ReceptionRoomDecorSystem.DecorInstance，只是把 TopLeft 换成了相对锚点格的
	// 偏移（OffsetX/OffsetY），方便结构整体挪到新位置时直接加回新锚点坐标。
	public struct DecorInstanceRecord
	{
		public ReceptionRoomDecorSystem.DecorKind Kind;
		public sbyte Direction;
		public byte Variant;
		public short OffsetX;
		public short OffsetY;
	}

	// 单个格子的方块数据。struct 是为了让 StructureCell[,] 数组不额外产生几十万个对象。
	public struct StructureCell
	{
		public bool HasTile;
		public ushort TileType;
		public short TileFrameX;
		public short TileFrameY;
		public byte TileColor;
		public byte Slope;

		public bool HasWall;
		public ushort WallType;
		public byte WallColor;
		public short WallFrameX;
		public short WallFrameY;

		public byte LiquidType;
		public byte LiquidAmount;

		public bool IsHalfBlock;
		public bool HasActuator;
		public bool IsActuated;

		public bool RedWire, BlueWire, GreenWire, YellowWire;

		// null 表示这一格没有挂会客室家具实例数据（绝大多数格子都是这样）。
		public List<DecorInstanceRecord> DecorInstances;
	}

	// 内存中的一份已加载结构，提供预览取色与放置逻辑。
	public class StructureData
	{
		public readonly int Width;
		public readonly int Height;
		private readonly StructureCell[,] _cells;

		public StructureData(int width, int height, StructureCell[,] cells) {
			Width = width;
			Height = height;
			_cells = cells;
		}

		public StructureCell this[int x, int y] => _cells[x, y];

		// 把结构放置到世界里，worldTopLeft 是结构左上角要落在的 tile 坐标。
		// ⚠ 目前是直接覆盖式放置（逐格 WorldGen.PlaceTile / 清墙/补液体），
		// 没有做"目标位置是否已被占用"的碰撞检查、
		// 没有处理需要 WorldGen.PlaceObject 的大型多格家具
		// （床、桌子这类"帧重要"物块如果只靠逐格 PlaceTile 摆放，
		// 锚点/朝向大概率会错，需要额外按物块的 origin 数据整体摆放），
		// 也没有处理 TileEntity。这些都要等真正拿到第一份 .akstruct
		// 测试文件、看清楚实际存的是什么内容之后再补——现在这些代码
		// 是按用户要求"提前部署"的骨架，还不能直接拿来对着正式建筑用。
		public void PlaceAt(Point16 worldTopLeft) {
			for (int y = 0; y < Height; y++) {
				for (int x = 0; x < Width; x++) {
					int wx = worldTopLeft.X + x;
					int wy = worldTopLeft.Y + y;
					if (!WorldGen.InWorld(wx, wy, 5))
						continue;

					PlaceCell(wx, wy, _cells[x, y]);
				}
			}

			// 批量改动后需要通知客户端/网络重新生成对应区块的帧信息，
			// 否则视觉上可能不会立刻刷新。
			WorldGen.RangeFrame(worldTopLeft.X, worldTopLeft.Y, worldTopLeft.X + Width, worldTopLeft.Y + Height);

			// ⚠ RangeFrame 不会正确重算墙的帧——本格式没有保存墙的 FrameX/FrameY
			// （StructureCell 只存了 WallType/WallColor），墙格子摆上去之后帧数据是
			// 默认的 0，图会错位。要单独按格调用 SquareWallFrame 才能让墙正确拼接，
			// 不要指望 RangeFrame 替你把这一步也做了。
			for (int y = 0; y < Height; y++) {
				for (int x = 0; x < Width; x++) {
					int wx = worldTopLeft.X + x;
					int wy = worldTopLeft.Y + y;
					if (WorldGen.InWorld(wx, wy, 5) && Main.tile[wx, wy].WallType != WallID.None)
						WorldGen.SquareWallFrame(wx, wy, true);
				}
			}

			// 会客室家具的锚点 tile 只是个空壳，PlaceCell 那一遍只把壳子（tile 类型/帧）
			// 摆上去了，真正"这里画的是什么家具"的数据要单独在这里按记录重建一遍
			// TileEntity，不然摆出来就是"能挖、有判定，但什么都不画"的空壳。
			for (int y = 0; y < Height; y++) {
				for (int x = 0; x < Width; x++) {
					StructureCell cell = _cells[x, y];
					if (cell.DecorInstances == null)
						continue;

					int wx = worldTopLeft.X + x;
					int wy = worldTopLeft.Y + y;
					if (!WorldGen.InWorld(wx, wy, 5))
						continue;

					Point16 anchorPos = new(wx, wy);
					foreach (DecorInstanceRecord rec in cell.DecorInstances) {
						Point16 topLeft = new(wx + rec.OffsetX, wy + rec.OffsetY);
						ReceptionRoomDecorSystem.RestoreInstanceAtAnchor(anchorPos, rec.Kind, topLeft, rec.Direction, rec.Variant);
					}
				}
			}
		}

		private static void PlaceCell(int x, int y, StructureCell cell) {
			Tile tile = Main.tile[x, y];

			if (cell.HasTile) {
				tile.HasTile = true;
				tile.TileType = cell.TileType;
				tile.TileFrameX = cell.TileFrameX;
				tile.TileFrameY = cell.TileFrameY;
				tile.TileColor = cell.TileColor;
				tile.Slope = (SlopeType)cell.Slope;
				tile.IsHalfBlock = cell.IsHalfBlock;
				tile.HasActuator = cell.HasActuator;
				tile.IsActuated = cell.IsActuated;
			}
			else {
				tile.HasTile = false;
			}

			if (cell.HasWall) {
				tile.WallType = cell.WallType;
				tile.WallColor = cell.WallColor;
			}
			else {
				tile.WallType = WallID.None;
			}

			tile.LiquidType = cell.LiquidType;
			tile.LiquidAmount = cell.LiquidAmount;

			tile.RedWire = cell.RedWire;
			tile.BlueWire = cell.BlueWire;
			tile.GreenWire = cell.GreenWire;
			tile.YellowWire = cell.YellowWire;
		}
	}
}
