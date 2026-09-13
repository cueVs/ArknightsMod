using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;

namespace ArknightsMod.Content.Projectiles.Sniper.Typhon
{
    internal static class TyphonArrowSkillTrailEffects
    {
        public static readonly Color TrailPink = new Color(248, 178, 255);

        public static readonly Color TrailPurpleDeep = new Color(98, 42, 130);

        public static readonly Color SpiralViolet = new Color(110, 62, 210);

        private static readonly Color TrailNearWhite = Color.White;

        private const int TrailLengthRampTicks = 36;

        private const float TrailSpeedStretchMax = 1.28f;

        private const float TrailSpeedStretchMin = 0.72f;

        private const float SpiralParamStep = 0.30f;

        private const float SpiralWorldScale = 9.2f;

        private const float SpiralCoreBrightnessMul = 1.32f;

        private const float SpiralMidHaloAlphaMul = 0.66f;

        private const float SpiralOuterBloomAlphaMul = 0.44f;

        public static Color SampleTrailColor(float tHeadToTail)
        {
            tHeadToTail = MathHelper.Clamp(tHeadToTail, 0f, 1f);

            Color rgb;
            if (tHeadToTail >= 0.52f)
            {
                float k = (tHeadToTail - 0.52f) / (1f - 0.52f);
                rgb = Color.Lerp(TrailPink, TrailNearWhite, MathHelper.SmoothStep(0f, 1f, k));
            }
            else if (tHeadToTail >= 0.20f)
            {
                float k = (tHeadToTail - 0.20f) / (0.52f - 0.20f);
                rgb = Color.Lerp(TrailPurpleDeep, TrailPink, MathHelper.SmoothStep(0f, 1f, k));
            }
            else
                rgb = TrailPurpleDeep;

            float tipFade = MathHelper.SmoothStep(0f, 1f, tHeadToTail / 0.42f);
            return rgb * tipFade;
        }

        public static void DrawSkillArrowTrails(TyphonArrow arrow, Projectile projectile)
        {
            if (Main.dedServ)
                return;

            int len = ProjectileID.Sets.TrailCacheLength[projectile.type];
            if (len <= 0)
                return;

            float growth = MathHelper.SmoothStep(0f, 1f, arrow.SkillTrailFlightTicks / (float)Math.Max(1, TrailLengthRampTicks));
            float spd = projectile.velocity.Length();
            float refSpd = Math.Max(arrow.SkillTrailSpawnSpeed, 4f);
            float speedStretch = MathHelper.Clamp(spd / refSpd, TrailSpeedStretchMin, TrailSpeedStretchMax);

            float trailMul = growth * speedStretch * arrow.SkillTrailHitShrink;
            trailMul = MathHelper.Clamp(trailMul, 0f, 1f);
            if (trailMul < 0.02f)
                return;

            int effectiveLen = Math.Max(3, (int)Math.Ceiling(len * trailMul));

            DrawMagicPixelGradientTrail(projectile, effectiveLen);
            DrawSpiralTrail(projectile, arrow.SkillTrailSpiralK, effectiveLen);
        }

        private static void DrawMagicPixelGradientTrail(Projectile projectile, int effectiveLen)
        {
            int len = ProjectileID.Sets.TrailCacheLength[projectile.type];
            effectiveLen = Math.Min(effectiveLen, len);

            Texture2D px = TextureAssets.MagicPixel.Value;
            var src = new Rectangle(0, 0, 1, 1);
            var leftOrigin = new Vector2(0f, 0.5f);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(
                SpriteSortMode.Deferred,
                BlendState.Additive,
                Main.DefaultSamplerState,
                DepthStencilState.None,
                Main.Rasterizer,
                null,
                Main.GameViewMatrix.TransformationMatrix);

            Vector2 prev = projectile.Center;
            for (int i = 0; i < effectiveLen; i++)
            {
                Vector2 cur = projectile.oldPos[i] + projectile.Size / 2f;
                if (projectile.oldPos[i] == Vector2.Zero)
                    break;

                float segHeadT = 1f - (float)i / effectiveLen;
                float segTailT = 1f - (float)(i + 1) / effectiveLen;
                float tMid = (segHeadT + segTailT) * 0.5f;

                Vector2 seg = cur - prev;
                float dist = seg.Length();
                if (dist < 0.5f)
                {
                    prev = cur;
                    continue;
                }

                float rot = seg.ToRotation();
                Vector2 drawPos = prev - Main.screenPosition;

                Color grad = SampleTrailColor(tMid);

                Main.spriteBatch.Draw(
                    px,
                    drawPos,
                    src,
                    grad * (segHeadT * 0.34f),
                    rot,
                    leftOrigin,
                    new Vector2(dist, 18f * segHeadT + 5f),
                    SpriteEffects.None,
                    0f);
                Main.spriteBatch.Draw(
                    px,
                    drawPos,
                    src,
                    grad * (segHeadT * 0.62f),
                    rot,
                    leftOrigin,
                    new Vector2(dist, 11f * segHeadT + 3f),
                    SpriteEffects.None,
                    0f);
                Main.spriteBatch.Draw(
                    px,
                    drawPos,
                    src,
                    grad * (segHeadT * 0.92f),
                    rot,
                    leftOrigin,
                    new Vector2(dist, 5f * segHeadT + 1.8f),
                    SpriteEffects.None,
                    0f);

                prev = cur;
            }

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(
                SpriteSortMode.Deferred,
                BlendState.AlphaBlend,
                Main.DefaultSamplerState,
                DepthStencilState.None,
                Main.Rasterizer,
                null,
                Main.GameViewMatrix.TransformationMatrix);
        }

        private static void DrawSpiralTrail(Projectile projectile, float spiralK, int effectiveLen)
        {
            int len = ProjectileID.Sets.TrailCacheLength[projectile.type];
            effectiveLen = Math.Min(effectiveLen, len);

            Vector2 fwd = projectile.velocity;
            if (fwd.LengthSquared() < 0.04f)
            {
                Vector2 fallBack = projectile.Center - (projectile.oldPos[0] + projectile.Size / 2f);
                fwd = fallBack.LengthSquared() > 0.04f ? fallBack : Vector2.UnitX;
            }

            fwd = fwd.SafeNormalize(Vector2.UnitX);
            Vector2 right = new Vector2(-fwd.Y, fwd.X);

            float totalLen = BuildTrailPolyline(projectile, effectiveLen, out Vector2[] pts, out float[] cum);
            if (totalLen < 2.5f || pts.Length < 2)
                return;

            Texture2D px = TextureAssets.MagicPixel.Value;
            var src = new Rectangle(0, 0, 1, 1);
            var origin = new Vector2(0.5f, 0.5f);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(
                SpriteSortMode.Deferred,
                BlendState.Additive,
                Main.DefaultSamplerState,
                DepthStencilState.None,
                Main.Rasterizer,
                null,
                Main.GameViewMatrix.TransformationMatrix);

            float tMax = 15f;
            Vector2 prevWorld = Vector2.Zero;
            bool hasPrev = false;

            for (float t = 0f; t <= tMax + 0.001f; t += SpiralParamStep)
            {
                float u = MathHelper.Clamp(t / tMax, 0f, 1f);
                float dist = u * totalLen;
                if (!SamplePolylineAtDistance(pts, cum, dist, out Vector2 spine, out Vector2 tangent))
                    continue;

                Vector2 f = tangent.SafeNormalize(fwd);
                Vector2 r = new Vector2(-f.Y, f.X);
                float amp = 2.05f * (1f - t / tMax);
                float theta = 2.35f * t + spiralK;
                float cosT = MathF.Cos(theta);
                float sinT = MathF.Sin(theta);
                Vector2 offset = (r * (amp * cosT) + (-f) * (amp * sinT)) * SpiralWorldScale;

                Vector2 world = spine + offset;
                if (hasPrev)
                {
                    Vector2 mid = (prevWorld + world) * 0.5f - Main.screenPosition;
                    Vector2 seg = world - prevWorld;
                    float segLen = seg.Length();
                    if (segLen > 0.18f)
                    {
                        float rot = seg.ToRotation();
                        float headW = MathHelper.Lerp(11.5f, 3.4f, u);
                        float alpha = (1f - u * u * 0.85f) * 0.92f;

                        Color core = SpiralViolet * (alpha * SpiralCoreBrightnessMul);
                        Main.spriteBatch.Draw(
                            px,
                            mid,
                            src,
                            core,
                            rot,
                            origin,
                            new Vector2(segLen * 1.08f, headW),
                            SpriteEffects.None,
                            0f);

                        Color midHalo = SpiralViolet * (alpha * SpiralMidHaloAlphaMul);
                        Main.spriteBatch.Draw(
                            px,
                            mid,
                            src,
                            midHalo,
                            rot,
                            origin,
                            new Vector2(segLen * 1.26f, headW * 1.58f),
                            SpriteEffects.None,
                            0f);

                        Color outerBloom = SpiralViolet * (alpha * SpiralOuterBloomAlphaMul);
                        Main.spriteBatch.Draw(
                            px,
                            mid,
                            src,
                            outerBloom,
                            rot,
                            origin,
                            new Vector2(segLen * 1.44f, headW * 2.25f),
                            SpriteEffects.None,
                            0f);
                    }
                }

                prevWorld = world;
                hasPrev = true;
            }

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(
                SpriteSortMode.Deferred,
                BlendState.AlphaBlend,
                Main.DefaultSamplerState,
                DepthStencilState.None,
                Main.Rasterizer,
                null,
                Main.GameViewMatrix.TransformationMatrix);
        }

        private static float BuildTrailPolyline(Projectile projectile, int effectiveLen, out Vector2[] pts, out float[] cumDist)
        {
            int n = 1;
            for (int i = 0; i < effectiveLen; i++)
            {
                if (projectile.oldPos[i] == Vector2.Zero)
                    break;
                n++;
            }

            if (n < 2)
            {
                pts = Array.Empty<Vector2>();
                cumDist = Array.Empty<float>();
                return 0f;
            }

            pts = new Vector2[n];
            cumDist = new float[n];
            pts[0] = projectile.Center;
            cumDist[0] = 0f;
            float total = 0f;
            int write = 1;
            for (int i = 0; i < effectiveLen && write < n; i++)
            {
                pts[write] = projectile.oldPos[i] + projectile.Size / 2f;
                total += Vector2.Distance(pts[write - 1], pts[write]);
                cumDist[write] = total;
                write++;
            }

            return total;
        }

        private static bool SamplePolylineAtDistance(Vector2[] pts, float[] cum, float dist, out Vector2 position, out Vector2 tangent)
        {
            position = pts[0];
            tangent = pts.Length > 1 ? pts[1] - pts[0] : Vector2.UnitX;
            if (cum.Length == 0)
                return false;

            if (dist <= 0f)
                return true;

            if (dist >= cum[^1])
            {
                position = pts[^1];
                tangent = pts[^1] - pts[^2];
                return true;
            }

            for (int i = 1; i < pts.Length; i++)
            {
                if (dist <= cum[i])
                {
                    float segLen = cum[i] - cum[i - 1];
                    float tt = segLen > 0.001f ? (dist - cum[i - 1]) / segLen : 0f;
                    tt = MathHelper.Clamp(tt, 0f, 1f);
                    position = Vector2.Lerp(pts[i - 1], pts[i], tt);
                    tangent = pts[i] - pts[i - 1];
                    return true;
                }
            }

            return false;
        }
    }
}
