using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using Microsoft.Xna.Framework;

namespace ArknightsMod.Content.Projectiles.Sniper.Exusiai
{
    public class ExusiaiVector_Bullet : ModProjectile
    {
        Player player => Main.player[Projectile.owner];

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 8;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
        }

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 8;
            Projectile.aiStyle = 1;
            Projectile.timeLeft = 600;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.friendly = true;
            Projectile.ignoreWater = true;
            Projectile.extraUpdates = 6;

            AIType = ProjectileID.Bullet;
        }

        public override void AI()
        {
            Lighting.AddLight(Projectile.Center, 0.48f, 0.33f, 0.11f);

            base.AI();
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Type].Value;

            var drawOrigin = new Vector2(texture.Width * 0.5f, Projectile.height * 0.5f);

            for (int k = Projectile.oldPos.Length - 1; k > 0; k--)
            {
                Vector2 drawPos = Projectile.oldPos[k] - Main.screenPosition + drawOrigin + new Vector2(0f, Projectile.gfxOffY);
                Color color = Projectile.GetAlpha(lightColor) * ((Projectile.oldPos.Length - k) / (float)Projectile.oldPos.Length);
                Main.EntitySpriteDraw(texture,
                    drawPos,
                    null,
                    color,
                    Projectile.rotation,
                    drawOrigin,
                    Projectile.scale,
                    SpriteEffects.None,
                    0
                    );
            }

            return true;
        }

        public override void OnKill(int timeLeft)
        {
            Collision.HitTiles(Projectile.position + Projectile.velocity, Projectile.velocity, Projectile.width, Projectile.height);
            SoundEngine.PlaySound(SoundID.Item10, Projectile.position);
            for (int j = 0; j < 3; j++)
            {
                float[] rand = {
                        Main.rand.NextFloat(-45f, 0f),
                        Main.rand.NextFloat(-15f, 15f),
                        Main.rand.NextFloat(0f, 45f)
                    };
                for (int i = 0; i < 3; i++)
                {
                    float angleMagnitude = Math.Abs(rand[i]);

                    Vector2 vel = Projectile.velocity.RotatedBy(MathHelper.ToRadians(rand[i])) * (1f - angleMagnitude / 30f * 0.3f) * Main.rand.NextFloat(0.5f, 1.5f);

                    Projectile.NewProjectileDirect(
                        Projectile.GetSource_FromThis(),
                        Projectile.position,
                        vel,
                        ModContent.ProjectileType<Exusiai_Gun_Effect>(),
                        0, 0, player.whoAmI
                    );
                }
            }
        }
    }
}
