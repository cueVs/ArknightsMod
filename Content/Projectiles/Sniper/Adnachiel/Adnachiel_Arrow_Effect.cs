using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Terraria.ModLoader;
using Terraria;

namespace ArknightsMod.Content.Projectiles.Sniper.Adnachiel
{
    public class Adnachiel_Arrow_Effect : ModProjectile
    {
        public override string Texture => "Terraria/Images/Extra_98";
        private float Skill => (int)Projectile.ai[2];
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

            base.AI();
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
            Vector2 origin = texture.Size() * 0.5f;
            Vector2 position = Projectile.Center - Main.screenPosition;

            Color color;
            if (Skill == 3)
            {
                color = new Color(255, 255, 255, 0) * Projectile.Opacity;
            }
            else
            {
                color = Color.Gold * Projectile.Opacity;
                color.A = 0;
            }

            Vector2 scale = Projectile.Size / texture.Size() * Projectile.scale * new Vector2(3, Main.rand.NextFloat(4, 6));

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

            return false;
        }
    }

    public class Adnachiel_Arrow_Circle : ModProjectile
    {
        public override string Texture => "Terraria/Images/Extra_174";
        ref float opa => ref Projectile.ai[0];
        ref float sca => ref Projectile.ai[1];
        private float Skill => (int)Projectile.ai[2];
        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 32;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 10;
            Projectile.Opacity = 0;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
        }

        public override void AI()
        {
            if (Projectile.timeLeft > 5)
            {
                opa += 0.2f;
            }
            else
            {
                opa -= 0.2f;
            }
            sca += 0.1f;

            Projectile.Opacity = opa;
            Projectile.scale = sca;

            base.AI();
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
            Vector2 origin = texture.Size() * 0.5f;
            Vector2 position = Projectile.Center - Main.screenPosition;

            Color color;
            if (Skill == 3)
            {
				color = new Color(255, 255, 255, 0);
            }
            else
            {
                color = Color.Gold * Projectile.Opacity;
                color.A = 0;
            }

            Vector2 scale = Projectile.Size / texture.Size() * Projectile.scale;

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

            return false;
        }
    }
}
