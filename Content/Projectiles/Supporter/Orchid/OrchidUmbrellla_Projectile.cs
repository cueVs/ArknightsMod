using Terraria.ModLoader;
using Terraria;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent;
using Terraria.ID;
using ArknightsMod.Content.Dusts;

namespace ArknightsMod.Content.Projectiles.Supporter.Orchid
{
    public class OrchidUmbrellla_Projectile : ModProjectile
    {
        public const int SlowTime = 48;
        private ref float Timer => ref Projectile.ai[0];
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailingMode[Type] = 2;
            ProjectileID.Sets.TrailCacheLength[Type] = 5;
            base.SetStaticDefaults();
        }

        public override void SetDefaults()
        {
            Projectile.width = 7;
            Projectile.height = 7;
            Projectile.aiStyle = -1;
            Projectile.penetrate = 1;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.timeLeft = 60;
        }

        public override void AI()
        {
            NPC target = Projectile.FindTargetWithinRange(float.MaxValue);

            if (target != null)
            {
                Vector2 vel = Projectile.DirectionTo(target.Center) * 10;
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, (Projectile.velocity * 7f + vel) / 8, 0.3f);
            }
            else
            {
                Projectile.velocity = Projectile.oldVelocity.SafeNormalize(Vector2.Zero) * 8;
            }

            Projectile.rotation = Projectile.velocity.ToRotation();

            DustEffect();

            base.AI();
        }

        private void DustEffect()
        {
            Timer++;
            if (Timer % 3 == 0)
            {
                int d = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, ModContent.DustType<Orchid_Dust>());
                Main.dust[d].velocity *= 0.5f;

                Timer = 0;
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            base.OnHitNPC(target, hit, damageDone);
        }

        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 6; i++)
            {
                Dust.NewDust(Projectile.Center, 0, 0, ModContent.DustType<Orchid_Dust>());
            }

            Vector2 vel = Main.rand.NextVector2Circular(1, 1);
            float rot = Main.rand.NextFloat(100, 140);
            for (int i = 0; i < 2; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    int dust = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.BlueTorch);
                    Main.dust[dust].noGravity = true;
                    Main.dust[dust].velocity = vel.RotatedBy(j * MathHelper.ToRadians(rot)) * Main.rand.NextFloat(0, 3);
                }
            }

            base.OnKill(timeLeft);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Type].Value;

            for (int i = 0; i < ProjectileID.Sets.TrailCacheLength[Type]; i++)
            {
                float factor = 1 - (float)i / ProjectileID.Sets.TrailCacheLength[Type];
                Vector2 oldcenter = Projectile.oldPos[i] + Projectile.Size / 2 - Main.screenPosition;

                Main.EntitySpriteDraw(texture,
                    oldcenter,
                    null,
                    lightColor,
                    Projectile.oldRot[i],
                    new Vector2(texture.Width / 2, texture.Height / 2),
                    Projectile.scale * factor,
                    SpriteEffects.None,
                    0
                    );
            }

            Main.EntitySpriteDraw(
                texture,
                Projectile.Center - Main.screenPosition,
                null,
                Color.White,
                Projectile.rotation,
                new Vector2(texture.Width / 2, texture.Height / 2),
                Projectile.scale,
                SpriteEffects.None,
                0
                );

            return false;
        }
    }
}
