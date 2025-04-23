using ArknightsMod.Common.Configs;
using Terraria;
using Terraria.ModLoader;

namespace ArknightsMod.Assets.SceneEffects
{
	internal class ArknightsHallowedOceanNighttimeScene : ModSceneEffect
	{
		public override int Music => MusicLoader.GetMusicSlot(Mod, "Music/upsidedowntreeshadow");
		public override SceneEffectPriority Priority => SceneEffectPriority.BiomeHigh;
		public override bool IsLoadingEnabled(Mod mod) {
			return ModContent.GetInstance<MusicConfig>().EnableArknightsHallowedOceanNighttime;
		}

		public override bool IsSceneEffectActive(Player player) {
			if (WorldGen.GetWorldSize() is WorldGen.WorldSize.Large) {
				return Main.player[Main.myPlayer].active && !Main.dayTime && (Main.player[Main.myPlayer].position.X <= 6080 || Main.player[Main.myPlayer].position.X >= WorldGen.WorldSizeLargeX * 16 - 6080) && !Main.player[Main.myPlayer].ZoneCorrupt && !Main.player[Main.myPlayer].ZoneCrimson && Main.player[Main.myPlayer].ZoneHallow;
			}
			else if (WorldGen.GetWorldSize() is WorldGen.WorldSize.Medium) {
				return Main.player[Main.myPlayer].active && !Main.dayTime && (Main.player[Main.myPlayer].position.X <= 6080 || Main.player[Main.myPlayer].position.X >= WorldGen.WorldSizeMediumX * 16 - 6080) && !Main.player[Main.myPlayer].ZoneCorrupt && !Main.player[Main.myPlayer].ZoneCrimson && Main.player[Main.myPlayer].ZoneHallow;
			}
			else {
				return Main.player[Main.myPlayer].active && !Main.dayTime && (Main.player[Main.myPlayer].position.X <= 6080 || Main.player[Main.myPlayer].position.X >= WorldGen.WorldSizeSmallX * 16 - 6080) && !Main.player[Main.myPlayer].ZoneCorrupt && !Main.player[Main.myPlayer].ZoneCrimson && Main.player[Main.myPlayer].ZoneHallow;
			}
		}
	}
}
