using ArknightsMod.Content.Items.Weapons.Defender.Horn;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Projectiles.Defender.Horn;

public sealed class HornShieldBash : ModProjectile
{
    private readonly System.Collections.Generic.HashSet<int> hitGroups = new();
    public override string Texture => "ArknightsMod/Content/Projectiles/Defender/Cuora/Cuora_Shield";
    public override void SetDefaults()
    {
        Projectile.width = Projectile.height = 2;
        Projectile.friendly = true;
        Projectile.DamageType = DamageClass.Ranged;
        Projectile.tileCollide = false;
        Projectile.penetrate = -1;
        Projectile.timeLeft = 14;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = -1;
    }
    public override bool ShouldUpdatePosition() => false;
    public override void AI()
    {
        Player owner = Main.player[Projectile.owner];
        if (!owner.active || owner.dead || owner.HeldItem.ModItem is not HornGrenadeLauncher)
        { Projectile.Kill(); return; }
        Projectile.Center = owner.MountedCenter;
    }
    public override bool? CanDamage() => Projectile.timeLeft >= 9;
    public override bool? CanHitNPC(NPC target) => hitGroups.Contains(target.realLife >= 0 ? target.realLife : target.whoAmI) ? false : null;
    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
    {
        Vector2 aim = Projectile.velocity.SafeNormalize(Vector2.UnitX);
        Vector2 direction = (targetHitbox.Center.ToVector2() - Projectile.Center).SafeNormalize(aim);
        return Vector2.Dot(aim, direction) > .35f && HornCombat.InCircle(Projectile.Center, 95f, targetHitbox)
            && HornCombat.Clear(Projectile.Center, targetHitbox.Center.ToVector2());
    }
    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
    {
        hitGroups.Add(target.realLife >= 0 ? target.realLife : target.whoAmI);
        HornVisuals.Sparks(target.Center, 12, 4f);
        if (Projectile.owner != Main.myPlayer || Projectile.localAI[0] != 0f) return;
        Projectile.localAI[0] = 1;
        // Shield hits already dealt the physical component. Only add the overload Arts burst once.
        if (Projectile.ai[2] > 0)
            Projectile.NewProjectile(Projectile.GetSource_FromThis(), target.Center, Vector2.Zero,
                ModContent.ProjectileType<HornBurst>(), (int)Projectile.ai[2], Projectile.knockBack,
                Projectile.owner, 95f, 1);
        if (Projectile.ai[1] > 0)
            Projectile.NewProjectile(Projectile.GetSource_FromThis(), target.Center, Vector2.Zero,
                ModContent.ProjectileType<HornFlare>(), 0, 0f, Projectile.owner, 130f, Projectile.ai[1]);
    }
    public override bool PreDraw(ref Color lightColor)
    {
        HornVisuals.Bash(Projectile.Center, Projectile.velocity.ToRotation(), 1f - Projectile.timeLeft / 14f);
        return false;
    }
}
