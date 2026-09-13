using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;

namespace ArknightsMod.Content.Items.Weapons.Supporter.Pramanix
{
	public static class PramanixHitVfx
	{
		private static readonly Color SlowSmokeColor = new(140, 195, 255);

		// 一技能命中时爆开雪花粒子
		public static void SpawnSnowflakeBurst(NPC npc) {
			if (Main.netMode == NetmodeID.Server || !npc.active)
				return;

			for (int i = 0; i < 14; i++) {
				Vector2 vel = Main.rand.NextVector2CircularEdge(1f, 1f) * Main.rand.NextFloat(1.8f, 5.5f);
				Vector2 pos = npc.Center + Main.rand.NextVector2Circular(npc.width * 0.4f, npc.height * 0.4f);
				Dust d = Dust.NewDustPerfect(pos, DustID.Snow, vel, 80,
					new Color(200, 235, 255), 1.0f + Main.rand.NextFloat(0.5f));
				d.noGravity = true;
				d.fadeIn    = 0.2f;
			}
			for (int i = 0; i < 6; i++) {
				Vector2 vel = Main.rand.NextVector2CircularEdge(0.5f, 0.5f) * Main.rand.NextFloat(1f, 3f);
				Vector2 pos = npc.Center + Main.rand.NextVector2Circular(npc.width * 0.25f, npc.height * 0.25f);
				Dust d = Dust.NewDustPerfect(pos, DustID.Cloud, vel, 40,
					new Color(225, 245, 255), 0.85f);
				d.noGravity = true;
			}
		}

		// 命中敌人时生成淡蓝色下落烟雾，表示被减速
		public static void SpawnSlowSmoke(NPC npc) {
			if (Main.netMode == NetmodeID.Server || !npc.active)
				return;

			for (int i = 0; i < 5; i++) {
				Vector2 pos = npc.Center + new Vector2(
					Main.rand.NextFloat(-npc.width * 0.35f, npc.width * 0.35f),
					Main.rand.NextFloat(-npc.height * 0.25f, npc.height * 0.2f));
				Dust d = Dust.NewDustPerfect(pos, DustID.Cloud,
					new Vector2(Main.rand.NextFloat(-0.6f, 0.6f), Main.rand.NextFloat(0.5f, 2.4f)),
					90, SlowSmokeColor, 0.75f + Main.rand.NextFloat(0.35f));
				d.noGravity = false;
				d.fadeIn = 0.35f;
			}
		}
	}
}
