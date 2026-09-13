using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace ArknightsMod.Content.NPCs
{
    /// <summary>
    /// 死亡碎块抛射物。由 DeathDebrisSystem 自动创建，无需手动注册到 NPC 以外的地方。
    /// 碎块会受重力影响、在地面上弹跳几次后消失，并带有旋转效果。
    /// </summary>
    public class DeathDebrisProjectile : ModProjectile
    {
        public override string Texture => ArknightsMod.noTexture;

        private Texture2D _debrisTexture;
        private bool _initialized;
        private int _bounceCount;
        private const int MaxBounces = 3;
        private float _rotationSpeed;
        private float _scaleVal = 1f;

        private Vector2 _burstFullVelocity;
        private int _burstPhase = -1;
        private const int BurstEaseFrames = 10;
        private float _spawnScaleAnim = 1f;

        public void InitDebris(string originalTexturePath, DebrisInfo info, int frameH)
        {
            if (_initialized || info == null)
                return;

            try
            {
                GraphicsDevice gd = Main.instance.GraphicsDevice;
                _debrisTexture = new Texture2D(gd, info.Width, info.Height);
                _debrisTexture.SetData(info.Pixels);
            }
            catch
            {
                _debrisTexture = null;
            }

            _rotationSpeed = Main.rand.NextFloat(-0.18f, 0.18f);
            if (Math.Abs(_rotationSpeed) < 0.03f)
                _rotationSpeed = 0.05f;

            Projectile.width = info.Width;
            Projectile.height = info.Height;

            _burstFullVelocity = Projectile.velocity;
            _burstPhase = 0;

            _initialized = true;
        }

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.aiStyle = 0;
            Projectile.friendly = false;
            Projectile.hostile = false;
            Projectile.penetrate = -1;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = false;
            Projectile.timeLeft = 300;
            Projectile.light = 0f;
            Projectile.alpha = 0;
        }

        public override void AI()
        {
            if (!_initialized)
                return;

            if (_burstPhase >= 0 && _burstPhase < BurstEaseFrames)
            {
                float t = (_burstPhase + 1) / (float)BurstEaseFrames;
                float ease = 1f - (float)Math.Pow(1f - t, 3);
                Projectile.velocity = _burstFullVelocity * ease;
                Projectile.rotation += _rotationSpeed * (0.35f + 0.65f * ease);
                _spawnScaleAnim = MathHelper.Lerp(0.42f, 1f, ease);
                _burstPhase++;
                if (_burstPhase >= BurstEaseFrames)
                    Projectile.velocity = _burstFullVelocity;
                return;
            }

            _spawnScaleAnim = 1f;

            Projectile.rotation += _rotationSpeed;

            float gravity = Projectile.wet ? 0.15f : 0.32f;
            Projectile.velocity.Y += gravity;
            Projectile.velocity.X *= 0.985f;

            if (Projectile.timeLeft < 60)
            {
                _scaleVal = Projectile.timeLeft / 60f;
            }
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            if (_bounceCount >= MaxBounces)
            {
                Projectile.velocity.X *= 0.3f;
                Projectile.velocity.Y = 0f;
                _rotationSpeed *= 0.1f;
                return false;
            }

            if (Math.Abs(Projectile.velocity.Y) > Math.Abs(oldVelocity.Y))
                Projectile.velocity.Y = -oldVelocity.Y * 0.35f;
            Projectile.velocity.X *= 0.6f;
            _rotationSpeed *= 0.6f;
            _bounceCount++;
            return false;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            if (_debrisTexture == null || _debrisTexture.IsDisposed)
                return false;

            float opacity = _scaleVal;
            Color drawColor = lightColor * opacity;
            Vector2 origin = _debrisTexture.Size() * 0.5f;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            Main.spriteBatch.Draw(
                _debrisTexture,
                drawPos,
                null,
                drawColor,
                Projectile.rotation,
                origin,
                _scaleVal * _spawnScaleAnim,
                SpriteEffects.None,
                0f
            );

            return false;
        }

        public override void OnKill(int timeLeft)
        {
            if (_debrisTexture != null && !_debrisTexture.IsDisposed)
            {
                _debrisTexture.Dispose();
                _debrisTexture = null;
            }
        }
    }
}
