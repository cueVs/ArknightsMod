using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Projectiles.Rogue.Dedication
{
    public class Light_horizontal : ModProjectile
    {
        
        private static class Config
        {
            
            public const int TotalFrames = 25;          // 总帧数
            public const int PeakFrame = 12;            // 峰值帧（快速拉伸结束点）
            public const int ShrinkEndFrame = 16;        // 快速收缩结束点（之后进入缓慢减小）

            // 尺寸参数（主光晕）
            public const float MainStartWidth = 35f;     // 初始宽度
            public const float MainStartHeight = 9f;    // 初始高度
            public const float MainPeakWidth = 80f;     // 拉伸最大宽度
            public const float MainPeakHeight = 18f;     // 拉伸时高度略微增加
            public const float MainShrinkWidth = 50f;    // 收缩后宽度
            public const float MainShrinkHeight = 13f;   // 收缩后高度
            public const float MainEndWidth = 10f;       // 最终宽度
            public const float MainEndHeight = 3f;      // 最终高度

            // 尺寸参数（外光晕） - 比例相对于主光晕
            public const float OuterScaleStart = 2.0f;   // 起始外光晕倍数
            public const float OuterScalePeak = 1.8f;    // 峰值外光晕倍数
            public const float OuterScaleShrink = 1.5f;  // 收缩后外光晕倍数
            public const float OuterScaleEnd = 1.2f;     // 最终外光晕倍数

            
            public static readonly Color MainColor = Color.White;
            public static readonly Color OuterColor = new Color(175, 121, 255);

            // 透明度曲线（使用与 ProjLightCore 相同的先慢后快）
            public const int AlphaCurveType = 0;         // 0: 先慢后快 (EaseInQuad)
        }

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
            Projectile.timeLeft = Config.TotalFrames;
            Projectile.penetrate = -1;
            Projectile.aiStyle = -1;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            if (_texture == null)
                _texture = Mod.Assets.Request<Texture2D>("Content/Projectiles/Rogue/Dedication/Light_horizontal").Value;
            if (_texture == null) return false;

            int currentFrame = Config.TotalFrames - Projectile.timeLeft;
            if (currentFrame < 0 || currentFrame >= Config.TotalFrames) return false;

            // 获取当前帧的主光晕尺寸
            GetMainSize(currentFrame, out float mainWidth, out float mainHeight);
            // 获取当前帧的外光晕尺寸
            GetOuterSize(mainWidth, mainHeight, out float outerWidth, out float outerHeight);
            // 获取透明度
            float alpha = CalculateAlpha(currentFrame);

            Vector2 screenPos = Projectile.Center - Main.screenPosition;

            // 绘制主光晕
            Color mainColor = Config.MainColor * alpha;
            Rectangle mainRect = new Rectangle(
                (int)(screenPos.X - mainWidth * 0.5f),
                (int)(screenPos.Y - mainHeight * 0.5f),
                (int)mainWidth, (int)mainHeight);
            VertexDrawingHelper.DrawTexture(_texture, mainRect, mainColor, additive: true, rotation: 0f);

            // 绘制外光晕
            Color outerColor = Config.OuterColor * alpha;
            Rectangle outerRect = new Rectangle(
                (int)(screenPos.X - outerWidth * 0.5f),
                (int)(screenPos.Y - outerHeight * 0.5f),
                (int)outerWidth, (int)outerHeight);
            VertexDrawingHelper.DrawTexture(_texture, outerRect, outerColor, additive: true, rotation: 0f);

            return false;
        }

        private void GetMainSize(int frame, out float width, out float height)
        {
            if (frame <= Config.PeakFrame)
            {
                
                float t = frame / (float)Config.PeakFrame;
                float easeOut = 1f - (1f - t) * (1f - t); 
                width = Config.MainStartWidth + (Config.MainPeakWidth - Config.MainStartWidth) * easeOut;
                height = Config.MainStartHeight + (Config.MainPeakHeight - Config.MainStartHeight) * easeOut;
            }
            else if (frame <= Config.ShrinkEndFrame)
            {
            
                float t = (frame - Config.PeakFrame) / (float)(Config.ShrinkEndFrame - Config.PeakFrame);
                float easeIn = t * t; 
                width = Config.MainPeakWidth + (Config.MainShrinkWidth - Config.MainPeakWidth) * easeIn;
                height = Config.MainPeakHeight + (Config.MainShrinkHeight - Config.MainPeakHeight) * easeIn;
            }
            else
            {
           
                float t = (frame - Config.ShrinkEndFrame) / (float)(Config.TotalFrames - 1 - Config.ShrinkEndFrame);
                float easeOutQuad = 1f - (1f - t) * (1f - t);
                width = Config.MainShrinkWidth + (Config.MainEndWidth - Config.MainShrinkWidth) * easeOutQuad;
                height = Config.MainShrinkHeight + (Config.MainEndHeight - Config.MainShrinkHeight) * easeOutQuad;
            }
          
            width = System.Math.Max(1, width);
            height = System.Math.Max(1, height);
        }

        private void GetOuterSize(float mainWidth, float mainHeight, out float width, out float height)
        {
    
            int frame = Config.TotalFrames - Projectile.timeLeft;
            float scaleMultiplier;
            if (frame <= Config.PeakFrame)
            {
                float t = frame / (float)Config.PeakFrame;
                float easeOut = 1f - (1f - t) * (1f - t);
                scaleMultiplier = Config.OuterScaleStart + (Config.OuterScalePeak - Config.OuterScaleStart) * easeOut;
            }
            else if (frame <= Config.ShrinkEndFrame)
            {
                float t = (frame - Config.PeakFrame) / (float)(Config.ShrinkEndFrame - Config.PeakFrame);
                float easeIn = t * t;
                scaleMultiplier = Config.OuterScalePeak + (Config.OuterScaleShrink - Config.OuterScalePeak) * easeIn;
            }
            else
            {
                float t = (frame - Config.ShrinkEndFrame) / (float)(Config.TotalFrames - 1 - Config.ShrinkEndFrame);
                float easeOutQuad = 1f - (1f - t) * (1f - t);
                scaleMultiplier = Config.OuterScaleShrink + (Config.OuterScaleEnd - Config.OuterScaleShrink) * easeOutQuad;
            }
            width = mainWidth * scaleMultiplier;
            height = mainHeight * scaleMultiplier;
        }

        private float CalculateAlpha(int frame)
        {
            float t = frame / (float)(Config.TotalFrames - 1);
            if (Config.AlphaCurveType == 0)
            {
         
                return 1f - t * t;
            }
            else
            {
             
                return 1f - t;
            }
        }
    }
}