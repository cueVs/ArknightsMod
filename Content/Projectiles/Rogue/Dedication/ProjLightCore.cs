using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Projectiles.Rogue.Dedication
{
    public class ProjLightCore : ModProjectile
    {
        private const int TotalFrames = 25;
        private const int PeakFrame = 12;

        private const float MainStartSize = 20f;
        private const float MainPeakSize = 35f;
        private const float MainEndSize = 5f;

        private const float OuterStartSize = 75f;
        private const float OuterPeakSize = 100f;
        private const float OuterEndSize = 5f;

        private Texture2D _texture;

      

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
            // 确保弹幕居中
            Projectile.Center = Projectile.Center;
        }

        private float EaseOutQuad(float t) => 1f - (1f - t) * (1f - t);
        private float EaseInQuad(float t) => t * t;
        private float EaseInCubic(float t) => t * t * t;

        public override bool PreDraw(ref Color lightColor)
        {
            if (_texture == null)
                _texture = Mod.Assets.Request<Texture2D>("Content/Projectiles/Rogue/Dedication/ProjLightCore").Value;

            if (_texture == null) return false;

            int currentFrame = TotalFrames - Projectile.timeLeft;
            if (currentFrame < 0 || currentFrame >= TotalFrames) return false;

            // 整体透明度 (先慢后快)
            float alphaProgress = currentFrame / (float)(TotalFrames - 1);
            float globalAlpha = 1f - alphaProgress * alphaProgress;

            float mainSize = 0f, outerSize = 0f;

            if (currentFrame <= PeakFrame)
            {
                float t = currentFrame / (float)PeakFrame;
                float easeOut = EaseOutQuad(t);
                mainSize = MainStartSize + (MainPeakSize - MainStartSize) * easeOut;
                outerSize = OuterStartSize + (OuterPeakSize - OuterStartSize) * easeOut;
            }
            else
            {
                float t = (currentFrame - PeakFrame) / (float)(TotalFrames - 1 - PeakFrame);
                float easeMain = EaseInQuad(t);
                mainSize = MainPeakSize - (MainPeakSize - MainEndSize) * easeMain;
                float easeOuter = EaseInCubic(t);
                outerSize = OuterPeakSize - (OuterPeakSize - OuterEndSize) * easeOuter;
            }

            if (currentFrame == TotalFrames - 1)
            {
                mainSize = MainEndSize;
                outerSize = OuterEndSize;
            }

            Vector2 screenPos = Projectile.Center - Main.screenPosition;

  
            Color mainColor = new Color(255, 255, 255, (int)(255 * globalAlpha));
            Rectangle mainRect = new Rectangle(
                (int)(screenPos.X - mainSize * 0.5f),
                (int)(screenPos.Y - mainSize * 0.5f),
                (int)mainSize,
                (int)mainSize);
            VertexDrawingHelper.DrawTexture(_texture, mainRect, mainColor, additive: true);


            Color outerColor = new Color(175, 121, 255, (int)(255 * globalAlpha));
            Rectangle outerRect = new Rectangle(
                (int)(screenPos.X - outerSize * 0.5f),
                (int)(screenPos.Y - outerSize * 0.5f),
                (int)outerSize,
                (int)outerSize);
            VertexDrawingHelper.DrawTexture(_texture, outerRect, outerColor, additive: true);

            return false;
        }
    }
}