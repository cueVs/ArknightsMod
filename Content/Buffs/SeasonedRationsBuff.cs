using Terraria;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Buffs
{
	public class SeasonedRationsBuff : ModBuff
	{

		public override void Update(Player player, ref int buffIndex) {
			player.statLifeMax2 += (int)(player.statLifeMax2 * 0.1f); // Grant a +10 defense boost to the player while the buff is active.

		}
	}
}
