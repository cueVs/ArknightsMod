using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Projectiles.Sniper.Exusiai
{
    public class Exusiai_Gun_Effect : ModProjectile
    {
        public override string Texture => "Terraria/Images/Extra_98";
        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 8;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 10;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.RotatedBy(MathHelper.PiOver2).ToRotation();

            Projectile.velocity -= Projectile.velocity * 0.1f;
            Projectile.Opacity -= 0.1f;

            Lighting.AddLight(Projectile.Center, 0.48f, 0.33f, 0.11f);

            base.AI();
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
            Vector2 origin = texture.Size() * 0.5f;
            Vector2 position = Projectile.Center - Main.screenPosition;

            // 定义渐变颜色范围
            var startColor = new Color(162, 0, 0, 0);
            var endColor = new Color(247, 172, 56, 0);
            Color color = Color.Lerp(endColor, startColor, Projectile.Opacity) * Projectile.Opacity;

            Vector2 scale = Projectile.Size / texture.Size() * Projectile.scale * new Vector2(2, Main.rand.NextFloat(3, 5));

            for (int i = 0; i < 2; i++)
            {
                Main.EntitySpriteDraw(texture,
                position,
                null,
                color,
                Projectile.rotation,
                origin,
                scale,
                SpriteEffects.None,
                0
                );
            }

            return false;
        }
    }
}
