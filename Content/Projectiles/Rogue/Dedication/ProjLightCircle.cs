using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Projectiles.Rogue.Dedication
{
    public class ProjLightCircle : ModProjectile
    {
        private const int TotalFrames = 25;
        private const float StartSize = 45f;
        private const float MaxSize = 85f;
        private const int MaxScaleFrame = 23;

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
         
            Projectile.Center = Projectile.Center;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            if (_texture == null)
                _texture = Mod.Assets.Request<Texture2D>("Content/Projectiles/Rogue/Dedication/ProjLightCircle").Value;

            if (_texture == null) return false;

            int currentFrame = TotalFrames - Projectile.timeLeft;
            if (currentFrame < 0 || currentFrame >= TotalFrames) return false;

        
            float scaleProgress;
            if (currentFrame <= MaxScaleFrame)
            {
                float t = currentFrame / (float)MaxScaleFrame;
                scaleProgress = 1f - (1f - t) * (1f - t);
            }
            else
            {
                scaleProgress = 1f;
            }

            float currentScale = 1f + (MaxSize / StartSize - 1f) * scaleProgress;
            float currentSize = StartSize * currentScale;

            
            float alphaProgress = currentFrame / (float)(TotalFrames - 1);
            float alpha = 1f - alphaProgress * alphaProgress;

            
            Color drawColor = new Color(175, 121, 255, (int)(255 * alpha));

            
            Vector2 screenPos = Projectile.Center - Main.screenPosition;
            Rectangle destRect = new Rectangle(
                (int)(screenPos.X - currentSize * 0.5f),
                (int)(screenPos.Y - currentSize * 0.5f),
                (int)currentSize,
                (int)currentSize);

          
            VertexDrawingHelper.DrawTexture(_texture, destRect, drawColor, additive: true);

            return false;
        }
    }
}