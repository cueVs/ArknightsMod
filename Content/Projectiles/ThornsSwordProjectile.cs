using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;


namespace ArknightsMod.Content.Projectiles
{
	// Shortsword projectiles are handled in a special way with how they draw and damage things
	// The "hitbox" itself is closer to the player, the sprite is centered on it
	// However the interactions with the world will occur offset from this hitbox, closer to the sword's tip (CutTiles, Colliding)
	// Values chosen mostly correspond to Iron Shortword
	public class ThornsSwordProjectile : ModProjectile
	{

		public override void SetDefaults()
		{
			Projectile.width = 60;
			Projectile.height = 60;

			Projectile.aiStyle = -1;
			Projectile.friendly = true; // Deals damage to enemies
			Projectile.penetrate = -1; // How many monsters the projectile can penetrate.
			Projectile.DamageType = DamageClass.MeleeNoSpeed;
			Projectile.ownerHitCheck = true; // Prevents hits through tiles. Most melee weapons that use projectiles have this
			Projectile.scale = 0.3f;
			Projectile.alpha = 255; // How transparent to draw this projectile. 0 to 255. 255 is completely transparent.
		}

		// Allows you to determine the color and transparency in which a projectile is drawn
		// Return null to use the default color (normally light and buff color)
		// Returns null by default.
		//public override Color? GetAlpha(Color lightColor)
		//{
		//    return new Color(0, 50, 100, 0) * Projectile.Opacity;
		//}

		public override void AI()
		{
			Projectile.ai[0] += 1f;
			Projectile.rotation = Projectile.velocity.ToRotation();

			if (Projectile.ai[0] >= 50f)
				Projectile.Kill();

			FadeInAndOut();
			DelayCollision();

			// The code in this method is important to align the sprite with the hitbox how we want it to
			SetVisualOffsets();

			// Let's add some dust for special effect. In this case, it runs every other tick (30 ticks per second).
			//if (Projectile.timeLeft % 2 == 0)
			//{
			//    Dust.NewDustPerfect(new Vector2(Projectile.Center.X - (Projectile.width * Projectile.direction), Projectile.Center.Y), ModContent.DustType<Dusts.ThornsSwordDust2>(), null, 0, default, 0.4f); //Here we spawn the dust at the back of the projectile with half scale.
			//    // Dust.NewDustPerfect(new Vector2(Projectile.Center.X, Projectile.Center.Y), ModContent.DustType<Dusts.ThornsSwordDust1>(), null, 0, default, 1f); 
			//}
			Dust.NewDustPerfect(new Vector2(Projectile.Center.X, Projectile.Center.Y), ModContent.DustType<Dusts.ThornsSwordDust2>(), null, 0, default, 0.4f); //Here we spawn the dust at the back of the projectile with half scale.
			Dust.NewDustPerfect(new Vector2(Projectile.Center.X, Projectile.Center.Y), ModContent.DustType<Dusts.ThornsSwordDust1>(), null, 0, default, 0.4f);
		}

		// Many projectiles fade in so that when they spawn they don't overlap the gun muzzle they appear from
		public void FadeInAndOut()
		{
			// If last less than 50 ticks — fade in, than more — fade out
			if (Projectile.ai[0] <= 50f)
			{
				// Fade in
				Projectile.alpha -= 25;
				// Cap alpha before timer reaches 50 ticks
				if (Projectile.alpha < 100)
					Projectile.alpha = 0;

				return;
			}

			// Fade out
			Projectile.alpha += 25;
			// Cal alpha to the maximum 255(complete transparent)
			if (Projectile.alpha > 255)
				Projectile.alpha = 255;
		}

		private void DelayCollision()
		{
			if (Projectile.ai[0] <= 1f)
			{
				Projectile.tileCollide = false;
			}
			else Projectile.tileCollide = true;
		}

		private void SetVisualOffsets()
		{
			// 32 is the sprite size (here both width and height equal)
			const int HalfSpriteWidth = 60 / 2;
			const int HalfSpriteHeight = 60 / 2;

			int HalfProjWidth = Projectile.width / 2;
			int HalfProjHeight = Projectile.height / 2;

			// Vanilla configuration for "hitbox in middle of sprite"
			DrawOriginOffsetX = 0;
			DrawOffsetX = -(HalfSpriteWidth - HalfProjWidth);
			DrawOriginOffsetY = -(HalfSpriteHeight - HalfProjHeight);

			// Vanilla configuration for "hitbox towards the end"
			//if (Projectile.spriteDirection == 1) {
			//	DrawOriginOffsetX = -(HalfProjWidth - HalfSpriteWidth);
			//	DrawOffsetX = (int)-DrawOriginOffsetX * 2;
			//	DrawOriginOffsetY = 0;
			//}
			//else {
			//	DrawOriginOffsetX = (HalfProjWidth - HalfSpriteWidth);
			//	DrawOffsetX = 0;
			//	DrawOriginOffsetY = 0;
			//}
		}
		public override void OnHitNPC(NPC target, int damage, float knockBack, bool crit)
		{
			target.AddBuff(BuffID.Poisoned, 180);
		}
	}


}
