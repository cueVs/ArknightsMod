using ArknightsMod.Content.Buffs;
using ArknightsMod.Content.Items.Weapons.Sniper.Typhon;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Diagnostics.CodeAnalysis;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Projectiles.Sniper.Typhon
{
    public class TyphonArrow : ModProjectile
    {
        public bool Vanity;
        public bool Vanity2;
        public bool Stuck;
        public int  StuckNpcIndex = -1;
        public Vector2 StuckOffset;

        internal float SkillTrailSpiralK;

        private int _skillTrailSpiralSign = 1;

        internal int SkillTrailFlightTicks;

        internal float SkillTrailHitShrink = 1f;

        internal float SkillTrailSpawnSpeed = 16f;

        private const float RainHomingTurnDeg = 3.0f;
        private const float RainHomingRange   = 500f;
        private const int   StuckLifeTicks    = 600;
        private const float NormalGravity     = 0.15f;
        private const float AiS3RainHoming = 3f;
        private const float AiS3RainColumn = 3.25f;

        private const int S3RainVanity2DurationTicks = 120;

        private const int S3RainSpawnIntervalTicks = 24;

        private const int S3RainHitCrossFlashDurationTicks = 42;

        private const float S3LockedRainMinSpeed = 18f;

        private const float S3RainHitCrossBaseGeomScale = 0.84f;

        private int _s3RainHitCrossFlashTicks;

        private bool _s3RainHitLockedHexStar;

        private int _normalAttackMuzzleCrossTicks;

        private Vector2 _normalAttackMuzzleCrossWorld;

        private float _normalAttackMuzzleCrossRot;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 52;
            ProjectileID.Sets.TrailingMode[Type] = 1;
        }

        public override void SetDefaults()
        {
            Projectile.CloneDefaults(ProjectileID.WoodenArrowFriendly);
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.extraUpdates = 2;
        }

        public override bool PreAI()
        {
            if (Stuck)
            {
                if (StuckNpcIndex < 0 || StuckNpcIndex >= Main.maxNPCs)
                {
                    Projectile.Kill();
                    return false;
                }
                NPC host = Main.npc[StuckNpcIndex];
                if (!host.active || host.life <= 0)
                {
                    Projectile.Kill();
                    return false;
                }
                Projectile.Center  = host.Center + StuckOffset;
                Projectile.velocity = Vector2.Zero;
                if (_s3RainHitCrossFlashTicks > 0)
                    _s3RainHitCrossFlashTicks--;
                return false;
            }

            if (Vanity)
            {
                Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
                if (Projectile.alpha < 127)
                    Projectile.alpha += 7;

                if (Projectile.alpha > 127)
                    Projectile.alpha = 127;

                if (Projectile.timeLeft <= 2)
                {
                    Vanity = false;
                    Vanity2 = true;
                    Projectile.timeLeft = S3RainVanity2DurationTicks;
                }
                return false;
            }

            if (Vanity2)
            {
                if (Projectile.timeLeft % S3RainSpawnIntervalTicks == 0)
                {
                    SpawnS3RainArrow(
                        Projectile.GetSource_Death(),
                        Projectile.owner,
                        new Vector2(Projectile.ai[0], Projectile.ai[1]),
                        Projectile.velocity.Length(),
                        Projectile.damage,
                        Projectile.knockBack,
                        (int)Projectile.localAI[0]);
                }
                return false;
            }
            if (Projectile.ai[2] == 2f)
            {
                Projectile.position += Projectile.velocity;
                Projectile.rotation  = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

                if (Main.rand.NextBool(2))
                {
                    Vector2 dustPos = Projectile.Center - Projectile.velocity * 0.4f;
                    Dust d = Dust.NewDustPerfect(
                        dustPos,
                        DustID.PurpleTorch,
                        Projectile.velocity * -0.05f + Main.rand.NextVector2Circular(0.6f, 0.6f),
                        100,
                        default,
                        Main.rand.NextFloat(1.0f, 1.4f));
                    d.noGravity = true;
                    d.fadeIn = 1.2f;
                }
                return false;
            }
            if (IsS3RainMode())
            {
                if (IsS3RainColumnMode() || Projectile.localAI[1] == 1f)
                {
                    float spd = Projectile.velocity.Length();
                    if (spd < 4f)
                        spd = Math.Max(SkillTrailSpawnSpeed, 8f);

                    if (TryGetLockedRainTarget((int)Projectile.localAI[0], out NPC? lockedRainTarget))
                    {
                        spd = Math.Max(spd, S3LockedRainMinSpeed);
                        Vector2 toTarget = lockedRainTarget.Center - Projectile.Center;
                        if (toTarget.LengthSquared() > 16f)
                            Projectile.velocity = toTarget.SafeNormalize(Vector2.UnitY) * spd;
                        else
                            Projectile.velocity = new Vector2(0f, Math.Abs(spd));
                    }
                    else
                    {
                        Projectile.velocity = new Vector2(0f, Math.Abs(spd));
                    }

                    Projectile.position += Projectile.velocity;
                    Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

                    if (Main.rand.NextBool(2))
                    {
                        Dust d = Dust.NewDustPerfect(
                            Projectile.Center - Projectile.velocity * 0.3f,
                            DustID.PurpleTorch,
                            Projectile.velocity * -0.08f + Main.rand.NextVector2Circular(0.5f, 0.5f),
                            140,
                            default,
                            Main.rand.NextFloat(0.9f, 1.3f));
                        d.noGravity = true;
                        d.fadeIn = 1.0f;
                    }
                    if (Main.rand.NextBool(8))
                    {
                        Dust spark = Dust.NewDustPerfect(
                            Projectile.Center,
                            DustID.UltraBrightTorch,
                            Main.rand.NextVector2Circular(1.2f, 1.2f),
                            180,
                            Color.MediumPurple,
                            Main.rand.NextFloat(0.6f, 1.0f));
                        spark.noGravity = true;
                    }

                    return false;
                }

                if (Projectile.localAI[0] == 0f)
                {
                    Vector2 reticleCenter = new Vector2(Projectile.ai[0], Projectile.ai[1]);
                    NPC? pick = PickRandomEnemyInArea(reticleCenter, TyphonAimReticle.ReticleAreaRadius);
                    Projectile.localAI[0] = pick != null ? pick.whoAmI + 1f : -1f;
                }

                NPC? tgt = null;
                if (Projectile.localAI[0] > 0f)
                {
                    int idx = (int)Projectile.localAI[0] - 1;
                    if (idx >= 0 && idx < Main.maxNPCs)
                    {
                        NPC pinned = Main.npc[idx];
                        if (pinned.active && !pinned.friendly && pinned.life > 0 && !pinned.dontTakeDamage)
                            tgt = pinned;
                    }
                }
                if (tgt == null) tgt = FindNearestEnemy(RainHomingRange);

                if (tgt != null)
                {
                    float currentAngle = Projectile.velocity.ToRotation();
                    float targetAngle  = (tgt.Center - Projectile.Center).ToRotation();
                    float diff         = MathHelper.WrapAngle(targetAngle - currentAngle);
                    float maxTurn      = MathHelper.ToRadians(RainHomingTurnDeg);
                    Projectile.velocity = Projectile.velocity.RotatedBy(MathHelper.Clamp(diff, -maxTurn, maxTurn));
                }

                Projectile.position += Projectile.velocity;
                Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

                if (Main.rand.NextBool(2))
                {
                    Dust d = Dust.NewDustPerfect(
                        Projectile.Center - Projectile.velocity * 0.3f,
                        DustID.PurpleTorch,
                        Projectile.velocity * -0.08f + Main.rand.NextVector2Circular(0.5f, 0.5f),
                        140,
                        default,
                        Main.rand.NextFloat(0.9f, 1.3f));
                    d.noGravity = true;
                    d.fadeIn = 1.0f;
                }
                if (Main.rand.NextBool(8))
                {
                    Dust spark = Dust.NewDustPerfect(
                        Projectile.Center,
                        DustID.UltraBrightTorch,
                        Main.rand.NextVector2Circular(1.2f, 1.2f),
                        180,
                        Color.MediumPurple,
                        Main.rand.NextFloat(0.6f, 1.0f));
                    spark.noGravity = true;
                }

                return false;
            }

            if (Projectile.ai[2] == 0f)
            {
                Projectile.velocity.Y += NormalGravity;
                Projectile.rotation    = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

                if (Main.rand.NextBool(2))
                {
                    Dust d = Dust.NewDustPerfect(
                        Projectile.Center - Projectile.velocity * 0.3f,
                        DustID.WhiteTorch,
                        Projectile.velocity * -0.05f + Main.rand.NextVector2Circular(0.4f, 0.4f),
                        100,
                        new Color(150, 150, 150),
                        Main.rand.NextFloat(0.8f, 1.2f));
                    d.noGravity = true;
                    d.fadeIn = 1.0f;
                }
                return false;
            }

            if (Projectile.ai[2] == 4f)
            {
                Projectile.position += Projectile.velocity;
                Projectile.rotation  = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
                if (Main.rand.NextBool(2))
                {
                    Dust d = Dust.NewDustPerfect(
                        Projectile.Center - Projectile.velocity * 0.3f,
                        DustID.UltraBrightTorch,
                        Main.rand.NextVector2Circular(0.6f, 0.6f),
                        120,
                        Color.MediumPurple,
                        Main.rand.NextFloat(1.0f, 1.4f));
                    d.noGravity = true;
                    d.fadeIn = 1.2f;
                }
                return false;
            }

            return base.PreAI();
        }

        public override void PostAI()
        {
            if (Projectile.ai[2] == 0f && _normalAttackMuzzleCrossTicks > 0)
                _normalAttackMuzzleCrossTicks--;

            UpdateSkillTrailRuntime();
            base.PostAI();
        }

        private void UpdateSkillTrailRuntime()
        {
            if (Stuck && IsS3RainMode())
            {
                if (SkillTrailHitShrink > 0f)
                {
                    SkillTrailHitShrink -= 0.32f;
                    if (SkillTrailHitShrink < 0f)
                        SkillTrailHitShrink = 0f;
                }

                return;
            }

            if (!ShouldTrackSkillTrailRuntime())
                return;

            SkillTrailFlightTicks++;

            SkillTrailSpiralK += 0.1f * _skillTrailSpiralSign;
            if (SkillTrailSpiralK >= 5f)
            {
                SkillTrailSpiralK = 5f;
                _skillTrailSpiralSign = -1;
            }
            else if (SkillTrailSpiralK <= -5f)
            {
                SkillTrailSpiralK = -5f;
                _skillTrailSpiralSign = 1;
            }
        }

        private bool ShouldTrackSkillTrailRuntime()
        {
            if (Vanity2)
                return false;
            if (Projectile.ai[2] == 1f && Vanity)
                return true;
            return IsS3RainMode() || Projectile.ai[2] == 4f;
        }

        public override bool ShouldUpdatePosition()
        {
            return !Vanity2 && !Stuck;
        }

        public override bool? CanDamage()
        {
            if (Vanity || Stuck)
                return false;
            return base.CanDamage();
        }

        public override Color? GetAlpha(Color lightColor)
        {
            if (Vanity || IsS3RainMode())
            {
                Color color = Color.MediumPurple;
                color.A = 250;
                return color;
            }
            return null;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            if (ShouldDrawSkillTrailVisual())
                TyphonArrowSkillTrailEffects.DrawSkillArrowTrails(this, Projectile);

            if (_s3RainHitCrossFlashTicks > 0 && Stuck && IsS3RainMode() && !Main.dedServ)
            {
                int total = Math.Max(1, S3RainHitCrossFlashDurationTicks);
                float elapsed = total - _s3RainHitCrossFlashTicks;
                float p = MathHelper.Clamp(elapsed / total, 0f, 1f);

                const float expandEnd = 0.38f;
                float expandT = MathHelper.Clamp(p / expandEnd, 0f, 1f);
                expandT = MathHelper.SmoothStep(0f, 1f, expandT);
                float burstScale = MathHelper.Lerp(0.14f, 1f, expandT) * S3RainHitCrossBaseGeomScale;

                const float fadeStart = 0.74f;
                float opacity = 1f;
                if (p > fadeStart)
                    opacity = MathHelper.SmoothStep(1f, 0f, (p - fadeStart) / Math.Max(0.08f, 1f - fadeStart));

                Vector2 screen = Projectile.Center - Main.screenPosition;
                if (_s3RainHitLockedHexStar)
                {
                    float hexRot = GetS3LockedHitHexStarRotationRad(
                        Projectile.owner,
                        Math.Max(0, StuckNpcIndex),
                        Projectile.identity);
                    TyphonS3StarChargeEffects.DrawS3LockedHitHexStarFlash(screen, burstScale, opacity, hexRot);
                }
                else
                    TyphonS3StarChargeEffects.DrawChargeCrossStarOnly(screen, burstScale, opacity);
            }

			/* 普攻、一技能拖尾 */
			if (Projectile.ai[2] == 0f || Projectile.ai[2] == 2f) {
				int count = 0;
				for (int i = 0; i < Projectile.oldPos.Length; i++) {
					if (i > 0 && Projectile.oldPos[i] == Vector2.Zero)
						break;
					count++;
				}
				if (count < 2)
					return true;

				Texture2D pixel = TextureAssets.MagicPixel.Value;
				var pixelRect = new Rectangle(0, 0, 1, 1);
				var origin = new Vector2(0.5f, 0.5f);

				for (int i = 0; i < count - 1; i++) {
					Vector2 a = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;
					Vector2 b = Projectile.oldPos[i + 1] + Projectile.Size / 2f - Main.screenPosition;

					Vector2 delta = b - a;
					float length = delta.Length();
					if (length < 0.5f)
						continue;
					float rot = delta.ToRotation();

					float t = (float)i / (count - 1);
					float opacity;
					if (t <= 0.2f)
						opacity = t / 0.2f;
					else if (t <= 0.5f)
						opacity = 1f - (t - 0.2f) / 0.3f;
					else
						opacity = 0f;

					float thickness = MathHelper.Lerp(8f, 2f, t);

					Main.spriteBatch.Draw(
						pixel, a, pixelRect,
						new Color(150, 80, 230) * (opacity * 0.55f),
						rot, origin,
						new Vector2(length, thickness * 1.6f),
						SpriteEffects.None, 0f);

					Main.spriteBatch.Draw(
						pixel, a, pixelRect,
						new Color(230, 180, 255) * (opacity * 0.9f),
						rot, origin,
						new Vector2(length, thickness * 0.5f),
						SpriteEffects.None, 0f);
				}
			}

			return true;
        }

        public override void PostDraw(Color lightColor)
        {
            base.PostDraw(lightColor);

            if (Projectile.ai[2] != 0f || _normalAttackMuzzleCrossTicks <= 0 || Main.dedServ)
                return;

            float dur = Math.Max(1, TyphonBow.NormalAttackMuzzleCrossDurationTicks);
            float opacity = MathHelper.Clamp(_normalAttackMuzzleCrossTicks / dur, 0.15f, 1f);
            Vector2 scr = _normalAttackMuzzleCrossWorld - Main.screenPosition;
            float geom = TyphonBow.NormalAttackMuzzleCrossGeometryScale * TyphonBow.NormalAttackMuzzleVisualScale;
            TyphonS3StarChargeEffects.DrawNormalAttackMuzzleCrossOnly(scr, geom, opacity, _normalAttackMuzzleCrossRot);
        }

        private bool ShouldDrawSkillTrailVisual()
        {
            if (Vanity2)
                return false;
            if (Stuck && IsS3RainMode())
                return SkillTrailHitShrink > 0f;
            if (Stuck)
                return false;
            if (Projectile.ai[2] == 4f)
                return SkillTrailHitShrink > 0f;
            if (Projectile.ai[2] == 1f)
                return Vanity;
            return IsS3RainMode();
        }

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            if (IsS3RainMode())
            {
                int stack = CountStuckArrowsOn(target);
                if (stack > 0)
                    modifiers.SourceDamage *= 1f + 0.07f * Math.Min(stack,30);
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (Vanity || Stuck) return;

            if (Projectile.owner >= 0 && Projectile.owner < Main.maxPlayers)
                TyphonSkillArrowStun.TryApply(target, Main.player[Projectile.owner], Projectile);

            if (Projectile.ai[2] == 4f)
            {
                SkillTrailHitShrink = 0f;
                return;
            }

            if (!IsS3RainMode())
                return;

            Stuck         = true;
            StuckNpcIndex = target.whoAmI;
            StuckOffset   = Projectile.Center - target.Center;
            bool snappedOnTarget = TyphonAimReticle.TryGetSnappedChaseNpc(Projectile.owner, out NPC? chase)
                && chase == target;
            _s3RainHitLockedHexStar = IsS3RainColumnMode() || snappedOnTarget;

            if (!Main.dedServ)
            {
                _s3RainHitCrossFlashTicks = S3RainHitCrossFlashDurationTicks;
                if (_s3RainHitLockedHexStar)
                    SpawnS3LockedTargetHitBurstParticles(Projectile.Center);
            }
            SkillTrailHitShrink = 1f;
            Projectile.velocity     = Vector2.Zero;
            Projectile.tileCollide  = false;
            Projectile.extraUpdates = 0;
            Projectile.timeLeft     = StuckLifeTicks;
            Projectile.netUpdate    = true;

            if (Main.myPlayer == Projectile.owner && CountStuckArrowsOn(target) >= 10)
            {
                Player owner = Main.player[Projectile.owner];
                const float bonusSpeed = 22f;
                const float bonusDrop = 900f;
                Vector2 spawnAbove = new Vector2(target.Center.X, target.Center.Y - bonusDrop);
                Projectile.NewProjectile(
                    owner.GetSource_FromThis(),
                    spawnAbove,
                    new Vector2(0f, bonusSpeed),
                    Type,
                    Projectile.damage,
                    Projectile.knockBack,
                    Projectile.owner,
                    0f, 0f, 4f);
            }
        }

        public override void OnSpawn(IEntitySource source)
        {
            float mode = Projectile.ai[2];
            if (mode == 1f)
            {
                Vanity = true;
                Projectile.timeLeft = 120;
                Projectile.alpha = 255;
                SkillTrailSpawnSpeed = Math.Max(Projectile.velocity.Length(), 8f);
                return;
            }
            if (mode == 2f)
            {
                Projectile.aiStyle = -1;
                Projectile.tileCollide = true;
                return;
            }
            if (mode == AiS3RainHoming || mode == AiS3RainColumn)
            {
                Projectile.penetrate = -1;
                SkillTrailSpawnSpeed = Math.Max(Projectile.velocity.Length(), 6f);
                Projectile.aiStyle = -1;

                if (mode == AiS3RainColumn)
                {
                    float spd = Projectile.velocity.Length();
                    if (spd < 4f)
                        spd = Math.Max(SkillTrailSpawnSpeed, 8f);
                    Projectile.velocity = new Vector2(0f, Math.Abs(spd));
                    Projectile.localAI[1] = 1f;
                    return;
                }

                Projectile.localAI[1] = 0f;
                return;
            }
            if (mode == 4f)
            {
                Projectile.aiStyle = -1;
                Projectile.tileCollide = false;
                Projectile.penetrate = 1;
                Projectile.timeLeft = 90;
                Projectile.extraUpdates = 1;
                SkillTrailSpawnSpeed = Math.Max(Projectile.velocity.Length(), 10f);
                return;
            }
            if (!Main.dedServ)
            {
                Player own = Projectile.owner >= 0 && Projectile.owner < Main.maxPlayers
                    ? Main.player[Projectile.owner]
                    : null;

                if (own != null && own.active)
                {
                    Vector2 center = own.MountedCenter;
                    float aimAngle;
                    if (own.whoAmI == Main.myPlayer)
                        aimAngle = (Main.MouseWorld - center).ToRotation();
                    else if (Projectile.velocity.LengthSquared() > 9f)
                        aimAngle = Projectile.velocity.ToRotation();
                    else
                        aimAngle = own.direction > 0 ? 0f : MathHelper.Pi;

                    Vector2 flashPos = center + aimAngle.ToRotationVector2() * TyphonBow.NormalAttackMuzzleRingRadius;
                    _normalAttackMuzzleCrossWorld = flashPos;
                    _normalAttackMuzzleCrossRot = aimAngle;
                    _normalAttackMuzzleCrossTicks = TyphonBow.NormalAttackMuzzleCrossDurationTicks;

                    TyphonS3StarChargeEffects.SpawnTyphonBowNormalMuzzleFlash(
                        flashPos,
                        aimAngle,
                        TyphonBow.NormalAttackMuzzleVisualScale);
                }
            }

            Projectile.tileCollide  = true;
            Projectile.aiStyle      = -1;
            Projectile.extraUpdates = 0;
        }

        public override void OnKill(int timeLeft)
        {
            if (!Vanity || Main.myPlayer != Projectile.owner)
                return;
            SpawnS3RainArrow(
                        Projectile.GetSource_Death(),
                        Projectile.owner,
                        new Vector2(Projectile.ai[0], Projectile.ai[1]),
                        Projectile.velocity.Length(),
                        Projectile.damage,
                        Projectile.knockBack,
                        (int)Projectile.localAI[0]);
        }

        private static void SpawnS3RainArrow(IEntitySource source, int ownerWho, Vector2 markerReticleAi01, float refSpeed, int damage, float knockback, int lockedNpcWhoPlusOne = 0)
        {
            bool hasLockedNpc = TryGetLockedRainTarget(lockedNpcWhoPlusOne, out NPC? lockedNpc);
            Vector2 zoneCenter;
            if (lockedNpcWhoPlusOne > 0)
            {
                if (hasLockedNpc && lockedNpc != null)
                    zoneCenter = lockedNpc.Center;
                else
                    zoneCenter = markerReticleAi01;
            }
            else
            {
                zoneCenter = TyphonAimReticle.GetCurrentPos(ownerWho) ?? markerReticleAi01;
            }

            NPC? columnNpc = hasLockedNpc && lockedNpc != null
                ? lockedNpc
                : FindS3RainColumnTarget(ownerWho, zoneCenter, TyphonAimReticle.ReticleAreaRadius);

            float yDrop = 1000f;
            Vector2 spawnPos;
            if (columnNpc != null && columnNpc.active)
                spawnPos = new Vector2(columnNpc.Center.X, columnNpc.Center.Y - yDrop);
            else
                spawnPos = new Vector2(zoneCenter.X, zoneCenter.Y - yDrop);

            float spd = Math.Max(refSpeed, 4f) * Main.rand.NextFloat(0.95f, 1.05f) * 0.5f;
            Vector2 vel = new Vector2(0f, Math.Abs(spd));

            int projIdx = Projectile.NewProjectile(
                source,
                spawnPos,
                vel,
                ModContent.ProjectileType<TyphonArrow>(),
                damage,
                knockback,
                ownerWho,
                markerReticleAi01.X,
                markerReticleAi01.Y,
                AiS3RainColumn);

            if (lockedNpcWhoPlusOne > 0 && projIdx >= 0 && projIdx < Main.maxProjectiles)
            {
                Main.projectile[projIdx].localAI[0] = lockedNpcWhoPlusOne;
                Main.projectile[projIdx].netUpdate = true;
            }
        }

        private static bool TryGetLockedRainTarget(int lockedNpcWhoPlusOne, [NotNullWhen(true)] out NPC? npc)
        {
            npc = null;
            int idx = lockedNpcWhoPlusOne - 1;
            if (idx < 0 || idx >= Main.maxNPCs)
                return false;

            NPC candidate = Main.npc[idx];
            if (!candidate.active || candidate.friendly || candidate.life <= 0 || candidate.dontTakeDamage)
                return false;

            npc = candidate;
            return true;
        }

        private static NPC? FindS3RainColumnTarget(int ownerWho, Vector2 zoneCenter, float radius)
        {
            if (ownerWho < 0 || ownerWho >= Main.maxPlayers || !Main.player[ownerWho].active)
                return null;
            Player plr = Main.player[ownerWho];
            float r2 = radius * radius;
            NPC? best = null;
            float bestD = r2;
            foreach (NPC npc in Main.ActiveNPCs)
            {
                if (!npc.CanBeChasedBy(plr) || npc.friendly || npc.life <= 0 || npc.dontTakeDamage)
                    continue;
                float d = Vector2.DistanceSquared(npc.Center, zoneCenter);
                if (d <= r2 && d < bestD)
                {
                    bestD = d;
                    best = npc;
                }
            }
            return best;
        }

        private bool IsS3RainMode() =>
            Projectile.ai[2] == AiS3RainHoming || Projectile.ai[2] == AiS3RainColumn;

        private bool IsS3RainColumnMode() => Projectile.ai[2] == AiS3RainColumn;

        private static void SpawnS3LockedTargetHitBurstParticles(Vector2 worldCenter)
        {
            if (Main.dedServ || Main.gamePaused)
                return;

            Lighting.AddLight(worldCenter, 0.52f, 0.38f, 0.88f);

            const int purpleCount = 15;
            for (int i = 0; i < purpleCount; i++)
            {
                float ang = MathHelper.TwoPi * i / purpleCount + Main.rand.NextFloat(-0.1f, 0.1f);
                float spd = Main.rand.NextFloat(1.6f, 5.2f);
                Vector2 vel = ang.ToRotationVector2() * spd + Main.rand.NextVector2Circular(0.35f, 0.35f);
                vel.Y -= Main.rand.NextFloat(0.15f, 1.1f);

                int pick = Main.rand.Next(8);
                int dustType;
                Color dustColor = default;
                float scale = Main.rand.NextFloat(1f, 1.45f);
                if (pick < 2)
                    dustType = DustID.GemAmethyst;
                else if (pick < 4)
                    dustType = DustID.Enchanted_Pink;
                else if (pick < 5)
                    dustType = DustID.CrystalPulse;
                else if (pick < 7)
                    dustType = DustID.PurpleTorch;
                else
                {
                    dustType = DustID.PortalBolt;
                    dustColor = new Color(200, 160, 255);
                    scale *= 1.05f;
                }

                Dust d = Dust.NewDustPerfect(
                    worldCenter + Main.rand.NextVector2Circular(3.5f, 3.5f),
                    dustType,
                    vel,
                    0,
                    dustColor,
                    scale);
                d.noGravity = Main.rand.NextFloat() < 0.5f;
                d.fadeIn = Main.rand.NextFloat(0.06f, 0.22f);
            }

            for (int j = 0; j < 3; j++)
            {
                float ang = MathHelper.TwoPi * j / 3f + Main.rand.NextFloat(-0.18f, 0.18f);
                float spd = Main.rand.NextFloat(1.4f, 4f);
                Vector2 vel = ang.ToRotationVector2() * spd + Main.rand.NextVector2Circular(0.4f, 0.4f);
                vel.Y -= Main.rand.NextFloat(0.1f, 0.9f);
                Dust w = Dust.NewDustPerfect(
                    worldCenter + Main.rand.NextVector2Circular(2.8f, 2.8f),
                    DustID.FireworksRGB,
                    vel,
                    0,
                    Color.Lerp(Color.White, new Color(245, 238, 255), Main.rand.NextFloat(0f, 0.28f)),
                    Main.rand.NextFloat(0.9f, 1.25f));
                w.noGravity = Main.rand.NextFloat() < 0.42f;
                w.fadeIn = Main.rand.NextFloat(0.05f, 0.15f);
            }

            for (int k = 0; k < 2; k++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(2.2f, 2.2f) + new Vector2(0f, -Main.rand.NextFloat(0.6f, 2f));
                Dust.NewDustPerfect(
                    worldCenter + Main.rand.NextVector2Circular(2.5f, 2.5f),
                    DustID.WhiteTorch,
                    vel,
                    100,
                    Color.White,
                    Main.rand.NextFloat(0.78f, 1.1f));
            }
        }

        private NPC? FindNearestEnemy(float range)
        {
            int reticleType = ModContent.ProjectileType<TyphonAimReticle>();
            NPC? best = null;
            float bestDistSq = range * range;
            foreach (Projectile p in Main.ActiveProjectiles)
            {
                if (p.owner != Projectile.owner || p.type != reticleType)
                    continue;

                foreach (NPC npc in Main.ActiveNPCs)
                {
                    if (!npc.CanBeChasedBy(Projectile)) continue;

                    float d = Vector2.DistanceSquared(npc.Center, p.Center);
                    if (d < bestDistSq)
                    {
                        bestDistSq = d;
                        best = npc;
                    }
                }

                break;
            }

            return best;
        }

        private static float GetS3LockedHitHexStarRotationRad(int owner, int stuckNpcIndex, int projectileIdentity)
        {
            unchecked
            {
                int h = owner * 374761393 + stuckNpcIndex * 668265263 + projectileIdentity * 1274126177;
                h = (h ^ (h >> 13)) * 1274126177;
                h ^= h >> 16;
                uint u = (uint)h;
                return (u & 0xFFFFFF) / (float)0x01000000 * MathHelper.TwoPi;
            }
        }

        private static NPC? PickRandomEnemyInArea(Vector2 center, float radius)
        {
            float r2 = radius * radius;
            int count = 0;
            foreach (NPC npc in Main.ActiveNPCs)
            {
                if (npc.friendly || npc.life <= 0 || npc.dontTakeDamage) continue;
                if (Vector2.DistanceSquared(npc.Center, center) > r2) continue;
                count++;
            }
            if (count == 0) return null;
            int pick = Main.rand.Next(count);
            int seen = 0;
            foreach (NPC npc in Main.ActiveNPCs)
            {
                if (npc.friendly || npc.life <= 0 || npc.dontTakeDamage) continue;
                if (Vector2.DistanceSquared(npc.Center, center) > r2) continue;
                if (seen == pick) return npc;
                seen++;
            }
            return null;
        }

        private int CountStuckArrowsOn(NPC target)
        {
            int count = 0;
            int targetWho = target.whoAmI;
            foreach (Projectile p in Main.ActiveProjectiles)
            {
                if (p.type != Type) continue;
                if (p.ModProjectile is TyphonArrow ta && ta.Stuck && ta.StuckNpcIndex == targetWho)
                    count++;
            }

            return count;
        }
    }
}



