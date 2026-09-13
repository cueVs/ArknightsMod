using System;
using ArknightsMod.Content.Items.Weapons.Caster.Hoolheyak;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Projectiles.Caster.Hoolheyak;

public sealed class HoolheyakSmallVortex : ModProjectile
{
    internal const int MaxVortices = 3;
    internal const int Lifetime = 180;
    internal float Strength => MathHelper.Clamp(Projectile.ai[0], 1f, 3f);
    internal float PullRadius => 200f + Strength * 20f;
    internal float WindScale => .78f + Strength * .13f;
    public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.None;

    public override void SetStaticDefaults() => ProjectileID.Sets.DrawScreenCheckFluff[Type] = 250;
    public override void SetDefaults()
    {
        Projectile.width = Projectile.height = 12;
        Projectile.DamageType = DamageClass.Magic;
        Projectile.friendly = true;
        Projectile.penetrate = -1;
        Projectile.timeLeft = Lifetime;
        Projectile.tileCollide = true;
        Projectile.ignoreWater = true;
        Projectile.netImportant = true;
        // 独立免疫不占用其他玩家的伤害窗口；同一处通过合并风场控制叠放。
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 15;
    }

    internal static void CreateOrFeed(Projectile bolt, int baseDamage, NPC hit)
    {
        int type = ModContent.ProjectileType<HoolheyakSmallVortex>();
        Projectile nearby = null, oldest = null;
        float best = 120f * 120f;
        int count = 0;
        foreach (Projectile other in Main.ActiveProjectiles)
        {
            if (other.type != type || other.owner != bolt.owner)
                continue;
            count++;
            if (oldest == null || other.timeLeft < oldest.timeLeft)
                oldest = other;
            float distance = other.DistanceSQ(bolt.Center);
            if (distance < best && HoolheyakWindCombat.ClearPath(other.Center, bolt.Center))
            {
                best = distance;
                nearby = other;
            }
        }
        int damage = Math.Max(1, (int)MathF.Round(baseDamage * .35f));
        if (nearby != null)
        {
            nearby.ai[0] = Math.Min(3f, nearby.ai[0] + .5f);
            nearby.ai[2] = hit.whoAmI + 1;
            nearby.timeLeft = Lifetime;
            nearby.damage = damage;
            nearby.CritChance = bolt.CritChance;
            nearby.netUpdate = true;
            return;
        }
        if (count >= MaxVortices)
            oldest?.Kill();
        int index = Projectile.NewProjectile(bolt.GetSource_FromThis(), bolt.Center, bolt.velocity * .12f,
            type, damage, 0f, bolt.owner, 1f, 0f, hit.whoAmI + 1);
        if (Main.projectile.IndexInRange(index))
            Main.projectile[index].CritChance = bolt.CritChance;
    }

    public override bool CanHitPvp(Player target) => false;
    public override bool? CanCutTiles() => false;
    public override bool? CanHitNPC(NPC target) => target.CanBeChasedBy(Projectile)
        && HoolheyakWindCombat.ClearPath(Projectile.Center, target.Center) ? null : false;
    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        => new Rectangle((int)(Projectile.Center.X - 42f * WindScale), (int)(Projectile.Center.Y - 62f * WindScale),
            (int)(84f * WindScale), (int)(124f * WindScale)).Intersects(targetHitbox);

    public override bool OnTileCollide(Vector2 oldVelocity) => false;
    public override void AI()
    {
        Player owner = Main.player[Projectile.owner];
        if (!owner.active || owner.dead || owner.HeldItem.ModItem is not HoolheyakWindArts)
        {
            Projectile.Kill();
            return;
        }
        Projectile.ai[1]++;
        if (!Main.dedServ && (int)Projectile.ai[1] % 4 == 0)
        {
            float phase = Projectile.ai[1] * .20f + Projectile.identity;
            float height = Main.rand.NextFloat();
            float radius = (10f + 33f * height) * WindScale;
            Vector2 rim = new(MathF.Cos(phase) * radius,
                (52f - height * 118f) * WindScale + MathF.Sin(phase) * radius * .22f);
            Vector2 tangent = new(-MathF.Sin(phase) * 1.5f, -.9f + MathF.Cos(phase) * .3f);
            HoolheyakWindVisuals.WindSpark(Projectile.Center + rim, tangent + Projectile.velocity * .2f, .7f);
            if ((int)Projectile.ai[1] % 12 == 0)
                HoolheyakWindVisuals.Mote(Projectile.Center + rim, tangent * .7f, .45f);
        }
        if ((int)Projectile.ai[1] % 15 == 0 && Projectile.owner == Main.myPlayer)
        {
            // 原本分开的风场追到同一处时也会合并，低 identity 保留，避免互相吞掉。
            foreach (Projectile other in Main.ActiveProjectiles)
            {
                if (other.type != Type || other.owner != Projectile.owner || other.identity <= Projectile.identity
                    || other.DistanceSQ(Projectile.Center) > 100f * 100f
                    || !HoolheyakWindCombat.ClearPath(Projectile.Center, other.Center))
                    continue;
                Projectile.ai[0] = Math.Min(3f, Math.Max(Strength, other.ai[0]) + .5f);
                Projectile.timeLeft = Math.Max(Projectile.timeLeft, other.timeLeft);
                Projectile.damage = Math.Max(Projectile.damage, other.damage);
                Projectile.netUpdate = true;
                other.Kill();
            }
        }
        NPC target = null;
        int index = (int)Projectile.ai[2] - 1;
        if (Main.npc.IndexInRange(index) && Main.npc[index].CanBeChasedBy(Projectile)
            && Main.npc[index].DistanceSQ(Projectile.Center) < 900f * 900f
            && HoolheyakWindCombat.ClearPath(Projectile.Center, Main.npc[index].Center))
            target = Main.npc[index];
        if (target == null && (int)Projectile.ai[1] % 15 == 0 && Projectile.owner == Main.myPlayer)
        {
            target = HoolheyakWindCombat.Closest(Projectile.Center, 900f);
            Projectile.ai[2] = target?.whoAmI + 1 ?? 0;
            Projectile.netUpdate = true;
        }
        Projectile.velocity = target == null ? Projectile.velocity * .985f
            : HoolheyakWindCombat.Drift(Projectile.velocity, Projectile.Center, target.Center);
        if (!Main.dedServ && (int)Projectile.ai[1] % 5 == 0)
        {
            float angle = Projectile.ai[1] * .13f + Projectile.identity;
            Vector2 rim = new Vector2(MathF.Cos(angle) * PullRadius * .65f, MathF.Sin(angle) * 36f);
            HoolheyakWindVisuals.Mote(Projectile.Center + rim, -rim.SafeNormalize(Vector2.UnitY) * 2f - Vector2.UnitY, .7f);
        }
    }

    public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
    {
        modifiers.SourceDamage *= 1f + (Strength - 1f) * .15f;
        if (target.GetGlobalNPC<HoolheyakWindNPC>().IsLifted)
            modifiers.SourceDamage *= 1.2f;
    }

    public override bool PreDraw(ref Color lightColor)
    {
        float fade = Math.Min(1f, Projectile.ai[1] / 12f) * Math.Min(1f, Projectile.timeLeft / 24f);
        HoolheyakWindVisuals.Vortex(Projectile.Center, Projectile.ai[1], WindScale, fade, Projectile.identity);
        return false;
    }

    public override void OnKill(int timeLeft) => HoolheyakWindVisuals.Burst(Projectile.Center, Vector2.UnitY * -2f, 8);
}
