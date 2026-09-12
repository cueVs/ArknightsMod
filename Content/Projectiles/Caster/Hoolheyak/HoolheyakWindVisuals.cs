using System;
using ArknightsMod.Common.Particle;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;

namespace ArknightsMod.Content.Projectiles.Caster.Hoolheyak;

/// <summary>独立的螺旋风带绘制；全部在局部尺寸内构造，不引用灾厄贴图或全屏滤镜。</summary>
internal static class HoolheyakWindVisuals
{
    private static readonly Color Mint = new(137, 220, 192);
    private static readonly Color Core = new(224, 250, 229);
    private static readonly Color Shade = new(29, 66, 66);
    private static readonly Rectangle Pixel = new(0, 0, 1, 1);

    // MagicPixel 原图并非 1×1；必须显式截取一个像素，避免长度再次乘上贴图宽度。
    private static void Segment(Vector2 a, Vector2 b, Color color, float width)
    {
        Vector2 delta = b - a;
        float length = delta.Length();
        if (!float.IsFinite(length) || length < .01f || length > 40f)
            return;
        Main.EntitySpriteDraw(TextureAssets.MagicPixel.Value, a - Main.screenPosition, Pixel, color,
            delta.ToRotation(), new Vector2(0f, .5f), new Vector2(length + .45f, MathHelper.Clamp(width, .3f, 8f)), SpriteEffects.None);
    }

    internal static void Bolt(Vector2 center, float rotation, float age, float scale)
    {
        Vector2 forward = rotation.ToRotationVector2();
        Vector2 side = forward.RotatedBy(MathHelper.PiOver2);
        for (int strand = 0; strand < 3; strand++)
        {
            Vector2 previous = Vector2.Zero;
            for (int i = 0; i <= 24; i++)
            {
                float t = i / 24f;
                float angle = t * MathHelper.TwoPi * 1.3f - age * .55f + strand * MathHelper.TwoPi / 3f;
                float radius = MathF.Sin(t * MathHelper.Pi) * 9f * scale;
                Vector2 point = center + forward * ((t - .75f) * 56f * scale) + side * MathF.Sin(angle) * radius;
                if (i > 0)
                {
                    float alpha = MathF.Sin(t * MathHelper.Pi) * (.48f + .25f * MathF.Cos(angle));
                    Segment(previous, point, Shade * alpha, 3.5f * scale);
                    Segment(previous, point, Color.Lerp(Mint, Core, t) * alpha, 1.7f * scale);
                }
                previous = point;
            }
        }
    }

    internal static void Vortex(Vector2 center, float age, float scale, float opacity, int seed)
    {
        if (opacity <= 0f)
            return;
        float phase = age * .20f + seed * .71f;
        // 后半圈先画，前半圈后画；窄底宽顶的风带沿竖直轴盘旋，保持透明风眼。
        for (int front = 0; front < 2; front++)
        {
            for (int band = 0; band < 3; band++)
            {
                Vector2 previous = Vector2.Zero;
                for (int i = 0; i <= 64; i++)
                {
                    float t = i / 64f;
                    float angle = phase + band * MathHelper.TwoPi / 3f + t * MathHelper.TwoPi * 2.2f;
                    float depth = MathF.Sin(angle);
                    float radius = (10f + 33f * t) * scale;
                    Vector2 point = center + new Vector2(MathF.Cos(angle) * radius,
                        (52f - t * 118f) * scale + depth * radius * .22f);
                    if (i > 0 && (depth >= 0 ? 1 : 0) == front)
                    {
                        float edge = MathF.Pow(MathF.Sin(t * MathHelper.Pi), .6f);
                        float alpha = edge * opacity * (front == 0 ? .24f : .8f);
                        float width = (1.2f + t * 2.5f) * scale;
                        Segment(previous, point, Shade * alpha * .75f, width + 1.8f);
                        Segment(previous, point, Color.Lerp(Mint, Core, (depth + 1f) * .5f) * alpha, width);
                    }
                    previous = point;
                }
            }
        }
        // 顶部碎风弧随旋转交替显现，避免风柱像完整的弹簧。
        for (int arc = 0; arc < 3; arc++)
        {
            Vector2 previous = Vector2.Zero;
            for (int i = 0; i <= 16; i++)
            {
                float t = i / 16f;
                float angle = -phase * .7f + arc * 2.1f + t * 1.5f;
                Vector2 point = center + new Vector2(MathF.Cos(angle) * 45f, -54f + MathF.Sin(angle) * 11f) * scale;
                if (i > 0)
                    Segment(previous, point, Core * (MathF.Sin(t * MathHelper.Pi) * opacity * .45f), 1.5f * scale);
                previous = point;
            }
        }
    }

    internal static void Mote(Vector2 position, Vector2 velocity, float scale)
    {
        if (Main.dedServ)
            return;
        Dust dust = Dust.NewDustPerfect(position, DustID.Cloud, velocity, 150, Mint, scale);
        dust.noGravity = true;
        dust.noLight = true;
        if (ParticleManager.activeParticles.Count < 1100)
            new DefaultParticle(position, velocity * .65f, 18, .18f * scale, Core, true)
                { Deformation = new Vector2(.5f, 1.7f) }.Spawn();
    }

    // 小颗粒沿风带切线逸出；只补粒子，不改变原有风带与云尘的绘制。
    internal static void WindSpark(Vector2 position, Vector2 velocity, float scale)
    {
        if (Main.dedServ)
            return;
        Dust dust = Dust.NewDustPerfect(position, DustID.TintableDustLighted, velocity, 100, Mint, scale);
        dust.noGravity = true;
        dust.noLight = true;
        if (ParticleManager.activeParticles.Count < 1100)
            new DefaultParticle(position, velocity, 22, .23f * scale, Core, true)
                { Deformation = new Vector2(.35f, 2.4f) }.Spawn();
    }

    internal static void Burst(Vector2 center, Vector2 velocity, int count)
    {
        if (Main.dedServ)
            return;
        for (int i = 0; i < count; i++)
        {
            Vector2 outward = Main.rand.NextVector2CircularEdge(1f, 1f);
            Mote(center + outward * 10f, outward * Main.rand.NextFloat(1f, 3f) + velocity * .15f - Vector2.UnitY, .75f);
            WindSpark(center + outward * 8f, outward.RotatedBy(.7f) * Main.rand.NextFloat(2f, 4f), .9f);
        }
    }
}
