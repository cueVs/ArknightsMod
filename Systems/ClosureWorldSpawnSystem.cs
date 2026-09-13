using System.Collections.Generic;
using System.IO;
using ArknightsMod.Content.NPCs.Friendly;
using Terraria;
using Terraria.DataStructures;
using Terraria.IO;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.WorldBuilding;

namespace ArknightsMod.Systems
{
	public sealed class ClosureWorldSpawnSystem : ModSystem
	{
		public static bool ClosureTownUnlocked;

		public override void ClearWorld() {
			ClosureTownUnlocked = false;
		}

		public override void SaveWorldData(TagCompound tag) {
			if (ClosureTownUnlocked) {
				tag["ArknightsMod.ClosureTownUnlocked"] = true;
			}
		}

		public override void LoadWorldData(TagCompound tag) {
			ClosureTownUnlocked = tag.ContainsKey("ArknightsMod.ClosureTownUnlocked");
			ClosureTownUnlocked |= NPC.AnyNPCs(ModContent.NPCType<Closure>());
		}

		public override void NetSend(BinaryWriter writer) {
			writer.Write(ClosureTownUnlocked);
		}

		public override void NetReceive(BinaryReader reader) {
			ClosureTownUnlocked = reader.ReadBoolean();
		}

		public override void ModifyWorldGenTasks(List<GenPass> tasks, ref double totalWeight)
		{
			int index = tasks.FindIndex(static pass => pass.Name == "Guide");
			if (index < 0)
				index = tasks.Count - 1;

			tasks.Insert(index + 1, new ClosureSpawnGenPass("ArknightsMod: Closure", 0.016));
		}

		private sealed class ClosureSpawnGenPass : GenPass
		{
			public ClosureSpawnGenPass(string name, double loadWeight) : base(name, loadWeight)
			{
			}

			protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration)
			{
				int closureType = ModContent.NPCType<Closure>();
				for (int i = 0; i < Main.maxNPCs; i++) {
					if (Main.npc[i].active && Main.npc[i].type == closureType) {
						progress.Set(1.0);
						return;
					}
				}

				NPC closure = NPC.NewNPCDirect(new EntitySource_WorldGen(), Main.spawnTileX * 16, Main.spawnTileY * 16, closureType);
				closure.homeTileX = Main.spawnTileX;
				closure.homeTileY = Main.spawnTileY;
				closure.direction = 1;
				closure.homeless = true;

				progress.Set(1.0);
			}
		}
	}
}
