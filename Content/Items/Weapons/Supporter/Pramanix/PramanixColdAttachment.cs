using System.Collections.Generic;
using ArknightsMod.Content.Buffs.Supporter.Pramanix;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Weapons.Supporter.Pramanix
{
	public static class PramanixColdAttachment
	{
		private static readonly Dictionary<int, int> Stacks = new();
		private static readonly Dictionary<int, int> Skill2HitCounts = new();
		private static readonly Dictionary<int, int> PendingFreezes = new();
		private static readonly Dictionary<int, int> KnockbackGraceTicks = new();

		public const int ChilledAttachTicks = 6;
		public const int DomainFreezeThreshold = 8;
		public const int Skill2FreezeInterval = 3;
		// 一技能击退生效后再冰冻、再叠减速
		public const int Skill1KnockbackGraceTicks = 40;
		public const int Skill1FreezeDelayTicks = 40;

		public static void ApplyDomain(NPC npc, Player player) {
			if (Main.netMode == NetmodeID.MultiplayerClient)
				return;
			if (!CanAffect(npc))
				return;

			AddStack(npc, 1);
			ApplyChilled(npc);
			if (GetStacks(npc) >= DomainFreezeThreshold)
				TryFreeze(npc);
		}

		public static void ApplySkill2Hit(NPC npc, Player player) {
			if (Main.netMode == NetmodeID.MultiplayerClient)
				return;
			if (!CanAffect(npc))
				return;

			AddStack(npc, 4);
			ApplyChilled(npc);

			int id = npc.whoAmI;
			Skill2HitCounts.TryGetValue(id, out int hits);
			hits++;
			Skill2HitCounts[id] = hits;
			if (hits % Skill2FreezeInterval == 0 || GetStacks(npc) >= DomainFreezeThreshold)
				TryFreeze(npc);
		}

		public static void ApplyBurst(NPC npc, int chilledSeconds = 3) {
			if (Main.netMode == NetmodeID.MultiplayerClient)
				return;
			if (!CanAffect(npc))
				return;

			AddStack(npc, 3);
			ApplyChilled(npc, chilledSeconds * 60);
			TryFreeze(npc);
		}

		// 一技能：叠层与 Chilled 立即生效，冰冻延后以免打断击退
		public static void ApplySkill1Hit(NPC npc, int chilledSeconds = 3) {
			if (Main.netMode == NetmodeID.MultiplayerClient)
				return;
			if (!CanAffect(npc))
				return;

			AddStack(npc, 3);
			ApplyChilled(npc, chilledSeconds * 60);
			ScheduleFreeze(npc, Skill1FreezeDelayTicks);
		}

		public static void GrantKnockbackGrace(NPC npc, int ticks = Skill1KnockbackGraceTicks) {
			int id = npc.whoAmI;
			KnockbackGraceTicks.TryGetValue(id, out int current);
			KnockbackGraceTicks[id] = System.Math.Max(current, ticks);
		}

		public static bool HasKnockbackGrace(NPC npc) =>
			KnockbackGraceTicks.TryGetValue(npc.whoAmI, out int ticks) && ticks > 0;

		public static void TickPending() {
			if (Main.netMode == NetmodeID.MultiplayerClient)
				return;

			TickGraceTimers();
			TickScheduledFreezes();
		}

		private static void ScheduleFreeze(NPC npc, int delayTicks, int freezeTicks = 120) {
			if (npc.boss)
				return;

			int id = npc.whoAmI;
			PendingFreezes.TryGetValue(id, out int current);
			PendingFreezes[id] = System.Math.Max(current, delayTicks);
		}

		private static void TickGraceTimers() {
			var remove = new List<int>();
			foreach (var (id, ticks) in KnockbackGraceTicks) {
				if (id < 0 || id >= Main.maxNPCs || !Main.npc[id].active) {
					remove.Add(id);
					continue;
				}
				if (ticks <= 1)
					remove.Add(id);
				else
					KnockbackGraceTicks[id] = ticks - 1;
			}
			foreach (int id in remove)
				KnockbackGraceTicks.Remove(id);
		}

		private static void TickScheduledFreezes() {
			var remove = new List<int>();
			foreach (var (id, delay) in PendingFreezes) {
				if (id < 0 || id >= Main.maxNPCs || !Main.npc[id].active) {
					remove.Add(id);
					continue;
				}
				if (delay <= 1) {
					TryFreeze(Main.npc[id]);
					remove.Add(id);
				}
				else {
					PendingFreezes[id] = delay - 1;
				}
			}
			foreach (int id in remove)
				PendingFreezes.Remove(id);
		}

		public static void Clear(NPC npc) {
			int id = npc.whoAmI;
			Stacks.Remove(id);
			Skill2HitCounts.Remove(id);
			PendingFreezes.Remove(id);
			KnockbackGraceTicks.Remove(id);
		}

		private static bool CanAffect(NPC npc) =>
			npc.active && !npc.friendly && !npc.dontTakeDamage && npc.life > 0;

		private static int GetStacks(NPC npc) {
			Stacks.TryGetValue(npc.whoAmI, out int value);
			return value;
		}

		private static void AddStack(NPC npc, int amount) {
			int id = npc.whoAmI;
			Stacks.TryGetValue(id, out int value);
			Stacks[id] = value + amount;
		}

		private static void ApplyChilled(NPC npc, int ticks = ChilledAttachTicks) {
			npc.buffImmune[BuffID.Chilled] = false;
			npc.AddBuff(BuffID.Chilled, ticks);
			npc.netUpdate = true;
		}

		public static void TryFreeze(NPC npc, int ticks = 120) {
			if (!CanAffect(npc) || npc.boss)
				return;

			int freezeType = ModContent.BuffType<PramanixFreezeDebuff>();
			if (!npc.HasBuff(freezeType)) {
				npc.AddBuff(freezeType, ticks);
				if (Main.netMode != NetmodeID.Server)
					SoundEngine.PlaySound(new SoundStyle("ArknightsMod/Sounds/Frozen") with { Volume = 0.75f, Pitch = Main.rand.NextFloat(-0.06f, 0.06f) }, npc.Center);
			}
			else {
				npc.AddBuff(freezeType, ticks);
			}

			Stacks[npc.whoAmI] = 0;
			npc.netUpdate = true;
		}

		public static void CleanupInactive() {
			var remove = new List<int>();
			foreach (var (id, _) in Stacks) {
				if (id < 0 || id >= Main.maxNPCs || !Main.npc[id].active)
					remove.Add(id);
			}
			foreach (int id in remove) {
				Stacks.Remove(id);
				Skill2HitCounts.Remove(id);
				PendingFreezes.Remove(id);
				KnockbackGraceTicks.Remove(id);
			}
		}
	}
}
