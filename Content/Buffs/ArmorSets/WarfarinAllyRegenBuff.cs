using Terraria;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Buffs.ArmorSets
{
	public class WarfarinAllyRegenBuff : ModBuff
	{
		public const int DurationTicks = 10 * 60;
		public const int RegenPerSecond = 20;

		public override void SetStaticDefaults() {
			Main.buffNoSave[Type] = true;
		}
	}
}
