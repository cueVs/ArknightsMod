using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Projectiles.Sniper.Typhon
{
    // S3 蓄力、十字星、烟雾环等绘制参数集中在这里，别和 TyphonStar.AI 混在一起改
    internal static class TyphonS3StarChargeEffects
    {
        public const float SmokeRingSpawnIntervalSeconds = 0.15f;

        public const int SmokeRingMaxConcurrent = 3;

        public const float SmokeRingLifetimeSeconds = 0.55f;

        public const float CrossArmCenterThicknessMul = 0.2f;

        public const float CrossChargeSizePulseMin = 0.58f;

        private const string SmokeTexturePathPrefix =
            "ArknightsMod/Content/Projectiles/Sniper/Typhon/Effects/ChargeSmoke";

        private const string ChargeCrossTexturePath =
            "ArknightsMod/Content/Projectiles/Sniper/Typhon/Effects/Cross";

        private static Texture2D ChargeCrossTexture;

        private static readonly Color ChargeCrossGlowColor = new Color(255, 161, 255);

        private const float CrossTexCardinalMaskHalfFracOfMinSide = 0.092f;

        private const int SmokeRingArcSlicesMin = 48;
        private const int SmokeRingArcSlicesMax = 128;

        private const float SmokeRingSliceOverlapMul = 1.12f;

        private const int SmokeRingSegmentCount = 5;

        private const float SmokeRingSliceGlowTintMul = 0.26f;

        private const float SmokeRingChargePulseBrightToDimSeconds = 0.06f;

        private const float SmokeRingChargePulseDimFrac = 0.56f;

        private const float SmokeRingChargeWhiteMixToWhite = 0.66f;

        private const float SmokeRingChargeWhiteGlowBoost = 1.55f;

        private const float SmokeRingTexWarpInwardPx = 13f;

        private const float SmokeRingTexWarpRadialScaleMin = 0.56f;

        private const float SmokeRingTexWarpTangentPinchMax = 0.11f;

        private const float SmokeRingPathRConvex = 1.4f;

        private const float SmokeRingPathRConcave = 1.1f;

        private static Texture2D[] SmokeTextures;

        private static Vector2 SmokeRingCurveOffset(float t, float ringRadius)
        {
            float k = 1f - SmokeRingPathRConcave / SmokeRingPathRConvex;
            float s2 = MathF.Sin(2f * t);
            float s4 = s2 * s2;
            s4 *= s4;
            float radialFactor = 1f - k * s4;
            float r = ringRadius * radialFactor;
            return new Vector2(MathF.Cos(t), MathF.Sin(t)) * r;
        }

        private static Vector2 SmokeRingCurveTangent(float t, float ringRadius)
        {
            float k = 1f - SmokeRingPathRConcave / SmokeRingPathRConvex;
            float sin2t = MathF.Sin(2f * t);
            float cos2t = MathF.Cos(2f * t);
            float sin2t3 = sin2t * sin2t * sin2t;
            float dFactorDt = -k * 8f * sin2t3 * cos2t;
            float s4 = sin2t * sin2t;
            s4 *= s4;
            float radialFactor = 1f - k * s4;
            float cost = MathF.Cos(t);
            float sint = MathF.Sin(t);
            float r = ringRadius * radialFactor;
            float drdt = ringRadius * dFactorDt;
            return new Vector2(drdt * cost - r * sint, drdt * sint + r * cost);
        }

        public struct ChargeSmokeRingInstance
        {
            public int SpawnVisibleTick;

            public int LayoutSalt;

            public int SegmentCount;
            public int Index0;
            public int Index1;
            public int Index2;
            public int Index3;
            public int Index4;

            public readonly int GetAssetIndex(int seg)
            {
                return seg switch
                {
                    0 => Index0,
                    1 => Index1,
                    2 => Index2,
                    3 => Index3,
                    4 => Index4,
                    _ => 0
                };
            }
        }

        public static void LoadSmokeTextures()
        {
            SmokeTextures = new Texture2D[5];
            for (int i = 0; i < 5; i++)
            {
                string path = $"{SmokeTexturePathPrefix}{(i + 1):D2}";
                if (ModContent.RequestIfExists<Texture2D>(path, out var asset, AssetRequestMode.ImmediateLoad))
                    SmokeTextures[i] = asset.Value;
                else
                    SmokeTextures[i] = null;
            }
        }

        public static void LoadChargeCrossTexture()
        {
            if (ModContent.RequestIfExists<Texture2D>(ChargeCrossTexturePath, out var asset, AssetRequestMode.ImmediateLoad))
                ChargeCrossTexture = asset.Value;
            else
                ChargeCrossTexture = null;
        }

        public static void TickChargeSmokeRingQueue(
            int visibleElapsedTicks,
            int projectileIdentity,
            int ownerWhoAmI,
            ChargeSmokeRingInstance[] ringBuffer,
            ref int ringCount,
            ref float spawnAccumulatorSeconds)
        {
            if (ringBuffer == null || ringBuffer.Length < SmokeRingMaxConcurrent)
                return;

            float dt = 1f / 60f;
            RemoveExpiredRings(ringBuffer, ref ringCount, visibleElapsedTicks);

            if (visibleElapsedTicks == 0)
            {
                spawnAccumulatorSeconds = 0f;
                TryPushNewRing(visibleElapsedTicks, projectileIdentity, ownerWhoAmI, ringBuffer, ref ringCount);
            }

            spawnAccumulatorSeconds += dt;
            while (spawnAccumulatorSeconds >= SmokeRingSpawnIntervalSeconds)
            {
                spawnAccumulatorSeconds -= SmokeRingSpawnIntervalSeconds;
                TryPushNewRing(visibleElapsedTicks, projectileIdentity, ownerWhoAmI, ringBuffer, ref ringCount);
            }
        }

        public static void ResetChargeSmokeRingQueue(ref int ringCount, ref float spawnAccumulatorSeconds)
        {
            ringCount = 0;
            spawnAccumulatorSeconds = 0f;
        }

        public static void SpawnTyphonBowNormalMuzzleFlash(Vector2 worldPos, float armBaseRotationRad, float visualScale = 1f)
        {
            if (Main.dedServ || Main.gamePaused)
                return;

            visualScale = MathHelper.Clamp(visualScale, 0.35f, 3f);
            float s = visualScale;
            float velS = MathF.Sqrt(s);

            Lighting.AddLight(worldPos, 0.58f * s, 0.32f * s, 0.9f * s);

            Color brightCore = new Color(248, 205, 255);
            Color armTint = new Color(200, 125, 255);

            for (int i = 0; i < 3; i++)
            {
                Dust d = Dust.NewDustPerfect(
                    worldPos + Main.rand.NextVector2Circular(1.1f, 1.1f) * s,
                    DustID.CrystalPulse,
                    Main.rand.NextVector2Circular(0.14f, 0.14f) * velS,
                    0,
                    Color.Lerp(brightCore, Color.White, Main.rand.NextFloat(0.15f, 0.48f)),
                    Main.rand.NextFloat(0.5f, 0.68f) * s);
                d.noGravity = true;
                d.fadeIn = 0.55f;
            }

            float armReachMin = 5.5f * s;
            float armReachMax = 10f * s;
            for (int k = 0; k < 4; k++)
            {
                float ang = armBaseRotationRad + k * MathHelper.PiOver2 + Main.rand.NextFloat(-0.06f, 0.06f);
                Vector2 dir = ang.ToRotationVector2();
                Dust d = Dust.NewDustPerfect(
                    worldPos + dir * Main.rand.NextFloat(armReachMin, armReachMax),
                    DustID.Enchanted_Pink,
                    dir * Main.rand.NextFloat(0.25f, 0.55f) * velS,
                    0,
                    armTint,
                    Main.rand.NextFloat(0.42f, 0.58f) * s);
                d.noGravity = true;
                d.fadeIn = 0.45f;
            }

            Dust gem = Dust.NewDustPerfect(
                worldPos,
                DustID.GemAmethyst,
                Vector2.Zero,
                0,
                new Color(225, 165, 255),
                Main.rand.NextFloat(0.55f, 0.72f) * s);
            gem.noGravity = true;
            gem.fadeIn = 0.6f;

            Dust pulse = Dust.NewDustPerfect(
                worldPos + Main.rand.NextVector2Circular(0.6f, 0.6f) * s,
                DustID.PurpleTorch,
                Main.rand.NextVector2Circular(0.2f, 0.2f) * velS,
                0,
                new Color(235, 190, 255),
                Main.rand.NextFloat(0.48f, 0.62f) * s);
            pulse.noGravity = true;
            pulse.fadeIn = 0.5f;
        }

        public static void SpawnChargeGatherDust(Vector2 worldCenter, int projectileIdentity, int visibleElapsedTick)
        {
            if (Main.gamePaused || Main.dedServ)
                return;

            int h = MixHash(projectileIdentity, visibleElapsedTick, 0x4B1D, 0);
            if ((h & 0xFF) > 148)
                return;

            int count = 1 + ((h >> 8) & 1);
            for (int i = 0; i < count; i++)
            {
                int hh = MixHash(projectileIdentity, visibleElapsedTick, i, unchecked((int)0xC001D00Du));
                float ang = (hh & 0xFFFFFFF) / (float)0x10000000 * MathHelper.TwoPi;
                float dist = 38f + (hh % 1000) / 1000f * 95f;
                Vector2 pos = worldCenter + ang.ToRotationVector2() * dist;
                Vector2 toC = worldCenter - pos;
                if (toC.LengthSquared() < 4f)
                    continue;

                Vector2 dir = toC.SafeNormalize(Vector2.Zero);
                float spd = 0.9f + (hh % 256) / 256f * 3.4f;
                Vector2 vel = dir * spd + Main.rand.NextVector2Circular(0.2f, 0.2f);

                int pick = (hh >> 13) % 4;
                int dustType;
                Color dustColor = default;
                float scale = 0.82f + (hh % 120) / 200f;

                switch (pick)
                {
                    case 0:
                        dustType = DustID.CrystalPulse;
                        break;
                    case 1:
                        dustType = DustID.GemAmethyst;
                        break;
                    case 2:
                        dustType = DustID.Enchanted_Pink;
                        break;
                    default:
                        dustType = DustID.FireworksRGB;
                        dustColor = new Color(255, 250, 255);
                        scale *= 0.75f;
                        break;
                }

                Dust d = Dust.NewDustDirect(pos - new Vector2(2f), 4, 4, dustType, vel.X, vel.Y, 0, dustColor, scale);
                d.noGravity = true;
                d.velocity = vel;
            }
        }

        private static void RemoveExpiredRings(ChargeSmokeRingInstance[] ringBuffer, ref int ringCount, int visibleElapsedTicks)
        {
            int write = 0;
            for (int i = 0; i < ringCount; i++)
            {
                float ageSec = (visibleElapsedTicks - ringBuffer[i].SpawnVisibleTick) / 60f;
                if (ageSec <= SmokeRingLifetimeSeconds)
                {
                    if (write != i)
                        ringBuffer[write] = ringBuffer[i];
                    write++;
                }
            }
            ringCount = write;
        }

        private static void TryPushNewRing(
            int spawnVisibleTick,
            int projectileIdentity,
            int ownerWhoAmI,
            ChargeSmokeRingInstance[] ringBuffer,
            ref int ringCount)
        {
            if (!TryBuildRingPick(projectileIdentity, ownerWhoAmI, spawnVisibleTick, out ChargeSmokeRingInstance inst))
                return;

            inst.SpawnVisibleTick = spawnVisibleTick;
            inst.LayoutSalt = MixHash(projectileIdentity, ownerWhoAmI, spawnVisibleTick, 0x7E11BEE);

            if (ringCount >= SmokeRingMaxConcurrent)
            {
                for (int i = 0; i < ringCount - 1; i++)
                    ringBuffer[i] = ringBuffer[i + 1];
                ringCount = SmokeRingMaxConcurrent - 1;
            }

            ringBuffer[ringCount++] = inst;
        }

        private static bool TryBuildRingPick(int projectileIdentity, int ownerWhoAmI, int spawnVisibleTick, out ChargeSmokeRingInstance inst)
        {
            inst = default;
            Span<int> valid = stackalloc int[5];
            int nValid = 0;
            if (SmokeTextures != null)
            {
                for (int i = 0; i < 5 && i < SmokeTextures.Length; i++)
                {
                    if (SmokeTextures[i] != null)
                        valid[nValid++] = i;
                }
            }

            if (nValid < SmokeRingSegmentCount)
                return false;

            Span<int> pick = stackalloc int[5];
            for (int i = 0; i < nValid; i++)
                pick[i] = valid[i];

            for (int i = nValid - 1; i > 0; i--)
            {
                int h = MixHash(projectileIdentity, ownerWhoAmI, spawnVisibleTick, unchecked(i * 1103515245));
                int j = ((h & 0x7FFFFFFF) % (i + 1));
                (pick[i], pick[j]) = (pick[j], pick[i]);
            }

            inst.SegmentCount = SmokeRingSegmentCount;
            inst.Index0 = pick[0];
            inst.Index1 = pick[1];
            inst.Index2 = pick[2];
            inst.Index3 = pick[3];
            inst.Index4 = pick[4];

            return true;
        }

        private static int MixHash(int a, int b, int c, int salt)
        {
            unchecked
            {
                int h = salt + a * 374761393 + b * 668265263 + c * 1274126177;
                h = (h ^ (h >> 13)) * 1274126177;
                return h ^ (h >> 16);
            }
        }

        public static void DrawChargePhase(
            Vector2 centerScreen,
            float chargeRatio,
            float pulseT,
            Texture2D px,
            Rectangle src,
            ChargeSmokeRingInstance[] smokeRings,
            int smokeRingCount,
            int visibleElapsedTicks,
            int projectileIdentity,
            float geometryScale = 1f,
            bool drawInwardWhiteLines = true)
        {
            geometryScale = MathHelper.Clamp(geometryScale, 0.05f, 4f);

            float pulseU = GetChargeSizePulse01(pulseT);

            var originCenter = new Vector2(0.5f, 0.5f);

            float strength = 0.38f + 0.62f * chargeRatio;
            float lenEase = MathHelper.SmoothStep(0f, 1f, chargeRatio);
            float envelope = 0.78f + 0.22f * lenEase;
            float bloomScale = MathHelper.Lerp(72f, 218f, pulseU) * envelope * (0.86f + 0.14f * strength) * geometryScale;

            float crossEndPeakEnv = GetCrossChargeEndPeakEnvelope01(chargeRatio);
            float crossEndPeakSmooth = MathHelper.SmoothStep(0f, 1f, crossEndPeakEnv);
            float crossPulseU = MathHelper.Lerp(CrossChargeSizePulseMin, 1f, crossEndPeakEnv);
            float envelopeCross = MathHelper.Lerp(0.84f, 1f, crossEndPeakSmooth);
            float crossPulseUBumped = MathHelper.Clamp(
                crossPulseU + 0.065f * (1f - crossEndPeakEnv),
                CrossChargeSizePulseMin,
                1f);
            float halfArmCross = MathHelper.Lerp(38f, 108f, crossPulseUBumped) * envelopeCross * geometryScale;

            const float crossGlowVisualT = 1f;
            const float crossGlowStrength = 1f;
            const float crossGlowOuterStrength = 0.28f;

            Color barTipPurple = new Color(180, 80, 255);
            Color bloomTint = new Color(150, 60, 255);

            float anchorDiskRadius = MathHelper.Lerp(38f, 108f, CrossChargeSizePulseMin) * 0.78f * geometryScale;

            Main.spriteBatch.End();

            Main.spriteBatch.Begin(
                SpriteSortMode.Deferred,
                BlendState.AlphaBlend,
                Main.DefaultSamplerState,
                DepthStencilState.None,
                Main.Rasterizer,
                null,
                Main.GameViewMatrix.TransformationMatrix);
            DrawPurpleAnchorDiskAlphaBase(px, centerScreen, src, anchorDiskRadius, chargeRatio);
            Main.spriteBatch.End();

            Main.spriteBatch.Begin(
                SpriteSortMode.Deferred,
                BlendState.Additive,
                SamplerState.LinearClamp,
                DepthStencilState.None,
                RasterizerState.CullCounterClockwise,
                null,
                Main.GameViewMatrix.TransformationMatrix);

            const int bloomAngleSteps = 18;
            for (int ring = 0; ring < 8; ring++)
            {
                float k = 1f - ring / 8f;
                float radius = bloomScale * (0.12f + 0.32f * k);
                float a = (0.07f + 0.10f * chargeRatio) * k * k * strength * 1.5f / bloomAngleSteps;
                Color c = bloomTint * a;
                for (int ang = 0; ang < bloomAngleSteps; ang++)
                {
                    float rotation = ang * MathHelper.TwoPi / bloomAngleSteps;
                    Vector2 scale = new Vector2(radius * 2f, radius * 0.28f);
                    Main.spriteBatch.Draw(px, centerScreen, src, c, rotation, originCenter,
                        scale, SpriteEffects.None, 0f);
                }
            }

            DrawPurpleAnchorAdditiveCorona(px, centerScreen, src, anchorDiskRadius, chargeRatio, pulseU);

            DrawChargeSmokeRingInstances(
                centerScreen,
                anchorDiskRadius,
                chargeRatio,
                smokeRings,
                smokeRingCount,
                visibleElapsedTicks,
                projectileIdentity);

            DrawChargeCrossTextureUnderlay(centerScreen, halfArmCross, strength);

            DrawAxisAlignedCrossArms(px, centerScreen, src, halfArmCross, crossGlowVisualT, crossGlowStrength, barTipPurple);
            DrawAxisAlignedCrossArms(px, centerScreen, src, halfArmCross * 1.08f, crossGlowVisualT, crossGlowOuterStrength, new Color(215, 130, 255));

            DrawCrossEdgeGradientGlow(px, centerScreen, src, halfArmCross, crossGlowVisualT);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(
                SpriteSortMode.Deferred,
                BlendState.AlphaBlend,
                Main.DefaultSamplerState,
                DepthStencilState.None,
                Main.Rasterizer,
                null,
                Main.GameViewMatrix.TransformationMatrix);

            if (drawInwardWhiteLines)
                DrawChargeInwardWhiteLines(px, centerScreen, src, anchorDiskRadius, chargeRatio, visibleElapsedTicks, projectileIdentity);
        }

        internal static void DrawChargeCrossStarOnly(Vector2 centerScreen, float geometryScale = 1f, float opacity01 = 1f)
        {
            if (Main.dedServ)
                return;

            opacity01 = MathHelper.Clamp(opacity01, 0f, 1f);
            geometryScale = MathHelper.Clamp(geometryScale, 0.05f, 4f);

            const float chargeRatio = 1f;
            float crossEndPeakEnv = GetCrossChargeEndPeakEnvelope01(chargeRatio);
            float crossEndPeakSmooth = MathHelper.SmoothStep(0f, 1f, crossEndPeakEnv);
            float crossPulseU = MathHelper.Lerp(CrossChargeSizePulseMin, 1f, crossEndPeakEnv);
            float envelopeCross = MathHelper.Lerp(0.84f, 1f, crossEndPeakSmooth);
            float crossPulseUBumped = MathHelper.Clamp(
                crossPulseU + 0.065f * (1f - crossEndPeakEnv),
                CrossChargeSizePulseMin,
                1f);
            float halfArmCross = MathHelper.Lerp(38f, 108f, crossPulseUBumped) * envelopeCross * geometryScale;

            const float crossGlowVisualT = 1f;
            float crossGlowStrength = 1f * opacity01;
            float crossGlowOuterStrength = 0.28f * opacity01;
            Color barTipPurple = new Color(180, 80, 255);

            Texture2D px = TextureAssets.MagicPixel.Value;
            var src = new Rectangle(0, 0, 1, 1);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(
                SpriteSortMode.Deferred,
                BlendState.Additive,
                SamplerState.LinearClamp,
                DepthStencilState.None,
                RasterizerState.CullCounterClockwise,
                null,
                Main.GameViewMatrix.TransformationMatrix);

            DrawChargeCrossTextureUnderlay(centerScreen, halfArmCross, opacity01);

            DrawAxisAlignedCrossArms(px, centerScreen, src, halfArmCross, crossGlowVisualT, crossGlowStrength, barTipPurple);
            DrawAxisAlignedCrossArms(px, centerScreen, src, halfArmCross * 1.08f, crossGlowVisualT, crossGlowOuterStrength, new Color(215, 130, 255));
            DrawCrossEdgeGradientGlow(px, centerScreen, src, halfArmCross, crossGlowVisualT * opacity01);

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

        internal static void DrawNormalAttackMuzzleCrossOnly(
            Vector2 centerScreen,
            float geometryScale,
            float opacity01,
            float rotationRad)
        {
            if (Main.dedServ)
                return;

            opacity01 = MathHelper.Clamp(opacity01, 0f, 1f);
            geometryScale = MathHelper.Clamp(geometryScale, 0.08f, 2.5f);

            float halfArm = 40f * geometryScale;
            const float crossVisualT = 1f;
            Color tip = new Color(185, 75, 255);
            Color outer = new Color(230, 165, 255);

            Texture2D px = TextureAssets.MagicPixel.Value;
            var src = new Rectangle(0, 0, 1, 1);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(
                SpriteSortMode.Deferred,
                BlendState.Additive,
                SamplerState.LinearClamp,
                DepthStencilState.None,
                RasterizerState.CullCounterClockwise,
                null,
                Main.GameViewMatrix.TransformationMatrix);

            DrawRotatedStarArms(
                px,
                centerScreen,
                src,
                halfArm,
                crossVisualT,
                opacity01,
                tip,
                4,
                rotationRad,
                1.22f);
            DrawRotatedStarArms(
                px,
                centerScreen,
                src,
                halfArm * 1.1f,
                crossVisualT,
                opacity01 * 0.36f,
                outer,
                4,
                rotationRad,
                1.05f);

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

        private const float S3LockedHexStarSizeMul = 0.66f;

        private const float S3LockedHexStarArmSpanMul = 0.86f;

        private const float S3LockedHexStarArmThickCompMul = 1.12f;

        internal static void DrawS3LockedHitHexStarFlash(
            Vector2 centerScreen,
            float geometryScale,
            float opacity01,
            float rotationRad)
        {
            if (Main.dedServ)
                return;

            opacity01 = MathHelper.Clamp(opacity01, 0f, 1f);
            geometryScale = MathHelper.Clamp(geometryScale, 0.05f, 4f);

            const float chargeRatio = 1f;
            float crossEndPeakEnv = GetCrossChargeEndPeakEnvelope01(chargeRatio);
            float crossEndPeakSmooth = MathHelper.SmoothStep(0f, 1f, crossEndPeakEnv);
            float crossPulseU = MathHelper.Lerp(CrossChargeSizePulseMin, 1f, crossEndPeakEnv);
            float envelopeCross = MathHelper.Lerp(0.84f, 1f, crossEndPeakSmooth);
            float crossPulseUBumped = MathHelper.Clamp(
                crossPulseU + 0.065f * (1f - crossEndPeakEnv),
                CrossChargeSizePulseMin,
                1f);
            float halfArm = MathHelper.Lerp(38f, 108f, crossPulseUBumped) * envelopeCross * geometryScale * S3LockedHexStarSizeMul;
            float halfArmRadial = halfArm * S3LockedHexStarArmSpanMul;

            const float crossGlowVisualT = 1f;
            float crossGlowStrength = 1f * opacity01;
            float crossGlowOuterStrength = 0.28f * opacity01;
            Color barTipPurple = new Color(180, 80, 255);

            Texture2D px = TextureAssets.MagicPixel.Value;
            var src = new Rectangle(0, 0, 1, 1);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(
                SpriteSortMode.Deferred,
                BlendState.Additive,
                SamplerState.LinearClamp,
                DepthStencilState.None,
                RasterizerState.CullCounterClockwise,
                null,
                Main.GameViewMatrix.TransformationMatrix);

            const int hexArms = 6;
            const float armThickMul = 1.38f;
            float thickMul = armThickMul * S3LockedHexStarArmThickCompMul;

            DrawRotatedStarArms(
                px,
                centerScreen,
                src,
                halfArmRadial,
                crossGlowVisualT,
                crossGlowStrength,
                barTipPurple,
                hexArms,
                rotationRad,
                thickMul);
            DrawRotatedStarArms(
                px,
                centerScreen,
                src,
                halfArmRadial * 1.06f,
                crossGlowVisualT,
                crossGlowOuterStrength,
                new Color(215, 130, 255),
                hexArms,
                rotationRad,
                thickMul);
            DrawHexStarEdgeGradientGlow(px, centerScreen, src, halfArmRadial, crossGlowVisualT * opacity01, rotationRad, thickMul);

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

        private static void DrawRotatedStarArms(
            Texture2D px,
            Vector2 centerScreen,
            Rectangle src,
            float halfArm,
            float crossVisualT,
            float strength,
            Color tipPurple,
            int armCount,
            float baseRotation,
            float thicknessMul)
        {
            var origin = new Vector2(0.5f, 0.5f);
            float span = 2f * halfArm;
            int segments = Math.Clamp((int)Math.Round(span / 2.55f), 9, 16);
            const int thickSlices = 6;

            float armStep = MathHelper.TwoPi / armCount;

            for (int a = 0; a < armCount; a++)
            {
                float armAngle = baseRotation + a * armStep;
                Vector2 dir = armAngle.ToRotationVector2();
                float denom = Math.Max(halfArm, 0.001f);

                for (int s = 0; s < segments; s++)
                {
                    float t0 = s / (float)segments;
                    float t1 = (s + 1f) / segments;
                    float r0 = -halfArm + t0 * span;
                    float r1 = -halfArm + t1 * span;
                    float rMid = (r0 + r1) * 0.5f;
                    float segLen = Math.Max(Math.Abs(r1 - r0), 0.45f) * 1.04f;

                    float distFromCenter = Math.Abs(rMid) / denom;
                    distFromCenter = MathHelper.Clamp(distFromCenter, 0f, 1f);

                    float bell = (MathF.Cos(distFromCenter * MathF.PI) + 1f) * 0.5f;
                    bell = 0.06f + 0.94f * bell;

                    Vector2 segCenter = centerScreen + dir * rMid;

                    Color cap = SampleArmColor(distFromCenter, crossVisualT, tipPurple) * strength;

                    float baseThick = MathHelper.Lerp(5f, 18f, crossVisualT) * (0.7f + 0.3f * strength)
                        * CrossArmCenterThicknessMul * thicknessMul;
                    float coreThick = baseThick * bell;
                    float totalThickness = coreThick * 3.35f;

                    for (int i = 0; i < thickSlices; i++)
                    {
                        float yNorm = (i + 0.5f) / thickSlices - 0.5f;
                        float gauss = MathF.Exp(-8.5f * yNorm * yNorm);
                        if (gauss < 0.02f)
                            continue;

                        float yOff = yNorm * totalThickness;
                        float sliceH = Math.Max((totalThickness / thickSlices) * 1.45f, 0.55f);
                        float layerA = 0.18f + 0.82f * gauss;

                        Vector2 perp = new Vector2(-dir.Y, dir.X);
                        Vector2 pos = segCenter + perp * yOff;

                        Main.spriteBatch.Draw(
                            px,
                            pos,
                            src,
                            cap * layerA * 1.2f,
                            armAngle,
                            origin,
                            new Vector2(segLen, sliceH),
                            SpriteEffects.None,
                            0f);
                    }
                }
            }
        }

        private static void DrawHexStarEdgeGradientGlow(
            Texture2D px,
            Vector2 centerScreen,
            Rectangle src,
            float halfArm,
            float rimEnvelopeT,
            float baseRotation,
            float thicknessMul)
        {
            var origin = new Vector2(0.5f, 0.5f);
            float span = 2f * halfArm;
            int segments = Math.Clamp((int)Math.Round(span / 2.55f), 9, 16);
            float denom = Math.Max(halfArm, 0.001f);
            const int rimSteps = 7;
            const int hexArms = 6;
            float armStep = MathHelper.TwoPi / hexArms;

            rimEnvelopeT = MathHelper.Clamp(rimEnvelopeT, 0f, 1f);
            float waveSmooth = MathHelper.SmoothStep(0f, 1f, rimEnvelopeT);
            float rimPulse = MathHelper.Lerp(0.32f, 1f, waveSmooth);
            rimPulse *= MathHelper.Lerp(0.68f, 1f, rimEnvelopeT);

            for (int a = 0; a < hexArms; a++)
            {
                float armAngle = baseRotation + a * armStep;
                Vector2 dir = armAngle.ToRotationVector2();

                for (int s = 0; s < segments; s++)
                {
                    float t0 = s / (float)segments;
                    float t1 = (s + 1f) / segments;
                    float r0 = -halfArm + t0 * span;
                    float r1 = -halfArm + t1 * span;
                    float rMid = (r0 + r1) * 0.5f;
                    float segLen = Math.Max(Math.Abs(r1 - r0), 0.45f) * 1.04f;

                    float distFromCenter = MathHelper.Clamp(Math.Abs(rMid) / denom, 0f, 1f);

                    float bell = (MathF.Cos(distFromCenter * MathF.PI) + 1f) * 0.5f;
                    bell = 0.06f + 0.94f * bell;

                    float thickRef = MathHelper.Lerp(5f, 18f, rimEnvelopeT) * CrossArmCenterThicknessMul * thicknessMul;
                    float thickOuter = thickRef * bell * 2.1f;
                    float baseOffset = thickOuter * 0.35f + 0.65f + 0.45f * (1f - bell);

                    Color grad = Color.Lerp(RimGlowPink, RimGlowPurple, MathHelper.SmoothStep(0f, 1f, distFromCenter));
                    float tipFade = 1f - MathHelper.SmoothStep(0.08f, 0.99f, distFromCenter);
                    float baseAlpha = (0.088f + 0.11f * rimEnvelopeT) * rimPulse * tipFade;

                    Vector2 segCenter = centerScreen + dir * rMid;

                    float lenPad = segLen + 1.2f;
                    Vector2 perp = new Vector2(-dir.Y, dir.X);

                    for (int sideSign = -1; sideSign <= 1; sideSign += 2)
                    {
                        for (int step = 0; step < rimSteps; step++)
                        {
                            float u = (step + 0.5f) / rimSteps;
                            float d = baseOffset + u * 5.8f;
                            float fall = MathF.Exp(-3.2f * u * u);
                            if (fall < 0.03f)
                                continue;

                            float rimH = MathHelper.Lerp(1.55f, 2.85f, rimEnvelopeT)
                                * (0.42f + 0.58f * waveSmooth)
                                * (0.5f + 0.5f * fall)
                                * thicknessMul;
                            float alpha = baseAlpha * fall * (0.45f + 0.55f * (1f - u));

                            Color c = grad * alpha;
                            float off = d * sideSign;
                            Vector2 pos = segCenter + perp * off;

                            Main.spriteBatch.Draw(
                                px,
                                pos,
                                src,
                                c,
                                armAngle,
                                origin,
                                new Vector2(lenPad, rimH),
                                SpriteEffects.None,
                                0f);
                        }
                    }
                }
            }
        }

        private static void DrawChargeCrossTextureUnderlay(Vector2 centerScreen, float halfArmCross, float opacityMul)
        {
            if (Main.dedServ || ChargeCrossTexture == null)
                return;

            opacityMul = MathHelper.Clamp(opacityMul, 0f, 2f);
            if (opacityMul <= 0f)
                return;

            Texture2D tex = ChargeCrossTexture;
            float armSpan = halfArmCross * 2f;
            float texMaxDim = Math.Max(tex.Width, tex.Height);
            float baseScale = armSpan / Math.Max(texMaxDim, 1f) * 0.92f;

            int iw = tex.Width;
            int ih = tex.Height;
            float imx = iw * 0.5f;
            float imy = ih * 0.5f;
            int strip = Math.Max(
                2,
                (int)Math.Round(Math.Min(iw, ih) * CrossTexCardinalMaskHalfFracOfMinSide));

            float[] glowScales = { 1.18f, 1.11f, 1.05f };
            float[] glowAlphas = { 0.11f, 0.16f, 0.14f };

            for (int i = 0; i < glowScales.Length; i++)
            {
                float s = baseScale * glowScales[i];
                DrawChargeCrossTextureMaskedCorners(
                    tex,
                    centerScreen,
                    imx,
                    imy,
                    iw,
                    ih,
                    strip,
                    s,
                    ChargeCrossGlowColor * (glowAlphas[i] * opacityMul));
            }

            DrawChargeCrossTextureMaskedCorners(
                tex,
                centerScreen,
                imx,
                imy,
                iw,
                ih,
                strip,
                baseScale,
                ChargeCrossGlowColor * (0.68f * opacityMul));
        }

        private static void DrawChargeCrossTextureMaskedCorners(
            Texture2D tex,
            Vector2 centerScreen,
            float imx,
            float imy,
            int iw,
            int ih,
            int strip,
            float uniformScale,
            Color tint)
        {
            int tlW = (int)Math.Round(imx - strip);
            int tlH = (int)Math.Round(imy - strip);
            if (tlW < 2 || tlH < 2)
            {
                Main.spriteBatch.Draw(
                    tex,
                    centerScreen,
                    null,
                    tint,
                    0f,
                    new Vector2(imx, imy),
                    uniformScale,
                    SpriteEffects.None,
                    0f);
                return;
            }

            int trX = (int)Math.Round(imx + strip);
            int trW = iw - trX;
            int blY = (int)Math.Round(imy + strip);
            int blH = ih - blY;

            void DrawCorner(Rectangle src, Vector2 originInSrc)
            {
                if (src.Width < 1 || src.Height < 1)
                    return;

                float cx = src.X + originInSrc.X;
                float cy = src.Y + originInSrc.Y;
                Vector2 pos = centerScreen + new Vector2(cx - imx, cy - imy) * uniformScale;

                Main.spriteBatch.Draw(
                    tex,
                    pos,
                    src,
                    tint,
                    0f,
                    originInSrc,
                    uniformScale,
                    SpriteEffects.None,
                    0f);
            }

            DrawCorner(
                new Rectangle(0, 0, tlW, tlH),
                new Vector2(tlW * 0.5f, tlH * 0.5f));

            DrawCorner(
                new Rectangle(trX, 0, trW, tlH),
                new Vector2(trW * 0.5f, tlH * 0.5f));

            DrawCorner(
                new Rectangle(0, blY, tlW, blH),
                new Vector2(tlW * 0.5f, blH * 0.5f));

            DrawCorner(
                new Rectangle(trX, blY, trW, blH),
                new Vector2(trW * 0.5f, blH * 0.5f));
        }

        private static void DrawPurpleAnchorDiskAlphaBase(
            Texture2D px,
            Vector2 centerScreen,
            Rectangle src,
            float radius,
            float chargeRatio)
        {
            var originCenter = new Vector2(0.5f, 0.5f);
            Color rgb = new Color(155, 85, 235);
            const float anchorDiskOpacityMul = 0.68f;
            float visibility = MathHelper.Clamp((0.48f + 0.52f * chargeRatio) * anchorDiskOpacityMul, 0f, 1f);

            const int angleSteps = 40;
            const int radialLayers = 11;

            for (int layer = 0; layer < radialLayers; layer++)
            {
                float lt = layer / (float)Math.Max(1, radialLayers - 1);
                float layerRadius = radius * MathHelper.Lerp(1.05f, 0.56f, lt);
                float outerFade = MathF.Pow(1f - MathHelper.SmoothStep(0.18f, 0.96f, lt), 1.45f);
                float stripThickness = layerRadius * MathHelper.Lerp(0.34f, 0.095f, lt);

                float u = layerRadius / Math.Max(radius, 1e-3f);
                float rimSuppress = u <= 1.0f
                    ? 1f
                    : MathF.Pow(
                        MathHelper.Clamp((1.062f - u) / 0.062f, 0f, 1f),
                        3.85f);

                float alphaPerStrip =
                    (0.048f + 0.042f * (1f - lt)) * outerFade * visibility * rimSuppress;

                if (alphaPerStrip < 0.003f)
                    continue;

                for (int ang = 0; ang < angleSteps; ang++)
                {
                    float rotation = ang * MathHelper.TwoPi / angleSteps;
                    Vector2 scale = new Vector2(layerRadius * 2.12f, stripThickness);
                    Main.spriteBatch.Draw(
                        px,
                        centerScreen,
                        src,
                        rgb * MathHelper.Clamp(alphaPerStrip, 0f, 1f),
                        rotation,
                        originCenter,
                        scale,
                        SpriteEffects.None,
                        0f);
                }
            }
        }

        private static void DrawPurpleAnchorAdditiveCorona(
            Texture2D px,
            Vector2 centerScreen,
            Rectangle src,
            float radius,
            float chargeRatio,
            float pulseU)
        {
            var originCenter = new Vector2(0.5f, 0.5f);
            Color rgb = new Color(175, 95, 255);
            float vis = (0.28f + 0.72f * MathHelper.Clamp((pulseU - CrossChargeSizePulseMin) / Math.Max(1f - CrossChargeSizePulseMin, 0.01f), 0f, 1f))
                * (0.5f + 0.5f * chargeRatio);

            const int angleSteps = 38;
            const int radialLayers = 5;

            for (int layer = 0; layer < radialLayers; layer++)
            {
                float lt = layer / (float)Math.Max(1, radialLayers - 1);
                float layerRadius = radius * MathHelper.Lerp(1.02f, 1.21f, lt);
                float outerFade = MathF.Pow(1f - MathHelper.SmoothStep(0.28f, 0.995f, lt), 1.75f);
                float stripThickness = layerRadius * MathHelper.Lerp(0.11f, 0.052f, lt);
                float u = layerRadius / Math.Max(radius, 1e-3f);
                float coronaFeatherOut = MathHelper.Clamp((1.195f - u) / 0.11f, 0f, 1f);
                coronaFeatherOut = MathF.Pow(coronaFeatherOut, 3.1f);

                float alphaPerStrip =
                    (0.022f + 0.012f * (1f - lt)) * outerFade * vis * coronaFeatherOut;

                if (alphaPerStrip < 0.003f)
                    continue;

                for (int ang = 0; ang < angleSteps; ang++)
                {
                    float rotation = ang * MathHelper.TwoPi / angleSteps;
                    Vector2 scale = new Vector2(layerRadius * 2.04f, stripThickness);
                    Main.spriteBatch.Draw(
                        px,
                        centerScreen,
                        src,
                        rgb * MathHelper.Clamp(alphaPerStrip, 0f, 1f),
                        rotation,
                        originCenter,
                        scale,
                        SpriteEffects.None,
                        0f);
                }
            }
        }

        private static void DrawChargeInwardWhiteLines(
            Texture2D px,
            Vector2 centerScreen,
            Rectangle src,
            float anchorDiskRadius,
            float chargeRatio,
            int visibleElapsedTicks,
            int projectileIdentity)
        {
            var origin = new Vector2(0.5f, 0.5f);
            float outer = anchorDiskRadius * (2.35f + 0.35f * MathF.Sin(visibleElapsedTicks * 0.07f));
            int lineCount = 4 + (MixHash(projectileIdentity, visibleElapsedTicks, 0x51EC, 0) % 6);

            float gate = MathHelper.Clamp(0.35f + 0.65f * chargeRatio, 0f, 1f);

            for (int i = 0; i < lineCount; i++)
            {
                int h = MixHash(projectileIdentity, visibleElapsedTicks, i, 0xBEEF);
                float theta = ((h & 0xFFFFFF) / (float)0x1000000) * MathHelper.TwoPi + visibleElapsedTicks * 0.018f;
                float rNorm = 0.5f + ((h >> 24) & 255) / 255f * 0.5f;
                float rStart = outer * rNorm;
                float lineLen = 8f + (h % 55);

                int period = 56 + (i * 3 + ((h >> 16) & 7)) % 24;
                float phase = ((visibleElapsedTicks + (h & 15)) % period) / (float)period;
                float headR = rStart - lineLen * phase;
                float tailR = headR + lineLen;
                if (tailR < anchorDiskRadius * 0.12f)
                    continue;

                Vector2 radial = theta.ToRotationVector2();
                Vector2 pHead = centerScreen + radial * headR;
                Vector2 pTail = centerScreen + radial * tailR;
                Vector2 mid = (pHead + pTail) * 0.5f;
                float dist = Vector2.Distance(pHead, pTail);
                if (dist < 0.5f)
                    continue;

                float thickness = 0.95f + (i % 4) * 0.28f + ((h >> 8) & 3) * 0.15f;
                float a = (0.11f + 0.09f * gate) * (0.55f + 0.45f * (1f - phase));

                Main.spriteBatch.Draw(
                    px,
                    mid,
                    src,
                    Color.White * MathHelper.Clamp(a, 0f, 1f),
                    theta,
                    origin,
                    new Vector2(dist, thickness),
                    SpriteEffects.None,
                    0f);
            }
        }

        private static float CrossChargeWave01(float t)
        {
            t = MathHelper.Clamp(t, 0f, 1f);
            return (1f - MathF.Cos(4f * MathF.PI * t)) * 0.5f;
        }

        public static float GetChargeSizePulse01(float t)
        {
            float wave = CrossChargeWave01(t);
            return MathHelper.Lerp(CrossChargeSizePulseMin, 1f, wave);
        }

        public static float GetCrossChargeEndPeakEnvelope01(float chargeRatio)
        {
            chargeRatio = MathHelper.Clamp(chargeRatio, 0f, 1f);
            return (1f + MathF.Cos(2f * MathF.PI * chargeRatio)) * 0.5f;
        }

        private static Color SampleArmColor(float u, float brightnessT, Color tipPurple)
        {
            u = MathHelper.Clamp(u, 0f, 1f);
            brightnessT = MathHelper.Clamp(brightnessT, 0f, 1f);
            Color along = Color.Lerp(Color.White, tipPurple, MathHelper.SmoothStep(0f, 1f, u * 0.92f));
            Color lavender = new Color(235, 215, 255);
            float t2 = MathHelper.SmoothStep(0.35f, 1f, u);
            along = Color.Lerp(along, lavender, t2 * 0.85f);
            float tipAlpha = 1f - MathHelper.SmoothStep(0.48f, 1f, u);
            tipAlpha *= 1f - MathHelper.SmoothStep(0.85f, 1f, u);
            float intensity = (0.72f + 0.28f * brightnessT) * (0.52f + 0.48f * tipAlpha);
            intensity = Math.Max(intensity, 0.38f);
            return along * intensity;
        }

        private static void DrawAxisAlignedCrossArms(
            Texture2D px,
            Vector2 centerScreen,
            Rectangle src,
            float halfArm,
            float crossVisualT,
            float strength,
            Color tipPurple)
        {
            var origin = new Vector2(0.5f, 0.5f);
            float span = 2f * halfArm;
            int segments = Math.Max(28, (int)Math.Round(span / 1.15f));
            const int thickSlices = 17;

            void DrawOneAxis(bool horizontal)
            {
                float denom = Math.Max(halfArm, 0.001f);

                for (int s = 0; s < segments; s++)
                {
                    float t0 = s / (float)segments;
                    float t1 = (s + 1f) / segments;
                    float r0 = -halfArm + t0 * span;
                    float r1 = -halfArm + t1 * span;
                    float rMid = (r0 + r1) * 0.5f;
                    float segLen = Math.Max(Math.Abs(r1 - r0), 0.45f) * 1.04f;

                    float distFromCenter = Math.Abs(rMid) / denom;
                    distFromCenter = MathHelper.Clamp(distFromCenter, 0f, 1f);

                    float bell = (MathF.Cos(distFromCenter * MathF.PI) + 1f) * 0.5f;
                    bell = 0.06f + 0.94f * bell;

                    Vector2 segCenter = horizontal
                        ? centerScreen + new Vector2(rMid, 0f)
                        : centerScreen + new Vector2(0f, rMid);

                    Color cap = SampleArmColor(distFromCenter, crossVisualT, tipPurple) * strength;

                    float baseThick = MathHelper.Lerp(5f, 18f, crossVisualT) * (0.7f + 0.3f * strength) * CrossArmCenterThicknessMul;
                    float coreThick = baseThick * bell;
                    float totalThickness = coreThick * 3.2f;

                    for (int i = 0; i < thickSlices; i++)
                    {
                        float yNorm = (i + 0.5f) / thickSlices - 0.5f;
                        float gauss = MathF.Exp(-10f * yNorm * yNorm);
                        if (gauss < 0.014f)
                            continue;

                        float yOff = yNorm * totalThickness;
                        float sliceH = Math.Max((totalThickness / thickSlices) * 1.38f, 0.5f);
                        float layerA = 0.16f + 0.84f * gauss;

                        Vector2 pos = horizontal
                            ? segCenter + new Vector2(0f, yOff)
                            : segCenter + new Vector2(yOff, 0f);
                        Vector2 scale = horizontal
                            ? new Vector2(segLen, sliceH)
                            : new Vector2(sliceH, segLen);

                        Main.spriteBatch.Draw(
                            px,
                            pos,
                            src,
                            cap * layerA * 1.18f,
                            0f,
                            origin,
                            scale,
                            SpriteEffects.None,
                            0f);
                    }
                }
            }

            DrawOneAxis(horizontal: true);
            DrawOneAxis(horizontal: false);
        }

        private static readonly Color RimGlowPink = new Color(255, 189, 255);
        private static readonly Color RimGlowPurple = new Color(138, 97, 247);

        private static void DrawCrossEdgeGradientGlow(
            Texture2D px,
            Vector2 centerScreen,
            Rectangle src,
            float halfArm,
            float rimEnvelopeT)
        {
            var origin = new Vector2(0.5f, 0.5f);
            float span = 2f * halfArm;
            int segments = Math.Max(32, (int)Math.Round(span / 1.05f));
            float denom = Math.Max(halfArm, 0.001f);
            const int rimSteps = 26;

            rimEnvelopeT = MathHelper.Clamp(rimEnvelopeT, 0f, 1f);
            float waveSmooth = MathHelper.SmoothStep(0f, 1f, rimEnvelopeT);
            float rimPulse = MathHelper.Lerp(0.32f, 1f, waveSmooth);
            rimPulse *= MathHelper.Lerp(0.68f, 1f, rimEnvelopeT);

            void DrawOneAxis(bool horizontal)
            {
                for (int s = 0; s < segments; s++)
                {
                    float t0 = s / (float)segments;
                    float t1 = (s + 1f) / segments;
                    float r0 = -halfArm + t0 * span;
                    float r1 = -halfArm + t1 * span;
                    float rMid = (r0 + r1) * 0.5f;
                    float segLen = Math.Max(Math.Abs(r1 - r0), 0.45f) * 1.04f;

                    float distFromCenter = MathHelper.Clamp(Math.Abs(rMid) / denom, 0f, 1f);

                    float bell = (MathF.Cos(distFromCenter * MathF.PI) + 1f) * 0.5f;
                    bell = 0.06f + 0.94f * bell;

                    float thickRef = MathHelper.Lerp(5f, 18f, rimEnvelopeT) * CrossArmCenterThicknessMul;
                    float thickOuter = thickRef * bell * 2.1f;
                    float baseOffset = thickOuter * 0.35f + 0.65f + 0.45f * (1f - bell);

                    Color grad = Color.Lerp(RimGlowPink, RimGlowPurple, MathHelper.SmoothStep(0f, 1f, distFromCenter));
                    float tipFade = 1f - MathHelper.SmoothStep(0.08f, 0.99f, distFromCenter);
                    float baseAlpha = (0.072f + 0.09f * rimEnvelopeT) * rimPulse * tipFade;

                    Vector2 segCenter = horizontal
                        ? centerScreen + new Vector2(rMid, 0f)
                        : centerScreen + new Vector2(0f, rMid);

                    float lenPad = segLen + 1.2f;

                    for (int sideSign = -1; sideSign <= 1; sideSign += 2)
                    {
                        for (int step = 0; step < rimSteps; step++)
                        {
                            float u = (step + 0.5f) / rimSteps;
                            float d = baseOffset + u * 6.5f;
                            float fall = MathF.Exp(-3.4f * u * u);
                            if (fall < 0.02f)
                                continue;

                            float rimH = MathHelper.Lerp(1.4f, 2.6f, rimEnvelopeT)
                                * (0.42f + 0.58f * waveSmooth)
                                * (0.5f + 0.5f * fall);
                            float a = baseAlpha * fall * (0.4f + 0.6f * (1f - u));

                            Color c = grad * a;
                            float off = d * sideSign;

                            if (horizontal)
                            {
                                Main.spriteBatch.Draw(px, segCenter + new Vector2(0f, off), src, c, 0f, origin,
                                    new Vector2(lenPad, rimH), SpriteEffects.None, 0f);
                            }
                            else
                            {
                                Main.spriteBatch.Draw(px, segCenter + new Vector2(off, 0f), src, c, 0f, origin,
                                    new Vector2(rimH, lenPad), SpriteEffects.None, 0f);
                            }
                        }
                    }
                }
            }

            DrawOneAxis(true);
            DrawOneAxis(false);
        }

        private static void BuildSmokeRingSegmentThetaBounds(
            in ChargeSmokeRingInstance slot,
            int ringBufferIndex,
            Span<float> segThetaStart,
            Span<float> segThetaEnd,
            out float ringSpin)
        {
            ringSpin = Main.GlobalTimeWrappedHourly * 0.12f + ringBufferIndex * 0.031f;
            int n = slot.SegmentCount;
            if (n <= 0)
                return;

            float thetaBase = ringSpin - MathHelper.PiOver2;
            float segAngle = MathHelper.TwoPi / n;

            for (int i = 0; i < n; i++)
            {
                segThetaStart[i] = thetaBase + i * segAngle;
                segThetaEnd[i] = thetaBase + (i + 1) * segAngle;
            }
        }

        private static void DrawChargeSmokeRingInstances(
            Vector2 centerScreen,
            float anchorDiskRadius,
            float chargeRatio,
            ChargeSmokeRingInstance[] rings,
            int ringCount,
            int visibleElapsedTicks,
            int projectileIdentity)
        {
            if (SmokeTextures == null || rings == null || ringCount <= 0)
                return;

            float fadeInCharge = MathHelper.SmoothStep(0f, 1f, chargeRatio / 0.09f);
            float fadeOutCharge = MathHelper.SmoothStep(1f, 0f, MathHelper.Clamp((chargeRatio - 0.91f) / 0.09f, 0f, 1f));
            float chargeGate = fadeInCharge * fadeOutCharge;

            const float sliceWorldPad = 1.045f;
            Color baseLavender = new Color(218, 206, 248);
            Color chargeRingBodyRgb = Color.Lerp(baseLavender, Color.White, SmokeRingChargeWhiteMixToWhite);

            Span<float> segThetaStart = stackalloc float[SmokeRingSegmentCount];
            Span<float> segThetaEnd = stackalloc float[SmokeRingSegmentCount];

            for (int r = 0; r < ringCount; r++)
            {
                ChargeSmokeRingInstance slot = rings[r];
                if (slot.SegmentCount < SmokeRingSegmentCount)
                    continue;

                float ageSec = (visibleElapsedTicks - slot.SpawnVisibleTick) / 60f;
                if (ageSec < 0f || ageSec > SmokeRingLifetimeSeconds + 0.05f)
                    continue;

                float age01 = MathHelper.Clamp(ageSec / Math.Max(SmokeRingLifetimeSeconds, 0.05f), 0f, 1f);
                float ringFadeIn = MathHelper.SmoothStep(0f, 1f, ageSec / 0.06f);
                float ringFadeOut = MathHelper.SmoothStep(1f, 0f, MathHelper.Clamp((ageSec - SmokeRingLifetimeSeconds + 0.12f) / 0.14f, 0f, 1f));
                float ringLife = ringFadeIn * ringFadeOut;

                float visibility = chargeGate * ringLife;
                if (visibility < 0.02f)
                    continue;

                float shrinkT = MathHelper.SmoothStep(0f, 1f, age01);
                float ringRadius = MathHelper.Lerp(anchorDiskRadius * 1.42f, anchorDiskRadius * 0.38f, shrinkT);

                int segCount = slot.SegmentCount;
                BuildSmokeRingSegmentThetaBounds(slot, r, segThetaStart, segThetaEnd, out _);

                float chargeSec = visibleElapsedTicks / 60f;
                float pulsePeriod = SmokeRingChargePulseBrightToDimSeconds * 2f;
                float pulsePhase =
                    MathHelper.TwoPi * chargeSec / Math.Max(pulsePeriod, 1e-4f)
                    + r * 0.71f
                    + (slot.LayoutSalt & 4095) * (MathHelper.TwoPi / 4096f) * 0.35f;
                float waveToDim = 0.5f - 0.5f * MathF.Cos(pulsePhase);
                float pulseMul = MathHelper.Lerp(SmokeRingChargePulseDimFrac, 1f, 1f - waveToDim);

                Color tintBase = chargeRingBodyRgb * (visibility * 0.56f * pulseMul);

                for (int seg = 0; seg < segCount; seg++)
                {
                    int texIdx = slot.GetAssetIndex(seg);
                    if (texIdx < 0 || texIdx >= SmokeTextures.Length)
                        continue;

                    Texture2D tex = SmokeTextures[texIdx];
                    if (tex == null)
                        continue;

                    float thetaSegStart = segThetaStart[seg];
                    float thetaSegEnd = segThetaEnd[seg];
                    float segSpan = thetaSegEnd - thetaSegStart;
                    if (segSpan < 0.015f)
                        continue;

                    int tw = Math.Max(tex.Width, 1);
                    int th = Math.Max(tex.Height, 1);
                    float spanNorm = MathHelper.Clamp(segSpan / (MathHelper.TwoPi * 0.28f), 0.52f, 1.58f);
                    int sliceBase = Math.Clamp(Math.Max(SmokeRingArcSlicesMin, (tw * 2 + 2) / 3), SmokeRingArcSlicesMin, SmokeRingArcSlicesMax);
                    int sliceCount = Math.Clamp((int)Math.Round(sliceBase * spanNorm), SmokeRingArcSlicesMin, SmokeRingArcSlicesMax);

                    float scaleY = (anchorDiskRadius * 0.62f) / th * (0.82f + 0.18f * visibility);

                    for (int k = 0; k < sliceCount; k++)
                    {
                        float u0 = k / (float)sliceCount;
                        float u1 = (k + 1f) / sliceCount;
                        float uMid = (u0 + u1) * 0.5f;
                        float sliceFeather = 0.91f + 0.09f * MathF.Sin(uMid * MathF.PI);
                        float tMid = MathHelper.Lerp(thetaSegStart, thetaSegEnd, uMid);

                        int sx = (int)(tw * u0);
                        int sxNext = k >= sliceCount - 1 ? tw : (int)(tw * u1);
                        int sw = Math.Max(1, sxNext - sx);
                        if (sx >= tw)
                            continue;
                        if (sx + sw > tw)
                            sw = tw - sx;

                        var sliceSrc = new Rectangle(sx, 0, sw, th);

                        Vector2 pos = centerScreen + SmokeRingCurveOffset(tMid, ringRadius);
                        Vector2 toCenter = centerScreen - pos;
                        float toCenterLen = toCenter.Length();
                        Vector2 radialIn = toCenterLen > 2f ? toCenter / toCenterLen : Vector2.Zero;

                        float warpAng = MathF.Sin(tMid * 5.11f + r * 1.37f + (slot.LayoutSalt & 8191) * 0.0017f);
                        float warpU = MathF.Sin(uMid * MathHelper.TwoPi * 2.63f + tMid * 3.09f + projectileIdentity * 0.0029f + seg * 0.91f);
                        float warpSeg = MathF.Cos(seg * 2.17f + tMid * 1.83f + ringRadius * 0.018f);
                        float ang01 = 0.5f + 0.5f * warpAng;
                        float u01 = 0.5f + 0.5f * warpU;
                        float seg01 = 0.5f + 0.5f * warpSeg;
                        float warpComb = MathHelper.Clamp(0.22f + 0.78f * ang01 * u01 * seg01, 0.14f, 1f);

                        float inwardPull = SmokeRingTexWarpInwardPx * warpComb * visibility * (0.52f + 0.48f * sliceFeather);
                        Vector2 drawPos = pos + radialIn * inwardPull;

                        float radialShrink = MathHelper.Lerp(SmokeRingTexWarpRadialScaleMin, 1f, warpComb);
                        float tangentPinch = 1f - SmokeRingTexWarpTangentPinchMax * (1f - warpComb);

                        float sliceArc = segSpan / sliceCount;
                        Vector2 tan = SmokeRingCurveTangent(tMid, ringRadius);
                        float tanLen = tan.Length();
                        if (tanLen < 0.001f)
                            tan = new Vector2(-MathF.Sin(tMid), MathF.Cos(tMid)) * ringRadius;
                        float worldW = tanLen * sliceArc * sliceWorldPad * SmokeRingSliceOverlapMul;
                        float scaleX = worldW / Math.Max(sw, 1) * tangentPinch;
                        float scaleYWarped = scaleY * radialShrink;

                        float rot = MathF.Atan2(tan.Y, tan.X);
                        var originSlice = new Vector2(sw * 0.5f, th * 0.5f);

                        Color tint = tintBase * sliceFeather;

                        Main.spriteBatch.Draw(
                            tex,
                            drawPos,
                            sliceSrc,
                            tint,
                            rot,
                            originSlice,
                            new Vector2(scaleX, scaleYWarped),
                            SpriteEffects.None,
                            0f);

                        Color glowCol = new Color(255, 252, 255)
                            * (sliceFeather * visibility * SmokeRingSliceGlowTintMul * SmokeRingChargeWhiteGlowBoost * pulseMul);
                        Main.spriteBatch.Draw(
                            tex,
                            drawPos,
                            sliceSrc,
                            glowCol,
                            rot,
                            originSlice,
                            new Vector2(scaleX * 1.09f, scaleYWarped * 1.06f),
                            SpriteEffects.None,
                            0f);
                    }
                }
            }
        }
    }
}

