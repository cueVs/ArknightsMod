using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using System.Collections.Generic;
using System.Security.Principal;

namespace ArknightsMod.Content.Buffs
{
	public class StunDebuff : ModBuff
	{
		public override void SetStaticDefaults() {
			Main.pvpBuff[Type] = true;
			Main.buffNoSave[Type] = true;
			Main.debuff[Type] = true;
		}

		public override void Update(Player player, ref int buffIndex) {
			//stun parameters
		}

		public override void Update(NPC npc, ref int buffIndex) {
			//stun parameters
		}
	}


	public class StunDebuffPlayer : ModPlayer
	{
		
	}

	public class StunDebuffGlobalNPC : GlobalNPC
	{

	}
}
