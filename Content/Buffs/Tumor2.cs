using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Buffs
{
	public class Tumor2 : ModBuff
	{


		public override void Update(Player player, ref int buffIndex) {
			player.maxRunSpeed *= 0.35f;
			player.GetAttackSpeed(DamageClass.Generic) -= 0.75f;
			Dust.NewDust(player.position, player.width, player.height, DustID.Blood, 0f, 0f, 150, default, 1.5f);
		}
	}
}
