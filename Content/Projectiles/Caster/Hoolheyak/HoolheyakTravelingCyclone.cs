using System;
using ArknightsMod.Content.Items.Weapons.Caster.Hoolheyak;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Projectiles.Caster.Hoolheyak;

/// <summary>三技能的三条独立平行风道：随飞行距离增强，无限穿敌并缓慢减速。</summary>
public sealed class HoolheyakTravelingCyclone : ModProjectile
{
    public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.None;
    public override void SetStaticDefaults() => ProjectileID.Sets.DrawScreenCheckFluff[Type] = 250;
    public override void SetDefaults()
    {
        Projectile.width = 38;
        Projectile.height = 64;
        Projectile.DamageType = DamageClass.Magic;
        Projectile.friendly = true;
        Projectile.ignoreWater = true;
        Projectile.penetrate = -1;
        Projectile.timeLeft = 100;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = -1;
    }
    public override bool CanHitPvp(Player target) => false;
    public override bool? CanCutTiles() => false;
    public override bool? CanHitNPC(NPC target) => target.CanBeChasedBy(Projectile) ? null : false;
    public override void AI()
    {
        if (!Main.player[Projectile.owner].active || Main.player[Projectile.owner].dead)
        {
            Projectile.Kill();
            return;
        }
        Projectile.velocity *= .99f;
        Projectile.ai[1] += Projectile.velocity.Length();
        Projectile.ai[2]++;
        if (!Main.dedServ && (int)Projectile.ai[2] % 4 == 0)
            HoolheyakWindVisuals.Mote(Projectile.Center + Main.rand.NextVector2Circular(28f, 45f), -Projectile.velocity * .1f, 1f);
    }
    public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
    {
        int rank = Math.Clamp((int)Projectile.ai[0], 0, 9);
        modifiers.SourceDamage *= HoolheyakWindArts.CycloneDamage[rank] * HoolheyakWindCombat.DistanceDamage(Projectile.ai[1]);
        if (target.GetGlobalNPC<HoolheyakWindNPC>().IsLifted)
            modifiers.SourceDamage *= 1.2f;
    }
    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
    {
        if (Projectile.owner == Main.myPlayer && HoolheyakWindCombat.CanLift(target))
            target.AddBuff(ModContent.BuffType<HoolheyakUpdraft>(), (int)Projectile.ai[0] < 3 ? 96 : (int)Projectile.ai[0] < 6 ? 108 : (int)Projectile.ai[0] < 9 ? 120 : 132);
    }
    public override bool PreDraw(ref Color lightColor)
    {
        float scale = MathHelper.Lerp(1f, 1.45f, MathHelper.Clamp(Projectile.ai[1] / 480f, 0f, 1f));
        float fade = Math.Min(1f, Projectile.ai[2] / 8f) * Math.Min(1f, Projectile.timeLeft / 12f);
        HoolheyakWindVisuals.Vortex(Projectile.Center, Projectile.ai[2], scale, fade, Projectile.identity);
        return false;
    }
    public override void OnKill(int timeLeft) => HoolheyakWindVisuals.Burst(Projectile.Center, Projectile.velocity * .2f, 16);
}
