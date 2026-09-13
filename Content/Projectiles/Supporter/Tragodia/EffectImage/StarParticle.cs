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
	//酒神每个攻击产生的那个会发光的星星
    public class StarParticle : ModProjectile
    {
        private const int Duration = 70;
        private const int FadeStartFrame = 50;
        private const float BaseAlpha = 0.7f;
        private const float SizeMultiplier = 0.5f;
        private const float GlowAlpha = 0.8f;
        private const float GlowScaleMultiplier = 0.8f;
        private const float MoveTime = 15f;
        private const float VelocityDamping = 0.98f;
        private const float MinSpeed = 0.3f;
        private static readonly Color StarColor = new Color(180, 120, 255);

        private Texture2D starTexture;
        private Texture2D glowTexture;
        private float initialScale;
        private float targetScale;
        private Vector2 velocity;
        private int frameCounter = 0;

        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.friendly = false;
            Projectile.hostile = false;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = Duration;
            Projectile.penetrate = -1;
            Projectile.aiStyle = -1;
        }

        public override void AI()
        {
            if (starTexture == null)
            {
                starTexture = ModContent.Request<Texture2D>("ArknightsMod/Content/Projectiles/Supporter/Tragodia/EffectImage/StarParticle").Value;
                glowTexture = ModContent.Request<Texture2D>("ArknightsMod/Content/Projectiles/Supporter/Tragodia/EffectImage/ProjLightCore").Value;
            }

            if (frameCounter == 0)
            {
                initialScale = Main.rand.NextFloat(0.4f, 0.9f) * SizeMultiplier;
                targetScale = Main.rand.NextFloat(0.1f, 0.35f) * SizeMultiplier;

                float speed = Main.rand.NextFloat(1.0f, 2.5f);
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                velocity = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * speed;
            }

            if (frameCounter < MoveTime)
            {
                Projectile.Center += velocity;
                velocity *= VelocityDamping;
            }
            else
            {
                if (velocity.Length() < MinSpeed)
                    velocity = Vector2.Normalize(velocity) * MinSpeed;
                Projectile.Center += velocity;
                velocity *= VelocityDamping;
                if (velocity.Length() < MinSpeed)
                    velocity = Vector2.Normalize(velocity) * MinSpeed;
            }

            frameCounter++;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            if (starTexture == null || glowTexture == null)
                return false;

            float progress = frameCounter / (float)Duration;
            if (progress >= 1f)
                return false;

            float linearScale = MathHelper.Lerp(initialScale, targetScale, progress);
            float bounce = (float)Math.Sin(progress * MathHelper.Pi * 5f) * (1f - progress) * 0.12f;
            float scale = Math.Max(linearScale + bounce, 0.01f);

            float alpha = BaseAlpha;
            if (frameCounter >= FadeStartFrame)
            {
                float fadeProgress = (frameCounter - FadeStartFrame) / (float)(Duration - FadeStartFrame);
                alpha = BaseAlpha * (1f - fadeProgress);
            }

            Color starDrawColor = StarColor * alpha;
            Color glowDrawColor = StarColor * alpha * GlowAlpha;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Vector2 starOrigin = starTexture.Size() / 2f;
            Vector2 glowOrigin = glowTexture.Size() / 2f;

            float glowScale = scale * GlowScaleMultiplier;

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive,
                SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone,
                null, Main.GameViewMatrix.TransformationMatrix);

            Main.spriteBatch.Draw(glowTexture, drawPos, null, glowDrawColor,
                0f, glowOrigin, glowScale, SpriteEffects.None, 0f);

            Main.spriteBatch.Draw(starTexture, drawPos, null, starDrawColor,
                0f, starOrigin, scale, SpriteEffects.None, 0f);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer,
                null, Main.GameViewMatrix.TransformationMatrix);

            return false;
        }
    }
}