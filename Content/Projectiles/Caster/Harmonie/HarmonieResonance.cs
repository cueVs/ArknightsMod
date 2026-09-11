using System.Collections.Generic;
using ArknightsMod.Content.Items.Weapons.Caster.Harmonie;
using ArknightsMod.Content.Projectiles.Caster.Ebenholz;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Projectiles.Caster.Harmonie;

public sealed class HarmonieResonance : ModProjectile
{
    internal const float Radius = 140f;
    private readonly HashSet<int> hitGroups = new();
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
        Projectile.timeLeft = 3600;
        Projectile.netImportant = true;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 29;
    }
    public override bool ShouldUpdatePosition() => false;
    public override void AI()
    {
        Player owner = Main.player[Projectile.owner];
        if (!owner.active || owner.dead || owner.CCed || owner.noItems || owner.HeldItem.ModItem is not HarmonieStaff
            || Projectile.ai[0]-- <= 0 || owner.DistanceSQ(Projectile.Center) > 1000f * 1000f
            || (Projectile.owner == Main.myPlayer && owner.GetModPlayer<HarmonieStaffPlayer>().Mode != 2))
        { Projectile.Kill(); return; }
        Projectile.timeLeft = 2;
        Projectile.localAI[0]++;
        bool pulse = (int)Projectile.localAI[0] % 30 == 0;
        if (pulse)
        {
            hitGroups.Clear();
            HarmonieVisuals.Scatter(Projectile.Center, 5, 2.5f);
        }
        if (!Main.dedServ && (int)Projectile.localAI[0] % 12 == 0)
            HarmonieVisuals.Scatter(Projectile.Center + Main.rand.NextVector2Circular(Radius * .8f, Radius * .4f), 1, .4f);
        Lighting.AddLight(Projectile.Center, .10f, .32f, .22f);
        if (Main.netMode == NetmodeID.MultiplayerClient) return;
        foreach (NPC npc in Main.ActiveNPCs)
        {
            if (!EbenholzCombat.CanPull(npc) || !EbenholzCombat.Circle(npc.Hitbox, Projectile.Center, Radius)
                || !EbenholzCombat.Clear(Projectile.Center, npc.Center)) continue;
            // 减缓移动，仍保留敌人的攻击与AI；首领、分节敌人不受减速。
            npc.velocity *= .88f;
            if (Main.netMode == NetmodeID.Server && pulse) npc.netUpdate = true;
        }
    }
    public override bool? CanDamage() => Projectile.localAI[0] > 0 && (int)Projectile.localAI[0] % 30 == 0 ? null : false;
    public override bool? CanHitNPC(NPC target) => hitGroups.Contains(EbenholzCombat.Identity(target))
        || !EbenholzCombat.Clear(Projectile.Center, target.Center) ? false : null;
    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) => hitGroups.Add(EbenholzCombat.Identity(target));
    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        => EbenholzCombat.Circle(targetHitbox, Projectile.Center, Radius);
    public override bool PreDraw(ref Color lightColor)
    {
        HarmonieVisuals.Resonance(Projectile.Center, Radius, Projectile.localAI[0], Projectile.ai[0]);
        return false;
    }
}
