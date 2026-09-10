using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;

namespace ArknightsMod.Content.Projectiles.Caster.Ebenholz;

// 使用本地几何和透明边缘的程序纹理；不引用灾厄资源，也不改变外部 SpriteBatch 状态。
internal static class EbenholzVisuals
{
    internal static Texture2D Disc;
    internal static readonly Color Gold = new(214, 179, 113);
    private static readonly Color Ivory = new(250, 232, 201);
    private static readonly Rectangle Pixel = new(0, 0, 1, 1);
    internal static readonly Vector3[] Corners = [new(-1,-1,-1), new(1,-1,-1), new(1,1,-1), new(-1,1,-1),
        new(-1,-1,1), new(1,-1,1), new(1,1,1), new(-1,1,1)];
    internal static readonly (int A, int B)[] Edges = [(0,1),(1,2),(2,3),(3,0),(4,5),(5,6),(6,7),(7,4),(0,4),(1,5),(2,6),(3,7)];
    internal static Color[] DiscPixels(int size)
    {
        var pixels = new Color[size * size];
        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float distance = new Vector2((x + .5f) / size * 2f - 1f, (y + .5f) / size * 2f - 1f).Length();
                float opacity = 1f - MathHelper.SmoothStep(0f, 1f, MathHelper.Clamp((distance - .70f) / .27f, 0f, 1f));
                pixels[y * size + x] = Color.White * opacity;
            }
        return pixels;
    }
    internal static void Line(Vector2 a, Vector2 b, Color color, float width)
    {
        Vector2 delta = b - a;
        float length = delta.Length();
        if (!float.IsFinite(a.X) || !float.IsFinite(a.Y) || !float.IsFinite(length) || length < .01f || length > 96f) return;
        Main.EntitySpriteDraw(TextureAssets.MagicPixel.Value, a - Main.screenPosition, Pixel, color,
            delta.ToRotation(), new Vector2(0f, .5f), new Vector2(length, MathHelper.Clamp(width, .35f, 10f)), SpriteEffects.None);
    }
    internal static Color Light(Color color, float opacity) => new Color(color.R, color.G, color.B, 0) * MathHelper.Clamp(opacity, 0f, 1f);
    internal static void Soft(Vector2 center, float radius, Color color)
    {
        if (Main.dedServ || !float.IsFinite(radius) || radius <= 0f || radius > 240f) return;
        if (Disc == null || Disc.IsDisposed)
        {
            Disc = new Texture2D(Main.graphics.GraphicsDevice, 64, 64);
            Disc.SetData(DiscPixels(64));
        }
        Main.EntitySpriteDraw(Disc, center - Main.screenPosition, null, color, 0f,
            Disc.Size() * .5f, radius * 2f / Disc.Width, SpriteEffects.None);
    }
    internal static void ProjectCube(Span<Vector2> result, Vector2 center, float radius, float time, int seed)
    {
        radius = MathHelper.Clamp(radius, 1f, 32f);
        // 参考矩阵核心的三轴旋转，透视分母保持正数，绝不会经过投影奇点。
        Matrix rotation = Matrix.CreateFromYawPitchRoll(time * .047f + seed * .13f,
            time * .034f + seed * .21f, time * .022f + seed * .08f);
        for (int i = 0; i < 8; i++)
        {
            Vector3 point = Vector3.Transform(Corners[i], rotation);
            float perspective = 5f / MathF.Max(3f, 5f + point.Z);
            result[i] = center + new Vector2(point.X, point.Y) * radius * perspective;
        }
    }
    internal static void Cube(Vector2 center, float radius, float time, int seed, float opacity = 1f)
    {
        if (opacity <= 0f) return;
        Span<Vector2> vertices = stackalloc Vector2[8];
        ProjectCube(vertices, center, radius, time, seed);
        float top = float.MaxValue, bottom = float.MinValue;
        foreach (Vector2 vertex in vertices) { top = MathF.Min(top, vertex.Y); bottom = MathF.Max(bottom, vertex.Y); }
        // 黑色凸包用有限的两像素扫描行填充，不把不透明矩形当作黑色辉光。
        for (float y = top + .5f; y < bottom; y += 1.5f)
        {
            float left = float.MaxValue, right = float.MinValue;
            foreach (var edge in Edges)
            {
                Vector2 a = vertices[edge.A], b = vertices[edge.B];
                if ((a.Y <= y && b.Y > y) || (b.Y <= y && a.Y > y))
                {
                    float x = a.X + (b.X - a.X) * ((y - a.Y) / (b.Y - a.Y));
                    left = MathF.Min(left, x); right = MathF.Max(right, x);
                }
            }
            if (right > left) Line(new Vector2(left, y), new Vector2(right, y), new Color(5, 4, 7) * opacity, 1.65f);
        }
        foreach (var edge in Edges)
        {
            Vector2 a = vertices[edge.A], b = vertices[edge.B];
            Line(a, b, Light(Gold, opacity * .15f), 3f);
            Line(a, b, Light(Ivory, opacity * .70f), .75f);
        }
        Soft(vertices[(seed & int.MaxValue) % 8], 2.2f, Light(Ivory, opacity * .8f));
    }
    internal static void Arc(Vector2 center, float radius, float start, float sweep, float squash, Color color, float width)
    {
        if (!float.IsFinite(radius) || radius <= 0f || radius > 230f) return;
        int steps = Math.Clamp((int)(MathF.Abs(sweep) * radius / 7f) + 1, 8, 96);
        Vector2 previous = center + new Vector2(MathF.Cos(start), MathF.Sin(start) * squash) * radius;
        for (int i = 1; i <= steps; i++)
        {
            float angle = start + sweep * i / steps;
            Vector2 next = center + new Vector2(MathF.Cos(angle), MathF.Sin(angle) * squash) * radius;
            Line(previous, next, color, width);
            previous = next;
        }
    }
    internal static void Trail(Projectile p, bool wisp)
    {
        for (int i = p.oldPos.Length - 1; i > 0; i--)
        {
            if (p.oldPos[i] == Vector2.Zero || p.oldPos[i - 1] == Vector2.Zero) continue;
            Vector2 a = p.oldPos[i] + p.Size * .5f, b = p.oldPos[i - 1] + p.Size * .5f;
            float fade = 1f - i / (float)p.oldPos.Length;
            float radius = (wisp ? 4f : 7f) * fade;
            Soft(a, radius, Color.Black * (fade * .7f));
            Vector2 side = (b - a).SafeNormalize(Vector2.UnitX).RotatedBy(MathHelper.PiOver2) * radius * .6f;
            Line(a + side, b + side, Light(Gold, fade * .35f), .8f);
        }
    }
    internal static void Well(Vector2 center, float age, float amount, int seed)
    {
        float radius = 23f + 18f * amount;
        Soft(center, radius * 1.45f, Color.Black * .4f);
        // 外层流线逆向旋转，内层黑核脉动；每条螺旋都在局部半径内。
        for (int strand = 0; strand < 5; strand++)
        {
            Vector2 previous = center;
            for (int i = 0; i <= 26; i++)
            {
                float t = i / 26f;
                float angle = strand * MathHelper.TwoPi / 5f - age * .045f + t * 3.2f + seed;
                float r = radius * (.6f + t * (1f + amount * .8f));
                Vector2 next = center + angle.ToRotationVector2() * r;
                if (i > 0)
                {
                    Line(previous, next, Color.Black * ((1f - t) * .85f), 4f);
                    Line(previous, next, Light(Gold, MathF.Sin(t * MathHelper.Pi) * (.15f + .3f * amount)), 1f);
                }
                previous = next;
            }
        }
        Soft(center, radius, Color.Black);
        Arc(center, radius * .9f, age * .025f, 4.6f, 1f, Light(Ivory, .4f), 1.2f);
        Arc(center, radius * 1.3f, -age * .045f, 5.2f, .38f, Light(Gold, .55f), 1.1f);
        Cube(center, 8f + amount * 4f, age, seed, .9f);
    }
    internal static void Explosion(Vector2 center, float progress, float radius, int seed)
    {
        progress = MathHelper.Clamp(progress, 0f, 1f);
        float expansion = 1f - MathF.Pow(1f - progress, 4f);
        float fade = MathF.Pow(1f - progress, 1.4f);
        float r = radius * (.12f + .88f * expansion);
        // 不规则黑色烟冠先于核心绘制；不同瓣的速度/大小错开，避免只有两个同心圆。
        for (int lobe = 0; lobe < 9; lobe++)
        {
            float phase = lobe * MathHelper.TwoPi / 9f + seed * .71f;
            float variation = .5f + .5f * MathF.Sin(seed * 1.13f + lobe * 4.37f);
            float angle = phase + progress * (lobe % 2 == 0 ? .5f : -.4f);
            Vector2 plume = center + angle.ToRotationVector2() * r * (.55f + variation * .32f);
            float puffRadius = r * (.18f + variation * .13f) * (1f - progress * .4f);
            Soft(plume, puffRadius, Color.Black * fade * .8f);
            Arc(plume, puffRadius * .83f, angle - 1.6f + progress * 2f, 1.8f, .8f,
                Light(Gold, fade * .18f), .8f);
        }
        Soft(center, r * .9f, Color.Black * fade);
        Soft(center, r * .56f, new Color(8, 5, 13) * fade);
        Arc(center, r, seed, MathHelper.TwoPi, .94f, Light(Gold, fade * .8f), 2f * fade);
        Arc(center, r * .68f, -seed, MathHelper.TwoPi, 1f, Light(Ivory, fade * .45f), .8f);
        for (int i = 0; i < 7; i++)
        {
            float angle = i * MathHelper.TwoPi / 7f + seed * .63f + progress * .4f;
            Vector2 direction = angle.ToRotationVector2();
            Vector2 tip = center + direction * r * (.8f + (i % 3) * .15f);
            // 短促断裂，不使用从中心伸出的整屏光刺。
            Line(tip - direction * MathF.Min(18f, r * .25f) * fade, tip, Color.Black * fade, 4f * fade);
            Line(tip - direction * MathF.Min(12f, r * .2f) * fade, tip, Light(Ivory, fade * .7f), .8f);
            if (radius > 80f || i % 2 == 0)
                Cube(tip, (radius > 80f ? 3.5f : 1.8f) * fade + 1f, progress * 80f, seed + i, fade);
        }
    }
    internal static void Scatter(Vector2 center, int count, float speed)
    {
        if (Main.dedServ) return;
        for (int i = 0; i < Math.Min(count, 32); i++)
        {
            Vector2 velocity = Main.rand.NextVector2Unit() * Main.rand.NextFloat(.4f, 1f) * speed;
            Dust dust = Dust.NewDustPerfect(center + velocity * 2f, DustID.Smoke, velocity, 80,
                new Color(15, 11, 20), Main.rand.NextFloat(.7f, 1.35f));
            dust.noGravity = dust.noLight = true;
            if (i % 3 == 0)
            {
                Dust spark = Dust.NewDustPerfect(center, DustID.TintableDustLighted, velocity * .8f, 70, Gold, .6f);
                spark.noGravity = true;
            }
        }
    }
}
