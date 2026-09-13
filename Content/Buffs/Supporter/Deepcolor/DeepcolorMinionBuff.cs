using ArknightsMod.Content.Projectiles.Supporter.Deepcolor;
using Terraria;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Buffs.Supporter.Deepcolor
{
	public class DeepcolorMinionBuff : ModBuff
	{
		public override void SetStaticDefaults() {
			Main.buffNoSave[Type] = true;
			Main.buffNoTimeDisplay[Type] = true;
		}

		public override void Update(Player player, ref int buffIndex) {
			if (player.ownedProjectileCounts[ModContent.ProjectileType<DeepcolorMinion>()] > 0)
				player.buffTime[buffIndex] = 2;
			else {
				player.DelBuff(buffIndex);
				buffIndex--;
			}
		}
	}
}
