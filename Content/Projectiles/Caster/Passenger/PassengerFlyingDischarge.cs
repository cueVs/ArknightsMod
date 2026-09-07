using System;
using ArknightsMod.Content.Buffs;
using ArknightsMod.Content.Items.Weapons.Caster.Passenger;
using ArknightsMod.Players;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Projectiles.Caster.Passenger;

/// <summary>非追踪的实体闪电弹。鼠标只决定方向，实际撞击后才启动链式传导。</summary>
public sealed class PassengerFlyingDischarge : ModProjectile
{
    internal const float SpeedPerUpdate = 16f;
    internal const int UpdatesPerTick = 3;
    private float hitMultiplier = 1f;
    private bool empoweredHit;
    private bool impactHandled;
    private float VisualScale => Projectile.ai[2] == 5f ? 1.15f : 1f;
    public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.None;

    // ai[0] 是总更新次数；由发射者按其可视宽度计算后随弹幕同步，远端不重算屏幕尺寸。
    internal static int FlightUpdates(float visibleWidth, bool focus)
        => (int)MathF.Ceiling(MathHelper.Clamp(visibleWidth * (focus ? 2.9f : 2.5f),
            2400f, 24000f) / SpeedPerUpdate);

    public override void SetDefaults()
    {
        Projectile.width = Projectile.height = 20;
        Projectile.DamageType = DamageClass.Magic;
        Projectile.friendly = true;
        Projectile.penetrate = 1;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.extraUpdates = UpdatesPerTick - 1;
        Projectile.timeLeft = 300;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = -1;
    }

    public override void OnSpawn(IEntitySource source)
        => Projectile.timeLeft = Math.Clamp((int)Projectile.ai[0], 150, 1500);

    public override bool CanHitPvp(Player target) => false;
    public override bool? CanCutTiles() => false;
    public override bool? CanHitNPC(NPC target) => !impactHandled && target.CanBeChasedBy(Projectile) ? null : false;

    public override void AI()
    {
        Player owner = Main.player[Projectile.owner];
        if (!owner.active || owner.dead)
        {
            Projectile.Kill();
            return;
        }
        // 不修改 velocity，不读鼠标、不寻找目标。额外更新只提高移动/碰撞精度。
        if (Main.dedServ || Projectile.numUpdates != 0)
            return;
        Vector2 end = Projectile.Center + Projectile.velocity;
        Vector2 start = end - Projectile.velocity * UpdatesPerTick;
        int phase = (int)Projectile.localAI[0]++;
        PassengerLightningVisuals.Fracture(start, end, VisualScale,
            Projectile.identity * 397 + phase * 13, 0.5f, 9);
        PassengerLightningVisuals.EmitSpan(start, end, VisualScale, phase * 6);
        Lighting.AddLight(Projectile.Center, new Vector3(0.55f, 0.26f, 0.035f) * VisualScale);
    }

    public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
    {
        Player owner = Main.player[Projectile.owner];
        WeaponPlayer skills = owner.GetModPlayer<WeaponPlayer>();
        // 只在真实命中、原技能仍被持有且有库存时启用强化。
        empoweredHit = Projectile.ai[1] > 1f && owner.HeldItem.ModItem is PassengerConductor
            && skills.Skill == 0 && skills.StockCount > 0;
        hitMultiplier = empoweredHit ? Projectile.ai[1] : 1f;
        modifiers.SourceDamage *= hitMultiplier;
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
    {
        if (impactHandled)
            return;
        impactHandled = true;
        int slowTicks = empoweredHit ? 90 : 30;
        if (!target.boss && target.knockBackResist > 0f && target.realLife < 0)
            target.AddBuff(ModContent.BuffType<PassengerCurrentSlow>(), slowTicks);
        if (Projectile.owner != Main.myPlayer)
            return;
        Player owner = Main.player[Projectile.owner];
        if (empoweredHit)
            owner.GetModPlayer<WeaponPlayer>().DelStockCount();

        // 首个目标已经由此弹命中；后继链只结算后续目标，延续 15% 逐跳衰减。
        PassengerTargeting.FireChain(Projectile.GetSource_FromThis(), owner, target.Center,
            (int)MathF.Round(Projectile.damage * hitMultiplier), Projectile.knockBack,
            empoweredHit ? 1.4f : VisualScale, (int)Projectile.ai[2], slowTicks, Projectile.CritChance,
            emissionOrigin: Projectile.Center, firstHit: target);
    }

    public override bool PreDraw(ref Color lightColor) => false;
}
