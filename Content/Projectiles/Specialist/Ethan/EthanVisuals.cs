using System;
using ArknightsMod.Common.VisualEffects;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;

namespace ArknightsMod.Content.Projectiles.Specialist.Ethan;

internal static class EthanVisuals
{
    internal static readonly Color Mint = new(80, 255, 174), Pink = new(239, 93, 219);
    internal static void Scatter(Vector2 position, int count, float speed)
    {
        if (Main.dedServ) return;
        for (int i = 0; i < count; i++)
        {
            Vector2 velocity = Main.rand.NextVector2Circular(1f, 1f) * speed;
            Color color = i % 5 == 0 ? Pink : Mint;
            Dust dust = Dust.NewDustPerfect(position, DustID.TintableDustLighted, velocity, 120,
                color, Main.rand.NextFloat(.7f, 1.2f));
            dust.noGravity = true;
            if (i % 3 == 0) OperatorTextureEffects.Mote(position, velocity, color, Main.rand.NextFloat(5f, 11f));
        }
    }
    internal static void Burst(Vector2 center, float radius)
    {
        if (Main.dedServ) return;
        for (int i = 0; i < 18; i++)
        {
            Vector2 scatter = Main.rand.NextVector2Circular(radius * .35f, radius * .35f);
            Vector2 velocity = scatter.SafeNormalize(Vector2.UnitY) * Main.rand.NextFloat(1.8f, 5f);
            OperatorTextureEffects.Mote(center + scatter, velocity, i % 4 == 0 ? Pink : Mint,
                Main.rand.NextFloat(8f, 18f), Main.rand.Next(18, 32));
            if (i % 3 == 0)
                OperatorTextureEffects.Puff(center + scatter, velocity * .45f,
                    i % 2 == 0 ? new Color(56, 123, 95) : new Color(101, 58, 114), Main.rand.NextFloat(24f, 40f));
        }
        Scatter(center, 12, 5f);
    }
    internal static void Yoyo(Projectile p)
    {
        // Soft sprite afterimages stay local to the bob; no connected trails or orbit curves.
        for (int i = 1; i < p.oldPos.Length; i += 2)
        {
            if (p.oldPos[i] == Vector2.Zero) continue;
            float fade = 1f - i / (float)p.oldPos.Length;
            OperatorTextureEffects.Glow(p.oldPos[i] + p.Size * .5f, 12f * fade, Mint, fade * .22f);
        }
        OperatorTextureEffects.Glow(p.Center, 30f, Mint, .28f);
    }
    internal static void Pulse(Vector2 center, float progress, float radius)
    {
        float t = MathHelper.Clamp(progress, 0f, 1f);
        float fade = MathF.Pow(1f - t, 3f);
        OperatorTextureEffects.Glow(center, radius * (.45f + t * .75f), Mint, fade * .65f);
        OperatorTextureEffects.Glow(center, 24f + t * 18f, Color.White, fade * .55f);
    }
    internal static void Binding(Vector2 center, float radius, int time)
    {
        float fade = Math.Min(1f, time / 20f);
        OperatorTextureEffects.Cloud(center, radius * 1.4f, new Color(60, 162, 120), fade * .18f, time * .006f);
        OperatorTextureEffects.Glow(center, radius, Mint, fade * .12f);
    }
}
