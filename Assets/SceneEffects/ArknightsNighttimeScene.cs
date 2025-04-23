using ArknightsMod.Common.Configs;
using Terraria;
using Terraria.ModLoader;

namespace ArknightsMod.Assets.SceneEffects
{
	internal class ArknightsNighttimeScene : ModSceneEffect
	{
		public override int Music => MusicLoader.GetMusicSlot(Mod, "Music/asisaw");
		public override SceneEffectPriority Priority => SceneEffectPriority.BiomeLow;
		public override bool IsLoadingEnabled(Mod mod) {
			return ModContent.GetInstance<MusicConfig>().EnableArknightsForestNighttime;
		}

		public override bool IsSceneEffectActive(Player player) => Main.player[Main.myPlayer].active && Main.player[Main.myPlayer].ZoneOverworldHeight && !Main.dayTime && !Main.player[Main.myPlayer].ZoneDesert && !Main.player[Main.myPlayer].ZoneBeach;
	}
}
