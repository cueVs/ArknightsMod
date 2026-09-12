using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Projectiles.Medic.Warfarin;

public sealed class WarfarinBloodBolt : ModProjectile
{
    public override string Texture => "Terraria/Images/Extra_98";

    public override void SetStaticDefaults()
    {
        ProjectileID.Sets.TrailCacheLength[Type] = 12;
        ProjectileID.Sets.TrailingMode[Type] = 0;
    }

    public override void SetDefaults()
    {
        Projectile.width = Projectile.height = 10;
        Projectile.friendly = true;
        Projectile.DamageType = DamageClass.Summon;
        Projectile.penetrate = 2;
        Projectile.timeLeft = 180;
        Projectile.tileCollide = true;
        Projectile.ignoreWater = true;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 12;
    }

    public override void AI()
    {
        Projectile.localAI[0]++;
        // 比煌的锯屑更轻、初速更高，不衰减水平速度，仍保持明显的抛物线。
        Projectile.velocity.Y = Math.Min(Projectile.velocity.Y + .12f, 16f);
        Projectile.rotation = Projectile.velocity.ToRotation();
        Lighting.AddLight(Projectile.Center, .34f, .012f, .045f);
        if (!Main.dedServ && Main.rand.NextBool(3))
        {
            Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.Blood,
                -Projectile.velocity * .06f, 20, default, .85f);
            dust.noGravity = true;
        }
    }

    public override bool OnTileCollide(Vector2 oldVelocity) => true;

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) => Splash(4);
    public override void OnKill(int timeLeft) => Splash(8);

    private void Splash(int count)
    {
        if (Main.dedServ)
            return;
        for (int i = 0; i < count; i++)
            Dust.NewDustPerfect(Projectile.Center, DustID.Blood,
                Main.rand.NextVector2Circular(2.4f, 2.4f) - Projectile.velocity * .08f,
                15, default, Main.rand.NextFloat(.8f, 1.2f));
    }

    public override bool PreDraw(ref Color lightColor)
    {
        Texture2D glow = TextureAssets.Extra[ExtrasID.ThePerfectGlow].Value;
        Vector2 origin = glow.Size() * .5f;
        float fade = MathHelper.Clamp(Projectile.timeLeft / 12f, 0f, 1f);
        Vector2 previous = Projectile.Center;
        for (int i = 0; i < Projectile.oldPos.Length; i++)
        {
            if (Projectile.oldPos[i] == Vector2.Zero)
                continue;
            Vector2 center = Projectile.oldPos[i] + Projectile.Size * .5f;
            float strength = 1f - i / (float)Projectile.oldPos.Length;
            Vector2 delta = center - previous;
            WarfarinPlasmaAura.DrawLine(previous, center, new Color(100, 2, 24) * (fade * strength),
                7f * strength + 1f);
            Main.EntitySpriteDraw(glow, center - Main.screenPosition, null,
                new Color(210, 8, 40, 0) * (strength * fade * .5f), delta.ToRotation(), origin,
                new Vector2(22f, 12f) / glow.Size() * strength, SpriteEffects.None);
            previous = center;
        }
        // 不透明暗红液核与零 Alpha 发光层分开，明亮背景下依然看得清轮廓。
        Main.EntitySpriteDraw(glow, Projectile.Center - Main.screenPosition, null,
            new Color(115, 0, 22) * fade, Projectile.rotation, origin,
            new Vector2(32f, 19f) / glow.Size(), SpriteEffects.None);
        Main.EntitySpriteDraw(glow, Projectile.Center - Main.screenPosition, null,
            new Color(245, 24, 60, 0) * fade, Projectile.rotation, origin,
            new Vector2(25f, 12f) / glow.Size(), SpriteEffects.None);
        Main.EntitySpriteDraw(glow, Projectile.Center + Projectile.velocity.SafeNormalize(Vector2.UnitX) * 3f
            - Main.screenPosition, null, new Color(255, 190, 200, 0) * fade,
            Projectile.rotation, origin, new Vector2(10f, 5f) / glow.Size(), SpriteEffects.None);
        return false;
    }
}
