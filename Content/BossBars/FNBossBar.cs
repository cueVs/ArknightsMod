using Terraria.ModLoader;
using Terraria.GameContent.UI.BigProgressBar;

namespace ArknightsMod.Content.BossBars
{
	public class FNBossBar : ModBossBar
	{
		public override string Texture => ArknightsMod.noTexture;

		public override bool? ModifyInfo(ref BigProgressBarInfo info, ref float life, ref float lifeMax, ref float shield, ref float shieldMax) {
			return false;
		}
	}
}