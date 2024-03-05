using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;
using ReLogic.Content;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;
namespace ArknightsMod.Content.Projectiles.Bosses.FrostNova
{
	// Shortsword projectiles are handled in a special way with how they draw and damage things
	// The "hitbox" itself is closer to the player, the sprite is centered on it
	// However the interactions with the world will occur offset from this hitbox, closer to the sword's tip (CutTiles, Colliding)
	// Values chosen mostly correspond to Iron Shortword
	public class FrostNovaSmoke : ModProjectile
	{

		public override void SetDefaults() {
			Projectile.width = 42;
			Projectile.height = 12;
			Projectile.aiStyle = 0;
			Projectile.timeLeft = 480;
			Projectile.penetrate = -1;
			Projectile.scale = 0.9f;
			Projectile.alpha = 255;
			Projectile.hide = true;
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
			Projectile.ai[0] += 1;
			if(Projectile.velocity.X > 0f) {
				Projectile.spriteDirection = Projectile.direction = -1;
			}

			if (Projectile.ai[0] >= 200f)
				Projectile.Kill();

			FadeInAndOut();
		}

		// Many projectiles fade in so that when they spawn they don't overlap the gun muzzle they appear from
		public void FadeInAndOut() {
			// If last less than 50 ticks — fade in, than more — fade out
			//if (Projectile.ai[0] <= 50f) {
			//	// Fade in
			//	Projectile.alpha -= 2;
			//	Projectile.scale += 0.2f;
			//	// Cap alpha before timer reaches 50 ticks
			//	if (Projectile.alpha < 0)
			//		Projectile.alpha = 0;
			//	if (Projectile.scale > 1.5f) {
			//		Projectile.scale = 1.5f;
			//	}
			//	return;
			//}

			// Fade out
			//Projectile.alpha += 10;
			//// Cal alpha to the maximum 255(complete transparent)
			//if (Projectile.alpha > 255)
			//	Projectile.alpha = 255;
		}

		public override bool PreDraw(ref Color lightColor) {
			Main.spriteBatch.End();
			Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);


			Texture2D texture = (Texture2D) Request<Texture2D>("ArknightsMod/Assets/Effects/Smoke", AssetRequestMode.ImmediateLoad);
			Main.spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition, null, Color.White, 0f, Vector2.Zero, 1, 0, 0);

			return false;
		}

	}


}
