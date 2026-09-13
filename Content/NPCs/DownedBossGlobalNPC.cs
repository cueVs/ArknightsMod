using ArknightsMod.Content.NPCs.Enemy.Chapter6.FrostNova;
using ArknightsMod.Content.NPCs.Enemy.Evolution;
using ArknightsMod.Content.NPCs.Enemy.OF.Pmp;
using ArknightsMod.Content.NPCs.Enemy.RoaringFlare.ImperialArtilleyCoreTargeteer;
using ArknightsMod.Content.NPCs.Enemy.Seamonster;
using ArknightsMod.Systems;
using Terraria;
using Terraria.ModLoader;

namespace ArknightsMod.Content.NPCs
{
	public class DownedBossGlobalNPC : GlobalNPC
	{
		public override void OnKill(NPC npc)
		{
			if (npc.type == ModContent.NPCType<Pompeii>())
				DownedBossSystem.MarkDowned(ref DownedBossSystem.DownedPompeii);
			else if (npc.type == ModContent.NPCType<TheFirstToTalk>())
				DownedBossSystem.MarkDowned(ref DownedBossSystem.DownedTheFirstToTalk);
			else if (npc.type == ModContent.NPCType<FrostNova>())
				DownedBossSystem.MarkDowned(ref DownedBossSystem.DownedFrostNova);
			else if (npc.type == ModContent.NPCType<AACT>())
				DownedBossSystem.MarkDowned(ref DownedBossSystem.DownedAACT);
			else if (npc.type == ModContent.NPCType<Evolution>())
				DownedBossSystem.MarkDowned(ref DownedBossSystem.DownedEvolution);
		}
	}
}
