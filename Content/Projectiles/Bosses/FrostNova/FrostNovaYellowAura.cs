using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;
using ReLogic.Content;
using System;
using static Terraria.ModLoader.ModContent;
using Terraria.ID;
using ArknightsMod.Content.Items.Material;

namespace ArknightsMod.Content.Projectiles.Bosses.FrostNova
{
	// Shortsword projectiles are handled in a special way with how they draw and damage things
	// The "hitbox" itself is closer to the player, the sprite is centered on it
	// However the interactions with the world will occur offset from this hitbox, closer to the sword's tip (CutTiles, Colliding)
	// Values chosen mostly correspond to Iron Shortword
	public class FrostNovaYellowAura : ModProjectile
	{
		public override void SetStaticDefaults() {
			Main.projFrames[Projectile.type] = 15;

			//DrawOriginOffsetY = -40;
		}
		public override void SetDefaults() {
			Projectile.width = 62;
			Projectile.height = 64;
			Projectile.aiStyle = 0;
			Projectile.timeLeft = 25 * 60;
			Projectile.penetrate = -1;
			//Projectile.scale = 0.01f;
			//Projectile.Opacity = 0f;
			Projectile.hide = false;
			Projectile.hostile = false;
			Projectile.friendly = false;
			Projectile.ignoreWater = true;
			Projectile.tileCollide = false;
			Projectile.netUpdate = true;
		}

		// Allows you to determine the color and transparency in which a projectile is drawn
		// Return null to use the default color (normally light and buff color)
		// Returns null by default.
		//public override Color? GetAlpha(Color lightColor)
		//{
		//    return new Color(0, 50, 100, 0) * Projectile.Opacity;
		//}

		public override void AI() {
			Projectile.direction = (int)Projectile.ai[0];
			Projectile.ai[1]++;
			Projectile.spriteDirection = Projectile.direction;
			Projectile.position = Projetile

			if (++Projectile.frameCounter >= 15) {
				Projectile.frameCounter = 0;
				if (Projectile.frame < 15) {
					Projectile.frame++;
				}
				else {
					Projectile.frame = 0;
				}
			}

			if (Projectile.ai[1] > 20 * 60) {
				Projectile.Kill();
			}
		}


	}
}
