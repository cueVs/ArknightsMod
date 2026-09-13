using System;
using System.Collections.Generic;
using ArknightsMod.Content.Tiles;
using ArknightsMod.Content.Tiles.Natural;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent.Events;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Systems
{
	// 地表小型植物/自然物的“自然生长”注册表：/plant 调试指令、无特殊条件的低概率环境刷新（IsAmbient）、
	// 以及各自 ModPlayer 里额外判定过的刷新（比如需要背包里有别的自然物）都走这一套放置逻辑。
	// 具体“能不能长在这块地上”交给每个 ModTile 自己的 TileObjectData.AnchorValidTiles/AnchorType 校验，
	// 这里只负责挑坐标、按概率触发、以及跳过校验的强制刷新（血月刷霜晶树用）。
	public class NaturalGrowthSystem : ModSystem
	{
		// extraCondition：放置位置的额外校验（比如“城镇范围”“正在下雨”），参数是放置坐标 (x, y)。
		// isAmbient：是否参与本系统统一的低概率环境刷新（不依赖玩家背包/buff 状态的那类自然物）。
		public readonly struct Entry(int tileType, int width, int height, Func<int, int, bool> extraCondition = null, bool isAmbient = false)
		{
			public readonly int TileType = tileType;
			public readonly int Width = width;
			public readonly int Height = height;
			public readonly Func<int, int, bool> ExtraCondition = extraCondition;
			public readonly bool IsAmbient = isAmbient;
		}

		private const int AmbientRadius = 30;
		private const float AmbientRollChance = 0.001f; // 每个在线玩家，约每秒判定一次

		private static readonly List<Entry> entries = [];
		private static bool wasBloodMoon;
		private static bool wasDayTime;

		public static int GameDayCounter { get; private set; }

		public override void PostSetupContent() {
			entries.Clear();
			entries.Add(new Entry(ModContent.TileType<BloodMushroom>(), 2, 2)); // 血蕈：饱食buff触发，见 BloodMushroomPlayer
			entries.Add(new Entry(ModContent.TileType<FrostCrystalTree>(), 3, 4, isAmbient: true));
			entries.Add(new Entry(ModContent.TileType<EchoCorn>(), 2, 4, (x, y) => IsNearTownNPC(x, y), isAmbient: true));
			entries.Add(new Entry(ModContent.TileType<WaveSpray>(), 2, 2, (x, y) => Main.raining, isAmbient: true));
			entries.Add(new Entry(ModContent.TileType<ProliferatingMoss>(), 2, 2, isAmbient: true));
			entries.Add(new Entry(ModContent.TileType<WitheredMossBall>(), 2, 2, isAmbient: true));
			entries.Add(new Entry(ModContent.TileType<BoardVine>(), 4, 2)); // 板藤：背包里有别的自然物才触发，见 BoardVinePlayer
			entries.Add(new Entry(ModContent.TileType<HomesickFruit>(), 3, 3, (x, y) => IsNearTownNPC(x, y, 2), isAmbient: true));
			entries.Add(new Entry(ModContent.TileType<GlowingTruffle>(), 4, 2, (x, y) => LanternNight.LanternsUp && IsNearTownNPC(x, y), isAmbient: true));
		}

		// “城镇环境”：附近有至少 minCount 个已安家的镇民。
		private static bool IsNearTownNPC(int x, int y, int minCount = 1) {
			Vector2 pos = new Vector2(x, y) * 16f;
			const float radius = 50 * 16f;
			int count = 0;

			for (int k = 0; k < Main.maxNPCs; k++) {
				NPC npc = Main.npc[k];
				if (npc.active && npc.townNPC && Vector2.Distance(npc.Center, pos) < radius) {
					count++;
					if (count >= minCount) return true;
				}
			}
			return false;
		}

		private static bool TryGrowFromCandidates(Player player, int radiusTiles, List<Entry> candidates, bool forced) {
			if (candidates.Count == 0) return false;

			Entry entry = candidates[Main.rand.Next(candidates.Count)];
			int px = (int)(player.Center.X / 16f);
			int py = (int)(player.Center.Y / 16f);
			int attempts = forced ? 30 : 20;

			for (int a = 0; a < attempts; a++) {
				int x = px + Main.rand.Next(-radiusTiles, radiusTiles + 1);
				int y = py + Main.rand.Next(-radiusTiles, radiusTiles + 1);
				if (TryPlaceAt(entry, x, y, forced)) return true;
			}
			return false;
		}

		// specificTileType 传 -1 表示从全部注册条目里随机挑一个尝试，否则只尝试指定的那一种。
		public static bool TryGrowRandomNear(Player player, int radiusTiles, int specificTileType = -1) {
			if (Main.netMode == NetmodeID.MultiplayerClient) return false;
			List<Entry> candidates = specificTileType < 0 ? entries : entries.FindAll(e => e.TileType == specificTileType);
			return TryGrowFromCandidates(player, radiusTiles, candidates, forced: false);
		}

		// 无视地面种类强制种下，目前只给血月的霜晶树保底生成用。
		public static bool TryForceGrowNear(Player player, int tileType, int radiusTiles) {
			if (Main.netMode == NetmodeID.MultiplayerClient) return false;
			return TryGrowFromCandidates(player, radiusTiles, entries.FindAll(e => e.TileType == tileType), forced: true);
		}

		// 遍历玩家周围整个矩形区域，把所有满足条件的位置一次性种满（供 /plant 使用）。返回种下的数量。
		public static int GrowAllNear(Player player, int radiusTiles) {
			if (Main.netMode == NetmodeID.MultiplayerClient) return 0;

			int px = (int)(player.Center.X / 16f);
			int py = (int)(player.Center.Y / 16f);
			int planted = 0;

			for (int x = px - radiusTiles; x <= px + radiusTiles; x++) {
				for (int y = py - radiusTiles; y <= py + radiusTiles; y++) {
					foreach (Entry entry in entries) {
						if (TryPlaceAt(entry, x, y, forced: false)) {
							planted++;
							break;
						}
					}
				}
			}
			return planted;
		}

		public override void PostUpdateWorld() {
			if (Main.netMode == NetmodeID.MultiplayerClient) return;

			bool bloodMoonNow = Main.bloodMoon;
			if (bloodMoonNow && !wasBloodMoon) {
				int frostType = ModContent.TileType<FrostCrystalTree>();
				for (int i = 0; i < Main.maxPlayers; i++) {
					Player player = Main.player[i];
					if (player.active)
						TryForceGrowNear(player, frostType, 20);
				}
			}
			wasBloodMoon = bloodMoonNow;

			bool dayTimeNow = Main.dayTime;
			if (dayTimeNow && !wasDayTime)
				GameDayCounter++;
			wasDayTime = dayTimeNow;

			if (Main.GameUpdateCount % 60 == 0) {
				for (int i = 0; i < Main.maxPlayers; i++) {
					Player player = Main.player[i];
					if (player.active && Main.rand.NextFloat() < AmbientRollChance)
						TryGrowFromCandidates(player, AmbientRadius, entries.FindAll(e => e.IsAmbient), forced: false);
				}
			}
		}

		private static bool TryPlaceAt(Entry entry, int x, int y, bool forced) {
			if (!WorldGen.InWorld(x, y, 1) || Main.tile[x, y].HasTile) return false;

			if (forced) {
				Tile ground = Main.tile[x, y + entry.Height];
				if (!ground.HasTile) return false; // 无视具体种类，但至少要有地面
			}
			else if (entry.ExtraCondition != null && !entry.ExtraCondition(x, y)) {
				return false;
			}

			bool placed = WorldGen.PlaceTile(x, y, entry.TileType, mute: true, forced: forced);
			if (placed)
				NetMessage.SendTileSquare(-1, x, y, entry.Width + 1, entry.Height + 1);

			return placed;
		}
	}
}
