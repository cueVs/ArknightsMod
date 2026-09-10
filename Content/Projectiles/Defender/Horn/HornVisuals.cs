using System;
using ArknightsMod.Common.VisualEffects;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;

namespace ArknightsMod.Content.Projectiles.Defender.Horn;

internal static class HornVisuals
{
    internal static readonly Color Gold = new(255, 206, 100), White = new(255, 250, 215);
    internal static void Sparks(Vector2 center, int count, float speed)
    {
        if (Main.dedServ) return;
        for (int i = 0; i < count; i++)
        {
            Vector2 velocity = Main.rand.NextVector2Circular(speed, speed);
            Dust dust = Dust.NewDustPerfect(center, i % 4 == 0 ? DustID.Smoke : DustID.Torch,
                velocity, 100, default, Main.rand.NextFloat(.8f, 1.6f));
            dust.noGravity = i % 4 != 0;
            if (i % 4 == 1) OperatorTextureEffects.Mote(center, velocity, Gold, Main.rand.NextFloat(6f, 13f));
        }
    }
    internal static void Muzzle(Vector2 center, Vector2 aim, bool counter)
    {
        if (Main.dedServ) return;
        for (int i = 0; i < (counter ? 12 : 7); i++)
        {
            Vector2 velocity = aim.RotatedByRandom(.3f) * Main.rand.NextFloat(2f, 7f);
            OperatorTextureEffects.Mote(center, velocity, i % 2 == 0 ? Gold : White, Main.rand.NextFloat(8f, 16f), 14);
        }
        OperatorTextureEffects.Puff(center, aim * .7f - Vector2.UnitY * .5f, new Color(92, 86, 76), 23f, 24);
    }
    internal static void Detonation(Vector2 center, float radius)
    {
        if (Main.dedServ) return;
        for (int i = 0; i < 10; i++)
        {
            Vector2 scatter = Main.rand.NextVector2Circular(radius * .4f, radius * .3f);
            Vector2 velocity = scatter.SafeNormalize(Vector2.UnitY) * Main.rand.NextFloat(.8f, 2.6f);
            OperatorTextureEffects.Puff(center + scatter, velocity - Vector2.UnitY * .4f,
                new Color(84, 73, 63), Main.rand.NextFloat(28f, 50f), Main.rand.Next(24, 40));
            OperatorTextureEffects.Mote(center + scatter * .5f, velocity * 2f,
                i % 3 == 0 ? White : new Color(255, 153, 57), Main.rand.NextFloat(13f, 23f), 20);
        }
        Sparks(center, 24, 7f);
    }
    internal static void Explosion(Vector2 center, float progress, float radius)
    {
        float t = MathHelper.Clamp(progress, 0f, 1f), fade = MathF.Pow(1f - t, 3f);
        OperatorTextureEffects.Glow(center, radius * (.6f + .9f * t), new Color(255, 152, 55), fade * .9f);
        OperatorTextureEffects.Glow(center, 30f + 45f * t, White, fade);
    }
    internal static void Bash(Vector2 center, float angle, float progress)
    {
        Vector2 impact = center + angle.ToRotationVector2() * (38f + progress * 30f);
        OperatorTextureEffects.Glow(impact, 45f, Gold, (1f - progress) * .55f);
    }
    internal static void Flare(Vector2 center, float radius, int remaining)
    {
        float fade = Math.Min(1f, remaining / 45f);
        OperatorTextureEffects.Glow(center, radius * .75f, Gold, fade * .16f);
        OperatorTextureEffects.Glow(center, 20f, White, fade * .6f);
    }
}
