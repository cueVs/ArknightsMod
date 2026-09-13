using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Projectiles.Specialist.ExusiaiAlter
{
	public class ExusiaiAlterBomb : ModProjectile
	{
		public override string Texture => "Terraria/Images/Projectile_116";

		public override void SetDefaults() {
			Projectile.width = 24;
			Projectile.height = 24;
			Projectile.friendly = true;
			Projectile.DamageType = DamageClass.Ranged;
			Projectile.penetrate = -1;
			Projectile.timeLeft = 45;
			Projectile.tileCollide = false;
		}

		public override void AI() {
			int targetIndex = (int)Projectile.ai[0];
			if (targetIndex >= 0 && targetIndex < Main.maxNPCs) {
				NPC target = Main.npc[targetIndex];
				if (target.active && target.CanBeChasedBy())
					Projectile.velocity = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitY) * 10f;
			}

			Projectile.rotation += 0.3f;
		}

		public override void OnKill(int timeLeft) {
			SoundEngine.PlaySound(SoundID.Item14, Projectile.Center);
			if (Main.myPlayer != Projectile.owner)
				return;

			for (int i = 0; i < Main.maxNPCs; i++) {
				NPC npc = Main.npc[i];
				if (!npc.active || !npc.CanBeChasedBy() || npc.friendly)
					continue;

				if (Vector2.Distance(npc.Center, Projectile.Center) > 96f)
					continue;

				npc.SimpleStrikeNPC(
					(int)(Projectile.damage * 0.85f),
					Projectile.owner,
					false,
					2f,
					DamageClass.Ranged);
			}
		}
	}
}
