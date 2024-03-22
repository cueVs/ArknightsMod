using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Projectiles
{
	/// <summary>
	/// ai[0] is used to base rotate angle.
	/// ai[1] is used to rotate speed.
	/// ai[2] is used to projectile scale.(recommended to use Entity.getRect().Width)
	/// </summary>
    public class StunDebuffProjectile : ModProjectile
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
			Projectile.width = 32;
			Projectile.height = 24;
			Projectile.aiStyle = 0;
			Projectile.timeLeft = 2;
			Projectile.penetrate = -1; // Infinite pierce

			Projectile.usesLocalNPCImmunity = true; // Used for hit cooldown changes in the ai hook
			Projectile.localNPCHitCooldown = -1; // We set this to -1 to make sure the projectile doesn't hit twice

			Projectile.hide = false;
			Projectile.ownerHitCheck = true;
			Projectile.DamageType = DamageClass.Melee;
			Projectile.tileCollide = false;
			Projectile.friendly = true;
			
			Projectile.scale = 1f;
			Projectile.alpha = 0;
		}


		// It appears that for this AI, only the ai0 field is used!
		public override void AI() {
			Projectile.rotation = Projectile.ai[0] * Projectile.ai[1];
			Projectile.scale = Projectile.ai[2] / 50;
		}
	}
}