using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace ArknightsMod.Systems.Gameplay.OperatorTags
{
	public class OperatorFactionGlobalNPC : GlobalNPC
	{
		public OperatorFaction Factions;

		public override bool InstancePerEntity => true;

		public override void OnSpawn(NPC npc, IEntitySource source) {
			if (NPCFactionRegistry.TryGet(npc.type, out OperatorFaction factions))
				Factions = factions;
		}
	}
}
