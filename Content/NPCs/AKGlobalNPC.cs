using ArknightsMod.Content.Events;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using ArknightsMod.Content.NPCs;
using ArknightsMod.Content.NPCs.Enemy.ThroughChapter4;


namespace ArknightsMod.Content.NPCs
{
	public class UnionInvadeSpawnSystem : GlobalNPC
	{
		public override void EditSpawnRate(Player player, ref int spawnRate, ref int maxSpawns) {
			if (UnionInvade.EventActive) {
				spawnRate = 60;  // 固定快速生成率（值越小生成越快）
				maxSpawns = 15;  // 最大怪物数量
			}
		}

		public override void EditSpawnPool(IDictionary<int, float> pool, NPCSpawnInfo spawnInfo) {
			if (UnionInvade.EventActive) {
				// 清空原有生成池
				pool.Clear();

				// 只生成事件怪物
				pool[ModContent.NPCType<Soldier>()] = 1f;
				pool[ModContent.NPCType<Hound>()] = 1f;
				if (UnionInvade.MonstersKilled >= 100) {
					pool[ModContent.NPCType<Crossbowman>()] = 1f;
				}
				if (UnionInvade.MonstersKilled >= 150) {
					pool[ModContent.NPCType<Drone>()] = 0.8f;
				}
				if (UnionInvade.MonstersKilled >= 250) {
					pool[ModContent.NPCType<MortarGunner>()] = 0.6f;
				}
				if (UnionInvade.MonstersKilled >= 300) {
					pool[ModContent.NPCType<Seniorcaster>()] = 0.45f;
				}
			}
		}
		public override void OnKill(NPC npc) {
			if (UnionInvade.EventActive && UnionInvade.EventMonsters.Contains(npc.type)) {
				UnionInvade.OnNPCKilled(npc);
			}
		}
	}
}
