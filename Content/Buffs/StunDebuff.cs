using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using System.Collections.Generic;
using System.Security.Principal;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;

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
			player.GetModPlayer<StunDebuffPlayer>().stunDebuff = true;

			//stun parameters
		}

		public override void Update(NPC npc, ref int buffIndex) {
			npc.GetGlobalNPC<StunDebuffGlobalNPC>().stunDebuff = true;

			//stun parameters
		}
	}


	public class StunDebuffPlayer : ModPlayer
	{
		public bool stunDebuff;

		public override void ResetEffects() {
			stunDebuff = false;
		}

		public override void DrawEffects(
			PlayerDrawSet drawInfo,
			ref float r, ref float g, ref float b, ref float a,
			ref bool fullBright
			)
			{
			r *= 0.5f;
			g *= 0.5f;
			b *= 0.5f;
		}
	}

	public class StunDebuffGlobalNPC : GlobalNPC
	{
		public bool stunDebuff;

		public override bool InstancePerEntity => true;

		public override void ResetEffects(NPC npc) {
			stunDebuff = false;
		}

		public override void DrawEffects(NPC npc, ref Color drawColor) {
			if (stunDebuff) {
				drawColor.R *= (byte)0.5;
				drawColor.G *= (byte)0.5;
				drawColor.B *= (byte)0.5;
			}
		}
	}
}
