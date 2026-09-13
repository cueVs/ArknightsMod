using System.Collections.Generic;
using ArknightsMod.Common;
using Terraria;
using Terraria.ModLoader;

namespace ArknightsMod.Players
{
	public class EchoCornPlayer : ModPlayer
	{
		public const int BaseValue = 4;

		public bool HasEchoCorn;
		public int Value = BaseValue;

		private readonly HashSet<int> talkedNpcTypes = [];
		private int lastTalkNPC = -1;

		public override void ResetEffects() {
			HasEchoCorn = false;
		}

		public override void PostUpdate() {
			int current = Player.talkNPC;

			if (current != lastTalkNPC && current >= 0 && HasEchoCorn) {
				NPC npc = Main.npc[current];
				if (npc.active && talkedNpcTypes.Add(npc.type)) {
					int happiness = TownNPCHappinessHelper.GetHappinessPercent(Player, npc);
					Value += happiness / 5;
				}
			}

			lastTalkNPC = current;
		}
	}
}
