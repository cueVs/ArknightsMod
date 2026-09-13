using System.Reflection;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent;

namespace ArknightsMod.Common
{
	// tModLoader 没有公开的“幸福度百分比”接口——NPC 实际的好感度价格修正（_currentPriceAdjustment）
	// 是 ShopHelper 内部私有字段。这里反射读取，并且用一个独立的 ShopHelper 实例跑 ProcessMood，
	// 不碰游戏正在用的 Main.ShopHelper 单例，避免影响玩家当前正在看的对话/商店文本。
	// 反射失败或计算异常时回退到 50（中性幸福度），不会让调用方崩掉。
	public static class TownNPCHappinessHelper
	{
		private static readonly MethodInfo ProcessMoodMethod =
			typeof(ShopHelper).GetMethod("ProcessMood", BindingFlags.NonPublic | BindingFlags.Instance);

		private static readonly FieldInfo PriceAdjustmentField =
			typeof(ShopHelper).GetField("_currentPriceAdjustment", BindingFlags.NonPublic | BindingFlags.Instance);

		private static readonly MethodInfo ReinitDatabaseMethod =
			typeof(ShopHelper).GetMethod("ReinitializePersonalityDatabase", BindingFlags.NonPublic | BindingFlags.Instance);

		private static ShopHelper isolatedHelper;

		private static ShopHelper GetIsolatedHelper() {
			if (isolatedHelper != null) return isolatedHelper;
			isolatedHelper = new ShopHelper();
			ReinitDatabaseMethod?.Invoke(isolatedHelper, null);
			return isolatedHelper;
		}

		// 价格修正大致落在 0.67（很喜欢，打折）~1.5（讨厌，加价）之间，换算成玩家直觉上的 0~100 幸福度百分比。
		public static int GetHappinessPercent(Player player, NPC npc) {
			if (ProcessMoodMethod == null || PriceAdjustmentField == null)
				return 50;

			try {
				ShopHelper helper = GetIsolatedHelper();
				ProcessMoodMethod.Invoke(helper, new object[] { player, npc });
				float adjustment = (float)PriceAdjustmentField.GetValue(helper);
				float percent = (1.5f - adjustment) / (1.5f - 0.67f) * 100f;
				return (int)MathHelper.Clamp(percent, 0f, 100f);
			}
			catch {
				return 50;
			}
		}
	}
}
