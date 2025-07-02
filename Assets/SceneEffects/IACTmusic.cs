using ArknightsMod.Content.NPCs.Enemy.RoaringFlare.ImperialArtilleyCoreTargeteer;
using Terraria;
using Terraria.ModLoader;

namespace ArknightsMod.Assets.SceneEffects
{
	internal class IACTmusic : ModSceneEffect
	{
		public override int Music => MusicLoader.GetMusicSlot(Mod, "Music/IACTBoss3");
		public override SceneEffectPriority Priority => SceneEffectPriority.BossLow;
		public override bool IsSceneEffectActive(Player player) => Main.player[Main.myPlayer].active && NPC.AnyNPCs(ModContent.NPCType<IACT>());
	}
}
