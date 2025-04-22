using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Enums;
using Terraria.ModLoader;
using Terraria.ID;
using System;
using ArknightsMod.Content.Dusts;
using Microsoft.Xna.Framework.Graphics;
using Terraria.Audio;
using Terraria.GameContent;

namespace ArknightsMod.Content.Projectiles
{
	public class PozemkaCrossbowSentryProjectile : ModProjectile
	{

		public override void SetDefaults() {
			Projectile.width = 42; // The width of projectile hitbox
			Projectile.height = 10; // The height of projectile hitbox
			Projectile.aiStyle = 1; // The ai style of the projectile, please reference the source code of Terraria
			Projectile.friendly = true; // Can the projectile deal damage to enemies?
			Projectile.hostile = false; // Can the projectile deal damage to the player?
			Projectile.DamageType = DamageClass.Ranged; // Is the projectile shoot by a ranged weapon?
			Projectile.penetrate = 1; // How many monsters the projectile can penetrate. (OnTileCollide below also decrements penetrate for bounces as well)
			Projectile.timeLeft = 600; // The live time for the projectile (60 = 1 second, so 600 is 10 seconds)
			Projectile.alpha = 0; // The transparency of the projectile, 255 for completely transparent. (aiStyle 1 quickly fades the projectile in) Make sure to delete this if you aren't using an aiStyle that fades in. You'll wonder why your projectile is invisible.
			Projectile.light = 0.5f; // How much light emit around the projectile
			Projectile.ignoreWater = true; // Does the projectile's speed be influenced by water?
			Projectile.tileCollide = true; // Can the projectile collide with tiles?
			Projectile.scale = 0.8f;
			Projectile.extraUpdates = 1; // Set to above 0 if you want the projectile to update multiple time in a frame

			AIType = ProjectileID.WoodenArrowFriendly; // Act exactly like default Bullet
		}

		public override bool OnTileCollide(Vector2 oldVelocity) {
			// If collide with tile, reduce the penetrate.
			// So the projectile can reflect at most 5 times
			Projectile.penetrate--;
			if (Projectile.penetrate <= 0) {
				Projectile.Kill();
			}
			else {
				Collision.HitTiles(Projectile.position, Projectile.velocity, Projectile.width, Projectile.height);
				SoundEngine.PlaySound(SoundID.Item10, Projectile.position);

				// If the projectile hits the left or right side of the tile, reverse the X velocity
				if (Math.Abs(Projectile.velocity.X - oldVelocity.X) > float.Epsilon) {
					Projectile.velocity.X = -oldVelocity.X;
				}

				// If the projectile hits the top or bottom side of the tile, reverse the Y velocity
				if (Math.Abs(Projectile.velocity.Y - oldVelocity.Y) > float.Epsilon) {
					Projectile.velocity.Y = -oldVelocity.Y;
				}
			}

			return false;
		}

		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			// Terraria.Audio.SoundEngine.PlaySound(PenNibHit);
			Vector2 vector14 = Projectile.Center + Projectile.velocity * 0.1f;
			int HitSparkle = Dust.NewDust(vector14 - Projectile.Size / 2f, Projectile.width, Projectile.height, 66, 0f, 0f, 150, default(Color), 0.9f);
			int PenNibDust = Dust.NewDust(vector14 - Projectile.Size / 2f, Projectile.width, Projectile.height, ModContent.DustType<PozemkaCrossbowDust>(), 0f, 0f, 150, default(Color), 0.9f);
			Main.dust[HitSparkle].noGravity = true;
			Main.dust[PenNibDust].noGravity = true;
			//if (target.life <= 0)
			//{
			//    if (Timer >= 600)
			//    {
			//        if (player.statLife < player.statLifeMax / 2)
			//        {
			//            texasTradition = true;
			//            Main.player[Projectile.owner].Heal(player.statLifeMax - player.statLife);
			//            Timer = 0;
			//        }

			//    }

			//}
		}

		public override bool PreDraw(ref Color lightColor) {
			Main.instance.LoadProjectile(Projectile.type);
			Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;

			// Redraw the projectile with the color not influenced by light
			Vector2 drawOrigin = new Vector2(texture.Width * 0.5f, Projectile.height * 0.5f);
			for (int k = 0; k < Projectile.oldPos.Length; k++) {
				Vector2 drawPos = Projectile.oldPos[k] - Main.screenPosition + drawOrigin + new Vector2(0f, Projectile.gfxOffY);
				Color color = Projectile.GetAlpha(lightColor) * ((Projectile.oldPos.Length - k) / (float)Projectile.oldPos.Length);
				Main.EntitySpriteDraw(texture, drawPos, null, color, Projectile.rotation, drawOrigin, Projectile.scale, SpriteEffects.None, 0);
			}

			return true;
		}



		public override void AI() {
			Projectile.rotation = Projectile.velocity.ToRotation();
			Vector2 vector14 = Projectile.Center + Projectile.velocity * 0.1f;
			int PenNibDust = Dust.NewDust(vector14 - Projectile.Size / 2f, Projectile.width, Projectile.height, ModContent.DustType<PozemkaCrossbowDust>(), 0f, 0f, 150, default(Color), 0.9f);

			Main.dust[PenNibDust].noGravity = true;
		}


	}
}
