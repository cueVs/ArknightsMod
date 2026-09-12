using ArknightsMod.Content.Items.Weapons.Specialist.Ethan;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Projectiles.Specialist.Ethan;

public sealed class EthanSpinningYoyo : ModProjectile
{
    // Split each game tick into smaller steering steps, matching Calamity's yoyo pattern.
    // The speed below is per step, so overall reach speed stays at 26 pixels per game tick.
    private const int MovementUpdates = 3;
    private int pulseClock;
    public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.Chik;
    public override void SetStaticDefaults()
    {
        ProjectileID.Sets.YoyosLifeTimeMultiplier[Type] = -1f;
        ProjectileID.Sets.YoyosMaximumRange[Type] = 360f;
        ProjectileID.Sets.YoyosTopSpeed[Type] = 26f / MovementUpdates;
        ProjectileID.Sets.TrailCacheLength[Type] = 8;
        ProjectileID.Sets.TrailingMode[Type] = 0;
    }
    public override void SetDefaults()
    {
        Projectile.width = Projectile.height = 16;
        Projectile.aiStyle = ProjAIStyleID.Yoyo;
        Projectile.friendly = true;
        Projectile.DamageType = DamageClass.MeleeNoSpeed;
        Projectile.penetrate = -1;
        Projectile.MaxUpdates = MovementUpdates;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 24 * MovementUpdates;
    }
    public override void AI()
    {
        Player owner = Main.player[Projectile.owner];
        var state = owner.GetModPlayer<EthanYoyoPlayer>();
        // ai[0]/ai[1] belong to vanilla yoyo steering/return and must not store skill data.
        if (!owner.active || owner.dead || !state.Holding || Projectile.Distance(owner.Center) > 2400f)
        { Projectile.Kill(); return; }
        if (Projectile.ai[0] < 0f) return;
        Lighting.AddLight(Projectile.Center, .08f, .45f, .28f);
        // numUpdates is -1 only on the final sub-update of a game tick.
        if (Projectile.numUpdates != -1) return;
        if (Main.rand.NextBool(3)) EthanVisuals.Scatter(Projectile.Center, 1, 1f);
        if (++pulseClock >= 42)
        {
            pulseClock = 0;
            if (Projectile.owner == Main.myPlayer)
            {
                int index = Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero,
                    ModContent.ProjectileType<EthanEcho>(), (int)(Projectile.damage * .65f * state.AttackMultiplier), 0f,
                    Projectile.owner, 0, 112f, state.CrossSuspension ? 1f : 0f);
                if (Main.projectile.IndexInRange(index)) Main.projectile[index].CritChance = Projectile.CritChance;
            }
        }
    }
    public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        => modifiers.SourceDamage *= Main.player[Projectile.owner].GetModPlayer<EthanYoyoPlayer>().AttackMultiplier;
    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) => EthanCombat.Hit(Projectile, target);
    public override bool PreDraw(ref Color lightColor)
    {
        EthanVisuals.Yoyo(Projectile);
        return true; // Vanilla still draws the string, bob and accessory connection.
    }
}
