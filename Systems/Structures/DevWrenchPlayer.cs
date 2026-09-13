using Terraria.DataStructures;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace ArknightsMod.Systems.Structures
{
	// 「开发测试扳手」的框选状态。挂在 ModPlayer 上而不是静态字段/ModSystem，
	// 是为了多人游戏下每个玩家的框选互不干扰（虽然这是开发者工具，
	// 但避免"静态字段在联机时互相覆盖"这种坑是本项目里反复踩过的教训，
	// 顺手就按正确方式写掉，不额外增加工作量）。
	public class DevWrenchPlayer : ModPlayer
	{
		// 当前是否正按住左键拖拽中（尚未松开）。
		public bool IsDragging;

		// 拖拽起点（按下左键那一刻的 tile 坐标）。
		public Point16 DragStart;

		// 拖拽过程中实时更新的当前点（tile 坐标）。
		public Point16 DragCurrent;

		// 松开左键后"定格"下来的选区，null 表示还没有任何有效选区。
		// /exp 指令导出的就是这个区域。
		public (Point16 start, Point16 end)? FinalizedSelection;

		public bool HasSelection => FinalizedSelection.HasValue;
	}
}
