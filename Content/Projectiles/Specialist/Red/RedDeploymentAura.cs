using System;
using ArknightsMod.Common.Particle;
using ArknightsMod.Content.Items.Weapons.Specialist.Red;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Projectiles.Specialist.Red;

// 无伤害的五秒状态载体。ai[0] 同步已过去的时间，迟加入观察者不会重新看到五秒完整效果。
public sealed class RedDeploymentAura : ModProjectile
{
    public override string Texture => ArknightsMod.noTexture;
    public override void SetDefaults()
    {
        Projectile.width = Projectile.height = 2;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.netImportant = true;
        Projectile.timeLeft = RedDeploymentState.Duration;
    }
    public override bool? CanDamage() => false;
    public override bool ShouldUpdatePosition() => false;
    public override void AI()
    {
        Player owner = Main.player[Projectile.owner];
        int remaining = RedDeploymentState.Duration - (int)Projectile.ai[0];
        if (!owner.active || owner.dead || remaining <= 0)
        {
            Projectile.Kill();
            return;
        }
        Projectile.Center = owner.MountedCenter;
        owner.GetModPlayer<RedDaggerPlayer>().ReceiveDeployment(remaining, (int)Projectile.ai[1], (int)Projectile.ai[2]);
        Projectile.timeLeft = remaining + 1;
        int age = (int)Projectile.ai[0]++;
        if (Projectile.owner == Main.myPlayer && age % 60 == 0)
            Projectile.netUpdate = true;
        if (Main.dedServ || owner.HeldItem.ModItem is not RedDagger)
            return;
        Lighting.AddLight(owner.Center, new Vector3(.24f, .012f, .028f) * Math.Min(1, remaining / 30f));
        if (age % 6 == 0)
        {
            for (int i = 0; i < 2; i++)
                new DefaultParticle(owner.Center + new Vector2(Main.rand.NextFloat(-15, 15), Main.rand.NextFloat(-20, 17)),
                    -owner.velocity * .16f + new Vector2(Main.rand.NextFloat(-.5f, .5f), -1.1f),
                    18, .26f, i == 0 ? new Color(230, 28, 65) : new Color(255, 160, 181), true)
                    { Deformation = new Vector2(.28f, 1.7f) }.Spawn();
        }
        if (age % 24 == 0)
        {
            Dust dust = Dust.NewDustPerfect(owner.Center + Main.rand.NextVector2Circular(12, 20),
                DustID.Blood, -owner.velocity * .1f + new Vector2(0, -.6f), Scale: .6f);
            dust.noGravity = true;
        }
    }
    // 持续状态只保留周身短粒子与 dust，两个技能均不绘制刀幕。
    public override bool PreDraw(ref Color lightColor) => false;
}
