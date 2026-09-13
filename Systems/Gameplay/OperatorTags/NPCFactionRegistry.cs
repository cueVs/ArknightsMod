using ArknightsMod.Content.NPCs.Enemy.Evolution;
using ArknightsMod.Content.NPCs.Enemy.Chapter6;
using ArknightsMod.Content.NPCs.Enemy.Chapter6.FrostNova;
using ArknightsMod.Content.NPCs.Enemy.Evolution;
using ArknightsMod.Content.NPCs.Enemy.Seamonster;
using ArknightsMod.Content.NPCs.Enemy.ThroughChapter4;
using System.Collections.Generic;
using Terraria.ModLoader;

namespace ArknightsMod.Systems.Gameplay.OperatorTags
{
	internal static class NPCFactionRegistry
	{
		private static readonly Dictionary<int, OperatorFaction> ByNpcType = new();

		internal static void Register(int npcType, OperatorFaction factions) {
			ByNpcType[npcType] = factions;
		}

		internal static bool TryGet(int npcType, out OperatorFaction factions) {
			return ByNpcType.TryGetValue(npcType, out factions);
		}

		internal static void Clear() => ByNpcType.Clear();
	}

	internal class NPCFactionLoader : ModSystem
	{
		public override void OnModLoad() {
			NPCFactionRegistry.Clear();
			RegisterAll();
		}

		private static void RegisterAll() {
			Register<Hound>(OperatorFaction.Sarkaz | OperatorFaction.Reunion);
			Register<HoundPro>(OperatorFaction.Sarkaz | OperatorFaction.Reunion);
			Register<Crownslayer>(OperatorFaction.Sarkaz | OperatorFaction.Reunion);
			Register<Seniorcaster>(OperatorFaction.Sarkaz | OperatorFaction.Reunion);
			Register<Caster>(OperatorFaction.Reunion);
			Register<Evolution>(OperatorFaction.Sarkaz | OperatorFaction.Reunion);
			Register<tumor1>(OperatorFaction.Sarkaz | OperatorFaction.Reunion);
			Register<tumor2>(OperatorFaction.Sarkaz | OperatorFaction.Reunion);
			Register<Soldier>(OperatorFaction.Reunion);
			Register<SoldierLeader>(OperatorFaction.Reunion);
			Register<Crossbowman>(OperatorFaction.Reunion);
			Register<CrossbowmanLeader>(OperatorFaction.Reunion);
			Register<ShieldGuard>(OperatorFaction.Reunion);
			Register<DoubleSword>(OperatorFaction.Reunion);
			Register<MortarGunner>(OperatorFaction.Reunion);
			Register<LightShield>(OperatorFaction.Reunion);
			Register<Drone>(OperatorFaction.Reunion);
			Register<DroneII>(OperatorFaction.Reunion);
			Register<OriginiumSlug>(OperatorFaction.Reunion);
			Register<OriginiumSlugAlpha>(OperatorFaction.Reunion);
			Register<OriginiumSlugBeta>(OperatorFaction.Reunion);
			Register<FieryOriginiumSlugNPC>(OperatorFaction.Reunion);
			Register<RockCrab>(OperatorFaction.Reunion);

			Register<SnowSoldier>(OperatorFaction.Ursus);
			Register<SnowSniper>(OperatorFaction.Ursus);
			Register<SnowHound>(OperatorFaction.Ursus);
			Register<SnowCaster>(OperatorFaction.Ursus);
			Register<IceCleaver>(OperatorFaction.Ursus);
			Register<Oneiros>(OperatorFaction.Ursus);
			Register<FrostNova>(OperatorFaction.Ursus);

			Register<TheFirstToTalk>(OperatorFaction.Seaborn);
			Register<ShellSeaRunner>(OperatorFaction.Seaborn);
			Register<PrimalSeaPiercer>(OperatorFaction.Seaborn);
			Register<PocketSeaCrawler>(OperatorFaction.Seaborn);
			Register<NourishedPiercer>(OperatorFaction.Seaborn);
			Register<FloatingSeaDrifter>(OperatorFaction.Seaborn);
			Register<DeepSeaSlider>(OperatorFaction.Seaborn);
			Register<BasinSeaReaper>(OperatorFaction.Seaborn);
		}

		private static void Register<TNpc>(OperatorFaction factions) where TNpc : ModNPC {
			NPCFactionRegistry.Register(ModContent.NPCType<TNpc>(), factions);
		}
	}
}
