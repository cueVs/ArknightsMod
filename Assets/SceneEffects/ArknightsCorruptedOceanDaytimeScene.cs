using ArknightsMod.Common.Configs;
using Terraria;
using Terraria.ModLoader;

namespace ArknightsMod.Assets.SceneEffects
{
	internal class ArknightsCorruptedOceanDaytimeScene : ModSceneEffect
	{
		public override int Music => MusicLoader.GetMusicSlot(Mod, "Music/darktides");
		public override SceneEffectPriority Priority => SceneEffectPriority.BiomeHigh;
		public override bool IsLoadingEnabled(Mod mod) {
			return ModContent.GetInstance<MusicConfig>().EnableArknightsCorruptedOceanDaytime;
		}

		public override bool IsSceneEffectActive(Player player) {
			if (WorldGen.GetWorldSize() is WorldGen.WorldSize.Large) {
				return Main.player[Main.myPlayer].active && Main.dayTime && (Main.player[Main.myPlayer].position.X <= 6080 || Main.player[Main.myPlayer].position.X >= WorldGen.WorldSizeLargeX * 16 - 6080) && Main.player[Main.myPlayer].ZoneCorrupt && !Main.player[Main.myPlayer].ZoneCrimson && !Main.player[Main.myPlayer].ZoneHallow;
			}
			else {
				return WorldGen.GetWorldSize() is WorldGen.WorldSize.Medium
					? Main.player[Main.myPlayer].active && Main.dayTime && (Main.player[Main.myPlayer].position.X <= 6080 || Main.player[Main.myPlayer].position.X >= WorldGen.WorldSizeMediumX * 16 - 6080) && Main.player[Main.myPlayer].ZoneCorrupt && !Main.player[Main.myPlayer].ZoneCrimson && !Main.player[Main.myPlayer].ZoneHallow
					: Main.player[Main.myPlayer].active && Main.dayTime && (Main.player[Main.myPlayer].position.X <= 6080 || Main.player[Main.myPlayer].position.X >= WorldGen.WorldSizeSmallX * 16 - 6080) && Main.player[Main.myPlayer].ZoneCorrupt && !Main.player[Main.myPlayer].ZoneCrimson && !Main.player[Main.myPlayer].ZoneHallow;
			}
		}
	}
}
