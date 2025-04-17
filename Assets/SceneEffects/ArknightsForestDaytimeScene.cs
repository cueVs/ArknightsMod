using Terraria;
using Terraria.ModLoader;

namespace ArknightsMod.Assets.SceneEffects
{
	internal class ArknightsForestDaytimeScene : ModSceneEffect
	{
		public override int Music => MusicLoader.GetMusicSlot(Mod, "Music/lifeglow");
		public override SceneEffectPriority Priority => SceneEffectPriority.BiomeLow;
		public override bool IsLoadingEnabled(Mod mod)
		{
			return ModContent.GetInstance<MusicConfig>().EnableArknightsForestDaytime;
		}

		public override bool IsSceneEffectActive(Player player) => Main.player[Main.myPlayer].active && Main.player[Main.myPlayer].ZoneOverworldHeight && Main.dayTime && !Main.player[Main.myPlayer].ZoneDesert && !Main.player[Main.myPlayer].ZoneBeach && !Main.player[Main.myPlayer].ZoneHallow;
	}
}
