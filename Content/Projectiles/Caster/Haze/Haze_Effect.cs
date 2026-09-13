using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Projectiles.Caster.Haze
{
	public class Haze_Effect : ModProjectile
	{
		public override string Texture => "Terraria/Images/Extra_89";

		public override void SetDefaults() {
			Projectile.width = 12;
			Projectile.height = 12;
			Projectile.timeLeft = 10;
			Projectile.tileCollide = false;
			Projectile.aiStyle = -1;
		}

		public override void AI() {
			Projectile.rotation = Projectile.velocity.ToRotation();
		}

		public override bool ShouldUpdatePosition() => false;

		public override bool PreDraw(ref Color lightColor) {
			Texture2D texture = TextureAssets.Extra[89].Value;

			float progress = 1f - (float)Projectile.timeLeft / 10;

			float scaleX, scaleY;
			if (progress <= 0.5f) {
				float t = progress / 0.5f;
				scaleX = 0.5f;
				scaleY = t * 2f;
			}
			else {
				float t = (progress - 0.5f) / 0.5f;
				scaleX = 0.5f * (1f - t);
				scaleY = 2f;
			}

			Vector2 scale = new Vector2(scaleX, scaleY);
			Main.EntitySpriteDraw(
				texture,
				Projectile.Center - Main.screenPosition,
				null,
				Color.Red,
				Projectile.rotation,
				texture.Size() / 2f,
				scale,
				SpriteEffects.None,
				0
			);

			return false;
		}
	}
}
