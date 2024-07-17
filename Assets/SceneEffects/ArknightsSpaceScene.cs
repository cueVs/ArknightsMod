using Terraria;
using Terraria.ModLoader;

namespace ArknightsMod.Assets.SceneEffects
{
	internal class ArknightsSpaceScene : ModSceneEffect
	{
		public override int Music => MusicLoader.GetMusicSlot(Mod, "Music/adastra");
		public override SceneEffectPriority Priority => SceneEffectPriority.Environment;
		public override bool IsLoadingEnabled(Mod mod)
		{
			return ModContent.GetInstance<MusicConfig>().EnableArknightsSpace;
		}

		public override bool IsSceneEffectActive(Player player) => Main.player[Main.myPlayer].active && !Main.dayTime && Main.player[Main.myPlayer].Center.Y <= 1099.1121f && !Main.player[Main.myPlayer].ZoneCorrupt && !Main.player[Main.myPlayer].ZoneCrimson;
	}
}
