using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ModLoader;

namespace ArknightsMod.Common.NPCDeathDebris
{
    /// <summary>
    /// 死亡碎块弹幕。持有运行时生成的贴图，并模拟物理抛体运动、旋转、淡出。
    /// </summary>
    public class NPCDebrisProjectile : ModProjectile
    {
        // 使用空贴图占位，实际贴图由 SetDebrisTexture 注入
        public override string Texture => ArknightsMod.noTexture;

        /// <summary>运行时动态贴图（由 NPCDebrisSystem 注入）</summary>
        private Texture2D _debrisTexture;

        /// <summary>旋转速度</summary>
        private float _rotationSpeed;

        /// <summary>是否已初始化旋转速度</summary>
        private bool _initialized;

        /// <summary>生成时记录的完整初速度（用于前几帧缓入，形成轻微向外爆开感）</summary>
        private Vector2 _burstFullVelocity;

        /// <summary>-1 未采样；-1 之后为已渡过爆发帧计数</summary>
        private int _burstPhase = -1;

        private const int BurstEaseFrames = 10;

        private float _drawScale = 1f;

        /// <summary>是否允许落地后滚动一小段（概率由生成侧决定）</summary>
        private bool _groundRollEligible;

        /// <summary>落地后沿地面滑动剩余帧数（类似原版碎块）</summary>
        private int _groundSlideTicks;

        public void SetDebrisTexture(Texture2D texture)
        {
            _debrisTexture = texture;
        }

        public void SetGroundRollEligible(bool eligible)
        {
            _groundRollEligible = eligible;
        }

        public override void SetDefaults()
        {
            Projectile.width = 8;
            Projectile.height = 8;
            Projectile.aiStyle = 0;
            Projectile.friendly = false;
            Projectile.hostile = false;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = false;
            Projectile.timeLeft = 180; // 3秒后消失
            Projectile.penetrate = -1;
            Projectile.damage = 0;
            Projectile.alpha = 0;
        }

        public override void AI()
        {
            if (!_initialized)
            {
                _rotationSpeed = Main.rand.NextFloat(-0.18f, 0.18f);
                _initialized = true;
            }

            if (_burstPhase == -1)
            {
                _burstFullVelocity = Projectile.velocity;
                _burstPhase = 0;
            }

            if (_burstPhase < BurstEaseFrames)
            {
                float t = (_burstPhase + 1) / (float)BurstEaseFrames;
                float ease = 1f - (float)Math.Pow(1f - t, 3);
                Projectile.velocity = _burstFullVelocity * ease;
                Projectile.rotation += _rotationSpeed * (0.35f + 0.65f * ease);
                _drawScale = MathHelper.Lerp(0.42f, 1f, ease);
                _burstPhase++;
                if (_burstPhase >= BurstEaseFrames)
                    Projectile.velocity = _burstFullVelocity;
                return;
            }

            _drawScale = 1f;

            if (_groundSlideTicks > 0)
            {
                Projectile.velocity.Y += 0.14f;
                Projectile.velocity.X *= 0.988f;
                Projectile.rotation += Projectile.velocity.X * 0.048f;
                _rotationSpeed *= 0.99f;
                _groundSlideTicks--;
                if (_groundSlideTicks <= 0 || Math.Abs(Projectile.velocity.X) < 0.07f)
                    _groundSlideTicks = 0;
                return;
            }

            // 重力
            Projectile.velocity.Y += 0.35f;
            // 空气阻力
            Projectile.velocity.X *= 0.985f;

            // 旋转
            Projectile.rotation += _rotationSpeed;

            // 碎块存活时间剩余60帧时开始淡出
            if (Projectile.timeLeft <= 60)
            {
                Projectile.alpha = (int)(255 * (1f - Projectile.timeLeft / 60f));
            }
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            // 落地（下落撞地）：生成时已标记的碎块获得短暂地面滑动（概率在生成侧决定）
            if (_groundRollEligible && _groundSlideTicks <= 0 && oldVelocity.Y > 0.55f)
            {
                _groundRollEligible = false;
                _groundSlideTicks = Main.rand.Next(22, 48);
                Projectile.velocity.X = Projectile.velocity.X * 0.58f + Main.rand.NextFloat(-2.6f, 2.6f);
                Projectile.velocity.Y = -Math.Abs(oldVelocity.Y) * 0.12f;
                _rotationSpeed *= 1.35f;
                return false;
            }

            // 落地后弹跳，减少速度
            if (Math.Abs(Projectile.velocity.Y) > Math.Abs(oldVelocity.Y))
                Projectile.velocity.Y = -oldVelocity.Y * 0.3f;
            Projectile.velocity.X *= 0.6f;

            // 旋转减速
            _rotationSpeed *= 0.5f;

            return false;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            if (_debrisTexture == null || _debrisTexture.IsDisposed)
                return false;

            float opacity = 1f - Projectile.alpha / 255f;
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
                Projectile.scale * _drawScale,
                SpriteEffects.None,
                0f
            );

            return false;
        }

        public override void OnKill(int timeLeft)
        {
            // 死亡时释放动态贴图，防止内存泄漏
            if (_debrisTexture != null && !_debrisTexture.IsDisposed)
            {
                _debrisTexture.Dispose();
                _debrisTexture = null;
            }
        }
    }
}
