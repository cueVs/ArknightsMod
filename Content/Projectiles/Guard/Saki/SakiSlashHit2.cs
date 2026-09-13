using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Projectiles.Guard.Saki
{
	public class SakiSlashHit2 : ModProjectile
	{
		public override string Texture => ArknightsMod.noTexture;
		public override void SetDefaults()
		{
			Projectile.width = 48;
			Projectile.height = 48;
			Projectile.friendly = true;
			Projectile.DamageType = DamageClass.MeleeNoSpeed;
			Projectile.ignoreWater = true;
			Projectile.tileCollide = false;
			Projectile.penetrate = -1;
			Projectile.timeLeft = 30;
			Projectile.scale = 0.8f;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.noEnchantmentVisuals = true;
			Projectile.rotation = Main.rand.NextFloat(MathHelper.Pi);
		}
		public int timer;
		public override void AI()
		{
			timer++;
			Projectile.Opacity = Projectile.timeLeft / 15f;
		}

        public override bool ShouldUpdatePosition()
		{
			return false;
		}
		public override bool? CanDamage() => false;
		
		public override bool PreDraw(ref Color lightColor)
		{
			Main.spriteBatch.End();
			Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.Default,
				Main.Rasterizer, null, Main.GameViewMatrix.ZoomMatrix);

			for (int i = 0; i < 4; i++) {
				float progress = (Projectile.timeLeft) / 20f;
				progress = MathHelper.Clamp(progress, 0.2f, 1.5f);
				Texture2D tex1 = ModContent.Request<Texture2D>("ArknightsMod/Content/Projectiles/Guard/Saki/Assets/ray_129").Value;
				Vector2 drawPosition = Projectile.Center - Main.screenPosition;
				Vector2 origin = new Vector2(0, tex1.Height / 4);
				Vector2 scale = new Vector2(1f, progress) * 0.5f * Projectile.scale;
				Main.spriteBatch.Draw(tex1, drawPosition, new Rectangle(0, 128, 256, 128), Color.White * Projectile.Opacity, Projectile.rotation + MathHelper.PiOver2 * i, origin, scale, 0, 0f);
			}

			if (Projectile.timeLeft < 20) {
				for (int i = 0; i < 4; i++) {
					float progress = (Projectile.timeLeft+20) / 20f;
					progress = MathHelper.Clamp(progress, 0.2f, 1.5f);
					Texture2D tex1 = ModContent.Request<Texture2D>("ArknightsMod/Content/Projectiles/Guard/Saki/Assets/ray_129").Value;
					Vector2 drawPosition = Projectile.Center - Main.screenPosition;
					Vector2 origin = new Vector2(0, tex1.Height / 4);
					Vector2 scale = new Vector2(1f, progress) * 0.3f * Projectile.scale;
					Main.spriteBatch.Draw(tex1, drawPosition, new Rectangle(0, 128, 256, 128), Color.White * Projectile.Opacity, Projectile.rotation+MathHelper.PiOver4 + MathHelper.PiOver2 * i, origin, scale, 0, 0f);
				}
			}
			Main.spriteBatch.End();
			Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.Default,
				Main.Rasterizer, null, Main.GameViewMatrix.ZoomMatrix);

			return false;
		}
	}
}
