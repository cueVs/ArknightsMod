using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace ArknightsMod.Systems
{
	public class DownedBossSystem : ModSystem
	{
		public static bool DownedPompeii;
		public static bool DownedTheFirstToTalk;
		public static bool DownedFrostNova;
		public static bool DownedAACT;
		public static bool DownedEvolution;

		public override void OnWorldLoad()
		{
			DownedPompeii = false;
			DownedTheFirstToTalk = false;
			DownedFrostNova = false;
			DownedAACT = false;
			DownedEvolution = false;
		}

		public override void OnWorldUnload()
		{
			DownedPompeii = false;
			DownedTheFirstToTalk = false;
			DownedFrostNova = false;
			DownedAACT = false;
			DownedEvolution = false;
		}

		public override void SaveWorldData(TagCompound tag)
		{
			if (DownedPompeii)
				tag["ArknightsMod.DownedPompeii"] = true;
			if (DownedTheFirstToTalk)
				tag["ArknightsMod.DownedTheFirstToTalk"] = true;
			if (DownedFrostNova)
				tag["ArknightsMod.DownedFrostNova"] = true;
			if (DownedAACT)
				tag["ArknightsMod.DownedAACT"] = true;
			if (DownedEvolution)
				tag["ArknightsMod.DownedEvolution"] = true;
		}

		public override void LoadWorldData(TagCompound tag)
		{
			DownedPompeii = tag.ContainsKey("ArknightsMod.DownedPompeii");
			DownedTheFirstToTalk = tag.ContainsKey("ArknightsMod.DownedTheFirstToTalk");
			DownedFrostNova = tag.ContainsKey("ArknightsMod.DownedFrostNova");
			DownedAACT = tag.ContainsKey("ArknightsMod.DownedAACT");
			DownedEvolution = tag.ContainsKey("ArknightsMod.DownedEvolution");
		}

		public static void MarkDowned(ref bool flag)
		{
			if (flag)
				return;

			flag = true;
			if (Main.netMode == NetmodeID.Server)
				NetMessage.SendData(MessageID.WorldData);
		}
	}
}
