using ArknightsMod.Content.Items.Weapons.Supporter.Pramanix;
using ArknightsMod.Content.Projectiles.Supporter.Pramanix;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Systems
{
	// 在 DrawInfernoRings 阶段统一绘制三技能路径与标志
	public class PramanixSkill3VisualSystem : ModSystem
	{
		public override void OnModLoad() {
			if (Main.netMode == NetmodeID.Server)
				return;

			On_Main.DrawInfernoRings += DrawSkill3Detour;
		}

		public override void Unload() {
			On_Main.DrawInfernoRings -= DrawSkill3Detour;
		}

		private static void DrawSkill3Detour(On_Main.orig_DrawInfernoRings orig, Main self) {
			DrawSkill3Effects();
			orig(self);
		}

		private static void DrawSkill3Effects() {
			if (Main.gameMenu || Main.dedServ)
				return;

			PramanixSkill3MeshRenderer.Clear();

			int volleyType = ModContent.ProjectileType<PramanixSkill3VolleyFx>();
			for (int i = 0; i < Main.maxProjectiles; i++) {
				Projectile proj = Main.projectile[i];
				if (!proj.active || proj.type != volleyType)
					continue;
				if (proj.ModProjectile is not PramanixSkill3VolleyFx volley)
					continue;

				Player owner = Main.player[proj.owner];
				if (!owner.active || owner.dead)
					continue;

				PramanixSkill3MeshRenderer.AppendSkill3Volley(volley, owner.Center);
			}

			for (int p = 0; p < Main.maxPlayers; p++) {
				Player player = Main.player[p];
				if (!player.active || player.dead || player.HeldItem.ModItem is not SaintBell)
					continue;

				var state = player.GetModPlayer<SaintBellPlayer>();
				if (!state.Skill3Active)
					continue;

				float emblemAlpha = state.GetEmblemDisplayAlpha();
				PramanixSkill3MeshRenderer.AppendEmblems(player.Center, emblemAlpha);
			}

			PramanixSkill3MeshRenderer.FlushIfAny();
		}
	}
}
