using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Projectiles
{
	public class DurinWandProjectile : ModProjectile
	{

		public override void SetDefaults() {
			Projectile.width = 10;
			Projectile.height = 10;

			Projectile.aiStyle = ProjAIStyleID.ThrownProjectile;
			Projectile.friendly = true;
			Projectile.penetrate = 1; // How many monsters the projectile can penetrate.
			Projectile.tileCollide = true;
			Projectile.DamageType = DamageClass.Magic;
			Projectile.ownerHitCheck = true; // Prevents hits through tiles. Most melee weapons that use projectiles have this

			Projectile.alpha = 255; // How transparent to draw this projectile. 0 to 255. 255 is completely transparent.

			AIType = ProjectileID.EnchantedBoomerang;
		}

		// Allows you to determine the color and transparency in which a projectile is drawn
		// Return null to use the default color (normally light and buff color)
		// Returns null by default.
		public override Color? GetAlpha(Color lightColor) {
			// return Color.White;
			return new Color(255, 255, 255, 0) * Projectile.Opacity;
		}

		public override void AI() {
			Projectile.ai[0] += 1f;
			FadeInAndOut();

			// Despawn this projectile after 1 second (60 ticks)
			// You can use Projectile.timeLeft = 60f in SetDefaults() for same goal
			// Please check the total duration of 'FadeInAndOut'.
			if (Projectile.ai[0] >= 60f)
				Projectile.Kill();
		}

		// Many projectiles fade in so that when they spawn they don't overlap the gun muzzle they appear from
		public void FadeInAndOut() {
			// If last less than 50 ticks — fade in, than more — fade out
			if (Projectile.ai[0] <= 50f) {
				// Fade in
				Projectile.alpha -= 25;
				// Cap alpha before timer reaches 50 ticks
				if (Projectile.alpha < 100)
					Projectile.alpha = 100;

				return;
			}

			// Fade out
			Projectile.alpha += 25;
			// Cal alpha to the maximum 255(complete transparent)
			if (Projectile.alpha > 255)
				Projectile.alpha = 255;
		}

	}


}
