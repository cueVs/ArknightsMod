using System;
using System.Collections.Generic;
using ArknightsMod.Content.Items.Weapons.Caster.Ebenholz;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Projectiles.Caster.Ebenholz;

public sealed class EbenholzRemnant : ModProjectile
{
    public override string Texture => "Terraria/Images/Projectile_0";
    public override void SetStaticDefaults() => ProjectileID.Sets.DrawScreenCheckFluff[Type] = 300;
    public override void SetDefaults()
    {
        Projectile.width = Projectile.height = 40;
        Projectile.timeLeft = 1800;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.penetrate = -1;
        Projectile.DamageType = DamageClass.Magic;
        Projectile.netImportant = true;
    }
    public override bool? CanDamage() => false;
    public override void AI()
    {
        if (!Main.player[Projectile.owner].active || Main.player[Projectile.owner].dead) { Projectile.Kill(); return; }
        float age = 1800 - Projectile.timeLeft;
        if (Projectile.ai[0] == 0f)
        {
            if (age >= 20 && Projectile.owner == Main.myPlayer
                && EbenholzCombat.FindTarget(Projectile.Center, 160f, Projectile.Center, false) >= 0)
            {
                Projectile.ai[0] = 1f;
                Projectile.ai[1] = 0f;
                Projectile.netUpdate = true;
            }
            return;
        }
        Projectile.ai[1]++;
        float progress = MathHelper.Clamp(Projectile.ai[1] / 60f, 0f, 1f);
        EbenholzCombat.Pull(Projectile.Center, .5f + progress * .5f);
        if (!Main.dedServ && (int)Projectile.ai[1] % 3 == 0)
        {
            Vector2 radial = Main.rand.NextVector2Unit();
            Dust dust = Dust.NewDustPerfect(Projectile.Center + radial * Main.rand.NextFloat(55f, 120f),
                DustID.Smoke, -radial * 4f + radial.RotatedBy(MathHelper.PiOver2) * 2f, 70, new Color(12, 8, 16), 1.2f);
            dust.noGravity = dust.noLight = true;
        }
        if (Projectile.ai[1] < 60f) return;
        if (Projectile.owner == Main.myPlayer)
        {
            int index = Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero,
                ModContent.ProjectileType<EbenholzBurst>(), Projectile.damage, Projectile.knockBack, Projectile.owner, 1);
            if (Main.projectile.IndexInRange(index)) Main.projectile[index].CritChance = Projectile.CritChance;
        }
        Projectile.Kill();
    }
    public override bool PreDraw(ref Color lightColor)
    {
        float age = 1800 - Projectile.timeLeft;
        float strength = Projectile.ai[0] == 0 ? .08f : MathF.Sin(MathHelper.Clamp(Projectile.ai[1] / 60f, 0f, 1f) * MathHelper.Pi);
        EbenholzVisuals.Well(Projectile.Center, age, strength, Projectile.identity);
        return false;
    }
}

public sealed class EbenholzBurst : ModProjectile
{
    private readonly HashSet<int> hitEnemies = new();
    public override string Texture => "Terraria/Images/Projectile_0";
    public override void SetStaticDefaults() => ProjectileID.Sets.DrawScreenCheckFluff[Type] = 180;
    internal float Radius => Projectile.ai[0] == 1f ? 135f : 48f;
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
    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) => hitEnemies.Add(EbenholzCombat.Identity(target));
    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) => EbenholzCombat.Circle(targetHitbox, Projectile.Center, Radius);
    public override void AI()
    {
        if (Projectile.timeLeft != 26) return;
        SoundEngine.PlaySound(SoundID.Item14 with { Volume = Projectile.ai[0] == 1f ? .55f : .22f,
            Pitch = -.65f, MaxInstances = 3 }, Projectile.Center);
        EbenholzVisuals.Scatter(Projectile.Center, Projectile.ai[0] == 1f ? 28 : 8, Projectile.ai[0] == 1f ? 7f : 3.5f);
    }
    public override bool PreDraw(ref Color lightColor)
    {
        EbenholzVisuals.Explosion(Projectile.Center, (26f - Projectile.timeLeft) / 26f, Radius, Projectile.identity);
        return false;
    }
}

public sealed class EbenholzOrbit : ModProjectile
{
    public override string Texture => "Terraria/Images/Projectile_0";
    public override void SetStaticDefaults() => ProjectileID.Sets.DrawScreenCheckFluff[Type] = 80;
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
        if (!owner.active || owner.dead || owner.HeldItem.ModItem is not EbenholzStaff) { Projectile.Kill(); return; }
        Projectile.timeLeft = 2;
        Projectile.Center = owner.MountedCenter;
        Projectile.localAI[0]++;
    }
    public override bool PreDraw(ref Color lightColor)
    {
        float age = Projectile.localAI[0];
        int count = Math.Clamp((int)Projectile.ai[0], 0, 3);
        for (int i = 0; i < count; i++)
        {
            float angle = age * .018f + i * MathHelper.TwoPi / 3f;
            Vector2 offset = new(MathF.Cos(angle) * 35f, -38f + MathF.Sin(angle) * 10f);
            EbenholzVisuals.Cube(Projectile.Center + offset, 5.8f, age, i + Projectile.owner * 3);
        }
        if (Projectile.ai[1] is 1f or 3f)
        {
            Color glow = EbenholzVisuals.Light(EbenholzVisuals.Gold, .45f);
            EbenholzVisuals.Arc(Projectile.Center + new Vector2(0, 17), 36f, -age * .04f, 5.4f, .32f, glow, 1f);
            for (int i = 0; i < 4; i++)
            {
                float angle = age * .03f + i * MathHelper.PiOver2;
                EbenholzVisuals.Cube(Projectile.Center + angle.ToRotationVector2() * 45f, 3.5f, age, i, .75f);
            }
        }
        return false;
    }
}
