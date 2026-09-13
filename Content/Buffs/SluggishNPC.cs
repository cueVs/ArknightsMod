using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Buffs
{
	// 「迟钝」减速：可叠加的移动减速效果（可露希尔三技能命中施加）。
	//   每层 6% 移动减速，最多 10 层（60%），每次命中刷新持续时间为 3 秒。
	//   纯逻辑（无 Buff 图标）：直接用 GlobalNPC 记录层数/计时，PostAI 阶段按层数缩放移动速度。
	public class SluggishNPC : GlobalNPC
	{
		public const int MaxStacks = 10;         // 6% * 10 = 60% 上限
		public const float PerStackSlow = 0.06f; // 每层 6%
		public const int DurationTicks = 180;    // 持续 3 秒（60TPS）

		public override bool InstancePerEntity => true;

		public int Stacks;
		private int _timer;

		/// <summary>对目标叠加一层迟钝并刷新持续时间（服务器/单机权威）。</summary>
		public static void AddStack(NPC npc) {
			if (npc == null || !npc.active || npc.friendly || npc.dontTakeDamage || npc.lifeMax <= 5)
				return;
			if (Main.netMode == NetmodeID.MultiplayerClient)
				return;

			var g = npc.GetGlobalNPC<SluggishNPC>();
			g.Stacks = Math.Min(g.Stacks + 1, MaxStacks);
			g._timer = DurationTicks;
			npc.netUpdate = true;
		}

		public override void PostAI(NPC npc) {
			if (Stacks <= 0)
				return;

			if (_timer > 0) {
				_timer--;
				float keep = 1f - PerStackSlow * Stacks;
				npc.velocity *= keep;
			}

			if (_timer <= 0)
				Stacks = 0;
		}
	}
}
