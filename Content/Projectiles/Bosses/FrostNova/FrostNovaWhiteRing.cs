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
	public class FrostNovaWhiteRing : ModProjectile
	{
		public override void SetStaticDefaults() {
			Main.projFrames[Projectile.type] = 12;

			//DrawOriginOffsetY = -40;
		}
		public override void SetDefaults() {
			Projectile.width = 1000;
			Projectile.height = 1000;
			Projectile.aiStyle = 0;
			Projectile.timeLeft = 250;
			Projectile.penetrate = -1;
			Projectile.scale = 2f;
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
			Projectile.ai[0]++;
			if (Projectile.ai[0] < 60) {
				Projectile.scale -= 0.06f;
				if (Projectile.scale < 0.001f) {
					Projectile.scale = 0.001f;
				}
			}
		}

		public override bool PreDraw(ref Color lightColor) {
			Main.spriteBatch.End();
			Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);


			Texture2D texture = Request<Texture2D>("ArknightsMod/Assets/GrayScaleTexture/WhiteRing", AssetRequestMode.ImmediateLoad).Value;
			float opacity = Projectile.Opacity * 0.6f;
			Color color = Color.White * opacity;
			float scale = Projectile.scale;
			Vector2 origin = texture.Size() * 0.5f;
			//float rotation = 2f * (float)Math.PI * Main.rand.NextFloat();
			Main.spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition, null, color, Projectile.rotation, origin, scale, SpriteEffects.None, 0);

			Main.spriteBatch.End();
			Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);

			return false;
		}

	}
}
