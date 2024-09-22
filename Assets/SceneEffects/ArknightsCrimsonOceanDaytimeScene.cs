﻿using Terraria;
using Terraria.IO;
using Terraria.ModLoader;
using Terraria.WorldBuilding;

namespace ArknightsMod.Assets.SceneEffects
{
	internal class ArknightsCrimsonOceanDaytimeScene : ModSceneEffect
	{
		public override int Music => MusicLoader.GetMusicSlot(Mod, "Music/resonance");
		public override SceneEffectPriority Priority => SceneEffectPriority.BiomeHigh;
		public override bool IsLoadingEnabled(Mod mod)
		{
			return ModContent.GetInstance<MusicConfig>().EnableArknightsCrimsonOceanDaytime;
		}

		public override bool IsSceneEffectActive(Player player){
			if (WorldGen.GetWorldSize() is WorldGen.WorldSize.Large) {
				return Main.player[Main.myPlayer].active && Main.dayTime && (Main.player[Main.myPlayer].position.X <= 6080 || Main.player[Main.myPlayer].position.X >= WorldGen.WorldSizeLargeX * 16 - 6080) && !Main.player[Main.myPlayer].ZoneCorrupt && Main.player[Main.myPlayer].ZoneCrimson && !Main.player[Main.myPlayer].ZoneHallow;
			}
			else if (WorldGen.GetWorldSize() is WorldGen.WorldSize.Medium) {
				return Main.player[Main.myPlayer].active && Main.dayTime && (Main.player[Main.myPlayer].position.X <= 6080 || Main.player[Main.myPlayer].position.X >= WorldGen.WorldSizeMediumX * 16 - 6080) && !Main.player[Main.myPlayer].ZoneCorrupt && Main.player[Main.myPlayer].ZoneCrimson && !Main.player[Main.myPlayer].ZoneHallow;
			}
			else {
				return Main.player[Main.myPlayer].active && Main.dayTime && (Main.player[Main.myPlayer].position.X <= 6080 || Main.player[Main.myPlayer].position.X >= WorldGen.WorldSizeSmallX * 16 - 6080) && !Main.player[Main.myPlayer].ZoneCorrupt && Main.player[Main.myPlayer].ZoneCrimson && !Main.player[Main.myPlayer].ZoneHallow;
			}
		}
	}
}
