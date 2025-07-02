using ArknightsMod.Common.Configs;
using Terraria;
using Terraria.ModLoader;

namespace ArknightsMod.Assets.SceneEffects
{
	internal class ArknightsSpaceNighttimeHighScene : ModSceneEffect
	{
		public override int Music => MusicLoader.GetMusicSlot(Mod, "Music/adastra");
		public override SceneEffectPriority Priority => SceneEffectPriority.Environment;
		public override bool IsLoadingEnabled(Mod mod) {
			return ModContent.GetInstance<MusicConfig>().EnableArknightsSpaceNighttimeHigh;
		}

		public override bool IsSceneEffectActive(Player player) => Main.player[Main.myPlayer].active && !Main.dayTime && Main.player[Main.myPlayer].position.Y <= 1099.89041096f /*本来打算用1099.1121f的，但这样极难卡到1099，于是用日期占比了（325/365）*/&& !Main.player[Main.myPlayer].ZoneCorrupt && !Main.player[Main.myPlayer].ZoneCrimson && !Main.player[Main.myPlayer].ZoneHallow;
	}
}
