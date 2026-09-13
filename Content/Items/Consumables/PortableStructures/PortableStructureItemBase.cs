using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using ArknightsMod.Systems.Structures;

namespace ArknightsMod.Content.Items.Consumables.PortableStructures
{
	// "便携建筑"类一次性消耗品的通用基类——手持时在光标处显示投影，
	// 左键点击后一键把 .akstruct 结构整体放置到世界里，联机下走
	// 客户端请求/服务器权威落地的标准流程。
	//
	// 怎么加一个新的便携建筑（给以后再加同类物品用）：
	//   public class MyNewBuilding : PortableStructureItemBase
	//   {
	//       protected override string StructureAssetPath =>
	//           "Assets/Structures/MyNewBuilding.akstruct";
	//
	//       public override void SetDefaults() {
	//           ApplyCommonDefaults();     // 消耗品的公共部分（见下）
	//           Item.width = 32;
	//           Item.height = 32;
	//           Item.rare = ItemRarityID.LightPurple;
	//           Item.value = Item.sellPrice(silver: 20);
	//       }
	//
	//       public override void AddRecipes() {
	//           CreateRecipe().AddIngredient<CarbonBrick>(4).AddTile(TileID.WorkBenches).Register();
	//       }
	//   }
	// 不需要再写任何联网/预览/放置代码——那些全在这个基类和
	// AkStructureDeploySystem 里，一次写好，所有便携建筑共用。
	//
	// 光标锚点约定：光标固定落在建筑"水平居中、贴底部"的位置
	// （GetDeploymentTopLeft），不是结构左上角。这是产品需求指定的，
	// 如果某个建筑想要不同的锚点规则，重写 GetDeploymentTopLeft 即可，
	// 其余放置/网络逻辑不用动。
	public abstract class PortableStructureItemBase : ModItem
	{
		// 结构文件相对模组根目录的路径，例如 "Assets/Structures/Xxx.akstruct"。
		protected abstract string StructureAssetPath { get; }

		// 这里用实例字段而不是 static 缓存：tModLoader 里每个 ModItem 子类型只会有
		// 唯一一个单例实例（跟"每次捡到的物品各一份"是两回事），所以实例字段
		// 天然就是"每个具体建筑各自缓存一份"，不会互相踩，不需要额外用字典按
		// 类型去区分。
		private StructureData _cachedStructure;
		private bool _loadAttempted;

		// 懒加载并缓存这个建筑对应的结构数据。读取失败（文件不存在/损坏）时
		// 返回 null 而不是抛异常——调用方（本类的 UseItem、预览系统）都必须
		// 对 null 做安全跳过，不能假设这里一定有数据。
		public StructureData GetStructure() {
			if (!_loadAttempted) {
				_loadAttempted = true;
				try {
					_cachedStructure = AkStructureFormat.LoadFromMod(Mod, StructureAssetPath);
				}
				catch (Exception ex) {
					Mod.Logger.Warn($"[{GetType().Name}] 结构文件读取失败：{StructureAssetPath}，{ex.Message}");
					_cachedStructure = null;
				}
			}
			return _cachedStructure;
		}

		// 子类在 SetDefaults 里先调用这个，填公共的"一次性消耗品"部分，再补自己的贴图尺寸/价格。
		protected void ApplyCommonDefaults() {
			Item.maxStack = 99;
			Item.consumable = true;
			Item.useStyle = ItemUseStyleID.HoldUp;
			Item.useTime = 30;
			Item.useAnimation = 30;
			Item.UseSound = SoundID.Item4;
			Item.noMelee = true;
		}

		// 结构左上角要落在世界的哪个 tile。默认约定：光标始终对着建筑"水平居中、
		// 贴底部"的位置——即结构左上角 = 光标所在格 - (宽度/2, 高度-1)。
		//
		// ⚠ 这里不能直接用 Main.MouseWorld：它的算法就是简单的
		// "MouseScreen + screenPosition"，完全没考虑游戏内的缩放（Zoom）设置，
		// 而我们画投影时是连同 Main.GameViewMatrix.TransformationMatrix 一起
		// 交给 spriteBatch 的，那个矩阵是会按 Zoom 缩放的。缩放不是 1:1 时，
		// 这两者就对不上了——且这种偏差是按"离缩放中心（一般是屏幕中心）的
		// 距离"放大的，所以表现出来正是"光标越往屏幕边缘移动、投影和光标
		// 错位越明显"。这里用 TransformationMatrix 的逆矩阵把屏幕像素换算回
		// "缩放前"的坐标，再加上 screenPosition，才是投影绘制那边实际认可的
		// 世界坐标，两边就能重新对齐。
		public virtual Point GetDeploymentTopLeft() {
			Vector2 unscaledMouse = Vector2.Transform(Main.MouseScreen, Matrix.Invert(Main.GameViewMatrix.TransformationMatrix));
			Point cursor = (unscaledMouse + Main.screenPosition).ToTileCoordinates();
			StructureData structure = GetStructure();
			if (structure == null)
				return cursor;

			return new Point(cursor.X - structure.Width / 2, cursor.Y - (structure.Height - 1));
		}

		public sealed override bool? UseItem(Player player) {
			if (player.whoAmI != Main.myPlayer)
				return null;

			StructureData structure = GetStructure();
			if (structure == null)
				return null; // 没有结构数据可放：静默什么都不做，ModifyTooltips 已经提示过缺文件了

			// 优先复用玩家点击前一帧刚看到的幽灵投影位置，而不是在这里重新采样鼠标——
			// 详见 AkStructureDeploySystem.TryGetLastPreviewTopLeft 处的说明，
			// 这是"投影比实际落地位置高一格"那个 bug 的修复。
			if (!AkStructureDeploySystem.TryGetLastPreviewTopLeft(Type, out Point topLeft))
				topLeft = GetDeploymentTopLeft();

			if (Main.netMode == NetmodeID.MultiplayerClient) {
				AkStructureDeploySystem.RequestDeploy(Type, topLeft);
				return false; // 不做本地预测：等服务器确认后再落地，跟便携自建安全屋是同一套约定
			}

			if (Main.netMode == NetmodeID.Server)
				return false;

			// 单人：本机既是客户端也是权威，直接落地
			if (!AkStructureDeploySystem.TryDeployLocally(structure, topLeft, out Rectangle area, out string failureKey)) {
				if (failureKey != null)
					Main.NewText(Terraria.Localization.Language.GetTextValue(failureKey), Color.OrangeRed);
				return false;
			}

			PortableStructureEntryEffect.TryBeginEntryEffect(area, structure);
			return true;
		}

		public override void ModifyTooltips(System.Collections.Generic.List<TooltipLine> tooltips) {
			if (GetStructure() == null) {
				tooltips.Add(new TooltipLine(Mod, "MissingStructureFile",
					$"[结构文件缺失或损坏：{StructureAssetPath}]") { OverrideColor = Color.OrangeRed });
			}
		}
	}
}
