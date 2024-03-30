using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Projectiles
{
    public class ChenSwordProjectileS3Dragon : ModProjectile
	{
		public override void SetStaticDefaults() {
			Main.projFrames[Projectile.type] = 27;

			//DrawOriginOffsetY = -40;
		}

		public override void SetDefaults() {
			Projectile.width = 40;
			Projectile.height = 56;
			Projectile.aiStyle = 0;
			Projectile.penetrate = -1;
			Projectile.scale = 1f;
			Projectile.alpha = 0;
			Projectile.timeLeft = 255;
			Projectile.hide = false;
			Projectile.ownerHitCheck = true;
			// Projectile.DamageType = DamageClass.Melee;

			// no damage
			Projectile.friendly = false;
			Projectile.hostile = false;
		}

		// It appears that for this AI, only the ai0 field is used!
		public override void AI() {
			Lighting.AddLight(Projectile.Center, new Vector3(1f, 1f, 1f));
			Projectile.ai[0] += 1f;
			// Since we access the owner player instance so much, it's useful to create a helper local variable for this
			// Sadly, Projectile/ModProjectile does not have its own
			Player projOwner = Main.player[Projectile.owner];
			// Here we set some of the projectile's owner properties, such as held item and itemtime, along with projectile direction and position based on the player
			//Vector2 ownerMountedCenter = projOwner.RotatedRelativePoint(projOwner.MountedCenter, true);
			//Projectile.direction = projOwner.direction;
			//projOwner.heldProj = Projectile.whoAmI;
			Projectile.timeLeft = 1000;
			Projectile.position.X += 0f;
			Projectile.position.Y += 0f;

			// As long as the player isn't frozen, the spear can move
			//if (!projOwner.frozen) {
			//	if (movementFactor == 0f) // When initially thrown out, the ai0 will be 0f
			//	{
			//		movementFactor = 2f; // Make sure the spear moves forward when initially thrown out
			//		Projectile.netUpdate = true; // Make sure to netUpdate this spear
			//	}
			//}
			// Projectile.alpha -= 20;
			// Projectile.position += Projectile.velocity * movementFactor;

			// Projectile.rotation = Projectile.velocity.ToRotation();
			Projectile.direction = (Main.MouseWorld.X > projOwner.Center.X).ToDirectionInt();
			Projectile.spriteDirection = Projectile.direction;



			if (++Projectile.frameCounter >= 1)
			{
				Projectile.frameCounter = 0;
				if (Projectile.frame < 26)
				{
					Projectile.frame++;
					if (Projectile.frame > 24) {
						Projectile.alpha += 50;
					}
				}
				else {
					Projectile.Kill();

				}

			}

		}
	}
}
