using System;
using ArknightsMod.Common.Particle;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Projectiles.Medic.Sussurro;

// 所有尺寸均为世界像素；仅复用本模组柔光贴图，不依赖灾厄贴图或着色器。
internal static class SussurroVisuals
{
    internal static readonly Color Emerald = new(34, 214, 100);
    internal static readonly Color Mint = new(134, 255, 186);
    internal static readonly Color Heart = new(237, 255, 207);
    private static Texture2D Glow => ModContent.Request<Texture2D>("ArknightsMod/Common/Particle/DefaultParticle").Value;

    private static bool Visible(Vector2 position) => !Main.dedServ
        && Math.Abs(position.X - Main.screenPosition.X - Main.screenWidth * .5f) < Main.screenWidth * .5f + 300f
        && Math.Abs(position.Y - Main.screenPosition.Y - Main.screenHeight * .5f) < Main.screenHeight * .5f + 300f;

    private static void Spawn(Vector2 position, Vector2 velocity, SussurroLightKind kind, int life,
        float size, float rotation = 0f, Color? color = null)
    {
        // 高速激光一帧内多次更新；限制屏外粒子及总量，群怪穿透也不无限叠加视觉。
        if (!Visible(position) || ParticleManager.activeParticles.Count >= 1100)
            return;
        new SussurroLightParticle
        {
            Position = position, Velocity = velocity, Kind = kind, Lifetime = life,
            Scale = size, Rotation = rotation, Color = color ?? Mint
        }.Spawn();
    }

    internal static void Needle(Vector2 position, Vector2 velocity, float size, int life)
        => Spawn(position, velocity * .45f, SussurroLightKind.Needle, life, size, velocity.ToRotation());

    internal static void Firefly(Vector2 position, Vector2 velocity, float size)
        => Spawn(position, velocity, SussurroLightKind.Mote, Main.rand.Next(18, 30), size,
            Main.rand.NextFloat(MathHelper.TwoPi), Main.rand.NextBool(3) ? Heart : Mint);

    internal static void Muzzle(Vector2 position, float direction, float size)
    {
        Spawn(position, Vector2.Zero, SussurroLightKind.Muzzle, 20, size, direction);
        for (int i = 0; i < 6; i++)
            Firefly(position, direction.ToRotationVector2().RotatedByRandom(.9f) * Main.rand.NextFloat(1f, 3f), size * .65f);
    }

    internal static void Impact(Vector2 position, float direction, float size)
    {
        if (!Visible(position))
            return;
        size = MathHelper.Clamp(size, .4f, 1.5f);
        Spawn(position, Vector2.Zero, SussurroLightKind.Flower, 30, size, direction);
        // FragmentNebula 的黄金角散点与切向火花，收束成绿色花状爆发。
        for (int i = 0; i < 14; i++)
        {
            float angle = direction + i * 2.399963f;
            Vector2 radial = angle.ToRotationVector2();
            float radius = MathF.Sqrt((i + 1f) / 14f) * 23f * size;
            Firefly(position + radial * radius, radial.RotatedBy(.45f) * (1.2f + i * .17f) * size, size * .8f);
        }
        for (int i = 0; i < 8; i++)
        {
            float angle = direction + MathHelper.TwoPi * i / 8f;
            Spawn(position, angle.ToRotationVector2() * (3f + i % 3) * size,
                SussurroLightKind.Streak, 19, size, angle, i % 2 == 0 ? Heart : Emerald);
        }
        for (int i = 0; i < 5; i++)
        {
            Dust dust = Dust.NewDustPerfect(position, DustID.GreenTorch,
                Main.rand.NextVector2Circular(3f, 3f) * size, 100, default, 1.15f * size);
            dust.noGravity = true;
        }
    }

    internal static void HealBloom(Vector2 position, float size)
    {
        if (!Visible(position))
            return;
        for (int i = 0; i < 16; i++)
        {
            float angle = i * MathHelper.TwoPi / 16f;
            Vector2 radial = angle.ToRotationVector2();
            Firefly(position + radial * new Vector2(26f, 34f) * size,
                radial.RotatedBy(.65f) * 1.4f + new Vector2(0f, -1.2f), size * .8f);
        }
        for (int i = 0; i < 4; i++)
            Spawn(position + new Vector2((i - 1.5f) * 15f, Main.rand.NextFloat(-8f, 16f)),
                new Vector2((i - 1.5f) * .15f, -1.1f - i * .12f),
                SussurroLightKind.Cross, 34 + i * 3, size * .85f, 0f, Mint);
        for (int i = 0; i < 6; i++)
        {
            Dust dust = Dust.NewDustPerfect(position + Main.rand.NextVector2Circular(24f, 26f),
                DustID.GreenTorch, new Vector2(0f, Main.rand.NextFloat(-2.5f, -1f)), 100, default, .9f);
            dust.noGravity = true;
        }
    }

    // AlphaBlend 下 A=0 保留加色 RGB，不打断其他弹幕的 SpriteBatch。
    private static Color Light(Color color, float opacity)
    {
        color *= MathHelper.Clamp(opacity, 0f, 1f);
        color.A = 0;
        return color;
    }

    internal static void Soft(Vector2 center, Vector2 size, Color color, float opacity, float rotation = 0f)
    {
        Texture2D texture = Glow;
        Main.spriteBatch.Draw(texture, center - Main.screenPosition, null, Light(color, opacity), rotation,
            texture.Size() * .5f, size / texture.Size(), SpriteEffects.None, 0f);
    }

    internal static void Line(Vector2 start, Vector2 end, Color color, float width, float opacity, float pigment = 0f)
    {
        Vector2 delta = end - start;
        float length = delta.Length();
        if (!float.IsFinite(length) || length < .01f)
            return;
        Color tint = Light(color, opacity);
        tint.A = (byte)(MathHelper.Clamp(pigment, 0f, 1f) * 255f);
        // MagicPixel 是纹理图集，必须限定为一个源像素，不能把整张纹理当作线条拉伸。
        Main.spriteBatch.Draw(TextureAssets.MagicPixel.Value, start - Main.screenPosition,
            new Rectangle(0, 0, 1, 1), tint, delta.ToRotation(), new Vector2(0f, .5f),
            new Vector2(length, Math.Max(.1f, width)), SpriteEffects.None, 0f);
    }

    internal static void Ring(Vector2 center, Vector2 radius, float rotation, float phase, Color color,
        float opacity, float width = 1f, float petals = 0f)
    {
        int segments = petals == 0f ? 64 : 112;
        Vector2 previous = Vector2.Zero;
        for (int i = 0; i <= segments; i++)
        {
            float angle = i * MathHelper.TwoPi / segments;
            float wave = petals == 0f ? 1f : .76f + .24f * MathF.Cos(angle * petals + phase);
            Vector2 current = center + (angle.ToRotationVector2() * radius * wave).RotatedBy(rotation);
            if (i > 0)
                Line(previous, current, color, width, opacity * (.65f + .35f * MathF.Sin(angle * 3f + phase)));
            previous = current;
        }
    }

    internal static void Star(Vector2 center, float radius, float rotation, float opacity, Color color)
    {
        Soft(center, new Vector2(radius * 4f), Emerald, opacity * .35f);
        Soft(center, new Vector2(radius * 2f, radius * .32f), color, opacity, rotation);
        Soft(center, new Vector2(radius * .32f, radius * 2.6f), color, opacity, rotation);
    }

    internal static void DrawCompression(Vector2 start, Vector2 direction, float length, float fade, bool deep)
    {
        float width = (deep ? 21f : 16f) * MathF.Sqrt(fade);
        float rotation = direction.ToRotation();
        Vector2 end = start + direction * length;
        Soft((start + end) * .5f, new Vector2(length + 80f, width * 7f), Emerald, fade * .48f, rotation);
        // 短段拼接并收尖：亮芯连续贯穿，外缘保留翠绿，不是一根等宽实心矩形。
        const int segments = 40;
        for (int i = 0; i < segments; i++)
        {
            float t = i / (float)segments;
            float taper = Math.Min(1f, (1f - t) * 9f) * (.82f + .18f * MathF.Sin(t * MathF.PI));
            Vector2 a = Vector2.Lerp(start, end, t);
            Vector2 b = Vector2.Lerp(start, end, (i + 1f) / segments);
            // 先铺很薄的绿色实体外缘，再叠加发光层，白天天空下仍保留翠绿辨识度。
            Line(a, b, Emerald, width * taper * 1.12f, fade * .35f, fade * .6f);
            Line(a, b, Emerald, width * taper, fade * .65f);
            Line(a, b, Mint, width * .45f * taper, fade * .85f);
            Line(a, b, Heart, width * .12f * taper, fade);
        }
        Vector2 normal = direction.RotatedBy(MathHelper.PiOver2);
        for (int side = -1; side <= 1; side += 2)
        {
            Vector2 previous = start;
            for (int i = 1; i <= 56; i++)
            {
                float t = i / 56f;
                Vector2 point = start + direction * (length * t)
                    + normal * (MathF.Sin(t * MathHelper.TwoPi * 5f - fade * 6f) * side * width * .75f * MathF.Sin(t * MathF.PI));
                Line(previous, point, Mint, 1f, fade * .45f);
                previous = point;
            }
        }
        Star(start, width * 2.7f, rotation, fade, Heart);
        for (int i = 0; i < 3; i++)
            Ring(start + direction * (28f + 24f * i), new Vector2(7f + i * 2f, width * (1.9f - .25f * i)),
                rotation, fade * 5f + i, Mint, fade * (.65f - .13f * i));
    }

    internal static void DrawButterfly(Vector2 center, float rotation, float age, float size, float opacity)
    {
        float flutter = .42f + .58f * Math.Abs(MathF.Sin(age * .22f));
        Soft(center, new Vector2(74f, 54f) * size, Emerald, opacity * .4f, rotation);
        for (int side = -1; side <= 1; side += 2)
        {
            Vector2 upper = new Vector2(side * 11f * flutter, -6f) * size;
            Vector2 lower = new Vector2(side * 8f * flutter, 7f) * size;
            Soft(center + upper.RotatedBy(rotation), new Vector2(25f * flutter, 29f) * size,
                Mint, opacity * .9f, rotation + side * .55f);
            Soft(center + lower.RotatedBy(rotation), new Vector2(20f * flutter, 20f) * size,
                Emerald, opacity, rotation - side * .55f);
            // 闭合的蝶翼外轮廓与三条叶脉，在亮背景上仍能辨认扑翼。
            Vector2 last = center;
            for (int i = 0; i <= 24; i++)
            {
                float t = i / 24f * MathF.PI;
                float waist = .38f + .62f * MathF.Sqrt(Math.Abs(MathF.Cos(t)));
                Vector2 wing = new(side * MathF.Sin(t) * (16f + 6f * MathF.Cos(t)) * flutter * waist,
                    -MathF.Sin(t * 2f) * 17f);
                Vector2 next = center + (wing * size).RotatedBy(rotation);
                if (i > 0)
                    Line(last, next, Mint, 1.1f * size, opacity * .85f);
                last = next;
            }
            for (int vein = 0; vein < 3; vein++)
            {
                Vector2 tip = new Vector2(side * (15f - vein * 2f) * flutter, -12f + vein * 10f) * size;
                Line(center, center + tip.RotatedBy(rotation), Heart, .75f * size, opacity * .6f);
            }
        }
        Soft(center, new Vector2(5f, 29f) * size, Heart, opacity, rotation);
        for (int side = -1; side <= 1; side += 2)
            Line(center + new Vector2(0f, -8f * size).RotatedBy(rotation),
                center + new Vector2(side * 4f, -15f).RotatedBy(rotation) * size, Mint, .7f * size, opacity * .7f);
        Star(center + new Vector2(0f, -8f * size).RotatedBy(rotation), 4f * size, rotation, opacity, Heart);
    }

    internal static void DrawHealSeal(Vector2 center, float age, float size)
    {
        float p = MathHelper.Clamp(age / 40f, 0f, 1f);
        float fade = MathF.Sin(MathF.PI * p) * (1f - p * .45f);
        float radius = (24f + 38f * (1f - MathF.Pow(1f - p, 3f))) * size;
        Soft(center, new Vector2(radius * 3f), Emerald, fade * .23f);
        Ring(center, new Vector2(radius, radius * .85f), -.2f, age * .05f, Emerald, fade * .8f, 1.6f);
        Ring(center, new Vector2(radius * .82f), .2f, -age * .07f, Mint, fade * .8f, 1.1f, 6f);
        for (int i = 0; i < 6; i++)
        {
            float angle = i * MathHelper.TwoPi / 6f + age * .018f;
            Star(center + angle.ToRotationVector2() * radius * .83f, (3.5f + 2f * fade) * size,
                angle, fade * .8f, Mint);
        }
    }

    internal static void DrawAura(Vector2 center, float age, bool deep, float fade)
    {
        Vector2 foot = center + new Vector2(0f, 24f);
        float radius = deep ? 68f : 42f;
        float strength = fade * (deep ? .65f : .3f);
        Ring(foot, new Vector2(radius, radius * .28f), 0f, age * .045f, Emerald, strength, 1.4f);
        Ring(foot, new Vector2(radius * .82f, radius * .23f), 0f, -age * .06f, Mint, strength * .7f);
        int butterflies = deep ? 3 : 1;
        for (int i = 0; i < butterflies; i++)
        {
            float angle = age * .032f + i * MathHelper.TwoPi / butterflies;
            Vector2 offset = angle.ToRotationVector2() * new Vector2(deep ? 54f : 33f, deep ? 37f : 28f);
            DrawButterfly(center + offset, MathF.Sin(angle) * .35f, age + i * 8f,
                deep ? .62f : .42f, fade * (deep ? .85f : .55f));
        }
        if (deep)
            for (int i = 0; i < 8; i++)
            {
                float angle = age * -.017f + i * MathHelper.TwoPi / 8f;
                Star(foot + angle.ToRotationVector2() * new Vector2(radius, radius * .28f),
                    4f, angle, strength, Mint);
            }
    }
}

internal enum SussurroLightKind { Needle, Mote, Streak, Muzzle, Flower, Cross }

public sealed class SussurroLightParticle : Particle
{
    internal SussurroLightKind Kind;
    public override string TexturePath => "ArknightsMod/Common/Particle/DefaultParticle";
    public override void Update()
    {
        Velocity *= Kind == SussurroLightKind.Needle ? .95f : .94f;
        if (Kind == SussurroLightKind.Mote || Kind == SussurroLightKind.Cross)
            Velocity.Y -= .025f;
    }

    public override void Draw()
    {
        float p = MathHelper.Clamp(LifetimeRatio, 0f, 1f);
        float fade = 1f - MathF.Pow(p, 3f);
        float size = Scale * (1f - .3f * p);
        switch (Kind)
        {
            case SussurroLightKind.Needle:
                // Halley 的内外双层纵向光粒，旋转严格跟随飞行方向。
                SussurroVisuals.Soft(Position, new Vector2(155f, 18f) * size, SussurroVisuals.Emerald, fade * .65f, Rotation);
                SussurroVisuals.Soft(Position, new Vector2(120f, 7f) * size, SussurroVisuals.Mint, fade, Rotation);
                SussurroVisuals.Soft(Position, new Vector2(85f, 2.8f) * size, SussurroVisuals.Heart, fade, Rotation);
                break;
            case SussurroLightKind.Mote:
                SussurroVisuals.Star(Position, 4.5f * size, Rotation + p * .7f, fade * .8f, Color);
                break;
            case SussurroLightKind.Streak:
                SussurroVisuals.Soft(Position, new Vector2(38f, 4f) * size, Color, fade, Rotation);
                break;
            case SussurroLightKind.Muzzle:
                float ring = (12f + 24f * p) * Scale;
                SussurroVisuals.Ring(Position, new Vector2(ring * .32f, ring), Rotation, p * 3f,
                    SussurroVisuals.Mint, (1f - p) * .8f, 1.4f);
                SussurroVisuals.Ring(Position, new Vector2(ring * .23f, ring * .7f), Rotation, -p * 4f,
                    SussurroVisuals.Emerald, (1f - p) * .6f);
                SussurroVisuals.Star(Position, 24f * size, Rotation, (1f - p) * .7f, SussurroVisuals.Heart);
                break;
            case SussurroLightKind.Flower:
                float bloom = (15f + 63f * (1f - MathF.Pow(1f - p, 3f))) * Scale;
                float flowerFade = MathF.Pow(1f - p, 1.4f);
                SussurroVisuals.Soft(Position, new Vector2(bloom * 2.7f), SussurroVisuals.Emerald, flowerFade * .48f);
                SussurroVisuals.Ring(Position, new Vector2(bloom), Rotation, p * 3f,
                    SussurroVisuals.Mint, flowerFade, 1.2f, 7f);
                SussurroVisuals.Ring(Position, new Vector2(bloom * .7f), -Rotation, -p * 5f,
                    SussurroVisuals.Emerald, flowerFade * .75f, 1.4f, 7f);
                SussurroVisuals.Ring(Position, new Vector2(bloom * 1.12f), Rotation, p,
                    SussurroVisuals.Mint, flowerFade * .35f);
                SussurroVisuals.Star(Position, 37f * size, Rotation, flowerFade, SussurroVisuals.Heart);
                break;
            case SussurroLightKind.Cross:
                float crossFade = MathF.Sin(MathF.PI * p);
                float r = 4.5f * size;
                SussurroVisuals.Soft(Position, new Vector2(24f) * size, SussurroVisuals.Emerald, crossFade * .5f);
                SussurroVisuals.Line(Position - Vector2.UnitX * r, Position + Vector2.UnitX * r, Color, 2f * size, crossFade);
                SussurroVisuals.Line(Position - Vector2.UnitY * r, Position + Vector2.UnitY * r, Color, 2f * size, crossFade);
                break;
        }
    }
}
