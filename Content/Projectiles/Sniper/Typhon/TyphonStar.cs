using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Projectiles.Sniper.Typhon
{
    public class TyphonStar : ModProjectile
    {
        private int DelayTicks;
        private int TotalTicks;
        private int StarLifeTicks;
        private const float ForwardOffset = 60f;

        private readonly TyphonS3StarChargeEffects.ChargeSmokeRingInstance[] _smokeRings =
            new TyphonS3StarChargeEffects.ChargeSmokeRingInstance[TyphonS3StarChargeEffects.SmokeRingMaxConcurrent];
        private int _smokeRingCount;
        private float _smokeRingSpawnAccumulatorSeconds;

        public const float CrossArmCenterThicknessMul = TyphonS3StarChargeEffects.CrossArmCenterThicknessMul;

        public const float CrossChargeSizePulseMin = TyphonS3StarChargeEffects.CrossChargeSizePulseMin;

        public const float StarLifetimeFractionOfSwing = 0.64f;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.DontAttachHideToAlpha[Type] = true;
        }

        public override void Load()
        {
            TyphonS3StarChargeEffects.LoadSmokeTextures();
            TyphonS3StarChargeEffects.LoadChargeCrossTexture();
        }

        public override void SetDefaults()
        {
            Projectile.width        = 40;
            Projectile.height       = 40;
            Projectile.aiStyle      = -1;
            Projectile.tileCollide  = false;
            Projectile.friendly     = false;
            Projectile.hostile      = false;
            Projectile.penetrate    = -1;
            Projectile.extraUpdates = 0;
            Projectile.netImportant = true;
            Projectile.hide         = true;
        }

        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
            overPlayers.Add(index);
        }

        public override bool? CanDamage() => false;

        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];
            Vector2 forwardUp = new Vector2(owner.direction, -1f);
            forwardUp = forwardUp.SafeNormalize(Vector2.UnitX);
            Projectile.Center   = owner.MountedCenter + forwardUp * ForwardOffset;

            int elapsed = TotalTicks - Projectile.timeLeft;
            if (elapsed < DelayTicks)
            {
                TyphonS3StarChargeEffects.ResetChargeSmokeRingQueue(ref _smokeRingCount, ref _smokeRingSpawnAccumulatorSeconds);
            }
            else
            {
                int visibleElapsed = elapsed - DelayTicks;
                TyphonS3StarChargeEffects.TickChargeSmokeRingQueue(
                    visibleElapsed,
                    Projectile.identity,
                    Projectile.owner,
                    _smokeRings,
                    ref _smokeRingCount,
                    ref _smokeRingSpawnAccumulatorSeconds);

                TyphonS3StarChargeEffects.SpawnChargeGatherDust(Projectile.Center, Projectile.identity, visibleElapsed);
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            int elapsed = TotalTicks - Projectile.timeLeft;
            if (elapsed < DelayTicks)
                return false;

            int visibleElapsed = elapsed - DelayTicks;
            float chargeRatio = StarLifeTicks > 0
                ? MathHelper.Clamp((float)visibleElapsed / StarLifeTicks, 0f, 1f)
                : 1f;

            float pulseT = MathHelper.SmoothStep(0f, 1f, chargeRatio);

            Texture2D px = TextureAssets.MagicPixel.Value;
            Vector2 center = Projectile.Center - Main.screenPosition;
            var src = new Rectangle(0, 0, 1, 1);

            TyphonS3StarChargeEffects.DrawChargePhase(
                center,
                chargeRatio,
                pulseT,
                px,
                src,
                _smokeRings,
                _smokeRingCount,
                visibleElapsed,
                Projectile.identity);

            return false;
        }

        public override void OnSpawn(IEntitySource source)
        {
            Player owner = Main.player[Projectile.owner];
            int interval = owner.itemAnimationMax;
            if (interval <= 0)
                interval = owner.HeldItem.useAnimation;

            DelayTicks    = (int)(interval * 0.15f);
            TotalTicks    = Math.Max(8, (int)(interval * StarLifetimeFractionOfSwing));
            StarLifeTicks = Math.Max(1, TotalTicks - DelayTicks);

            if (TyphonAimReticle.TryGetSnappedChaseNpc(owner.whoAmI, out NPC snappedTarget))
            {
                Projectile.localAI[0] = snappedTarget.whoAmI + 1f;
                Projectile.ai[0] = snappedTarget.Center.X;
                Projectile.ai[1] = snappedTarget.Center.Y;
            }
            else
            {
                Projectile.localAI[0] = 0f;
            }

            Projectile.timeLeft = TotalTicks;
            TyphonS3StarChargeEffects.ResetChargeSmokeRingQueue(ref _smokeRingCount, ref _smokeRingSpawnAccumulatorSeconds);
        }

        public static float PackSmokeRingAi() => 0f;

        public override void OnKill(int timeLeft)
        {
            Player owner = Main.player[Projectile.owner];
            NPC lockedTarget = null;
            int lockedNpcWhoPlusOne = (int)Projectile.localAI[0];
            Vector2 target = new Vector2(Projectile.ai[0], Projectile.ai[1]);
            if (lockedNpcWhoPlusOne > 0)
            {
                int idx = lockedNpcWhoPlusOne - 1;
                if (idx >= 0 && idx < Main.maxNPCs)
                {
                    NPC snappedTarget = Main.npc[idx];
                    if (snappedTarget.active && !snappedTarget.friendly && snappedTarget.life > 0 && !snappedTarget.dontTakeDamage)
                    {
                        lockedTarget = snappedTarget;
                        target = snappedTarget.Center;
                    }
                }
            }
            else
            {
                target = TyphonAimReticle.GetCurrentPos(owner.whoAmI) ?? target;
            }

            Vector2 aimDir = Vector2.UnitX.RotatedBy(Projectile.ai[2]);

            if (!Main.dedServ)
            {
                int burstIdx = Projectile.NewProjectile(
                    Projectile.GetSource_FromThis(),
                    Projectile.Center,
                    Vector2.Zero,
                    ModContent.ProjectileType<TyphonStarReleaseBurst>(),
                    0,
                    0f,
                    Projectile.owner,
                    aimDir.X,
                    aimDir.Y,
                    Projectile.ai[2]);

                if (burstIdx >= 0 && burstIdx < Main.maxProjectiles
                    && Main.projectile[burstIdx].ModProjectile is TyphonStarReleaseBurst burst)
                {
                    burst.ApplySmokeRingSnapshot(_smokeRings, _smokeRingCount);
                }
            }

            if (Main.myPlayer != Projectile.owner)
                return;

            SoundEngine.PlaySound(SoundID.Item5, owner.Center);

            Vector2 dir = aimDir;
            Vector2 vel = dir * 16f;
            int rainArrowIdx = Projectile.NewProjectile(
                owner.GetSource_FromThis(),
                owner.Center,
                vel,
                ModContent.ProjectileType<TyphonArrow>(),
                Projectile.damage,
                Projectile.knockBack,
                Projectile.owner,
                target.X, target.Y, 1f);

            if (lockedTarget != null && rainArrowIdx >= 0 && rainArrowIdx < Main.maxProjectiles)
            {
                Main.projectile[rainArrowIdx].localAI[0] = lockedTarget.whoAmI + 1f;
                Main.projectile[rainArrowIdx].netUpdate = true;
            }
        }
    }
}



