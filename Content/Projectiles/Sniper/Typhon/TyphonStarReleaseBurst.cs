using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using System.Collections.Generic;

namespace ArknightsMod.Content.Projectiles.Sniper.Typhon
{
    public class TyphonStarReleaseBurst : ModProjectile
    {
        public const int LifetimeTicks = 42;

        private const int ShrinkEndExclusive = 10;
        private const int FlashEndExclusive = 13;
        private const int RingStartInclusive = 13;

        private const float FlatRingMajorRadiusMin = 14f;
        private const float FlatRingMajorRadiusMax = 198f;

        private const float FlatRingBasisTiltRadians = 0.52f;

        private readonly TyphonS3StarChargeEffects.ChargeSmokeRingInstance[] _ringSnap =
            new TyphonS3StarChargeEffects.ChargeSmokeRingInstance[TyphonS3StarChargeEffects.SmokeRingMaxConcurrent];

        private int _ringSnapCount;
        private float _fireAngle;
        private bool _chargeDustSpawned;

        public override string Texture => ArknightsMod.noTexture;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.DrawScreenCheckFluff[Type] = 800;
        }

        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
            overPlayers.Add(index);
        }

        public override void SetDefaults()
        {
            Projectile.width = 2;
            Projectile.height = 2;
            Projectile.aiStyle = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.penetrate = -1;
            Projectile.timeLeft = LifetimeTicks;
            Projectile.friendly = false;
            Projectile.hostile = false;
            Projectile.damage = 0;
            Projectile.hide = true;
            Projectile.netImportant = false;
        }

        public override void AI()
        {
            Projectile.velocity = Vector2.Zero;

            int elapsed = LifetimeTicks - Projectile.timeLeft;
            Vector2 dir = Vector2.UnitX.RotatedBy(_fireAngle);

            if (!Main.dedServ && !_chargeDustSpawned && elapsed >= ShrinkEndExclusive)
            {
                _chargeDustSpawned = true;
                if (Main.myPlayer == Projectile.owner)
                    SpawnReleaseBurstDust(Projectile.Center, dir);
            }
        }

        internal void ApplySmokeRingSnapshot(TyphonS3StarChargeEffects.ChargeSmokeRingInstance[] src, int count)
        {
            _ringSnapCount = Math.Min(Math.Max(count, 0), _ringSnap.Length);
            if (src == null || _ringSnapCount <= 0)
            {
                _ringSnapCount = 0;
                return;
            }

            for (int i = 0; i < _ringSnapCount; i++)
                _ringSnap[i] = src[i];
        }

        public override void OnSpawn(IEntitySource source)
        {
            Vector2 xy = new Vector2(Projectile.ai[0], Projectile.ai[1]);
            _fireAngle = xy.LengthSquared() > 1e-10f ? xy.ToRotation() : Projectile.ai[2];
            _chargeDustSpawned = false;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            if (Main.dedServ)
                return false;

            Texture2D px = TextureAssets.MagicPixel.Value;
            var src = new Rectangle(0, 0, 1, 1);
            Vector2 centerScreen = Projectile.Center - Main.screenPosition;
            int elapsed = LifetimeTicks - Projectile.timeLeft;

            Vector2 fireDir = Vector2.UnitX.RotatedBy(_fireAngle);
            int facingSign = 1;
            if (Projectile.owner >= 0 && Projectile.owner < Main.maxPlayers)
            {
                int d = Main.player[Projectile.owner].direction;
                if (d != 0)
                    facingSign = d;
            }

            if (elapsed < ShrinkEndExclusive)
            {
                float shrinkT = ShrinkEndExclusive <= 1
                    ? 1f
                    : MathHelper.SmoothStep(0f, 1f, elapsed / (float)(ShrinkEndExclusive - 1));
                float geom = MathHelper.Lerp(1f, 0.12f, shrinkT);

                TyphonS3StarChargeEffects.DrawChargePhase(
                    centerScreen,
                    1f,
                    1f,
                    px,
                    src,
                    _ringSnap,
                    _ringSnapCount,
                    2048,
                    Projectile.identity,
                    geom,
                    drawInwardWhiteLines: false);

                if (elapsed == ShrinkEndExclusive - 1)
                    DrawWhiteFlashCore(px, src, centerScreen, 0.55f);
            }
            else if (elapsed < FlashEndExclusive)
            {
                float peak = elapsed == ShrinkEndExclusive ? 1f : elapsed == ShrinkEndExclusive + 1 ? 0.92f : 0.55f;
                DrawWhiteFlashCore(px, src, centerScreen, peak);

                float ringEarly = (elapsed - ShrinkEndExclusive) / (float)Math.Max(1, FlashEndExclusive - ShrinkEndExclusive);
                DrawFlatEllipseRing(px, src, centerScreen, fireDir, facingSign, ringEarly * 0.22f, 0.85f);
            }
            else
            {
                float denom = Math.Max(1, LifetimeTicks - RingStartInclusive);
                float prog = (elapsed - RingStartInclusive) / denom;
                prog = MathHelper.Clamp(prog, 0f, 1f);
                float fade = 1f - MathHelper.SmoothStep(0f, 1f, prog);
                DrawFlatEllipseRing(px, src, centerScreen, fireDir, facingSign, prog, fade);
            }

            return false;
        }

        private static void DrawWhiteFlashCore(Texture2D px, Rectangle src, Vector2 centerScreen, float intensity)
        {
            intensity = MathHelper.Clamp(intensity, 0f, 1f);
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

            for (int i = 0; i < 10; i++)
            {
                float k = i / 9f;
                float r = 38f + k * 118f;
                float a = (0.045f + 0.055f * (1f - k)) * intensity;
                Main.spriteBatch.Draw(
                    px,
                    centerScreen,
                    src,
                    Color.White * MathHelper.Clamp(a, 0f, 1f),
                    0f,
                    origin,
                    new Vector2(r * 2f, r * 2f),
                    SpriteEffects.None,
                    0f);
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

        private static void DrawFlatEllipseRing(
            Texture2D px,
            Rectangle src,
            Vector2 centerScreen,
            Vector2 fireDir,
            int playerFacingSign,
            float expand01,
            float fadeMul)
        {
            expand01 = MathHelper.Clamp(expand01, 0f, 1f);
            fadeMul = MathHelper.Clamp(fadeMul, 0f, 1f);
            if (fadeMul < 0.02f)
                return;

            if (playerFacingSign == 0)
                playerFacingSign = 1;

            Vector2 tangent = new Vector2(-fireDir.Y, fireDir.X);
            float tilt = FlatRingBasisTiltRadians * playerFacingSign;
            Vector2 axisMajor = tangent.RotatedBy(tilt);
            Vector2 axisMinor = fireDir.RotatedBy(tilt);
            float ease = 1f - (1f - expand01) * (1f - expand01);
            float major = MathHelper.Lerp(FlatRingMajorRadiusMin, FlatRingMajorRadiusMax, ease);
            float minor = major * 0.04f;

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(
                SpriteSortMode.Deferred,
                BlendState.Additive,
                Main.DefaultSamplerState,
                DepthStencilState.None,
                Main.Rasterizer,
                null,
                Main.GameViewMatrix.TransformationMatrix);

            var origin = new Vector2(0.5f, 0.5f);
            const int steps = 80;
            float thick = MathHelper.Lerp(8f, 3.5f, expand01);
            float baseAlpha = (0.38f + 0.42f * fadeMul) * (1f - expand01 * 0.15f);

            for (int i = 0; i < steps; i++)
            {
                float t0 = MathHelper.TwoPi * i / steps;
                float t1 = MathHelper.TwoPi * (i + 1f) / steps;
                float tm = (t0 + t1) * 0.5f;

                Vector2 p0 = axisMajor * (major * MathF.Cos(t0)) + axisMinor * (minor * MathF.Sin(t0));
                Vector2 p1 = axisMajor * (major * MathF.Cos(t1)) + axisMinor * (minor * MathF.Sin(t1));
                Vector2 mid = centerScreen + (p0 + p1) * 0.5f;
                float segLen = Vector2.Distance(p0, p1);

                Vector2 dtheta = axisMajor * (-major * MathF.Sin(tm)) + axisMinor * (minor * MathF.Cos(tm));
                if (dtheta.LengthSquared() < 1e-6f)
                    continue;

                float rot = MathF.Atan2(dtheta.Y, dtheta.X);
                float rim = 0.82f + 0.18f * MathF.Sin(tm * 3f + expand01 * 6f);
                float a = baseAlpha * rim;
                Main.spriteBatch.Draw(
                    px,
                    mid,
                    src,
                    Color.White * MathHelper.Clamp(a, 0f, 1f),
                    rot,
                    origin,
                    new Vector2(segLen * 1.12f, thick),
                    SpriteEffects.None,
                    0f);
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

        private static void SpawnReleaseBurstDust(Vector2 worldCenter, Vector2 fireDir)
        {
            if (Main.dedServ || Main.gamePaused)
                return;

            fireDir = fireDir.SafeNormalize(Vector2.UnitX);
            SoundEngine.PlaySound(SoundID.Item104.WithVolumeScale(0.45f).WithPitchOffset(0.32f), worldCenter);

            Lighting.AddLight(worldCenter, 1f, 1f, 1f);

            for (int i = 0; i < 56; i++)
            {
                float ang = MathHelper.TwoPi * i / 56f + Main.rand.NextFloat(-0.25f, 0.25f);
                float spd = Main.rand.NextFloat(2.8f, 11f);
                Vector2 vel = ang.ToRotationVector2() * spd + Main.rand.NextVector2Circular(1.2f, 1.2f);
                int dustType = Main.rand.Next(5) switch
                {
                    0 => DustID.PortalBolt,
                    1 => DustID.Enchanted_Pink,
                    2 => DustID.FireworksRGB,
                    3 => DustID.CrystalPulse,
                    _ => DustID.IceTorch
                };
                Color col = Color.Lerp(Color.White, new Color(235, 225, 255), Main.rand.NextFloat(0.2f, 0.75f));
                Dust d = Dust.NewDustPerfect(worldCenter + Main.rand.NextVector2Circular(16f, 16f), dustType, vel, 0, col, Main.rand.NextFloat(1f, 2.4f));
                d.noGravity = Main.rand.NextFloat() < 0.42f;
                d.fadeIn = Main.rand.NextFloat(0.06f, 0.28f);
            }

            for (int j = 0; j < 28; j++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(9f, 9f) + fireDir * Main.rand.NextFloat(2f, 7f);
                Dust.NewDustPerfect(
                    worldCenter + Main.rand.NextVector2Circular(8f, 8f),
                    DustID.FireworksRGB,
                    vel,
                    0,
                    Color.White,
                    Main.rand.NextFloat(0.65f, 1.35f));
            }

            for (int k = 0; k < 12; k++)
            {
                float a = Main.rand.NextFloat() * MathHelper.TwoPi;
                Dust.NewDustPerfect(
                    worldCenter,
                    DustID.CrystalPulse,
                    a.ToRotationVector2() * Main.rand.NextFloat(1.5f, 5f),
                    0,
                    Color.White,
                    Main.rand.NextFloat(0.85f, 1.5f));
            }
        }
    }
}

