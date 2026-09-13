using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Buffs
{
	// 模组内「眩晕」的统一判定与施加（文案中的「困惑」均指此类眩晕，不使用 BuffID.Confused）。
	public static class OperatorStunNPC
	{
		public const int DefaultDurationTicks = 30;
		public const int MaxDurationTicks = 60 * 5;

		public static bool HasStun(NPC npc) {
			return npc.HasBuff<StunDebuff>() || npc.HasBuff<TyphonSkillArrowStunBuff>();
		}

		public static bool CanStun(NPC npc) {
			return npc.active
				&& !npc.friendly
				&& !npc.dontTakeDamage
				&& npc.life > 0
				&& npc.lifeMax > 5;
		}

		public static void TryApply(NPC target, int durationTicks = DefaultDurationTicks) {
			if (Main.netMode == NetmodeID.MultiplayerClient)
				return;

			if (!CanStun(target))
				return;

			int buffType = ModContent.BuffType<StunDebuff>();
			int index = target.FindBuffIndex(buffType);
			if (index >= 0) {
				target.buffTime[index] = Math.Min(target.buffTime[index] + durationTicks, MaxDurationTicks);
			}
			else {
				target.AddBuff(buffType, durationTicks);
			}

			target.netUpdate = true;
		}

		// 干员 W 技能组命中时调用（武器/弹幕实现处）。
		public static void TryApplyFromWSkill(NPC target, int durationTicks = 45) {
			TryApply(target, durationTicks);
		}

		// 维什戴尔武器命中（含余震）时调用。
		public static void TryApplyFromWisadel(NPC target, int durationTicks = DefaultDurationTicks) {
			TryApply(target, durationTicks);
		}
	}
}
