using System;
using ArknightsMod.Common.Particle;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Projectiles.Caster.Harmonie;

// 青绿乐谱与香槟金音符，直接复用已有音符纹理；不创建技能图标，也不切换 SpriteBatch。
internal static class HarmonieVisuals
{
    internal static readonly Color Mint = new(126, 237, 191);
    internal static readonly Color Gold = new(249, 220, 155);
    private static readonly Color Ivory = new(240, 255, 224);
    private static readonly Color Teal = new(43, 104, 100);
    private const string NoteRoot = "ArknightsMod/Content/Projectiles/Guard/Saki/Note";
    internal static string NotePath(int seed) => NoteRoot + (1 + (seed & int.MaxValue) % 4);
    internal static Color Light(Color color, float opacity)
        => new Color(color.R, color.G, color.B, 0) * MathHelper.Clamp(opacity, 0f, 1f);

    private static void Line(Vector2 a, Vector2 b, Color color, float width)
    {
        Vector2 delta = b - a;
        float length = delta.Length();
        if (!float.IsFinite(length) || length < .01f || length > 160f) return;
        Main.EntitySpriteDraw(TextureAssets.MagicPixel.Value, a - Main.screenPosition, new Rectangle(0, 0, 1, 1),
            color, delta.ToRotation(), new Vector2(0f, .5f), new Vector2(length, Math.Max(.4f, width)), SpriteEffects.None);
    }
    private static void Glow(Vector2 center, Vector2 size, Color color)
    {
        Texture2D glow = TextureAssets.Extra[ExtrasID.ThePerfectGlow].Value;
        Main.EntitySpriteDraw(glow, center - Main.screenPosition, null, color, 0f, glow.Size() * .5f,
            size / glow.Size(), SpriteEffects.None);
    }
    private static void Arc(Vector2 center, float radius, float start, float sweep, float squash, Color color, float width)
    {
        int steps = Math.Clamp((int)(MathF.Abs(sweep) * radius / 5f), 12, 72);
        Vector2 previous = center + new Vector2(MathF.Cos(start), MathF.Sin(start) * squash) * radius;
        for (int i = 1; i <= steps; i++)
        {
            float angle = start + sweep * i / steps;
            Vector2 next = center + new Vector2(MathF.Cos(angle), MathF.Sin(angle) * squash) * radius;
            Line(previous, next, color, width);
            previous = next;
        }
    }
    internal static void Ripple(Vector2 center, float radius, float phase, float squash, Color color, float width)
        => Arc(center, radius, phase, MathHelper.TwoPi, squash, color, width);

    internal static void Note(Vector2 center, float radius, float time, int seed, float flightRotation, float opacity = 1f)
    {
        if (Main.dedServ || opacity <= 0f) return;
        Texture2D texture = ModContent.Request<Texture2D>(NotePath(seed)).Value;
        float scale = radius * 2.7f / texture.Height;
        float rotation = MathF.Sin(time * .07f + seed) * .12f + MathF.Sin(flightRotation) * .08f;
        Vector2 origin = texture.Size() * .5f;
        Glow(center, new Vector2(radius * 5f), Light(Mint, opacity * .28f));
        // 深青描边保证明亮背景可读，音符始终基本竖直；飞行方向只作用于谱线。
        for (int i = 0; i < 4; i++)
        {
            Vector2 offset = (i * MathHelper.PiOver2).ToRotationVector2() * 1.1f;
            Main.EntitySpriteDraw(texture, center + offset - Main.screenPosition, null,
                new Color(15, 51, 47) * opacity, rotation, origin, scale, SpriteEffects.None);
        }
        Main.EntitySpriteDraw(texture, center - Main.screenPosition, null,
            Color.Lerp(Mint, Gold, seed % 3 == 0 ? .85f : .15f) * opacity,
            rotation, origin, scale, SpriteEffects.None);
        Main.EntitySpriteDraw(texture, center - Main.screenPosition, null,
            Light(Ivory, opacity * .35f), rotation, origin, scale, SpriteEffects.None);
        Arc(center, radius * 1.6f, time * .035f + seed, 1.5f, .68f, Light(Gold, opacity * .6f), .8f);
    }

    internal static void Trail(Projectile projectile, bool echo)
    {
        float lifeFade = MathHelper.Clamp(projectile.timeLeft / 15f, 0f, 1f);
        // 五条等距流线随弹道弯曲；蓄能音符可读，远处则先看到细长的青金色乐谱。
        for (int i = projectile.oldPos.Length - 1; i > 0; i--)
        {
            if (projectile.oldPos[i] == Vector2.Zero || projectile.oldPos[i - 1] == Vector2.Zero) continue;
            Vector2 a = projectile.oldPos[i] + projectile.Size * .5f, b = projectile.oldPos[i - 1] + projectile.Size * .5f;
            float fade = (1f - i / (float)projectile.oldPos.Length) * lifeFade;
            Vector2 normal = (b - a).SafeNormalize(Vector2.UnitX).RotatedBy(MathHelper.PiOver2);
            float separation = (echo ? 1.1f : 2f) * fade;
            Line(a, b, Teal * (fade * .28f), separation * 5f);
            for (int staff = -2; staff <= 2; staff++)
            {
                Vector2 offset = normal * staff * separation;
                Line(a + offset, b + offset, Light(staff == 0 ? Gold : Mint, fade * (staff == 0 ? .7f : .36f)), .65f);
            }
        }
    }

    internal static void Staff(Vector2 center, float width, float age, float opacity)
    {
        for (int line = -2; line <= 2; line++)
        {
            Vector2 previous = center + new Vector2(-width * .5f, line * 3.5f);
            for (int i = 1; i <= 20; i++)
            {
                float t = i / 20f;
                Vector2 next = center + new Vector2((t - .5f) * width,
                    line * 3.5f + MathF.Sin(t * MathHelper.Pi) * MathF.Sin(age * .025f) * 6f);
                Line(previous, next, Light(line == 0 ? Gold : Mint, opacity * MathF.Sin(t * MathHelper.Pi)), .75f);
                previous = next;
            }
        }
    }

    internal static void Scatter(Vector2 center, int count, float speed)
    {
        if (Main.dedServ || ParticleManager.activeParticles.Count >= 1000) return;
        // 音符颗粒复用模组纹理，较大粒子限制为少量，其余是细小金绿光屑。
        for (int i = 0; i < count; i++)
        {
            Vector2 velocity = Main.rand.NextVector2Circular(speed, speed) - Vector2.UnitY * .35f;
            if (i < 3)
                new HarmonieNoteParticle(center, velocity, Main.rand.Next(4), i % 2 == 0 ? Mint : Gold).Spawn();
            else
                new DefaultParticle(center, velocity, Main.rand.Next(18, 28), .09f,
                    i % 2 == 0 ? Gold : Mint, true) { Deformation = Vector2.One }.Spawn();
        }
    }
    internal static void Muzzle(Vector2 center, Vector2 aim, bool charged)
    {
        if (Main.dedServ || ParticleManager.activeParticles.Count >= 1000) return;
        if (charged) new HarmonieNoteParticle(center, aim * 2f, Main.rand.Next(4), Gold).Spawn();
        new DefaultParticle(center, aim * 2f, 12, .12f, Mint, true) { Deformation = Vector2.One }.Spawn();
    }

    internal static void Explosion(Vector2 center, float progress, float radius, int seed)
    {
        float t = MathHelper.Clamp(progress, 0f, 1f), fade = MathF.Pow(1f - t, 1.4f);
        float spread = radius * (.12f + .88f * (1f - MathF.Pow(1f - t, 3f)));
        Glow(center, new Vector2(spread * 2f), Light(Mint, fade * .28f));
        Glow(center, new Vector2(27f * (1f - t) + 5f), Light(Ivory, fade));
        // 三道相位错开的声波，外侧甩出四枚音符，中间是一小段散开的五线谱。
        for (int wave = 0; wave < 3; wave++)
            Arc(center, spread * (1f - wave * .19f), seed + wave * 1.8f + t * .5f,
                4.5f, .85f, Light(wave == 1 ? Gold : Mint, fade * (1f - wave * .15f)), 1.1f);
        Staff(center, spread * 1.5f, seed + t * 15f, fade * .3f);
        for (int i = 0; i < 4; i++)
        {
            float angle = seed * .57f + i * MathHelper.PiOver2 + t * .25f;
            Vector2 position = center + angle.ToRotationVector2() * spread * .78f;
            Note(position, 3.3f * (1f - t * .5f), t * 30f, seed + i, 0f, fade * .9f);
        }
    }

    internal static void SkillAura(Vector2 center, float age, int mode, float bloom)
    {
        Glow(center, new Vector2(80f, 110f), Light(Mint, .13f));
        Staff(center + new Vector2(0, -36f), 82f, age, .5f);
        for (int i = 0; i < 2; i++)
        {
            float angle = age * (mode == 1 ? .055f : .025f) + i * MathHelper.Pi;
            Vector2 pos = center + new Vector2(MathF.Cos(angle) * 37f, MathF.Sin(angle) * 26f);
            Note(pos, 4f, age, i + mode, 0f, .7f);
        }
        Arc(center + new Vector2(0, 20f), 37f, age * .03f, 5.2f, .3f, Light(Gold, .5f), 1f);
        if (bloom > 0)
            Ripple(center, 35f + (1f - bloom) * 35f, age, .8f, Light(Mint, bloom * .6f), 1.2f);
    }

    internal static void Resonance(Vector2 center, float radius, float age, float remaining)
    {
        float fade = Math.Min(MathHelper.Clamp(age / 20f, 0f, 1f), MathHelper.Clamp(remaining / 25f, 0f, 1f));
        float beat = (age % 30f) / 30f;
        Glow(center, new Vector2(radius * 2f), Light(Mint, fade * .10f));
        Ripple(center, radius, 0f, 1f, Light(Mint, fade * .16f), .65f);
        // 五重椭圆谱线和低亮度水光构成技能场，边界完整保留真实作用范围。
        for (int i = 0; i < 5; i++)
            Arc(center, radius * (.50f + i * .075f), age * .01f + i * .05f, 5.4f, .58f,
                Light(i == 2 ? Gold : Mint, fade * .35f), .85f);
        Ripple(center, radius * (.15f + beat * .85f), age, .85f, Light(Ivory, fade * (1f - beat) * .35f), .9f);
        for (int i = 0; i < 7; i++)
        {
            float angle = age * .012f + i * MathHelper.TwoPi / 7f;
            Vector2 offset = new(MathF.Cos(angle) * radius * .78f, MathF.Sin(angle) * radius * .45f);
            offset.Y += MathF.Sin(age * .045f + i) * 5f;
            Note(center + offset, 4.6f, age, i, 0, fade * .8f);
        }
        Staff(center, radius * 1.2f, age, fade * .25f);
    }
}

public sealed class HarmonieNoteParticle : Particle
{
    private readonly int variant;
    private readonly Color tint;
    private readonly float tilt;
    public override string TexturePath => HarmonieVisuals.NotePath(0);
    public override BlendState DrawBlendState => BlendState.Additive;
    public HarmonieNoteParticle(Vector2 position, Vector2 velocity, int variant, Color color)
    {
        this.variant = variant;
        tint = color;
        Position = position;
        Velocity = velocity;
        Lifetime = 28;
        tilt = Main.rand.NextFloat(-.2f, .2f);
    }
    public override void Update()
    {
        Velocity *= .94f;
        Velocity.Y -= .015f;
    }
    public override void Draw()
    {
        float fade = MathF.Sin(MathHelper.Clamp(LifetimeRatio, 0f, 1f) * MathHelper.Pi);
        // 粒子管理器每个类型只缓存一张图；四种音符在绘制时从已有资源缓存选择。
        Texture2D note = ModContent.Request<Texture2D>(HarmonieVisuals.NotePath(variant)).Value;
        Main.spriteBatch.Draw(note, Position - Main.screenPosition, null, tint * (fade * .65f),
            tilt + MathF.Sin(LifetimeRatio * 4f) * .1f, note.Size() * .5f,
            8f / note.Height, SpriteEffects.None, 0f);
    }
}
