using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ModLoader;
using ArknightsMod.Content.Projectiles.Supporter.Tragodia;
using ArknightsMod.Content.Projectiles.Supporter.Tragodia.SP2;
using ArknightsMod.Content.Projectiles.Supporter.Tragodia.SP1;
using ArknightsMod.Content.Projectiles.Supporter.Tragodia.EffectImage;
using ArknightsMod.Content.Projectiles.Supporter.Tragodia.NormalAttack;
namespace ArknightsMod.Content.Projectiles.Supporter.Tragodia.EffectImage
{
    public class Light : ModProjectile
    {
        private const int TotalFrames = 20;
        private const float StartScale = 0.3f;
        private const float EndScale = 0.4f;
        private const float FadeSpeed = 1.3f;
        private const float BrightnessMultiplier = 1.3f;
        private const int DrawPasses = 2;
        private const int ParticleCount = 9;
        private const float ParticleSpreadSpeed = 1.5f;
        private bool particlesSpawned = false;

        private static readonly Color PurpleFilter = new Color(200, 150, 255);
        private static readonly Color ParticleColor = new Color(150, 80, 255);

        private Texture2D lightTexture;

        public override void SetDefaults()
        {
            Projectile.width = 2;
            Projectile.height = 2;
            Projectile.friendly = false;
            Projectile.hostile = false;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = TotalFrames;
            Projectile.penetrate = -1;
            Projectile.aiStyle = -1;
        }

        public override void AI()
        {
            if (!particlesSpawned)
            {
                SpawnParticles();
                particlesSpawned = true;
            }
        }

        private void SpawnParticles()
        {
            int particleCount = Main.rand.Next(7, 13);

            for (int i = 0; i < particleCount; i++)
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                float speed = Main.rand.NextFloat(0.5f, 2.5f) * ParticleSpreadSpeed;
                Vector2 velocity = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * speed;
                Vector2 spawnPos = Projectile.Center + new Vector2(0, -20f);

                Projectile particle = Projectile.NewProjectileDirect(
                    Projectile.GetSource_FromThis(),
                    spawnPos,
                    velocity,
                    ModContent.ProjectileType<LightParticle>(),
                    0,
                    0f,
                    Projectile.owner
                );

                if (particle.ModProjectile is LightParticle lightParticle)
                {
                    lightParticle.Initialize(
                        totalFrames: 35,
                        startSize: 15f,
                        endSize: 1f,
                        velocity: velocity,
                        position: spawnPos,
                        color: ParticleColor
                    );
                }
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            if (lightTexture == null)
                lightTexture = ModContent.Request<Texture2D>("ArknightsMod/Content/Projectiles/Supporter/Tragodia/EffectImage/Light").Value;
            if (lightTexture == null)
                return false;

            int frame = TotalFrames - Projectile.timeLeft;
            if (frame < 0 || frame >= TotalFrames)
                return false;

            Vector2 drawPos = Projectile.Center + new Vector2(0, -20f) - Main.screenPosition;

            float progress = frame / (float)(TotalFrames - 1);
            float scale = MathHelper.Lerp(StartScale, EndScale, progress);
            float fadeProgress = Math.Min(progress * FadeSpeed, 1f);
            float alpha = 1f - fadeProgress;

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive,
                SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone,
                null, Main.GameViewMatrix.TransformationMatrix);

            Vector2 origin = lightTexture.Size() / 2f;
            Color drawColor = PurpleFilter * alpha * BrightnessMultiplier;

            for (int i = 0; i < DrawPasses; i++)
            {
                Main.spriteBatch.Draw(lightTexture, drawPos, null, drawColor,
                    0f, origin, scale, SpriteEffects.None, 0f);
            }

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer,
                null, Main.GameViewMatrix.TransformationMatrix);

            return false;
        }
    }
}