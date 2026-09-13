using Terraria;
using Terraria.ModLoader;

namespace ArknightsMod.Common.GlobalNPCs
{
	public class EntelechiaGlobalNPC : GlobalNPC
	{
		public int LifeMaxReduction;

		public override bool InstancePerEntity => true;

		public override void OnKill(NPC npc) {
			LifeMaxReduction = 0;
		}
	}
}
