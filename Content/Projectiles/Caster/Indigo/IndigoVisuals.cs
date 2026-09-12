using System;
using ArknightsMod.Common.Particle;
using ArknightsMod.Common.VisualEffects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;

namespace ArknightsMod.Content.Projectiles.Caster.Indigo;

// 淡紫色水光：透明液体轮廓、弧形焦散、珍珠高光与水纹。
// 只使用现有纹理和局部曲线，不切换 SpriteBatch，不依赖额外技能图或全屏滤镜。
internal static class IndigoVisuals
{
    internal static readonly Color Lilac = new(184, 157, 247);
    internal static readonly Color Pearl = new(235, 225, 255);
    private static readonly Color Blue = new(148, 210, 245);
    private static readonly Color Deep = new(58, 42, 109);
    private static readonly Rectangle Pixel = new(0, 0, 1, 1);

    internal static Color Light(Color color, float opacity)
        => new Color(color.R, color.G, color.B, 0) * MathHelper.Clamp(opacity, 0f, 1f);

    private static void Line(Vector2 start, Vector2 end, Color color, float width)
    {
        Vector2 delta = end - start;
        float length = delta.Length();
        if (!float.IsFinite(length) || length < .01f || length > 160f) return;
        Main.EntitySpriteDraw(TextureAssets.MagicPixel.Value, start - Main.screenPosition, Pixel,
            color, delta.ToRotation(), new Vector2(0f, .5f), new Vector2(length, Math.Max(.35f, width)), SpriteEffects.None);
    }

    private static void Glow(Vector2 center, Vector2 size, Color color, float rotation = 0f)
    {
        Texture2D glow = TextureAssets.Extra[ExtrasID.ThePerfectGlow].Value;
        Main.EntitySpriteDraw(glow, center - Main.screenPosition, null, color, rotation, glow.Size() * .5f,
            size / glow.Size(), SpriteEffects.None);
    }

    internal static void Ripple(Vector2 center, float radius, float phase, float squash, Color color, float width)
        => Arc(center, radius, phase, MathHelper.TwoPi, squash, 0f, color, width);

    private static void Arc(Vector2 center, float radius, float start, float sweep, float squash,
        float rotation, Color color, float width)
    {
        int segments = Math.Clamp((int)(MathF.Abs(sweep) * radius / 4f), 10, 64);
        Vector2 previous = center + (new Vector2(MathF.Cos(start), MathF.Sin(start) * squash) * radius).RotatedBy(rotation);
        for (int i = 1; i <= segments; i++)
        {
            float angle = start + sweep * i / segments;
            Vector2 next = center + (new Vector2(MathF.Cos(angle), MathF.Sin(angle) * squash) * radius).RotatedBy(rotation);
            Line(previous, next, color, width);
            previous = next;
        }
    }

    internal static void Droplet(Vector2 center, float radius, float time, int seed, float rotation, float opacity = 1f)
    {
        if (Main.dedServ || opacity <= 0f) return;
        // 前端饱满，后端收窄；轻微呼吸形变让储能像悬浮的水，而不是固定形状的宝石。
        float wobble = MathF.Sin(time * .075f + seed * 1.3f);
        const int count = 32;
        Span<Vector2> rim = stackalloc Vector2[count];
        float top = float.MaxValue, bottom = float.MinValue;
        for (int i = 0; i < count; i++)
        {
            float angle = i * MathHelper.TwoPi / count;
            float cos = MathF.Cos(angle), sin = MathF.Sin(angle);
            Vector2 point = new((cos * 1.25f - .18f * MathF.Cos(angle * 2f)) * (1f + wobble * .04f),
                sin * (.86f + .23f * cos) * (1f - wobble * .05f));
            rim[i] = center + (point * radius).RotatedBy(rotation);
            top = Math.Min(top, rim[i].Y);
            bottom = Math.Max(bottom, rim[i].Y);
        }
        Glow(center, new Vector2(radius * 4.3f), Light(Lilac, opacity * .3f));
        // 扫描填充真正的圆润轮廓；深色底层保留液体厚度，亮背景下仍然可辨。
        for (float y = top + .5f; y < bottom; y += 1.25f)
        {
            float left = float.MaxValue, right = float.MinValue;
            for (int i = 0; i < count; i++)
            {
                Vector2 a = rim[i], b = rim[(i + 1) % count];
                if ((a.Y <= y && b.Y > y) || (b.Y <= y && a.Y > y))
                {
                    float x = a.X + (b.X - a.X) * ((y - a.Y) / (b.Y - a.Y));
                    left = Math.Min(left, x); right = Math.Max(right, x);
                }
            }
            if (right > left)
            {
                float depth = (y - top) / Math.Max(1f, bottom - top);
                Color fill = Color.Lerp(new Color(118, 104, 180), Deep, depth);
                Line(new Vector2(left, y), new Vector2(right, y), fill * (opacity * .86f), 1.45f);
            }
        }
        for (int i = 0; i < count; i++)
        {
            float glint = .45f + .35f * MathF.Sin(i * MathHelper.TwoPi / count + time * .025f);
            Line(rim[i], rim[(i + 1) % count], Light(Lilac, opacity * .24f), 2.5f);
            Line(rim[i], rim[(i + 1) % count], Light(Pearl, opacity * glint), .8f);
        }
        Glow(center, new Vector2(radius * 2.2f, radius * 1.6f), Light(Lilac, opacity * .32f), rotation);
        // 两道不同方向的液面焦散和一小段高光；没有尖角或方形元素。
        Arc(center, radius * .75f, -2.7f, 2.2f, .7f, rotation, Light(Pearl, opacity * .9f), 1.1f);
        Arc(center + new Vector2(radius * .12f, 0).RotatedBy(rotation), radius * .63f,
            .15f + wobble * .18f, 2.1f, .65f, rotation, Light(Blue, opacity * .6f), .7f);
        Vector2 highlight = center + new Vector2(radius * .1f, -radius * .4f).RotatedBy(rotation);
        Glow(highlight, new Vector2(radius * .7f, radius * .35f), Light(Pearl, opacity), rotation - .3f);
    }

    internal static void Trail(Projectile projectile, bool wisp)
    {
        float lifetimeFade = MathHelper.Clamp(projectile.timeLeft / 15f, 0f, 1f);
        for (int i = projectile.oldPos.Length - 1; i > 0; i--)
        {
            if (projectile.oldPos[i] == Vector2.Zero || projectile.oldPos[i - 1] == Vector2.Zero) continue;
            Vector2 a = projectile.oldPos[i] + projectile.Size * .5f;
            Vector2 b = projectile.oldPos[i - 1] + projectile.Size * .5f;
            float fade = (1f - i / (float)projectile.oldPos.Length) * lifetimeFade;
            float width = (wisp ? 3.5f : 7f) * fade;
            Vector2 side = (b - a).SafeNormalize(Vector2.UnitX).RotatedBy(MathHelper.PiOver2);
            Line(a, b, new Color(92, 69, 151) * (fade * .5f), width * 1.7f);
            Line(a, b, Light(Lilac, fade * .32f), width);
            Line(a + side * width * .4f, b + side * width * .4f, Light(Blue, fade * .5f), .7f);
            Line(a - side * width * .5f, b - side * width * .5f, Light(Pearl, fade * .4f), .65f);
            if (i % 3 == 0) Glow(a, new Vector2(width * 3f), Light(Lilac, fade * .22f));
        }
    }

    internal static void Scatter(Vector2 center, int count, float speed)
    {
        if (Main.dedServ || ParticleManager.activeParticles.Count >= 1000) return;
        for (int i = 0; i < count; i++)
        {
            Vector2 velocity = Main.rand.NextVector2Circular(speed, speed) - Vector2.UnitY * .2f;
            new DefaultParticle(center, velocity, Main.rand.Next(18, 30), Main.rand.NextFloat(.07f, .14f),
                i % 3 == 0 ? Pearl : i % 3 == 1 ? Lilac : Blue, true)
                { Deformation = new Vector2(.75f, 1.15f) }.Spawn();
        }
    }

    internal static void Muzzle(Vector2 center, Vector2 aim, bool charged)
    {
        if (Main.dedServ || ParticleManager.activeParticles.Count >= 1000) return;
        for (int i = 0; i < (charged ? 6 : 2); i++)
            OperatorTextureEffects.Mote(center, aim.RotatedByRandom(.6f) * Main.rand.NextFloat(1f, 3f),
                i % 2 == 0 ? Pearl : Lilac, 5f, 16);
    }

    internal static void Explosion(Vector2 center, float progress, float radius, int seed)
    {
        float t = MathHelper.Clamp(progress, 0f, 1f);
        float fade = MathF.Pow(1f - t, 1.5f);
        float spread = radius * (.16f + .84f * (1f - MathF.Pow(1f - t, 3f)));
        Glow(center, new Vector2(spread * 2f), Light(Lilac, fade * .42f));
        Glow(center, new Vector2(25f * (1f - t) + 6f), Light(Pearl, fade));
        // 开花般的水幕分成六瓣，每瓣是一条弯曲的水丝；外缘有不等速的涟漪。
        for (int petal = 0; petal < 6; petal++)
        {
            float phase = petal * MathHelper.TwoPi / 6f + seed * .37f;
            Vector2 previous = center;
            for (int segment = 1; segment <= 10; segment++)
            {
                float f = segment / 10f;
                float angle = phase + MathF.Sin(f * MathHelper.Pi) * (.8f - t * .3f);
                Vector2 next = center + angle.ToRotationVector2() * (spread * f);
                Color color = Light(petal % 2 == 0 ? Lilac : Blue, fade * MathF.Sin(f * MathHelper.Pi) * .75f);
                Line(previous, next, color * .45f, 3f);
                Line(previous, next, color, .85f);
                previous = next;
            }
            if (t < .85f)
                Glow(previous, new Vector2(5f, 3f) * fade, Light(Pearl, fade * .8f), phase);
        }
        Arc(center, spread, seed + t * .3f, 4.8f, .65f + .2f * t, .15f, Light(Pearl, fade * .75f), 1.1f);
        Arc(center, spread * .72f, -seed - t * .5f, 5.5f, .78f, -.3f, Light(Blue, fade * .55f), .8f);
        Arc(center, spread * .43f, seed, 4.2f, .7f, .4f, Light(Lilac, fade * .45f), .7f);
    }

    internal static void SkillAura(Vector2 center, float age, int mode, float bloom)
    {
        float pulse = .65f + .35f * MathF.Sin(age * .08f);
        Glow(center, new Vector2(84f, 110f), Light(Lilac, .09f + pulse * .04f));
        Vector2 feet = center + new Vector2(0, 22f);
        Ripple(feet, 37f + bloom * 12f, 0f, .28f, Light(Lilac, .5f), 1.1f);
        if (mode == 1)
        {
            // 灯塔守卫者：珍珠色的光弧在身体两侧向上汇聚。
            for (int i = 0; i < 2; i++)
                Arc(center + new Vector2(0, -8f), 33f, age * .06f + i * MathHelper.Pi,
                    1.8f, 1.25f, 0f, Light(Pearl, .6f), 1.2f);
        }
        else
        {
            // 光影迷宫：三条倾斜椭圆组成流动潮汐，不使用矩形网格。
            for (int i = 0; i < 3; i++)
                Arc(center, 44f, -age * .025f + i, 4.8f, .43f,
                    i * MathHelper.Pi / 3f + age * .007f, Light(i == 1 ? Blue : Lilac, .42f), .9f);
            Ripple(feet, 46f, 0f, .28f, Light(Pearl, .3f), .7f);
        }
        if (bloom > 0f)
            Ripple(center, 35f + (1f - bloom) * 40f, 0f, .8f, Light(Pearl, bloom * .6f), 1f);
    }

    internal static void Seal(Vector2 center, float radius, float age, float opacity)
    {
        float tick = (age % 30f) / 30f;
        float pulse = 1f - tick;
        Glow(center, new Vector2(radius * 2.3f), Light(Lilac, opacity * (.05f + pulse * .13f)));
        for (int i = 0; i < 3; i++)
            Arc(center, radius * (.72f + i * .13f), age * .027f + i * 2f, 4.7f, .55f,
                i * MathHelper.Pi / 3f, Light(i == 1 ? Blue : Lilac, opacity * (.4f + pulse * .3f)), 1f);
        if (pulse > .1f)
            Ripple(center, radius * (.7f + tick * .5f), age, .8f, Light(Pearl, opacity * pulse * .55f), .8f);
    }
}
