using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Projectiles.Caster.Durin
{
	public class DurinWand_Projectile : ModProjectile
	{

		public override void SetDefaults() {
			Projectile.width = 10;
			Projectile.height = 10;

			Projectile.aiStyle = 0;
			Projectile.friendly = true;
			Projectile.penetrate = 1; // How many monsters the projectile can penetrate.
			Projectile.tileCollide = true;
			Projectile.DamageType = DamageClass.Magic;
			Projectile.ownerHitCheck = true; // Prevents hits through tiles. Most melee weapons that use projectiles have this

			AIType = ProjectileID.EnchantedBoomerang;
		}

		public override Color? GetAlpha(Color lightColor) {
			// return Color.White;
			return new Color(255, 255, 255, 0) * Projectile.Opacity;
		}

		public override void AI() {
			if (Main.rand.NextBool(3))
			{
				Dust dust = Dust.NewDustDirect(
					Projectile.position,
					Projectile.width,
					Projectile.height,
					DustID.WhiteTorch,   
					Projectile.velocity.X * 0.2f,
					Projectile.velocity.Y * 0.2f,
					150,     
					new Color(73, 255, 255),
					3f  
				);

				dust.noGravity = true; 
				dust.fadeIn = 1.6f;  
				dust.velocity *= 0.5f; 
			}
			if (Projectile.ai[0] == 1f)
			{
				Vector2 toMouse = Main.MouseWorld - Projectile.Center;
				Projectile.rotation = toMouse.ToRotation();
			}
			Projectile.ai[0] += 1f;

			if (Projectile.ai[0] >= 30f)
				Projectile.Kill();
		}

	}


}
