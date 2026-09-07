using System;
using ArknightsMod.Common.Particle;
using ArknightsMod.Content.Items.Weapons.Specialist.Red;
using ArknightsMod.Content.Projectiles.BasePROJ;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Projectiles.Specialist.Red;

// 无伤害的五秒状态载体。ai[0] 同步已过去的时间，迟加入观察者不会重新看到五秒完整效果。
public sealed class RedDeploymentAura : ModProjectile
{
    public override string Texture => "Terraria/Images/MagicPixel";
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
    public override bool PreDraw(ref Color lightColor)
    {
        Player owner = Main.player[Projectile.owner];
        if (owner.HeldItem.ModItem is not RedDagger)
            return false;
        float fade = Math.Min(1, (RedDeploymentState.Duration - Projectile.ai[0]) / 30f);
        float beat = .65f + .15f * MathF.Sin(Projectile.ai[0] * .13f);
        // 身侧两条窄红刃随移动向后偏移，保持人物轮廓可见，不糊成整团光球。
        Vector2 behind = owner.Center - owner.velocity * 1.3f;
        RedDaggerVisuals.DrawSlash(behind + new Vector2(-12, 1), -.65f, .42f, fade * beat * .32f, false);
        RedDaggerVisuals.DrawSlash(behind + new Vector2(12, 1), MathHelper.Pi + .65f, .42f, fade * beat * .32f, true);
        return false;
    }
}

internal static class RedDeploymentVisuals
{
    internal static void Burst(Vector2 center)
    {
        if (Main.dedServ || Main.gameMenu) return;
        for (int i = 0; i < 22; i++)
        {
            Vector2 velocity = (MathHelper.TwoPi * i / 22f).ToRotationVector2() * Main.rand.NextFloat(3, 7);
            new DefaultParticle(center + velocity * 2, velocity, 16 + i % 6, .36f,
                i % 5 == 0 ? new Color(255, 210, 221) : new Color(220, 22, 54), true)
                { Deformation = new Vector2(.24f, 2.2f) }.Spawn();
        }
        for (int i = 0; i < 4; i++)
        {
            Dust dust = Dust.NewDustPerfect(center, DustID.Blood, Main.rand.NextVector2Circular(4, 3), Scale: .85f);
            dust.noGravity = true;
        }
    }

    internal static void DrawImpact(Vector2 center, float progress)
    {
        Texture2D ring = ModContent.Request<Texture2D>("ArknightsMod/Content/Textures/circle_03").Value;
        float expansion = 1f - MathF.Pow(1f - progress, 3);
        float fade = (1f - progress) * Math.Min(1, progress * 12);
        Vector2 foot = center + new Vector2(0, 19);
        BaseHeldMeleeSupport.BeginAdditive(Main.spriteBatch);
        Main.spriteBatch.Draw(ring, foot - Main.screenPosition, null, new Color(245, 30, 68) * (fade * .75f),
            0, ring.Size() * .5f, new Vector2(36 + expansion * 210, 10 + expansion * 48) / ring.Size(), SpriteEffects.None, 0);
        // 短促的径向刃线先冲出、后收细，和原有旋切刀幕分开演出。
        for (int i = 0; i < 12; i++)
        {
            float angle = MathHelper.TwoPi * i / 12f + .13f;
            Vector2 direction = angle.ToRotationVector2();
            Vector2 point = center + direction * (12 + 80 * expansion);
            Main.spriteBatch.Draw(TextureAssets.MagicPixel.Value, point - Main.screenPosition, null,
                (i % 3 == 0 ? new Color(255, 211, 222) : new Color(210, 18, 48)) * fade,
                angle, new Vector2(0, .5f), new Vector2(8 + 24 * (1 - progress), 1.5f * fade), SpriteEffects.None, 0);
        }
        BaseHeldMeleeSupport.EndAdditive(Main.spriteBatch);
    }
}
