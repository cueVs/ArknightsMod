using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Buffs
{
	public class Tumor1 : ModBuff
	{


		public override void Update(Player player, ref int buffIndex) {
			player.maxRunSpeed *= 0.8f; 
			player.GetAttackSpeed(DamageClass.Generic) -= 0.2f;
		}
	}
}