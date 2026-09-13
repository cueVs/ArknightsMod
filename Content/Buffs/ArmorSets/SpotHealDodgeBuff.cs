using Terraria;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Buffs.ArmorSets
{
	public class SpotHealDodgeBuff : ModBuff
	{
		public const int DurationTicks = 3 * 60;

		public override void SetStaticDefaults() {
			Main.buffNoSave[Type] = true;
		}
	}
}
