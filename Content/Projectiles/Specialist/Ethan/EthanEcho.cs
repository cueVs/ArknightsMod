using ArknightsMod.Systems.Gameplay.Damage;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Projectiles.Specialist.Ethan;

public sealed class EthanEcho : ModProjectile
{
    private int clock, targetType;
    private EthanBindingNPC boundEntity;
    private readonly System.Collections.Generic.HashSet<int> hitGroups = new();
    public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.Chik;
    private bool Residue => Projectile.ai[0] == 1;
    public override void SetDefaults()
    {
        Projectile.width = Projectile.height = 2;
        Projectile.friendly = true;
        Projectile.DamageType = DamageClass.MeleeNoSpeed;
        Projectile.penetrate = -1;
        Projectile.tileCollide = false;
        Projectile.timeLeft = 26;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = -1;
    }
    public override void AI()
    {
        clock++;
        if (!Residue)
        {
            if (clock == 1) EthanVisuals.Burst(Projectile.Center, Projectile.ai[1]);
            return;
        }
        Projectile.GetGlobalProjectile<ArtsProjectileMarker>().IsArtsDamage = true;
        Projectile.localNPCHitCooldown = 60;
        int index = (int)Projectile.ai[2] - 1;
        if (index < 0 || index >= Main.maxNPCs || !Main.npc[index].active || Main.npc[index].life <= 0)
        { Projectile.Kill(); return; }
        NPC target = Main.npc[index];
        if (boundEntity == null)
        {
            targetType = target.netID;
            boundEntity = target.GetGlobalNPC<EthanBindingNPC>();
            Projectile.timeLeft = (int)Projectile.ai[1];
        }
        if (target.netID != targetType || !ReferenceEquals(boundEntity, target.GetGlobalNPC<EthanBindingNPC>()))
        { Projectile.Kill(); return; }
        Projectile.Center = target.Center;
        if (Main.rand.NextBool(5)) EthanVisuals.Scatter(Projectile.Center, 1, 1.2f);
    }
    public override bool? CanDamage() => Residue ? clock % 60 == 0 : clock <= 2;
    public override bool? CanHitNPC(NPC target) => Residue
        ? (target.whoAmI != (int)Projectile.ai[2] - 1 ? false : null)
        : (hitGroups.Contains(target.realLife >= 0 ? target.realLife : target.whoAmI) ? false : null);
    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        => Residue ? targetHitbox.Contains(Projectile.Center.ToPoint())
            : EthanCombat.InCircle(Projectile.Center, Projectile.ai[1], targetHitbox)
                && EthanCombat.Clear(Projectile.Center, targetHitbox.Center.ToVector2());
    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
    {
        if (!Residue)
        {
            hitGroups.Add(target.realLife >= 0 ? target.realLife : target.whoAmI);
            EthanCombat.Hit(Projectile, target);
        }
    }
    public override bool PreDraw(ref Color lightColor)
    {
        if (!Residue) EthanVisuals.Pulse(Projectile.Center, 1f - Projectile.timeLeft / 26f, Projectile.ai[1]);
        return false;
    }
}
