using Terraria;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Buffs.Guard.Melantha
{
	// 无畏者图标与计时；生命再生由 MelanthaSetPlayer.FearlessStacks 叠加提供。
	public class FearlessDebuff : ModBuff
	{
		public override void SetStaticDefaults() {
			Main.buffNoTimeDisplay[Type] = false;
		}

		public override void Update(Player player, ref int buffIndex) {
			// 叠加层数与再生在 MelanthaSetPlayer 中处理
		}
	}
}
