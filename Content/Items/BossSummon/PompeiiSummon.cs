using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using ArknightsMod.Content.NPCs.Enemy.OF.Pmp;
using Terraria.Audio;
using Terraria.DataStructures;

namespace ArknightsMod.Content.Items.BossSummon
{
	public class PompeiiSummon : ModItem
	{
		public override void AddRecipes() {
			
			CreateRecipe()
				.AddIngredient<Material.OrirockCube>(1)
				.AddIngredient(22, 5)
                .AddIngredient(173,1)
				.Register();
		}

	
	public override void SetDefaults() {
			Item.width = 40;
			Item.height = 40;
			Item.maxStack = 1;
			Item.value = 325;
			Item.rare = 1;
			Item.useAnimation = 30;
			Item.useTime = 30;
			Item.useStyle = ItemUseStyleID.HoldUp;
			Item.consumable = false;
		}
		public override bool CanUseItem(Player player) {
			return
			!NPC.AnyNPCs(ModContent.NPCType<Pompeii>());
		}
		public override bool? UseItem(Player player) {
			if (player.whoAmI == Main.myPlayer) {
				SoundEngine.PlaySound(SoundID.Roar, player.position);
			}
			if (Main.netMode != NetmodeID.MultiplayerClient) {
				int npcType = ModContent.NPCType<Pompeii>();
				Vector2 spawnPos = FindPompeiiSpawnPosition(player);
				NPC.NewNPC(
					player.GetSource_ItemUse(Item),
					(int)spawnPos.X,
					(int)spawnPos.Y,
					npcType
				);
			}
			
			return true;
		}

		private static Vector2 FindPompeiiSpawnPosition(Player player)
		{
			// 目标：优先在玩家视野外生成（屏幕外），且落在可站立地面附近
			int playerTileX = (int)(player.Center.X / 16f);
			int playerTileY = (int)(player.Center.Y / 16f);
			float minOutOfViewPx = Math.Max(Main.screenWidth * 0.65f, 680f);
			float maxSearchPx = Math.Max(Main.screenWidth * 1.35f, 1500f);
			int minStepTiles = (int)(minOutOfViewPx / 16f);
			int maxStepTiles = (int)(maxSearchPx / 16f);
			int preferredDir = player.direction == 0 ? 1 : player.direction;

			for (int step = minStepTiles; step <= maxStepTiles; step += 2)
			{
				int[] candidates = new[] { preferredDir * step, -preferredDir * step };
				for (int i = 0; i < candidates.Length; i++)
				{
					int tx = playerTileX + candidates[i];
					if (tx < 20 || tx >= Main.maxTilesX - 20)
						continue;

					int topY = Math.Max(20, playerTileY - 35);
					int bottomY = Math.Min(Main.maxTilesY - 20, playerTileY + 35);

					for (int ty = playerTileY; ty <= bottomY; ty++)
					{
						Tile tile = Framing.GetTileSafely(tx, ty);
						if (!tile.HasTile)
							continue;

						bool solidTop = Main.tileSolidTop[tile.TileType] || TileID.Sets.Platforms[tile.TileType];
						bool solidBlock = Main.tileSolid[tile.TileType] && !Main.tileSolidTop[tile.TileType];
						if (!solidTop && !solidBlock)
							continue;

						float worldX = tx * 16f;
						float worldY = ty * 16f - 90f; // 把庞贝放在地表上方，防止卡地
						Vector2 pos = new Vector2(worldX, worldY);
						if (Vector2.Distance(pos, player.Center) >= minOutOfViewPx)
							return pos;
					}

					for (int ty = playerTileY - 1; ty >= topY; ty--)
					{
						Tile tile = Framing.GetTileSafely(tx, ty);
						if (!tile.HasTile)
							continue;

						bool solidTop = Main.tileSolidTop[tile.TileType] || TileID.Sets.Platforms[tile.TileType];
						bool solidBlock = Main.tileSolid[tile.TileType] && !Main.tileSolidTop[tile.TileType];
						if (!solidTop && !solidBlock)
							continue;

						float worldX = tx * 16f;
						float worldY = ty * 16f - 90f;
						Vector2 pos = new Vector2(worldX, worldY);
						if (Vector2.Distance(pos, player.Center) >= minOutOfViewPx)
							return pos;
					}
				}
			}

			// 找不到合适地形时，仍然强制在视野外回退，避免贴脸出生
			return player.Center + new Vector2(preferredDir * (minOutOfViewPx + 200f), -120f);
		}
	}
}
