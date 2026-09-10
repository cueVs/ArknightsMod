using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Projectiles.Caster.Ebenholz;

public sealed class EbenholzCube : ModProjectile
{
    public override string Texture => "Terraria/Images/Projectile_0";
    private int age;
    private int excludedTarget = -1;
    private bool Wisp => Projectile.ai[0] == 2f;
    public override void SetStaticDefaults()
    {
        ProjectileID.Sets.TrailCacheLength[Type] = 12;
        ProjectileID.Sets.TrailingMode[Type] = 0;
    }
    public override void SetDefaults()
    {
        Projectile.width = Projectile.height = 18;
        Projectile.friendly = true;
        Projectile.DamageType = DamageClass.Magic;
        Projectile.penetrate = 1;
        Projectile.timeLeft = 240;
        Projectile.ignoreWater = true;
        Projectile.tileCollide = true;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 20;
    }
    public override bool ShouldUpdatePosition() => Projectile.ai[2] <= 0f;
    public override bool? CanDamage() => Projectile.ai[2] > 0f ? false : null;
    public override void AI()
    {
        if (Projectile.ai[2] > 0f)
        {
            Player owner = Main.player[Projectile.owner];
            if (!owner.active || owner.dead) { Projectile.Kill(); return; }
            Projectile.tileCollide = false;
            Projectile.Center = owner.MountedCenter + new Vector2(-owner.direction * 18f, -28f);
            Projectile.ai[2]--;
            return;
        }
        Projectile.tileCollide = true;
        age++;
        int targetIndex = (int)Projectile.ai[1] - 1;
        NPC target = targetIndex >= 0 && targetIndex < Main.maxNPCs ? Main.npc[targetIndex] : null;
        if (target != null && (!target.CanBeChasedBy() || target.DistanceSQ(Projectile.Center) > 1000f * 1000f
            || !EbenholzCombat.Clear(Projectile.Center, target.Center))) target = null;
        if (target == null && Projectile.owner == Main.myPlayer && age % 12 == 0)
        {
            targetIndex = EbenholzCombat.FindTarget(Projectile.Center, 750f, Projectile.Center, Projectile.ai[0] == 1);
            Projectile.ai[1] = targetIndex + 1;
            Projectile.netUpdate = true;
            if (targetIndex >= 0) target = Main.npc[targetIndex];
        }
        if (target != null && age > 8)
            Projectile.velocity = EbenholzCombat.Home(Projectile.velocity, target.Center - Projectile.Center, age, Projectile.identity, Wisp);
        else if (age < 8)
            Projectile.velocity *= .994f;
        Projectile.rotation = Projectile.velocity.ToRotation();
        if (!Main.dedServ && age % 4 == 0)
        {
            Vector2 velocity = -Projectile.velocity * .1f;
            Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.Smoke, velocity, 110, new Color(15, 10, 20), .7f);
            dust.noGravity = dust.noLight = true;
        }
    }
    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) => excludedTarget = EbenholzCombat.Identity(target);
    public override void OnKill(int timeLeft)
    {
        if (Projectile.ai[2] > 0f) return;
        EbenholzVisuals.Scatter(Projectile.Center, Wisp ? 5 : 9, Wisp ? 2f : 3.2f);
        if (Projectile.owner != Main.myPlayer || Wisp) return;
        // 直击目标已经受过伤害；爆炸只补周围敌人，避免同一发重复结算。
        int index = Projectile.NewProjectile(Projectile.GetSource_Death(), Projectile.Center, Vector2.Zero,
            ModContent.ProjectileType<EbenholzBurst>(), Projectile.damage, 2f, Projectile.owner, 0, excludedTarget + 1);
        if (Main.projectile.IndexInRange(index)) Main.projectile[index].CritChance = Projectile.CritChance;
        if (Projectile.ai[0] != 1f || excludedTarget < 0) return;
        int target = EbenholzCombat.FindTarget(Projectile.Center, 600f, Projectile.Center, true);
        if (target < 0) return;
        Vector2 launch = new Vector2(Projectile.identity % 2 == 0 ? 5f : -5f, -7f);
        index = Projectile.NewProjectile(Projectile.GetSource_Death(), Projectile.Center, launch, Type,
            Math.Max(1, (int)(Projectile.damage * .35f)), 1f, Projectile.owner, 2, target + 1);
        if (Main.projectile.IndexInRange(index)) Main.projectile[index].CritChance = Projectile.CritChance;
    }
    public override bool PreDraw(ref Color lightColor)
    {
        if (Projectile.ai[2] > 0f) return false;
        EbenholzVisuals.Trail(Projectile, Wisp);
        float fade = MathHelper.Clamp(Projectile.timeLeft / 15f, 0f, 1f);
        if (Wisp)
        {
            EbenholzVisuals.Soft(Projectile.Center, 6f, Color.Black * fade);
            EbenholzVisuals.Arc(Projectile.Center, 5f, Projectile.rotation - 1.3f, 2.6f, 1f,
                EbenholzVisuals.Light(EbenholzVisuals.Gold, fade * .7f), 1f);
        }
        else EbenholzVisuals.Cube(Projectile.Center, Projectile.ai[0] == 1f ? 10f : 7.5f, age,
            Projectile.identity, fade);
        return false;
    }
}
