using ArknightsMod.Content.Buffs;
using ArknightsMod.Content.Projectiles.Caster.Passenger;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Projectiles.Caster.Leizi;

public sealed class LeiziFlyingDischarge : ModProjectile
{
    internal const float SpeedPerUpdate = 12f;
    private bool impactHandled;
    // 与异客同款表现，单发略小；两个技能保持普攻的 120% / 140%。
    internal static float Intensity(int mode) => 0.85f * (mode == 2 ? 1.4f : mode == 1 ? 1.2f : 1f);
    public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.None;
    public override void SetDefaults()
    {
        Projectile.width = Projectile.height = 18;
        Projectile.DamageType = DamageClass.Magic;
        Projectile.friendly = true;
        Projectile.penetrate = 1;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.extraUpdates = 2;
        Projectile.timeLeft = 180; // 12 像素 × 180 次更新 = 2160 像素，持续 60 帧。
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = -1;
    }
    public override bool CanHitPvp(Player target) => false;
    public override bool? CanCutTiles() => false;
    public override bool? CanHitNPC(NPC target) => !impactHandled && target.CanBeChasedBy(Projectile) ? null : false;
    public override bool PreDraw(ref Color lightColor) => false;

    public override void AI()
    {
        Player owner = Main.player[Projectile.owner];
        if (!owner.active || owner.dead)
        {
            Projectile.Kill();
            return;
        }
        if (Main.dedServ || Projectile.numUpdates != 0)
            return;
        int phase = (int)Projectile.localAI[0]++;
        Vector2 end = Projectile.Center + Projectile.velocity;
        Vector2 start = end - Projectile.velocity * 3f;
        float scale = Intensity((int)Projectile.ai[0]);
        PassengerLightningVisuals.Fracture(start, end, scale, Projectile.identity * 397 + phase * 13,
            0.5f, 9, LightningPalette.Leizi);
        PassengerLightningVisuals.EmitSpan(start, end, scale, phase * 6, LightningPalette.Leizi);
        Lighting.AddLight(Projectile.Center, LightningPalette.Leizi.Light * (scale * 0.7f));
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
    {
        if (impactHandled)
            return;
        impactHandled = true;
        if (!target.boss && target.knockBackResist > 0f && target.realLife < 0)
            target.AddBuff(ModContent.BuffType<PassengerCurrentSlow>(), 30);
        if (Projectile.owner != Main.myPlayer)
            return;
        int mode = (int)Projectile.ai[0];
        int index = Projectile.NewProjectile(Projectile.GetSource_FromThis(), target.Center, Vector2.Zero,
            ModContent.ProjectileType<LeiziChainDischarge>(), Projectile.damage, Projectile.knockBack,
            Projectile.owner, mode == 2 ? 1.4f : mode == 1 ? 1.2f : 1f, 30f, 4f);
        if (!Main.projectile.IndexInRange(index))
            return;
        Projectile chain = Main.projectile[index];
        chain.CritChance = Projectile.CritChance;
        ((LeiziChainDischarge)chain.ModProjectile).InitializeRoute(Main.player[Projectile.owner], target.Center, target);
    }
}

// 不复制选敌、同步和粒子系统；仅覆盖惊蛰自己的配色、尺寸与初雷规则。
public sealed class LeiziChainDischarge : PassengerChainDischarge
{
    internal override LightningPalette Palette => LightningPalette.Leizi;
    protected override float VisualSize => 0.85f;
    public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
    {
        // ai[0]=1.4 只由初雷生成，随原版弹幕消息同步；其余攻击保留 15% 递减。
        if (Projectile.ai[0] < 1.39f)
            base.ModifyHitNPC(target, ref modifiers);
    }
}
