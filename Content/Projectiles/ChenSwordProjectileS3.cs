using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Projectiles
{
    public class ChenSwordProjectileS3 : ModProjectile
	{
		public override void SetStaticDefaults()
		{
			// DisplayName.SetDefault("The Only Thing I Know For Real");
			// Main.projFrames[Projectile.type] = 4;
			//DrawOriginOffsetY = 30;
			//DrawOffsetX = -60;
		}
		public override void SetDefaults()
		{
			Projectile.width = 198;
			Projectile.height = 18;
			Projectile.aiStyle = 0;
			Projectile.timeLeft = 20;
			Projectile.penetrate = -1; // Infinite pierce

			Projectile.usesLocalNPCImmunity = true; // Used for hit cooldown changes in the ai hook
			Projectile.localNPCHitCooldown = -1; // We set this to -1 to make sure the projectile doesn't hit twice

			Projectile.hide = false;
			Projectile.ownerHitCheck = true;
			Projectile.DamageType = DamageClass.Melee;
			Projectile.tileCollide = false;
			Projectile.friendly = true;
			
			Projectile.scale = 0.2f;
			Projectile.alpha = 0;
		}


		// It appears that for this AI, only the ai0 field is used!
		public override void AI() {
			Lighting.AddLight(Projectile.Center, new Vector3(1f, 1f, 1f));

			// All projectiles have timers that help to delay certain events
			// Projectile.ai[0], Projectile.ai[1] — timers that are automatically synchronized on the client and server
			// Projectile.localAI[0], Projectile.localAI[0] — only on the client
			// In this example, a timer is used to control the fade in / out and despawn of the projectile
			Projectile.ai[0] += 1f;

			FadeInAndOut();

			// Slow down
			Projectile.velocity *= 0f;

			// Despawn this projectile after 1 second (60 ticks)
			// You can use Projectile.timeLeft = 60f in SetDefaults() for same goal
			if (Projectile.ai[0] >= 10f)
				Projectile.Kill();

			// Set both direction and spriteDirection to 1 or -1 (right and left respectively)
			// Projectile.direction is automatically set correctly in Projectile.Update, but we need to set it here or the textures will draw incorrectly on the 1st frame.
			Projectile.direction = Projectile.spriteDirection = (Projectile.velocity.X > 0f) ? 1 : -1;

			
			Projectile.rotation = (float)(Projectile.velocity.ToRotation() + Math.Cos(Main.rand.Next(0, 90)));

			Projectile.position.X += (float)Main.rand.Next(-7, 7) - 15;
			Projectile.position.Y += (float)Main.rand.Next(-7, 7);


		}

		// Many projectiles fade in so that when they spawn they don't overlap the gun muzzle they appear from
		public void FadeInAndOut() {
			// If last less than 50 ticks — fade in, than more — fade out
			if (Projectile.ai[0] >= 1f && Projectile.ai[0] <= (float)Main.rand.Next(4, 7)) {
				// Fade in
				Projectile.scale += 0.12f;
				// Cap scale 
				if (Projectile.scale > 0.80f)
					Projectile.scale = 0.80f;

				return;
			}

		}
	}
}