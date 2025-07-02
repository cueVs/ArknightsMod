using ArknightsMod.Common.Configs;
using Terraria;
using Terraria.ModLoader;

namespace ArknightsMod.Assets.SceneEffects
{
	internal class ArknightsSpaceDaytimeScene : ModSceneEffect
	{
		public override int Music => MusicLoader.GetMusicSlot(Mod, "Music/trimountsky");
		public override SceneEffectPriority Priority => SceneEffectPriority.Environment;
		public override bool IsLoadingEnabled(Mod mod) {
			return ModContent.GetInstance<MusicConfig>().EnableArknightsSpaceDaytime;
		}

		public override bool IsSceneEffectActive(Player player) => Main.player[Main.myPlayer].active && Main.dayTime && Main.player[Main.myPlayer].ZoneSkyHeight && !Main.player[Main.myPlayer].ZoneCorrupt && !Main.player[Main.myPlayer].ZoneCrimson && !Main.player[Main.myPlayer].ZoneHallow;
	}
}
