using Terraria.DataStructures;
using System;
using System.IO;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using ArknightsMod.Systems.Structures;

namespace ArknightsMod.Content.Items.Developer
{
	// 折叠式建筑：读取一份 .akstruct 结构文件，手持时在光标处显示投影/图纸预览，
	// 左键点击后整体放置到世界里。
	//
	// ⚠ 这个类目前是「提前部署好的骨架」，还不能真正拿去用。
	// 因为它要读取的 .akstruct 文件目前还不存在——「开发测试扳手」
	// （DevTestWrench）才是负责"生成第一份格式文件"的工具，
	// 顺序上必须先用扳手导出至少一份结构，才有东西可以让这个物品读。
	// 下面 StructureFileName 目前指向一个占位文件名，
	// 读取不到时会在 TryGetStructure 里安全地返回
	// false（不预览、不放置、不报错崩溃），而不是抛异常。
	// 等你们用扳手实际导出一份文件后，把这个常量改成真实文件名即可，
	// 其余放置/预览逻辑不需要再改。
	//
	// 已知待完善（等第一份真实结构文件出来后再验证/补齐）：
	//   - 放置目前是直接覆盖式的（同 StructureData.PlaceAt），
	//     没有检查目标位置是否已被占用、没有处理大型多格家具的锚点朝向、
	//     没有处理 TileEntity（箱子内容物等）。
	//   - 预览没有做"超出世界边界/与现有建筑严重冲突时变红提示"这类校验反馈，
	//     目前放置前唯一的检查只有 WorldGen.InWorld 越界裁剪。
	public class FoldableBuildingItem : ModItem
	{
		// 要加载的结构文件名（相对于 AkStructureFormat.GetDefaultDirectory）。
		// 目前是占位值，文件不存在，见类顶部说明。
		public const string StructureFileName = "example" + AkStructureFormat.FileExtension;

		private static StructureData _cachedStructure;
		private static bool _loadAttempted;

		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 0;
		}

		public override void SetDefaults() {
			Item.width = 32;
			Item.height = 32;
			Item.useStyle = ItemUseStyleID.HoldUp;
			Item.useTime = 10;
			Item.useAnimation = 10;
			Item.autoReuse = false;   // 避免连点/连发导致同一座建筑被反复叠放
			Item.noMelee = true;
			Item.value = 0;
			Item.rare = ItemRarityID.Quest;
		}

		public override bool? UseItem(Player player) {
			if (player.whoAmI != Main.myPlayer)
				return null;

			if (!TryGetStructure(out StructureData structure))
				return null;

			Point16 anchor = GetCursorAnchorTile();
			structure.PlaceAt(anchor);
			return true;
		}

		// 结构左上角要对齐到光标当前所在的 tile。
		// 用"光标所在格"而不是"玩家脚下"，是因为需求里明确写了"光标显示投影"，
		// 说明落点是跟着鼠标走的，不是跟着角色走的。
		// 同 PortableStructureItemBase.GetDeploymentTopLeft 里的说明：不能直接用
		// Main.MouseWorld，它没考虑 Zoom 缩放，缩放不是 1:1 时光标和投影会错位。
		public static Point16 GetCursorAnchorTile() {
			Vector2 unscaledMouse = Vector2.Transform(Main.MouseScreen, Matrix.Invert(Main.GameViewMatrix.TransformationMatrix));
			Vector2 mouseWorld = unscaledMouse + Main.screenPosition;
			return new Point16((int)(mouseWorld.X / 16f), (int)(mouseWorld.Y / 16f));
		}

		// 懒加载并缓存结构数据。只会真正尝试读一次文件（成功或失败都缓存结果），
		// 不会每帧都重新读盘。
		public static bool TryGetStructure(out StructureData structure) {
			if (!_loadAttempted) {
				_loadAttempted = true;
				try {
					string path = Path.Combine(AkStructureFormat.GetDefaultDirectory(), StructureFileName);
					if (File.Exists(path))
						_cachedStructure = AkStructureFormat.Load(path);
				}
				catch (Exception) {
					// 文件存在但格式不对/损坏：同样安全降级为"没有可用结构"，
					// 不让一个坏文件把整个客户端画面搞崩。
					_cachedStructure = null;
				}
			}

			structure = _cachedStructure;
			return structure != null;
		}

		public override void ModifyTooltips(System.Collections.Generic.List<TooltipLine> tooltips) {
			if (!TryGetStructure(out _)) {
				tooltips.Add(new TooltipLine(Mod, "FoldableBuildingMissing",
					$"[未找到结构文件：{StructureFileName}，请先用开发测试扳手导出]") { OverrideColor = Color.OrangeRed });
			}
		}
	}
}
