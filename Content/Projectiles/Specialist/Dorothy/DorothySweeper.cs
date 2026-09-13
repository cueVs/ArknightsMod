using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Projectiles.Specialist.Dorothy
{
	public class DorothySweeper : ModProjectile
	{
		public override string Texture => "Terraria/Images/Projectile_634";

		public override void SetStaticDefaults() {
			Main.projFrames[Type] = 1;
		}

		public override void SetDefaults() {
			Projectile.width = 32;
			Projectile.height = 32;
			Projectile.friendly = true;
			Projectile.minion = true;
			Projectile.minionSlots = 0f;
			Projectile.penetrate = -1;
			Projectile.tileCollide = false;
			Projectile.DamageType = DamageClass.Summon;
			Projectile.timeLeft = 2;
		}

		public override void AI() {
			Projectile.timeLeft = 2;
			Player owner = Main.player[Projectile.owner];
			if (!owner.active || owner.dead) {
				Projectile.Kill();
				return;
			}

			Vector2 targetPos = owner.Center + new Vector2(48f * owner.direction, -24f);
			Projectile.Center = Vector2.Lerp(Projectile.Center, targetPos, 0.2f);

			NPC target = Projectile.FindTargetWithinRange(520f, true);
			if (target == null)
				return;

			Projectile.ai[0]++;
			if (Projectile.ai[0] < 8f)
				return;

			Projectile.ai[0] = 0f;
			if (Main.myPlayer != Projectile.owner)
				return;

			Vector2 dir = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitX);
			Projectile.NewProjectile(
				Projectile.GetSource_FromThis(),
				Projectile.Center,
				dir * 12f,
				ModContent.ProjectileType<DorothySweeperShot>(),
				Projectile.damage,
				Projectile.knockBack,
				Projectile.owner);
		}
	}
}
