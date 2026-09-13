using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Projectiles.Rogue.Dedication
{
    public class ParticleExplosionParticle : ModProjectile
    {
        private static Texture2D _circleTexture;

   

        /// <summary>粒子存在总帧数（默认60帧=1秒）</summary>
        public int TotalFrames = 45;

        /// <summary>粒子初始大小（像素）</summary>
        public float StartSize =20f;

        /// <summary>粒子结束大小（像素）</summary>
        public float EndSize = 1f;

        /// <summary>初始速度乘数（实际速度 = 传入速度 × 此值 × 随机范围）</summary>
        public float VelocityMultiplier = 1f;

        /// <summary>速度随机范围（最小值，最大值）</summary>
        public Vector2 VelocityRandomRange = new Vector2(0.3f, 1.05f);

        /// <summary>速度衰减系数（每帧乘以此值，越小衰减越快）</summary>
        public float VelocityDamping = 0.915f;

        /// <summary>是否让粒子朝向运动方向</summary>
        public bool FaceMovementDirection = true;

        /// <summary>旋转速度随机范围（弧度/帧）</summary>
        public Vector2 RotationSpeedRange = new Vector2(-0.05f, 0.05f);

        /// <summary>X轴形变范围（最小值，最大值）</summary>
        public Vector2 DeformationXRange = new Vector2(0.1f, 0.6f);

        /// <summary>Y轴形变范围（最小值，最大值）</summary>
        public Vector2 DeformationYRange = new Vector2(1.2f, 2.0f);

        /// <summary>大小乘数（整体缩放）</summary>
        public float SizeMultiplier = 1.5f;

        /// <summary>透明度衰减曲线强度（1=线性，>1=先快后慢，<1=先慢后快）</summary>
        public float OpacityPower =0.6f;

        /// <summary>速度影响透明度系数（速度越大越透明）</summary>
        public float SpeedOpacityInfluence = 0.5f;

        /// <summary>最大速度影响透明度阈值</summary>
        public float MaxSpeedForOpacity = 10f;

        /// <summary>粒子颜色（RGB）</summary>
        public Color ParticleColor = new Color(102, 0, 255);

        /// <summary>是否使用叠加混合模式（发光效果）</summary>
        public bool UseAdditiveBlending = true;

        /// <summary>圆形纹理尺寸（2的幂次方）</summary>
        public int TextureSize = 32;

        /// <summary>圆形纹理边缘柔和度（1=线性，>1=更锐利，<1=更柔和）</summary>
        public float TextureSoftness = 1f;

        // ========== 私有字段 ==========

        private int totalFrames;
        private float startSize;
        private float endSize;
        private Vector2 velocity;
        private float rotation;
        private float rotationSpeed;
        private float deformationX;
        private float deformationY;
        private bool initialized = false;
        private Color currentColor;

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

        /// <summary>
        /// 初始化粒子（使用默认参数）
        /// </summary>
        public void Initialize(int totalFrames, float startSize, float endSize,
                              Vector2 velocity, Vector2 position)
        {
            Initialize(totalFrames, startSize, endSize, velocity, position,
                      this.ParticleColor, this.TotalFrames, this.StartSize, this.EndSize,
                      this.VelocityMultiplier, this.VelocityRandomRange, this.VelocityDamping,
                      this.FaceMovementDirection, this.RotationSpeedRange, this.DeformationXRange,
                      this.DeformationYRange, this.SizeMultiplier, this.OpacityPower,
                      this.SpeedOpacityInfluence, this.MaxSpeedForOpacity);
        }

        /// <summary>
        /// 初始化粒子（完整参数版本）
        /// </summary>
        public void Initialize(int totalFrames, float startSize, float endSize,
                              Vector2 velocity, Vector2 position,
                              Color particleColor,
                              int? totalFramesOverride = null,
                              float? startSizeOverride = null,
                              float? endSizeOverride = null,
                              float? velocityMultiplier = null,
                              Vector2? velocityRandomRange = null,
                              float? velocityDamping = null,
                              bool? faceMovementDirection = null,
                              Vector2? rotationSpeedRange = null,
                              Vector2? deformationXRange = null,
                              Vector2? deformationYRange = null,
                              float? sizeMultiplier = null,
                              float? opacityPower = null,
                              float? speedOpacityInfluence = null,
                              float? maxSpeedForOpacity = null)
        {
          
            this.totalFrames = totalFramesOverride ?? totalFrames;
            this.startSize = (startSizeOverride ?? startSize) * (sizeMultiplier ?? SizeMultiplier);
            this.endSize = (endSizeOverride ?? endSize) * (sizeMultiplier ?? SizeMultiplier);
            this.currentColor = particleColor;

            float velMult = velocityMultiplier ?? VelocityMultiplier;
            Vector2 velRange = velocityRandomRange ?? VelocityRandomRange;

        
            this.velocity = velocity * velMult * Main.rand.NextFloat(velRange.X, velRange.Y);

            Vector2 rotRange = rotationSpeedRange ?? RotationSpeedRange;
            this.rotationSpeed = Main.rand.NextFloat(rotRange.X, rotRange.Y);

            Vector2 deformXRange = deformationXRange ?? DeformationXRange;
            Vector2 deformYRange = deformationYRange ?? DeformationYRange;
            this.deformationX = Main.rand.NextFloat(deformXRange.X, deformXRange.Y);
            this.deformationY = Main.rand.NextFloat(deformYRange.X, deformYRange.Y);

            this.initialized = true;
            this.VelocityDamping = velocityDamping ?? VelocityDamping;
            this.FaceMovementDirection = faceMovementDirection ?? FaceMovementDirection;

            Projectile.Center = position;
            Projectile.timeLeft = this.totalFrames;
        }

        public override void AI()
        {
            if (!initialized) return;

          
            velocity *= VelocityDamping;
            Projectile.Center += velocity;

            
            rotation += rotationSpeed;

   
            if (FaceMovementDirection && velocity.Length() > 0.1f)
            {
                rotation = velocity.ToRotation() + MathHelper.PiOver2;
            }
        }

        private void EnsureTexture()
        {
            if (_circleTexture != null && _circleTexture.Width == TextureSize) return;

            _circleTexture = new Texture2D(Main.instance.GraphicsDevice, TextureSize, TextureSize);
            Color[] data = new Color[TextureSize * TextureSize];

            Vector2 center = new Vector2(TextureSize / 2f, TextureSize / 2f);
            float radius = TextureSize / 2f;

            for (int y = 0; y < TextureSize; y++)
            {
                for (int x = 0; x < TextureSize; x++)
                {
                    Vector2 pos = new Vector2(x, y);
                    float dist = Vector2.Distance(pos, center);
                    if (dist <= radius)
                    {
                        float alpha = 1f - (dist / radius);
                        alpha = MathHelper.Clamp(alpha, 0f, 1f);
                        alpha = (float)Math.Pow(alpha, TextureSoftness);
                        data[y * TextureSize + x] = Color.White * alpha;
                    }
                    else
                    {
                        data[y * TextureSize + x] = Color.Transparent;
                    }
                }
            }
            _circleTexture.SetData(data);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            if (!initialized) return false;

            EnsureTexture();

            int currentFrame = totalFrames - Projectile.timeLeft;
            float progress = currentFrame / (float)totalFrames;
            progress = MathHelper.Clamp(progress, 0f, 1f);

    
            float currentSize = MathHelper.Lerp(startSize, endSize, progress);

      
            float opacity = 1f - progress;
            opacity = (float)Math.Pow(opacity, OpacityPower);
            opacity = MathHelper.Clamp(opacity, 0f, 1f);

   
            float speedFactor = 1f - MathHelper.Clamp(velocity.Length() / MaxSpeedForOpacity, 0f, SpeedOpacityInfluence);
            opacity *= speedFactor;

            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Vector2 scale = new Vector2(currentSize / _circleTexture.Width * deformationX,
                                        currentSize / _circleTexture.Width * deformationY);

      
            var spriteBatch = Main.spriteBatch;
            spriteBatch.End();

 
            if (UseAdditiveBlending)
                spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive,
                    Main.DefaultSamplerState, DepthStencilState.Default,
                    Main.Rasterizer, null, Main.GameViewMatrix.ZoomMatrix);
            else
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                    Main.DefaultSamplerState, DepthStencilState.Default,
                    Main.Rasterizer, null, Main.GameViewMatrix.ZoomMatrix);

            // 绘制
            Color drawColor = currentColor * opacity;
            spriteBatch.Draw(_circleTexture, drawPos, null, drawColor,
                rotation, _circleTexture.Size() * 0.5f, scale, SpriteEffects.None, 0f);

    
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                Main.DefaultSamplerState, DepthStencilState.Default,
                Main.Rasterizer, null, Main.GameViewMatrix.ZoomMatrix);

            return false;
        }
    }
}