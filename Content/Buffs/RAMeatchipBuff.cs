using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Buffs
{
	public class RAMeatchipBuff : ModBuff
	{
		

		

		public override void Update(Player player, ref int buffIndex) {
			player.GetDamage(DamageClass.Generic)+=0.15f; // Grant a +10 defense boost to the player while the buff is active.
			player.GetAttackSpeed(DamageClass.Generic) += 0.15f;
		}
	}
}