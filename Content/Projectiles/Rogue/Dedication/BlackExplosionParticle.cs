using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Projectiles.Rogue.Dedication
{
    public class BlackExplosionParticle : ModProjectile
    {
      
        private static class Config
        {
            // 速度系数
            public const float SpeedMultiplier = 0.6325f;

      
            public const float DistanceFactor = 0.325f;

          
            public const float SizeScale = 0.7f;

            public const float FadeInEnd = 0.4f;
            public const float FadeHoldStart = 0.6f;
            public const float FadeHoldEnd = 0.85f;
            public const float FadeOutEnd = 1.0f;

            public const float HoldStartAlpha = 1.0f;
            public const float HoldEndAlpha = 0.1f;

            public const int MovementCurveType = 0;
            public const int SizeCurveType = 1;

           
            public const float GlowIntensity = 0.8f;
        }

   
        private int totalFrames;
        private float startSize;
        private float midSize;
        private float endSize;
        private int peakFrame1;
        private int valleyFrame;
        private float outwardSpeed;
        private float angle;
        private Vector2 orbitCenter;   
        private Vector2 orbitOffset;      
        private bool initialized = false;
        private Vector2 lastPosition;

        private Texture2D _texture;

        public override void SetStaticDefaults()
        {
        }

        public override void SetDefaults()
        {
            Projectile.width = 2;
            Projectile.height = 2;
            Projectile.friendly = false;
            Projectile.hostile = false;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.penetrate = -1;
            Projectile.aiStyle = -1;
        }

        public void Initialize(int totalFrames, float startSize, float midSize, float endSize,
                              int peakFrame1, int valleyFrame, float outwardSpeed, float angle,
                              Vector2 orbitCenter, Vector2 orbitOffset)
        {
            float sizeScale = Config.SizeScale;
            this.totalFrames = totalFrames;
            this.startSize = startSize * 2f * sizeScale;
            this.midSize = midSize * 2f * sizeScale;
            this.endSize = endSize * 2f * sizeScale;
            this.peakFrame1 = peakFrame1;
            this.valleyFrame = valleyFrame;
            this.outwardSpeed = outwardSpeed * Config.SpeedMultiplier;
            this.angle = angle;
            this.orbitCenter = orbitCenter;
            this.orbitOffset = orbitOffset;
            this.lastPosition = Projectile.Center;
            this.initialized = true;

            Projectile.timeLeft = totalFrames;
        }

        public override void AI()
        {
            if (!initialized) return;

            Vector2 oldPosition = Projectile.Center;

            int currentFrame = totalFrames - Projectile.timeLeft;
            if (currentFrame < 0) return;

            float progress = currentFrame / (float)totalFrames;
            float moveCurve = GetMovementCurve(progress);


            float distance = outwardSpeed * moveCurve * totalFrames * Config.DistanceFactor;

            Vector2 radialDir = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));

            Vector2 newPos = orbitCenter + orbitOffset + radialDir * distance;
            Projectile.Center = newPos;

            lastPosition = oldPosition;
        }

        private float GetMovementCurve(float t)
        {
            switch (Config.MovementCurveType)
            {
                case 0: return 1f - (1f - t) * (1f - t) * (1f - t);
                case 1: return 1f - (1f - t) * (1f - t);
                case 2: return t * t * (3f - 2f * t);
                default: return t;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            if (!initialized) return false;

            if (_texture == null)
                _texture = Mod.Assets.Request<Texture2D>("Content/Projectiles/Rogue/Dedication/BlackExplosion").Value;

            if (_texture == null) return false;

            int currentFrame = totalFrames - Projectile.timeLeft;
            if (currentFrame < 0 || currentFrame >= totalFrames) return false;

            float currentSize = CalculateSize(currentFrame);
            float alpha = CalculateAlpha(currentFrame);

            Vector2 direction = Projectile.Center - lastPosition;
            float rotation = 0f;
            if (direction.Length() > 0.01f)
            {
                rotation = (float)Math.Atan2(direction.Y, direction.X) + MathHelper.PiOver2;
            }

            Vector2 screenPos = Projectile.Center - Main.screenPosition;

            float textureWidth = _texture.Width;
            float textureHeight = _texture.Height;
            float scale = currentSize / Math.Max(textureWidth, textureHeight);
            float drawWidth = textureWidth * scale;
            float drawHeight = textureHeight * scale;

            Rectangle destRect = new Rectangle(
                (int)(screenPos.X - drawWidth * 0.5f),
                (int)(screenPos.Y - drawHeight * 0.5f),
                (int)drawWidth,
                (int)drawHeight);

            // 主粒子颜色 (17, 5, 34) 深紫黑色
            Color particleColor = new Color(78, 0, 195, (int)(255 * alpha));
            VertexDrawingHelper.DrawTexture(_texture, destRect, particleColor, additive: true, rotation: rotation);

            // 绘制外围光晕（淡紫色，Additive）
            float glowScale = scale * 1.5f;
            float glowWidth = textureWidth * glowScale;
            float glowHeight = textureHeight * glowScale;
            float glowAlpha = alpha * Config.GlowIntensity;
            Color glowColor = new Color(175, 121, 255, (int)(255 * glowAlpha));

            Rectangle glowRect = new Rectangle(
                (int)(screenPos.X - glowWidth * 0.5f),
                (int)(screenPos.Y - glowHeight * 0.5f),
                (int)glowWidth,
                (int)glowHeight);

            VertexDrawingHelper.DrawTexture(_texture, glowRect, glowColor, additive: true, rotation: rotation);

            return false;
        }

        private float CalculateSize(int frame)
        {
            float t, easeValue;
            if (frame <= peakFrame1)
            {
                t = frame / (float)peakFrame1;
                easeValue = GetSizeCurve(t);
                return startSize + (midSize - startSize) * easeValue;
            }
            else if (frame <= valleyFrame)
            {
                t = (frame - peakFrame1) / (float)(valleyFrame - peakFrame1);
                easeValue = GetSizeCurve(t);
                return midSize + (endSize - midSize) * easeValue;
            }
            else
            {
                return endSize;
            }
        }

        private float GetSizeCurve(float t)
        {
            if (Config.SizeCurveType == 0)
                return 1f - (1f - t) * (1f - t) * (1f - t);
            else
                return 1f - (1f - t) * (1f - t);
        }

        private float CalculateAlpha(int frame)
        {
            float t = frame / (float)totalFrames;
            if (t < Config.FadeInEnd)
            {
                float t2 = t / Config.FadeInEnd;
                return 1f - (1f - t2) * (1f - t2) * (1f - t2);
            }
            else if (t < Config.FadeHoldEnd)
            {
                float t2 = (t - Config.FadeInEnd) / (Config.FadeHoldEnd - Config.FadeInEnd);
                return Config.HoldStartAlpha - t2 * (Config.HoldStartAlpha - Config.HoldEndAlpha);
            }
            else
            {
                float t2 = (t - Config.FadeHoldEnd) / (Config.FadeOutEnd - Config.FadeHoldEnd);
                return Config.HoldEndAlpha * (1f - t2 * t2 * t2);
            }
        }
    }
}