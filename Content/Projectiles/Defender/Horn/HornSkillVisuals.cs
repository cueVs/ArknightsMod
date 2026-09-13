using System;
using ArknightsMod.Common.Particle;
using ArknightsMod.Common.VisualEffects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;

namespace ArknightsMod.Content.Projectiles.Defender.Horn;

internal static partial class HornVisuals
{
    // 2: S2装填，4: S2术式/倾泻；3: S3，5: S3过载。只用于表现，不参与伤害。
    private static Color SkillColor(int state) => state switch
    {
        4 => new Color(125, 215, 255),
        3 => new Color(255, 130, 48),
        5 => new Color(255, 55, 35),
        _ => Gold
    };

    internal static void SkillBody(Player player, int state, float age)
    {
        if (Main.dedServ || state == 0) return;
        Color tint = SkillColor(state);
        Lighting.AddLight(player.MountedCenter, tint.ToVector3() * .28f);
        if ((int)age % (state == 5 ? 3 : 5) != 0 || ParticleManager.activeParticles.Count >= 1000) return;
        bool barrage = state is 2 or 4;
        if (barrage)
        {
            // 双侧旋转装填火星，蓝白术式阶段沿相反方向穿过金色火星。
            for (int i = 0; i < 2; i++)
            {
                float angle = age * .065f + i * MathHelper.Pi;
                Vector2 offset = angle.ToRotationVector2() * new Vector2(25f, 30f);
                Vector2 velocity = (angle + MathHelper.PiOver2).ToRotationVector2() * 1.2f + player.velocity * .12f;
                OperatorTextureEffects.Mote(player.MountedCenter + offset, velocity,
                    i == 0 ? tint : Gold, 7f, 18);
            }
        }
        else
        {
            Vector2 position = player.Bottom + new Vector2(Main.rand.NextFloat(-19f, 19f), -Main.rand.NextFloat(4f, 24f));
            Vector2 velocity = new(Main.rand.NextFloat(-.6f, .6f), state == 5 ? -3.5f : -2f);
            new DefaultParticle(position, velocity + player.velocity * .15f, 22, .17f, tint, true)
                { Deformation = new Vector2(.45f, 2.4f) }.Spawn();
            if ((int)age % 15 == 0)
                OperatorTextureEffects.Puff(player.Center + new Vector2(Main.rand.NextFloat(-12f, 12f), -8f),
                    new Vector2(0, -1.2f), new Color(90, 65, 57), state == 5 ? 23f : 15f, 24);
        }
    }

    internal static void SkillBodyGlow(Player player, int state, float age)
    {
        if (state == 0 || Main.dedServ) return;
        float pulse = .5f + .5f * MathF.Sin(age * (state == 5 ? .19f : .09f));
        OperatorTextureEffects.Glow(player.MountedCenter, 62f + pulse * 8f, SkillColor(state), .12f + pulse * .06f);
    }

    internal static void SkillTrail(Projectile projectile, int state)
    {
        if (state == 0 || Main.dedServ || projectile.timeLeft % 3 != 0 || ParticleManager.activeParticles.Count >= 1000) return;
        OperatorTextureEffects.Mote(projectile.Center, -projectile.velocity * .08f, SkillColor(state), 9f, 16);
    }

    internal static void SkillDetonation(Vector2 center, float radius, int state)
    {
        if (state == 0 || Main.dedServ || ParticleManager.activeParticles.Count >= 1000) return;
        int count = state == 5 ? 24 : 16;
        for (int i = 0; i < count; i++)
        {
            Vector2 direction = (MathHelper.TwoPi * i / count + Main.rand.NextFloat(-.06f, .06f)).ToRotationVector2();
            Vector2 velocity = direction * Main.rand.NextFloat(4f, state == 5 ? 12f : 9f);
            new DefaultParticle(center + direction * 8f, velocity, 25, .25f,
                i % 3 == 0 ? White : SkillColor(state), true)
                { Deformation = new Vector2(.35f, state is 2 or 4 ? 3.5f : 2.5f) }.Spawn();
            if (state is 3 or 5 && i % 4 == 0)
                OperatorTextureEffects.Puff(center + direction * radius * .3f, velocity * .2f - Vector2.UnitY,
                    new Color(100, 48, 35), 32f, 32);
        }
    }

    internal static void SkillExplosion(Vector2 center, float progress, float radius, int state)
    {
        if (state == 0 || Main.dedServ) return;
        float t = MathHelper.Clamp(progress, 0f, 1f);
        float fade = (1f - t) * (1f - t);
        Color color = SkillColor(state);
        float ringRadius = radius * (.18f + .8f * MathF.Sqrt(t));
        // 二技能为分段术式冲击环，三技能为双重高热冲击环；过载有更亮的白热核心。
        Ring(center, ringRadius, color, fade * .8f, state is 2 or 4, t * .5f);
        if (state is 3 or 5)
            Ring(center, ringRadius * .72f, Gold, fade * .65f, false, 0f);
        OperatorTextureEffects.Glow(center, radius * .8f, color, fade * .65f);
        if (state is 4 or 5)
        {
            OperatorTextureEffects.Glow(center, 48f + t * 30f, White, fade);
            for (int i = 0; i < 4; i++)
            {
                Vector2 axis = (i * MathHelper.PiOver2 + (state == 4 ? MathHelper.PiOver4 : 0f)).ToRotationVector2();
                Line(center + axis * 10f, center + axis * ringRadius * .8f, White, fade * .8f, 2f);
            }
        }
    }

    private static void Ring(Vector2 center, float radius, Color color, float opacity, bool dashed, float rotation)
    {
        const int segments = 48;
        for (int i = 0; i < segments; i++)
        {
            if (dashed && i % 8 >= 5) continue;
            float a = MathHelper.TwoPi * i / segments + rotation;
            float b = MathHelper.TwoPi * (i + 1) / segments + rotation;
            Line(center + a.ToRotationVector2() * radius, center + b.ToRotationVector2() * radius,
                color, opacity, 2f);
        }
    }

    private static void Line(Vector2 start, Vector2 end, Color color, float opacity, float width)
    {
        Vector2 delta = end - start;
        Main.EntitySpriteDraw(TextureAssets.MagicPixel.Value, start - Main.screenPosition, new Rectangle(0, 0, 1, 1),
            new Color(color.R, color.G, color.B, 0) * opacity, delta.ToRotation(), new Vector2(0f, .5f),
            new Vector2(delta.Length(), width), SpriteEffects.None);
    }
}
