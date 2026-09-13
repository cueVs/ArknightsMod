using System;
using ArknightsMod.Content.Items.Weapons.Supporter.Pramanix;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Systems
{
	public class PramanixFrostVisualSystem : ModSystem
	{
		public override void PostUpdatePlayers() {
			if (Main.netMode == NetmodeID.Server)
				return;

			Player local = Main.LocalPlayer;
			if (local == null || !local.active)
				return;

			var sb = local.GetModPlayer<SaintBellPlayer>();
			if (local.HeldItem.ModItem is not SaintBell || sb.FrostTier <= 0)
				return;

			SpawnDomainDust(local, sb);
		}

		private static void SpawnDomainDust(Player player, SaintBellPlayer state) {
			int tier = state.FrostTier;

			// 各级粒子颜色：一级浅蓝，二级中蓝，三级鲜艳深蓝
			Color tierColor = tier switch {
				1 => new Color(200, 230, 255),
				2 => new Color(140, 200, 255),
				3 => new Color(80, 160, 255),
				_ => new Color(200, 230, 255),
			};

			int skipChance = tier switch {
				1 => 3,
				2 => 2,
				3 => 1,
				_ => 3,
			};
			if (state.Skill3Active)
				skipChance = Math.Max(1, skipChance - 1);

			if (Main.rand.NextBool(skipChance))
				return;

			int spawnPasses = tier + (state.Skill3Active ? 2 : 0);
			float radius = FrostDomainLogic.GetRadius(tier, state.Skill3Active);

			for (int pass = 0; pass < spawnPasses; pass++) {
				// 外缘边界粒子（给玩家清晰的范围感知）
				if (Main.rand.NextBool(2)) {
					float edgeAngle = Main.rand.NextFloat(MathHelper.TwoPi);
					float edgeDist = radius * Main.rand.NextFloat(0.90f, 1.02f);
					Vector2 edgePos = player.Center + edgeAngle.ToRotationVector2() * edgeDist;
					float edgeScale = tier switch { 1 => 0.9f, 2 => 1.1f, 3 => 1.35f, _ => 0.9f };
					Dust edge = Dust.NewDustPerfect(edgePos, DustID.Ice,
						Main.rand.NextVector2Circular(0.4f, 0.4f), 80, tierColor, edgeScale);
					edge.noGravity = true;
					edge.fadeIn = 0.4f;
				}

				// 域内漂浮粒子
				float ring = radius * Main.rand.NextFloat(0.25f, 0.88f);
				Vector2 pos = player.Center + Main.rand.NextVector2CircularEdge(ring, ring);
				float scale = tier switch { 1 => 0.75f, 2 => 0.90f, 3 => 1.05f, _ => 0.75f };
				Dust d = Dust.NewDustPerfect(pos, DustID.Ice,
					Main.rand.NextVector2Circular(0.4f, 0.4f), 130, tierColor, scale);
				d.noGravity = true;
				d.fadeIn = 0.55f;

				// 雪花/白点散布（高层级更多）
				if (Main.rand.NextBool(tier == 3 ? 2 : 3)) {
					Vector2 inner = player.Center + Main.rand.NextVector2Circular(radius * 0.6f, radius * 0.6f);
					Color snowColor = Color.Lerp(Color.White, tierColor, 0.35f);
					Dust snow = Dust.NewDustPerfect(inner, DustID.Snow,
						Main.rand.NextVector2Circular(0.3f, 0.3f), 90, snowColor, 0.8f + tier * 0.1f);
					snow.noGravity = true;
				}
			}
		}
	}
}
