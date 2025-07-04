using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Buffs
{
	public class RARicecrabBuff : ModBuff
	{




		public override void Update(Player player, ref int buffIndex) {
			player.statLifeMax2 += (int)(player.statLifeMax2*0.08f); // Grant a +10 defense boost to the player while the buff is active.
			player.statDefense *= 1.08f;
		}
	}
}
