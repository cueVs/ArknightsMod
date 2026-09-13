using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Projectiles.Caster.Valarqvin
{
    public class ValarqvinHitParticle : ModProjectile
    {
        private static Texture2D _circleTexture;

        public int TotalFrames = 35;
        public float StartSize = 15f;
        public float EndSize = 1f;
        public float VelocityMultiplier = 1.2f;
        public Vector2 VelocityRandomRange = new Vector2(0.4f, 1.1f);
        public float VelocityDamping = 0.92f;
        public bool FaceMovementDirection = true;
        public Vector2 RotationSpeedRange = new Vector2(-0.06f, 0.06f);
        public Vector2 DeformationXRange = new Vector2(0.2f, 0.7f);
        public Vector2 DeformationYRange = new Vector2(1.2f, 2.2f);
        public float SizeMultiplier = 1.2f;
        public float OpacityPower = 0.5f;
        public float SpeedOpacityInfluence = 0.4f;
        public float MaxSpeedForOpacity = 10f;
        public Color ParticleColor = new Color(68, 90, 172); // 深蓝
        public bool UseAdditiveBlending = true;
        public int TextureSize = 32;
        public float TextureSoftness = 0.8f;

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

        public void Initialize(int totalFrames, float startSize, float endSize,
                              Vector2 velocity, Vector2 position, Color color)
        {
            this.totalFrames = totalFrames;
            this.startSize = startSize * SizeMultiplier;
            this.endSize = endSize * SizeMultiplier;
            this.currentColor = color;

            this.velocity = velocity * VelocityMultiplier * Main.rand.NextFloat(VelocityRandomRange.X, VelocityRandomRange.Y);
            this.rotationSpeed = Main.rand.NextFloat(RotationSpeedRange.X, RotationSpeedRange.Y);
            this.deformationX = Main.rand.NextFloat(DeformationXRange.X, DeformationXRange.Y);
            this.deformationY = Main.rand.NextFloat(DeformationYRange.X, DeformationYRange.Y);

            Projectile.Center = position;
            Projectile.timeLeft = this.totalFrames;
            initialized = true;
        }

        public override void AI()
        {
            if (!initialized) return;
            velocity *= VelocityDamping;
            Projectile.Center += velocity;
            rotation += rotationSpeed;
            if (FaceMovementDirection && velocity.Length() > 0.1f)
                rotation = velocity.ToRotation() + MathHelper.PiOver2;
        }

        private void EnsureTexture()
        {
            if (_circleTexture != null && _circleTexture.Width == TextureSize) return;
            _circleTexture = new Texture2D(Main.instance.GraphicsDevice, TextureSize, TextureSize);
            Color[] data = new Color[TextureSize * TextureSize];
            Vector2 center = new Vector2(TextureSize / 2f);
            float radius = TextureSize / 2f;
            for (int y = 0; y < TextureSize; y++)
                for (int x = 0; x < TextureSize; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), center);
                    if (dist <= radius)
                    {
                        float alpha = 1f - dist / radius;
                        alpha = (float)Math.Pow(alpha, TextureSoftness);
                        data[y * TextureSize + x] = Color.White * alpha;
                    }
                    else
                        data[y * TextureSize + x] = Color.Transparent;
                }
            _circleTexture.SetData(data);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            if (!initialized) return false;
            EnsureTexture();

            int currentFrame = totalFrames - Projectile.timeLeft;
            float progress = currentFrame / (float)totalFrames;
            float currentSize = MathHelper.Lerp(startSize, endSize, progress);
            float opacity = (float)Math.Pow(1f - progress, OpacityPower);
            float speedFactor = 1f - MathHelper.Clamp(velocity.Length() / MaxSpeedForOpacity, 0f, SpeedOpacityInfluence);
            opacity *= speedFactor;

            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Vector2 scale = new Vector2(currentSize / _circleTexture.Width * deformationX,
                                        currentSize / _circleTexture.Height * deformationY);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive,
                Main.DefaultSamplerState, DepthStencilState.Default, Main.Rasterizer, null, Main.GameViewMatrix.ZoomMatrix);

            Main.spriteBatch.Draw(_circleTexture, drawPos, null, currentColor * opacity,
                rotation, _circleTexture.Size() * 0.5f, scale, SpriteEffects.None, 0f);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                Main.DefaultSamplerState, DepthStencilState.Default, Main.Rasterizer, null, Main.GameViewMatrix.ZoomMatrix);
            return false;
        }
    }
}