using Terraria;
using Terraria.ModLoader;

namespace ArknightsMod.Common.GlobalNPCs
{
	public class RaidianMarkGlobalNPC : GlobalNPC
	{
		public bool RaidianMarked;

		public override bool InstancePerEntity => true;

		public override void OnKill(NPC npc) {
			RaidianMarked = false;
		}
	}
}
