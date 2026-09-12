using System;
using ArknightsMod.Content.Items.Weapons.Caster.Hoolheyak;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Projectiles.Caster.Hoolheyak;

public sealed class HoolheyakWindBolt : ModProjectile
{
    internal const int SeekingSolo = 1, SeekingPair = 2, Barrage = 3, BarrageFinisher = 4;
    private int Mode => (int)Projectile.ai[0];
    private int Rank => Math.Clamp((int)Projectile.ai[2], 0, 9);
    public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.None;

    public override void SetDefaults()
    {
        Projectile.width = Projectile.height = 18;
        Projectile.DamageType = DamageClass.Magic;
        Projectile.friendly = true;
        Projectile.penetrate = 1;
        Projectile.timeLeft = 90;
        Projectile.ignoreWater = true;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = -1;
    }

    public override bool CanHitPvp(Player target) => false;
    public override bool? CanCutTiles() => false;
    public override bool? CanHitNPC(NPC target) => target.CanBeChasedBy(Projectile) ? null : false;

    public override void AI()
    {
        Player owner = Main.player[Projectile.owner];
        if (!owner.active || owner.dead)
        {
            Projectile.Kill();
            return;
        }
        int targetIndex = (int)Projectile.ai[1] - 1;
        if (Main.npc.IndexInRange(targetIndex))
        {
            NPC target = Main.npc[targetIndex];
            if (target.CanBeChasedBy(Projectile) && target.DistanceSQ(Projectile.Center) < 1100f * 1100f
                && HoolheyakWindCombat.ClearPath(Projectile.Center, target.Center))
                Projectile.velocity = (Projectile.velocity * 11f
                    + (target.Center - Projectile.Center).SafeNormalize(Projectile.velocity.SafeNormalize(Vector2.UnitX)) * 18f) / 12f;
        }
        Projectile.rotation = Projectile.velocity.ToRotation();
        Projectile.localAI[0]++;
        // Keep physical dust and custom particles layered over the procedural spiral throughout flight.
        if (!Main.dedServ && Projectile.localAI[0] % 2 == 0)
            HoolheyakWindVisuals.Mote(Projectile.Center, -Projectile.velocity * .08f, .65f);
        if (!Main.dedServ && Projectile.localAI[0] % (Mode >= Barrage ? 4 : 2) == 0)
        {
            Vector2 forward = Projectile.velocity.SafeNormalize(Vector2.UnitX);
            Vector2 side = forward.RotatedBy(MathHelper.PiOver2);
            float phase = Projectile.localAI[0] * .55f + Projectile.identity;
            Vector2 offset = side * MathF.Sin(phase) * 7f - forward * 12f;
            HoolheyakWindVisuals.WindSpark(Projectile.Center + offset,
                Projectile.velocity * .12f + side * MathF.Cos(phase) * .7f, .65f);
        }
    }

    public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
    {
        if (target.GetGlobalNPC<HoolheyakWindNPC>().IsLifted)
            modifiers.SourceDamage *= 1.2f;
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
    {
        if (Projectile.owner != Main.myPlayer)
            return;
        int liftTicks = Mode == SeekingSolo ? HoolheyakWindArts.SeekingLift[Rank]
            : Mode >= Barrage ? (Main.rand.NextFloat() < HoolheyakWindArts.BarrageLiftChance[Rank] ? 60 : 0)
            : Mode == 0 ? 24 : 0;
        if (liftTicks > 0 && HoolheyakWindCombat.CanLift(target))
            target.AddBuff(ModContent.BuffType<HoolheyakUpdraft>(), liftTicks);

        if (Mode == Barrage)
            return;
        float multiplier = Mode is SeekingSolo or SeekingPair ? HoolheyakWindArts.SeekingDamage[Rank]
            : Mode == BarrageFinisher ? HoolheyakWindArts.BarrageDamage[Rank] : 1f;
        // 风場伤害以普通攻击基数计算，强化弹不会把多倍伤害永久灌进持续风场。
        HoolheyakSmallVortex.CreateOrFeed(Projectile, (int)MathF.Round(Projectile.damage / multiplier), target);
    }

    public override void OnKill(int timeLeft) => HoolheyakWindVisuals.Burst(Projectile.Center, Projectile.velocity, Mode >= Barrage ? 4 : 9);

    public override bool PreDraw(ref Color lightColor)
    {
        HoolheyakWindVisuals.Bolt(Projectile.Center, Projectile.rotation, Projectile.localAI[0], Mode >= Barrage ? .65f : 1f);
        return false;
    }
}
