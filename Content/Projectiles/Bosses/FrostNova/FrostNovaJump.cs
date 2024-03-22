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
	public class FrostNovaJump : ModProjectile
	{
		public override void SetDefaults() {
			Projectile.width = 80;
			Projectile.height = 80;
			Projectile.aiStyle = 0;
			Projectile.timeLeft = 250;
			Projectile.penetrate = -1;
			Projectile.scale = 0.01f;
			Projectile.Opacity = 0f;
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
			Projectile.ai[0] += 1;
			if (Projectile.localAI[0] == 0f) {
				Projectile.rotation = Main.rand.NextFloat(MathHelper.TwoPi);
				Projectile.localAI[0] = Main.rand.Next(2) * 2 -1; // 1 or -1
				Projectile.localAI[1] = Main.rand.NextFloat(Projectile.ai[1], Projectile.ai[1] + 40);
				Projectile.localAI[2] = Main.rand.Next(1, 5);
			}
			//NPC npc = Main.npc[Projectile.owner];
			//float positionX = npc.position.X - 10;
			//float positionY = npc.position.Y;
			//if (Projectile.ai[1] > 0f) {
			//	Projectile.velocity.X = Math.Max(Projectile.velocity.X - 0.1f, 0f);
			//}
			//else {
			//	Projectile.velocity.X = Math.Min(Projectile.velocity.X + 0.1f, 0f);
			//}
			//if (Projectile.ai[2] > 0f) {
			//	Projectile.velocity.Y = Math.Max(Projectile.velocity.Y - 0.1f, 0f);
			//}
			//else {
			//	Projectile.velocity.Y = Math.Min(Projectile.velocity.Y + 0.1f, 0f);
			//}

			//Projectile.velocity.X = GetSPV(Projectile.ai[1], Projectile.ai[1] + 20, Projectile.velocity.X, 2);
			//Projectile.velocity.Y = GetSPV(Projectile.ai[2], Projectile.ai[2] + 20, Projectile.velocity.Y, 2);

			if (Projectile.ai[0] >= 250)
				Projectile.Kill();

			FadeInAndOut();
		}

		// Many projectiles fade in so that when they spawn they don't overlap the gun muzzle they appear from
		public void FadeInAndOut() {
			// If last less than 50 ticks — fade in, than more — fade out
			if (Projectile.ai[0] <= Projectile.localAI[1]) {
				// Fade in
				Projectile.Opacity += 0.1f;
				Projectile.scale += 0.05f;
				Projectile.rotation += 0.003f * Projectile.ai[2];
				Projectile.velocity *= 0.95f;
				// Cap
				if (Projectile.Opacity > 1f)
					Projectile.Opacity = 1f;
				if (Projectile.scale > 1f) {
					Projectile.scale = 1f;
				}
				return;
			}

			// Fade out
			Projectile.Opacity -= 0.03f;
			Projectile.velocity *= 0;
			Projectile.rotation += 0.005f * Projectile.ai[2];
			if (Projectile.Opacity < 0)
				Projectile.Opacity = 0;
			//if (Projectile.localAI[1] <= Projectile.ai[0] && Projectile.ai[0] <= (Projectile.localAI[1] + 60f) && Projectile.ai[0] % 10 == 0 && Projectile.localAI[0] == 1) {
			//	Dust dust = Main.dust[Dust.NewDust(Projectile.Left + new Vector2(-30, -25), 50, Projectile.height / 2, DustType<Dusts.Bosses.FrostNovaDeathDust>(), 0f, -2f)];
			//	dust.noGravity = true;
			//	dust.fadeIn = 0f;
			//	dust.scale = 1.5f;
			//}
		}

		public override bool PreDraw(ref Color lightColor) {
			Main.spriteBatch.End();
			Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);


			Texture2D texture = Request<Texture2D>("ArknightsMod/Assets/GrayScaleTexture/Smoke" + (int)Projectile.localAI[2], AssetRequestMode.ImmediateLoad).Value;
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
