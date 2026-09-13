using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Buffs.Supporter.Pramanix
{
	public class PramanixSlowDebuff : ModBuff
	{
		public override string Texture => $"Terraria/Images/Buff_{BuffID.Slow}";

		public override void SetStaticDefaults() {
			Main.debuff[Type] = true;
			Main.buffNoSave[Type] = true;
		}
	}
}
