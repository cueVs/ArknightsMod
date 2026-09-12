using System;
using System.Collections.Generic;
using ArknightsMod.Content.Items.Weapons.Caster.Indigo;
using ArknightsMod.Content.Projectiles.Caster.Ebenholz;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Projectiles.Caster.Indigo;

public sealed class IndigoBurst : ModProjectile
{
    private readonly HashSet<int> hitEnemies = new();
    public override string Texture => "Terraria/Images/Projectile_0";
    public override void SetStaticDefaults() => ProjectileID.Sets.DrawScreenCheckFluff[Type] = 180;
    public override void SetDefaults()
    {
        Projectile.width = Projectile.height = 2;
        Projectile.friendly = true;
        Projectile.DamageType = DamageClass.Magic;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.penetrate = -1;
        Projectile.timeLeft = 26;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = -1;
    }
    public override bool? CanDamage() => Projectile.timeLeft >= 23 ? null : false;
    public override bool? CanHitNPC(NPC target) => hitEnemies.Contains(EbenholzCombat.Identity(target))
        || (Projectile.ai[1] > 0 && EbenholzCombat.Identity(target) == (int)Projectile.ai[1] - 1)
        || !EbenholzCombat.Clear(Projectile.Center, target.Center) ? false : null;
    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
    {
        hitEnemies.Add(EbenholzCombat.Identity(target));
        IndigoTideSeal.TryApply(Projectile, target);
    }
    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        => EbenholzCombat.Circle(targetHitbox, Projectile.Center, 48f);
    public override void AI()
    {
        Lighting.AddLight(Projectile.Center, new Vector3(.18f, .10f, .28f) * (Projectile.timeLeft / 26f));
        if (Projectile.timeLeft != 26) return;
        SoundEngine.PlaySound(SoundID.Item21 with { Volume = .19f, Pitch = .35f, MaxInstances = 3 }, Projectile.Center);
        IndigoVisuals.Scatter(Projectile.Center, 8, 3.5f);
    }
    public override bool PreDraw(ref Color lightColor)
    {
        IndigoVisuals.Explosion(Projectile.Center, (26f - Projectile.timeLeft) / 26f, 48f, Projectile.identity);
        return false;
    }
}

public sealed class IndigoOrbit : ModProjectile
{
    private int previousCount, previousMode;
    private float bloom;
    public override string Texture => "Terraria/Images/Projectile_0";
    public override void SetStaticDefaults() => ProjectileID.Sets.DrawScreenCheckFluff[Type] = 100;
    public override void SetDefaults()
    {
        Projectile.width = Projectile.height = 2;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.penetrate = -1;
        Projectile.timeLeft = 2;
        Projectile.netImportant = true;
    }
    public override bool? CanDamage() => false;
    public override void AI()
    {
        Player owner = Main.player[Projectile.owner];
        if (!owner.active || owner.dead || owner.HeldItem.ModItem is not IndigoStaff || owner.CCed || owner.noItems)
        { Projectile.Kill(); return; }
        Projectile.timeLeft = 2;
        Projectile.Center = owner.MountedCenter;
        Projectile.localAI[0]++;
        if (previousCount != (int)Projectile.ai[0] || previousMode != (int)Projectile.ai[1])
        {
            bloom = 1f;
            previousCount = (int)Projectile.ai[0];
            previousMode = (int)Projectile.ai[1];
            if (previousMode > 0) IndigoVisuals.Scatter(Projectile.Center, 8, 2f);
        }
        bloom = Math.Max(0, bloom - .045f);
        if (Projectile.ai[1] > 0 && (int)Projectile.localAI[0] % 7 == 0)
            IndigoVisuals.Scatter(Projectile.Center + Main.rand.NextVector2Circular(28f, 32f), 1, .5f);
    }
    public override bool PreDraw(ref Color lightColor)
    {
        float age = Projectile.localAI[0];
        int count = Math.Clamp((int)Projectile.ai[0], 0, 3);
        for (int i = 0; i < count; i++)
        {
            float angle = age * .018f + i * MathHelper.TwoPi / 3f;
            Vector2 offset = new(MathF.Cos(angle) * 35f, -38f + MathF.Sin(angle) * 10f);
            Vector2 center = Projectile.Center + offset;
            IndigoVisuals.Droplet(center, 7f + bloom * 1.5f, age, i + Projectile.owner * 3,
                -MathHelper.PiOver2 + MathF.Sin(age * .025f + i) * .14f, .95f);
            IndigoVisuals.Ripple(center + new Vector2(0, 12f), 12f + bloom * 5f, age * .02f, .35f,
                IndigoVisuals.Light(IndigoVisuals.Pearl, .5f), 1f);
        }
        if (Projectile.ai[1] > 0)
            IndigoVisuals.SkillAura(Projectile.Center, age, (int)Projectile.ai[1], bloom);
        return false;
    }
}
