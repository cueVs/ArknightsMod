using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using ArknightsMod.Content.Dusts;

namespace ArknightsMod.Content.Projectiles { 
	public class SanCrash:ModProjectile
	{
		public override void SetStaticDefaults() {
			Main.projFrames[Projectile.type] = 6;
			ProjectileID.Sets.TrailingMode[Type] = 2;
			ProjectileID.Sets.TrailCacheLength[Type] = 40;
		}
		public override void SetDefaults() {

			Projectile.width = 100;
			Projectile.height = 100;
			Projectile.aiStyle = 0;
			Projectile.scale = 2.5f;
			Projectile.penetrate = -1;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = false;
			Projectile.timeLeft = 42;
			Projectile.light = 0.6f;
			Projectile.friendly = false;
			Projectile.hostile = false;
		}



		public override void AI() {
			Projectile.frameCounter++;
			if (Projectile.frameCounter > 7) {
				Projectile.frame += 1;
				Projectile.frameCounter = 0;
			}

		}
	}
}
