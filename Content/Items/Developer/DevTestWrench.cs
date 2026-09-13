using Terraria.DataStructures;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using ArknightsMod.Systems.Structures;

namespace ArknightsMod.Content.Items.Developer
{
	// 开发测试扳手：开发者用来"框选一块区域 → 导出成 .akstruct 结构文件"的工具。
	//
	// 用法：
	//   1. 手持本物品，在世界中按住左键并拖动 —— 会实时画出一个点状高亮的选框。
	//   2. 松开左键 —— 选框"定格"，此时选区已经确定，随时可以重新拖一次覆盖它。
	//   3. 在聊天栏输入 /exp —— 把当前定格的选区保存成 .akstruct 文件，
	//      位置在 AkStructureFormat.GetDefaultDirectory
	//      （游戏存档目录下的 ArknightsModStructures 文件夹）。
	//
	// 实现上的关键点：
	// 没有用 UseItem/useAnimation 那一套（那是给"挥砍/开火"这种
	// 一次性触发的动作用的）。而是让 Item.useStyle = ItemUseStyleID.None
	// 且 CanUseItem 恒为 false，彻底不进入原版的"使用物品"
	// 流程；真正的拖拽检测放在 HoldItem 里直接读
	// Main.mouseLeft——这个钩子只要物品被拿在手上就每帧都会跑，
	// 不依赖点击动画，才能连续跟踪"按下 → 拖动 → 松开"这个过程。
	public class DevTestWrench : ModItem
	{
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 0;
		}

		public override void SetDefaults() {
			Item.width = 32;
			Item.height = 32;
			Item.useStyle = ItemUseStyleID.None;
			Item.autoReuse = false;
			Item.noMelee = true;
			Item.noUseGraphic = true;
			// 开发工具：没有配方、没有出售价值，仅用于开发环境下手动给予（比如 /give 类调试指令）。
			Item.value = 0;
			Item.rare = ItemRarityID.Quest;
		}

		// 彻底关闭原版"使用物品"流程，避免按左键触发挥动动画/物品冷却，
		// 干扰我们自己在 HoldItem 里对 Main.mouseLeft 的连续检测。
		public override bool CanUseItem(Player player) => false;

		public override void HoldItem(Player player) {
			// 只处理本机玩家自己的输入；其他玩家客户端上也会因为别人拿着这把扳手
			// 而调用到这里，此时 Main.mouseLeft 反映的是"本机"鼠标状态，与那个
			// 远程玩家无关，必须过滤掉，否则会用错误的输入驱动别人的选框。
			if (player.whoAmI != Main.myPlayer)
				return;

			var wrenchPlayer = player.GetModPlayer<DevWrenchPlayer>();

			// 鼠标压在 UI 上（背包/对话框等）时不响应世界拖拽，避免"点 UI 顺手拖出一个选区"。
			bool blockedByUI = player.mouseInterface || Main.playerInventory || player.talkNPC != -1;

			Point16 mouseTile = new(
				(int)(Main.MouseWorld.X / 16f),
				(int)(Main.MouseWorld.Y / 16f));

			if (Main.mouseLeft && !blockedByUI) {
				if (!wrenchPlayer.IsDragging) {
					// 刚按下：新开一段拖拽
					wrenchPlayer.IsDragging = true;
					wrenchPlayer.DragStart = mouseTile;
				}
				wrenchPlayer.DragCurrent = mouseTile;
			}
			else if (wrenchPlayer.IsDragging) {
				// 松开：定格选区
				wrenchPlayer.IsDragging = false;
				wrenchPlayer.FinalizedSelection = (wrenchPlayer.DragStart, wrenchPlayer.DragCurrent);
			}
		}

		public override void ModifyTooltips(System.Collections.Generic.List<TooltipLine> tooltips) {
			tooltips.Add(new TooltipLine(Mod, "DevTestWrenchHint",
				"按住左键拖动以框选区域，松开后用聊天指令 /exp 导出"));
		}
	}
}
