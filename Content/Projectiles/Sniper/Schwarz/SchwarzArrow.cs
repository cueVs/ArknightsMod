using ArknightsMod.Content.Tiles.Infrastructure;
using ArknightsMod.Assets.Effects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using ArknightsMod.Players;

namespace ArknightsMod.Content.Projectiles.Sniper.Schwarz
{
    public class SchwarzArrow : ModProjectile
    {
        private static Asset<Effect> _distortEffect;
        private const float TrailHalfWidth = 55f;

        Player player => Main.player[Projectile.owner];

        public override void Load()
        {
            if (!Main.dedServ)
            {
                try
                {
                    _distortEffect = ModContent.Request<Effect>(
                        "ArknightsMod/Assets/Effects/SchwarzDistortion",
                        AssetRequestMode.ImmediateLoad);
                }
                catch { _distortEffect = null; }
            }
        }

        public override void Unload() => _distortEffect = null;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 15;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 10;
            Projectile.timeLeft = 300;
            Projectile.aiStyle = -1;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.arrow = true;
            Projectile.friendly = true;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            // 仅技能中发射的箭（ai[0] != 0）应用引力扭曲 + 灰色拖尾
            if (Projectile.ai[0] != 0f)
            {
                if (_distortEffect?.Value != null)
                    DrawDistortionTrail();
                DrawGrayTrail();
            }
            return true;
        }

        private void DrawGrayTrail()
        {
            int maxLen = ProjectileID.Sets.TrailCacheLength[Projectile.type];
            int count = 0;
            for (int i = 0; i < maxLen; i++)
            {
                if (i > 0 && Projectile.oldPos[i] == Vector2.Zero) break;
                count++;
            }
            if (count < 2) return;

            Texture2D pixel = TextureAssets.MagicPixel.Value;
            var pixelRect = new Rectangle(0, 0, 1, 1);
            var origin = new Vector2(0.5f, 0.5f);

            for (int i = 0; i < count - 1; i++)
            {
                Vector2 posA = Projectile.oldPos[i]     + Projectile.Size / 2f - Main.screenPosition;
                Vector2 posB = Projectile.oldPos[i + 1] + Projectile.Size / 2f - Main.screenPosition;

                Vector2 delta = posB - posA;
                float length = delta.Length();
                if (length < 0.5f) continue;
                float rot = delta.ToRotation();

                float t = (float)i / (count - 1); 
                float head = 1f - t; 
                float alpha = head * head * 0.65f;
                float thickness = MathHelper.Lerp(7f, 2f, t);

                Main.spriteBatch.Draw(
                    pixel,
                    posA,
                    pixelRect,
                    new Color(180, 180, 188) * alpha,
                    rot,
                    origin,
                    new Vector2(length, thickness * 1.6f),
                    SpriteEffects.None,
                    0f);

                // 内层亮芯
                Main.spriteBatch.Draw(
                    pixel,
                    posA,
                    pixelRect,
                    new Color(230, 232, 240) * (alpha * 1.1f),
                    rot,
                    origin,
                    new Vector2(length, thickness * 0.55f),
                    SpriteEffects.None,
                    0f);
            }
        }

        private void DrawDistortionTrail()
        {
            // 收集有效轨迹点
            int maxLen = ProjectileID.Sets.TrailCacheLength[Projectile.type];
            int count = 0;
            for (int i = 0; i < maxLen; i++)
            {
                if (i > 0 && Projectile.oldPos[i] == Vector2.Zero) break;
                count++;
            }
            if (count < 2) return;

            var gd = Main.instance?.GraphicsDevice;
            if (gd == null) return;

            var screenRT = Main.screenTarget;
            if (screenRT == null || screenRT.IsDisposed) return;

            int sw = screenRT.Width;
            int sh = screenRT.Height;

            // 按需创建/重建 ScreenSnapshot 与独立 capture batch
            if (SchwarzDistortionSystem.ScreenSnapshot == null ||
                SchwarzDistortionSystem.ScreenSnapshot.IsDisposed ||
                SchwarzDistortionSystem.ScreenSnapshot.Width  != sw ||
                SchwarzDistortionSystem.ScreenSnapshot.Height != sh)
            {
                SchwarzDistortionSystem.ScreenSnapshot?.Dispose();
                SchwarzDistortionSystem.ScreenSnapshot = new RenderTarget2D(
                    gd, sw, sh, false, screenRT.Format, DepthFormat.None);
            }
            if (SchwarzDistortionSystem.CaptureBatch == null ||
                SchwarzDistortionSystem.CaptureBatch.IsDisposed)
            {
                SchwarzDistortionSystem.CaptureBatch = new SpriteBatch(gd);
            }

            var snap = SchwarzDistortionSystem.ScreenSnapshot;
            var cap  = SchwarzDistortionSystem.CaptureBatch;
            var eff  = _distortEffect.Value;

            // 暂停 Main.spriteBatch（PreDraw 进入时它处于 active 状态）
            try { Main.spriteBatch.End(); } catch { }

            // ── 步骤1：把 Main.screenTarget 当前内容抓到 ScreenSnapshot ──
            //   Main.screenTarget 此刻持当前帧（世界+tile+NPC+先于本箭矢绘制的物体）
            gd.SetRenderTarget(snap);
            cap.Begin(SpriteSortMode.Deferred, BlendState.Opaque,
                      SamplerState.LinearClamp, DepthStencilState.None,
                      RasterizerState.CullNone, null, Matrix.Identity);
            cap.Draw(screenRT, Vector2.Zero, Color.White);
            cap.End();

            // ── 步骤2：重新绑定 Main.screenTarget 并立刻把 snap 画回去 ──
            //   重绑会触发 DiscardContents（如有），所以必须用 Opaque 整面填回
            gd.SetRenderTarget(screenRT);
            cap.Begin(SpriteSortMode.Deferred, BlendState.Opaque,
                      SamplerState.LinearClamp, DepthStencilState.None,
                      RasterizerState.CullNone, null, Matrix.Identity);
            cap.Draw(snap, Vector2.Zero, Color.White);
            cap.End();

            // 此后 RT 已是 screenTarget，内容完整。开始画扭曲。
            float snapW = sw;
            float snapH = sh;
            var   snapSz = new Vector2(snapW, snapH);
            float halfWidthUV = TrailHalfWidth / snapW;
            Matrix viewMtx = Main.GameViewMatrix.TransformationMatrix;

            const float kIntensity = 0.025f;
            eff.Parameters["uScreenResolution"]?.SetValue(snapSz);
            eff.Parameters["uTime"]?.SetValue((float)Main.timeForVisualEffects * 0.016f);
            eff.Parameters["uIntensity"]?.SetValue(kIntensity);

            // 用 Main.spriteBatch + Immediate 模式应用 effect
            Main.spriteBatch.Begin(
                SpriteSortMode.Immediate, BlendState.AlphaBlend,
                SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone,
                eff, Matrix.Identity);

            // 扭曲偏移的像素级填充（避免采样越界）
            float padPx = kIntensity * snapW + 8f;
            float halfWidthPx = TrailHalfWidth;

            for (int i = 0; i < count - 1; i++)
            {
                Vector2 posA = Projectile.oldPos[i]     + Projectile.Size / 2f;
                Vector2 posB = Projectile.oldPos[i + 1] + Projectile.Size / 2f;

                // 世界相对相机 → 实屏幕像素（关键修正：经过 zoom 变换）
                Vector2 scrA = Vector2.Transform(posA - Main.screenPosition, viewMtx);
                Vector2 scrB = Vector2.Transform(posB - Main.screenPosition, viewMtx);

                // 计算旋转包围盒的四个角（实屏幕像素）
                Vector2 dirPx   = scrB - scrA;
                float   dirLen  = dirPx.Length();
                if (dirLen < 0.5f) continue;
                Vector2 perpPx  = new(-dirPx.Y * halfWidthPx / dirLen, dirPx.X * halfWidthPx / dirLen);

                float bminX = Math.Max(0f,     MathF.Min(MathF.Min(scrA.X + perpPx.X, scrA.X - perpPx.X), MathF.Min(scrB.X + perpPx.X, scrB.X - perpPx.X)) - padPx);
                float bminY = Math.Max(0f,     MathF.Min(MathF.Min(scrA.Y + perpPx.Y, scrA.Y - perpPx.Y), MathF.Min(scrB.Y + perpPx.Y, scrB.Y - perpPx.Y)) - padPx);
                float bmaxX = Math.Min(snapW,  MathF.Max(MathF.Max(scrA.X + perpPx.X, scrA.X - perpPx.X), MathF.Max(scrB.X + perpPx.X, scrB.X - perpPx.X)) + padPx);
                float bmaxY = Math.Min(snapH,  MathF.Max(MathF.Max(scrA.Y + perpPx.Y, scrA.Y - perpPx.Y), MathF.Max(scrB.Y + perpPx.Y, scrB.Y - perpPx.Y)) + padPx);

                if (bmaxX <= bminX || bmaxY <= bminY) continue;

                var srcRect = new Rectangle((int)bminX, (int)bminY,
                                            (int)(bmaxX - bminX), (int)(bmaxY - bminY));

                // 段的屏幕 UV（与 shader 中 coords 完全同空间：实屏幕像素 / snap 尺寸）
                Vector2 uvA   = scrA / snapSz;
                Vector2 uvB   = scrB / snapSz;
                Vector2 uvDir = uvB - uvA;

                eff.Parameters["uTargetPosition"]?.SetValue(uvA);
                eff.Parameters["uDirection"]?.SetValue(uvDir);
                eff.Parameters["uImageSize1"]?.SetValue(new Vector2(uvDir.Length(), halfWidthUV));
                eff.Parameters["uColor"]?.SetValue(new Vector3(
                    (float)i       / (count - 1),
                    (float)(i + 1) / (count - 1),
                    0f));

                // 上传参数并立即绘制该段的包围盒区域
                eff.CurrentTechnique.Passes[0].Apply();
                Main.spriteBatch.Draw(snap, new Vector2(bminX, bminY), srcRect, Color.White);
            }

            // 恢复普通 SpriteBatch 状态（projectile 绘制阶段使用 GameViewMatrix）
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(
                SpriteSortMode.Deferred, BlendState.AlphaBlend,
                Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer,
                null, Main.GameViewMatrix.TransformationMatrix);
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            Vector2 dir  = Projectile.velocity.SafeNormalize(Vector2.Zero);
            Vector2 head = Projectile.Center + dir * 120f;
            Vector2 tail = Projectile.Center - dir * 6f;
            float t = 0f;
            if (Collision.CheckAABBvLineCollision(
                    targetHitbox.TopLeft(), targetHitbox.Size(),
                    tail, head, 4f, ref t))
                return true;
            return null;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            var modPlayer = Main.LocalPlayer.GetModPlayer<WeaponPlayer>();
            if (Main.myPlayer == player.whoAmI) {
                if (modPlayer.SkillActive == true){
                    if (modPlayer.Skill == 0 && Main.rand.NextFloat() < 0.8f) {
                        target.AddBuff(36, 300);
                    }
                    else if (modPlayer.Skill == 1 && Main.rand.NextFloat() < 0.5f) {
                        target.AddBuff(36, 300);
                    }
                    else if (modPlayer.Skill == 2 ) {
                        target.AddBuff(36, 300);
                    }
                }
                else {
                    if (Main.rand.NextFloat() < 0.2f)
					    target.AddBuff(36, 300);
                }
			}
        }

        public override void AI()
        {
            var modPlayer = player.GetModPlayer<WeaponPlayer>();
            Projectile.rotation = Projectile.velocity.ToRotation() - MathHelper.PiOver2;
            if (Main.rand.NextBool(3))
            {
                Vector2 spawnPos = Projectile.Center
                    - Projectile.velocity * Main.rand.NextFloat(0.1f, 0.5f);

                Vector2 perpendicular = Projectile.velocity.RotatedBy(MathHelper.PiOver2).SafeNormalize(Vector2.Zero);
                Vector2 scatterVel = perpendicular * Main.rand.NextFloat(-2f, 2f)
                    + Projectile.velocity * Main.rand.NextFloat(-0.1f, 0.3f);

                int idx = Projectile.NewProjectile(
                    Projectile.GetSource_FromAI(),
                    spawnPos,
                    scatterVel,
                    ModContent.ProjectileType<TrailShard>(),
                    0,
                    0,
                    Projectile.owner
                );

                if (idx >= 0 && idx < Main.maxProjectiles)
                {
                    Projectile p = Main.projectile[idx];
                    p.ai[0] = Main.rand.NextBool() ? 0f : 1f;
                    p.localAI[0] = Main.rand.NextFloat(4f, 10f);
                    p.localAI[1] = Main.rand.NextFloat(3f, 7f);
                    p.rotation = Main.rand.NextFloat(MathHelper.TwoPi);
                }
            }
            base.AI();
        }
    }

    public class TrailShard : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.width = 8;
            Projectile.height = 8;
            Projectile.timeLeft = 45;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.friendly = false;
            Projectile.hostile = false;
        }

        public override void AI()
        {
            Projectile.velocity *= 0.88f;
            Projectile.velocity.Y += 0.08f;
            Projectile.alpha += 5;
            Projectile.rotation += Projectile.ai[0] == 0f ? 0.12f : -0.12f;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            if (Projectile.alpha >= 255)
                return false;

            Texture2D tex = TextureAssets.MagicPixel.Value;
            Color color = Projectile.ai[0] == 0f ? Color.White : Color.Black;
            float alpha = 1f - Projectile.alpha / 255f;

            float w = Projectile.localAI[0];
            float h = Projectile.localAI[1];

            Main.spriteBatch.Draw(
                tex,
                Projectile.Center - Main.screenPosition,
                new Rectangle(0, 0, 1, 1),
                color * alpha,
                Projectile.rotation,
                new Vector2(0.5f, 0.5f),
                new Vector2(w, h),
                SpriteEffects.None,
                0f
            );

            Main.spriteBatch.Draw(
                tex,
                Projectile.Center - Main.screenPosition,
                new Rectangle(0, 0, 1, 1),
                color * (alpha * 0.6f),
                Projectile.rotation + MathHelper.PiOver4,
                new Vector2(0.5f, 0.5f),
                new Vector2(w * 0.6f, h * 0.6f),
                SpriteEffects.None,
                0f
            );

            return false;
        }
    }
}
