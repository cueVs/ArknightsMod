using Terraria;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Buffs.ArmorSets
{
	public class CivilightEternaHealBoostBuff : ModBuff
	{
		public const int DurationTicks = 6 * 60;

		public override void SetStaticDefaults() {
			Main.buffNoSave[Type] = true;
		}
	}
}
