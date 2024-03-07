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
			player.velocity.X = 0;
			if (player.velocity.Y < 0) player.velocity.Y = 0;
		}

		public override void Update(NPC npc, ref int buffIndex) {
			var sdgNPC = npc.GetGlobalNPC<StunDebuffGlobalNPC>();
			sdgNPC.stunDebuff = true;

			if(!sdgNPC.defaultSettings.isAlreadySet) {
				sdgNPC.defaultSettings = (true, npc.noGravity, npc.noTileCollide, npc.damage);
			}


			//stun parameters
			npc.velocity.X = 0;
			if(npc.velocity.Y < 0) npc.velocity.Y = 0;

			npc.damage = 0;
			npc.noGravity = false;
			npc.noTileCollide = false;
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
			r *= 0.8f;
			g *= 0.8f;
			b *= 0.8f;
		}
	}

	public class StunDebuffGlobalNPC : GlobalNPC
	{
		public bool stunDebuff;
		public (bool isAlreadySet, bool noGravity, bool noTileCollide, int damage)
				defaultSettings = (false, false, false, 0);

		public override bool InstancePerEntity => true;

		public override void ResetEffects(NPC npc) {
			stunDebuff = false;

			if (defaultSettings.isAlreadySet)
			{
				npc.noGravity = defaultSettings.noGravity;
				npc.noTileCollide = defaultSettings.noTileCollide;
				npc.damage = defaultSettings.damage;
				defaultSettings.isAlreadySet = false;
			}
		}

		public override void DrawEffects(NPC npc, ref Color drawColor) {
			if (stunDebuff) {
				drawColor.R *= (byte)0.8;
				drawColor.G *= (byte)0.8;
				drawColor.B *= (byte)0.8;
			}
		}
	}
}
