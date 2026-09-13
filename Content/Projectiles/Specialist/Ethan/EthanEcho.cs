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
            if (clock == 1)
            {
                if (Projectile.ai[2] > 0f) EthanVisuals.CrossDust(Projectile.Center, Projectile.ai[1], 0f);
                EthanVisuals.Burst(Projectile.Center, Projectile.ai[1]);
            }
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
        if (!Residue)
        {
            if (Projectile.ai[2] > 0f)
                EthanVisuals.Cross(Projectile.Center, 1f - Projectile.timeLeft / 26f, Projectile.ai[1], 0f);
            EthanVisuals.Pulse(Projectile.Center, 1f - Projectile.timeLeft / 26f, Projectile.ai[1]);
        }
        return false;
    }
}

// 独立同步命中闪光，远端客户端无需读取本地玩家的技能按键或技能状态。
public sealed class EthanCrossFlash : ModProjectile
{
    private readonly System.Collections.Generic.HashSet<int> hitGroups = new();
    public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.None;
    public override void SetDefaults()
    {
        Projectile.width = Projectile.height = 2;
        Projectile.friendly = true;
        Projectile.DamageType = DamageClass.MeleeNoSpeed;
        Projectile.penetrate = -1;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = -1;
        Projectile.tileCollide = false;
        Projectile.timeLeft = 22;
    }
    public override bool? CanDamage() => Projectile.timeLeft >= 20 ? null : false;
    public override bool CanHitPvp(Player target) => false;
    public override bool? CanHitNPC(NPC target) => target.CanBeChasedBy(Projectile)
        && !hitGroups.Contains(target.realLife >= 0 ? target.realLife : target.whoAmI) ? null : false;
    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
    {
        float progress = 1f - Projectile.timeLeft / 22f;
        float radius = Projectile.ai[0] * (.65f + .45f * System.Math.Min(1f, progress * 5f));
        if (!EthanCombat.Clear(Projectile.Center, targetHitbox.Center.ToVector2())) return false;
        for (int axis = 0; axis < 2; axis++)
        {
            Vector2 line = (Projectile.ai[1] + axis * MathHelper.PiOver2).ToRotationVector2() * radius;
            float collision = 0f;
            if (Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(),
                Projectile.Center - line, Projectile.Center + line, 6f, ref collision)) return true;
        }
        return false;
    }
    // 独立局部免疫，每个敌人本体只命中一次，且不再触发 EthanCombat.Hit 生成新的十字。
    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        => hitGroups.Add(target.realLife >= 0 ? target.realLife : target.whoAmI);
    public override bool ShouldUpdatePosition() => false;
    public override void AI()
    {
        if (Projectile.localAI[0]++ == 0f)
        {
            EthanVisuals.CrossDust(Projectile.Center, Projectile.ai[0], Projectile.ai[1]);
            EthanVisuals.Burst(Projectile.Center, Projectile.ai[0]);
        }
    }
    public override bool PreDraw(ref Color lightColor)
    {
        EthanVisuals.Cross(Projectile.Center, 1f - Projectile.timeLeft / 22f, Projectile.ai[0], Projectile.ai[1]);
        EthanVisuals.Pulse(Projectile.Center, 1f - Projectile.timeLeft / 22f, Projectile.ai[0]);
        return false;
    }
}
