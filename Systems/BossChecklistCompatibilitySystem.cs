using System;
using System.Collections.Generic;
using ArknightsMod.Content.Items.BossSummon;
using ArknightsMod.Content.NPCs.Enemy.Chapter6.FrostNova;
using ArknightsMod.Content.NPCs.Enemy.OF.Pmp;
using ArknightsMod.Content.NPCs.Enemy.RoaringFlare.ImperialArtilleyCoreTargeteer;
using ArknightsMod.Content.NPCs.Enemy.Seamonster;
using Terraria.ModLoader;

namespace ArknightsMod.Systems
{
	public class BossChecklistCompatibilitySystem : ModSystem
	{
		public override void PostSetupContent()
		{
			if (!ModLoader.TryGetMod("BossChecklist", out Mod bossChecklist))
				return;

			TryLogBoss(bossChecklist,
				internalName: "Pompeii",
				progression: 3.0f,
				downed: () => DownedBossSystem.DownedPompeii,
				npcIDs: new List<int> { ModContent.NPCType<Pompeii>() },
				spawnItems: new List<int> { ModContent.ItemType<PompeiiSummon>() }
			);

			TryLogBoss(bossChecklist,
				internalName: "TheFirstToTalk",
				progression: 3.5f,
				downed: () => DownedBossSystem.DownedTheFirstToTalk,
				npcIDs: new List<int> { ModContent.NPCType<TheFirstToTalk>() },
				spawnItems: new List<int> { ModContent.ItemType<TheFirstToTalkSummon>() }
			);

			TryLogBoss(bossChecklist,
				internalName: "FrostNova",
				progression: 10.0f,
				downed: () => DownedBossSystem.DownedFrostNova,
				npcIDs: new List<int> { ModContent.NPCType<FrostNova>() },
				spawnItems: new List<int> { ModContent.ItemType<SpicyCandy>() }
			);

			TryLogBoss(bossChecklist,
				internalName: "AACT",
				progression: 10.5f,
				downed: () => DownedBossSystem.DownedAACT,
				npcIDs: new List<int> { ModContent.NPCType<AACT>() },
				spawnItems: new List<int> { ModContent.ItemType<AACTSummon>() }
			);
		}

		private void TryLogBoss(Mod bossChecklist, string internalName, float progression, Func<bool> downed, List<int> npcIDs, List<int> spawnItems)
		{
			var extra = new Dictionary<string, object>
			{
				["spawnItems"] = spawnItems,
			};

			bossChecklist.Call("LogBoss", Mod, internalName, progression, downed, npcIDs, extra);
		}
	}
}
