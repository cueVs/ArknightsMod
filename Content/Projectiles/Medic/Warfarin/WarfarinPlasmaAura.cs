using System;
using ArknightsMod.Content.Items.Weapons.Medic.Warfarin;
using ArknightsMod.Players;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Projectiles.Medic.Warfarin;

public sealed class WarfarinPlasmaAura : ModProjectile
{
    public override string Texture => "Terraria/Images/MagicPixel";

    public override void SetDefaults()
    {
        Projectile.width = Projectile.height = 2;
        Projectile.friendly = false;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.penetrate = -1;
        Projectile.timeLeft = 3600;
        Projectile.netImportant = true;
    }

    public override bool? CanDamage() => false;
    public override bool ShouldUpdatePosition() => false;

    public override void AI()
    {
        Player owner = Main.player[Projectile.owner];
        // 持杖技能遵循公共武器规则：切走、死亡或主动结束均取消，不留下永久增伤。
        if (!owner.active || owner.dead || owner.HeldItem.ModItem is not WarfarinStaff || --Projectile.ai[0] <= 0
            || (Projectile.owner == Main.myPlayer && !owner.GetModPlayer<WeaponPlayer>().SkillActive))
        {
            Projectile.Kill();
            return;
        }
        Projectile.Center = owner.MountedCenter;
        Projectile.ai[1]++;
        Lighting.AddLight(Projectile.Center, .3f, .025f, .06f);
    }

    public override bool PreDraw(ref Color lightColor)
    {
        float age = Projectile.ai[1];
        float fade = Math.Min(MathHelper.Clamp(age / 12f, 0f, 1f), MathHelper.Clamp(Projectile.ai[0] / 20f, 0f, 1f));
        float pulse = .75f + .25f * MathF.Sin(age * .15f);
        Color red = new Color(240, 20, 65, 0) * (fade * .65f);
        Color white = new Color(255, 195, 205, 0) * (fade * .8f);
        Vector2 center = Projectile.Center;
        float radius = 31f + pulse * 3f;
        // 分段输血环，缓慢旋转；起手额外扩张一圈。
        for (int i = 0; i < 48; i++)
        {
            if (i % 12 >= 9)
                continue;
            float angle = MathHelper.TwoPi * i / 48f + age * .018f;
            Vector2 a = angle.ToRotationVector2() * new Vector2(radius, radius * .66f);
            Vector2 b = (angle + MathHelper.TwoPi / 48f).ToRotationVector2() * new Vector2(radius, radius * .66f);
            DrawLine(center + a, center + b, red, 1.6f);
            if (age < 25f)
                DrawLine(center + a * (1f + age / 18f), center + b * (1f + age / 18f),
                    red * (1f - age / 25f), 2f);
        }
        for (int i = 0; i < 3; i++)
        {
            float angle = -age * .025f + i * MathHelper.TwoPi / 3f;
            Vector2 cross = center + angle.ToRotationVector2() * new Vector2(40f, 27f);
            DrawLine(cross - Vector2.UnitX * 4f, cross + Vector2.UnitX * 4f, white, 2f);
            DrawLine(cross - Vector2.UnitY * 4f, cross + Vector2.UnitY * 4f, white, 2f);
        }
        // 生命体征折线随脉冲起伏，使用已有像素纹理，无额外特效贴图依赖。
        Vector2 baseline = center + new Vector2(-22f, -38f);
        Vector2[] ecg = [new(0, 0), new(10, 0), new(15, -3), new(19, 6),
            new(23, -10 * pulse), new(28, 3), new(33, 0), new(44, 0)];
        for (int i = 1; i < ecg.Length; i++)
            DrawLine(baseline + ecg[i - 1], baseline + ecg[i], white, 1.5f);
        return false;
    }

    internal static void DrawLine(Vector2 start, Vector2 end, Color color, float width)
    {
        Vector2 delta = end - start;
        Main.EntitySpriteDraw(TextureAssets.MagicPixel.Value, start - Main.screenPosition,
            new Rectangle(0, 0, 1, 1), color, delta.ToRotation(), new Vector2(0f, .5f),
            new Vector2(delta.Length(), width), SpriteEffects.None);
    }
}
