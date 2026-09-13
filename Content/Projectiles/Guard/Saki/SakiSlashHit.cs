using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Projectiles.Guard.Saki
{
	public class SakiSlashHit : ModProjectile
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
			Projectile.timeLeft = 35;
			Projectile.scale = 0.8f;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.noEnchantmentVisuals = true;
			Projectile.rotation = Main.rand.NextFloat(MathHelper.Pi);
		}
		public int timer;
		public override void AI()
		{
			timer++;
			Projectile.Opacity = Projectile.timeLeft / 35f;
			Projectile.scale *= 0.9f;
		}

        public override bool ShouldUpdatePosition()
		{
			return false;
		}
		public override bool? CanDamage() => false;
		
		public override bool PreDraw(ref Color lightColor)
		{
			if (Projectile.timeLeft >= 34f)
			{
				return false;
			}
			float progress = (33f - Projectile.timeLeft) / 33f;
			Texture2D tex1 = ModContent.Request<Texture2D>("ArknightsMod/Content/Projectiles/Guard/Saki/Assets/star_268").Value;
			Texture2D tex2 = ModContent.Request<Texture2D>("ArknightsMod/Content/Projectiles/Guard/Saki/Assets/star_268_2").Value;
			Texture2D bloomTexture = ModContent.Request<Texture2D>("ArknightsMod/Content/Projectiles/Guard/Saki/Assets/ray_130").Value;
			Vector2 drawPosition = Projectile.Center - Main.screenPosition;
			Vector2 origin = Utils.Size(tex1) * 0.5f;
			Vector2 scale = new Vector2(1f) * Projectile.scale;
			Vector2 bloomScale = Projectile.Size / Utils.Size(bloomTexture) * new Vector2(1f, 2f);
			Vector2 bloomOrigin = Utils.Size(bloomTexture) * 0.5f;
			//Main.spriteBatch.Draw(bloomTexture, drawPosition, null, Color.White * Projectile.Opacity, Projectile.rotation, bloomOrigin, bloomScale, 0, 0f);
			Main.spriteBatch.Draw(tex1, drawPosition, null, Color.White * Projectile.Opacity, Projectile.rotation, origin, scale, 0, 0f);
			Main.spriteBatch.Draw(tex2, drawPosition, null, Color.White * Projectile.Opacity, Projectile.rotation, origin, scale, 0, 0f);
			return false;
		}
	}
}
