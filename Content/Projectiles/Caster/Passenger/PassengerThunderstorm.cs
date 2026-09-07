using System;
using System.Collections.Generic;
using ArknightsMod.Content.Items.Weapons.Caster.Passenger;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Projectiles.Caster.Passenger;

public sealed class PassengerThunderstorm : ModProjectile
{
    internal const float Radius = 256f;
    private const int Lifetime = 240;
    private int Age => (int)Projectile.ai[1];
    public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.None;
    public override void SetStaticDefaults() => ProjectileID.Sets.DrawScreenCheckFluff[Type] = 1000;

    public override void SetDefaults()
    {
        Projectile.width = Projectile.height = 2;
        Projectile.friendly = false;
        Projectile.DamageType = DamageClass.Magic;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.penetrate = -1;
        Projectile.timeLeft = Lifetime;
        Projectile.netImportant = true;
    }

    public override bool ShouldUpdatePosition() => false;
    public override bool? CanDamage() => false;
    public override bool PreDraw(ref Color lightColor) => false;

    public override void AI()
    {
        Player owner = Main.player[Projectile.owner];
        // 收起武器即撤去雷暴，防止切换别的装备后继续后台输出。
        if (!owner.active || owner.dead || owner.HeldItem.ModItem is not PassengerConductor || Age >= Lifetime)
        {
            Projectile.Kill();
            return;
        }

        Projectile.ai[1]++;
        if (Age == 1)
        {
            // 仅部署这一瞬间允许超出常规技能上限；之后八次雷击各自仍保持 140%。
            PassengerLightningVisuals.Burst(Projectile.Center, 2f, Projectile.identity);
            PassengerLightningVisuals.PrepareImpact(Projectile.Center, 1.8f, Projectile.identity);
            if (!Main.dedServ)
                SoundEngine.PlaySound(SoundID.Item122 with { Volume = 0.60f, Pitch = -0.40f }, Projectile.Center);
        }

        // 0、0.5、...、3.5 秒共八次，不把视觉裂纹的更新次数乘进伤害次数。
        if ((Age - 1) % 30 == 0 && Projectile.owner == Main.myPlayer)
            Strike(owner);

        if (!Main.dedServ && Age % 10 == 0)
        {
            float phase = Age * 0.067f + Projectile.identity;
            Vector2 rim = Projectile.Center + phase.ToRotationVector2() * Radius;
            Vector2 next = Projectile.Center + (phase + 0.58f).ToRotationVector2() * Radius;
            PassengerLightningVisuals.Fracture(rim, next, 0.7f,
                Projectile.identity * 397 + Age, 3f, 18);
            PassengerLightningVisuals.EmitSpan(rim, next, 0.55f, Age);
            Lighting.AddLight(Projectile.Center, 0.4f, 0.20f, 0.03f);
        }
    }

    private void Strike(Player owner)
    {
        List<NPC> targets = new();
        HashSet<int> seen = new();
        foreach (NPC npc in Main.ActiveNPCs)
        {
            if (!npc.CanBeChasedBy(Projectile) || npc.Distance(Projectile.Center) > Radius
                || !PassengerTargeting.ClearPath(Projectile.Center, npc.Center)
                || !seen.Add(PassengerTargeting.EnemyIdentity(npc)))
                continue;
            targets.Add(npc);
        }
        int rank = Math.Clamp((int)Projectile.ai[0], 0, 9);
        float multiplier = rank == 9 ? 1.5f : 1f + rank * 0.05f;
        // 雷暴每半秒选择一名敌人降下主雷，再在区域内传导；空场仅装饰放电。
        Vector2 aim = targets.Count > 0 ? targets[Main.rand.Next(targets.Count)].Center : Projectile.Center;
        PassengerTargeting.FireChain(Projectile.GetSource_FromThis(), owner, aim,
            (int)MathF.Round(owner.GetWeaponDamage(owner.HeldItem) * multiplier), Projectile.knockBack,
            1.4f, 4, 30, owner.GetWeaponCrit(owner.HeldItem), Projectile.Center);
    }

    public override void OnKill(int timeLeft) => PassengerLightningVisuals.Burst(Projectile.Center, 0.85f, Projectile.identity + 7919);
}
