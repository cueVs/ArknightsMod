using System;
using ArknightsMod.Common.Particle;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Projectiles.Caster.Passenger;

/// <summary>
/// 取自 Fran 的 FranFlatLightningEffects / FranArcParticle / FranFissionArcParticle。
/// 保留空间采样、黄金角扰动、裂纹展开和冷却余弧，改为异客的橙黄配色。
/// </summary>
internal static class PassengerLightningVisuals
{
    internal static readonly Color Body = new(255, 145, 28);
    internal static readonly Color Edge = new(255, 218, 88);
    internal static readonly Color Core = new(255, 247, 210);
    private const float GoldenAngle = 2.39996323f;

    private static bool Visible(Vector2 start, Vector2 end) => !Main.dedServ && !Main.gameMenu
        && Math.Max(start.X, end.X) >= Main.screenPosition.X - 700f
        && Math.Min(start.X, end.X) <= Main.screenPosition.X + Main.screenWidth + 700f
        && Math.Max(start.Y, end.Y) >= Main.screenPosition.Y - 700f
        && Math.Min(start.Y, end.Y) <= Main.screenPosition.Y + Main.screenHeight + 700f;

    internal static void SpawnArc(IEntitySource source, int owner, Vector2 start, Vector2 end, float intensity)
    {
        if (owner == Main.myPlayer)
            Projectile.NewProjectile(source, start, end - start, ModContent.ProjectileType<PassengerArcVisual>(),
                0, 0f, owner, intensity);
    }

    internal static void Fracture(Vector2 start, Vector2 end, float intensity, int seed,
        float growth = 3f, int lifetime = 16, LightningPalette? palette = null)
    {
        float distance = Vector2.Distance(start, end);
        if (!Visible(start, end) || !float.IsFinite(distance) || distance < 7f || distance > 2400f
            || ParticleManager.activeParticles.Count >= 2200)
            return;
        new PassengerFractureParticle(start, end, (palette ?? LightningPalette.Passenger).Body, MathHelper.Clamp(2.5f * intensity, 0.8f, 6.5f),
            seed, lifetime, growth).Spawn();
    }

    internal static void Burst(Vector2 center, float intensity, int seed, LightningPalette? palette = null)
    {
        if (!Visible(center, center))
            return;
        float phase = seed * GoldenAngle;
        for (int i = 0; i < 4; i++)
        {
            Vector2 reach = (phase + MathHelper.TwoPi * i / 4f).ToRotationVector2()
                * (48f + 20f * intensity);
            Fracture(center + reach.SafeNormalize(Vector2.UnitX) * 7f,
                center + reach, intensity, seed + i * 397, 2.5f, 14, palette);
            EmitPoint(center + reach * 0.55f, reach, intensity, i * 2, palette);
        }
    }

    internal static void PrepareImpact(Vector2 center, float intensity, int seed, LightningPalette? palette = null)
    {
        if (Visible(center, center) && ParticleManager.activeParticles.Count < 2200)
            new PassengerLightningImpactParticle(center, intensity, seed, palette).Spawn();
    }

    internal static void Impact(Vector2 center, float intensity, int seed, bool mainStrike, LightningPalette? palette = null)
    {
        LightningPalette colors = palette ?? LightningPalette.Passenger;
        if (!Visible(center, center))
            return;
        // 对应 FranThunderDropVisual 的落点层：横向裂变、四向余电、喷出的细长闪片。
        int fragments = mainStrike ? 14 : 8;
        for (int i = 0; i < fragments && ParticleManager.activeParticles.Count < 1800; i++)
        {
            Vector2 direction = (seed * 0.017f + i * GoldenAngle).ToRotationVector2();
            direction.Y *= 0.65f;
            new DefaultParticle(center, direction * (4f + i % 5) * intensity,
                14 + i % 5, 0.60f * intensity, i % 4 == 0 ? colors.Core : colors.Edge, true)
                { Deformation = new Vector2(0.35f, 2.4f) }.Spawn();
        }
        for (int i = 0; i < 4; i++)
        {
            float side = i % 2 == 0 ? -1f : 1f;
            Vector2 reach = new(side * (100f + i * 16f) * intensity,
                MathF.Sin(seed + i) * 24f * intensity);
            Fracture(center, center + reach, intensity * 0.7f, seed + i * 397, 2.5f, 16, palette);
        }
        // Dust 仅作少量点缀，主体仍由爆裂贴图、扩散环和自定义电弧承担。
        for (int i = 0; i < (mainStrike ? 4 : 2); i++)
        {
            Dust dust = Dust.NewDustPerfect(center, DustID.Torch,
                Main.rand.NextVector2Circular(4f, 3f) - Vector2.UnitY * 2f, 35, colors.Edge, intensity);
            dust.noGravity = true;
        }
        if (mainStrike)
            SoundEngine.PlaySound(SoundID.Item122 with { Volume = 0.48f, Pitch = -0.22f, MaxInstances = 3 }, center);
    }

    internal static void ElectricImpactDust(Vector2 center, float intensity, bool firstHit, LightningPalette? palette = null)
    {
        if (!Visible(center, center))
            return;
        // 只叠加在真实撞敌后的传导爆闪，不用于飞行或三技能，避免沿途凭空爆炸。
        for (int i = 0; i < (firstHit ? 8 : 4); i++)
        {
            Vector2 velocity = (MathHelper.TwoPi * i / (firstHit ? 8f : 4f)
                + Main.rand.NextFloat(-0.15f, 0.15f)).ToRotationVector2()
                * Main.rand.NextFloat(2.5f, 6.5f) * intensity;
            Dust dust = Dust.NewDustPerfect(center, DustID.Electric, velocity, 60,
                (palette ?? LightningPalette.Passenger).Edge, Main.rand.NextFloat(0.65f, 1.05f) * intensity);
            // 保留原版生成的电弧帧，再接管颜色；无需新纹理，避免 Electric 忽略传入颜色。
            dust.type = ModContent.DustType<PassengerElectricDust>();
            dust.noGravity = true;
            dust.noLight = true;
        }
    }

    internal static void EmitSpan(Vector2 start, Vector2 end, float intensity, int phase, LightningPalette? palette = null)
    {
        if (!Visible(start, end) || ParticleManager.activeParticles.Count >= 1800)
            return;
        float length = Vector2.Distance(start, end);
        if (!float.IsFinite(length) || length < 0.5f)
            return;
        int samples = Math.Clamp((int)MathF.Ceiling(length / 24f), 1, 28);
        Vector2 direction = (end - start).SafeNormalize(Vector2.UnitX);
        for (int i = 0; i < samples; i++)
            EmitPoint(Vector2.Lerp(start, end, (i + 0.5f) / samples), direction,
                intensity, phase + i * 2, palette);
    }

    private static void EmitPoint(Vector2 center, Vector2 direction, float intensity, int phase, LightningPalette? palette = null)
    {
        LightningPalette colors = palette ?? LightningPalette.Passenger;
        if (ParticleManager.activeParticles.Count >= 1800)
            return;
        direction = direction.SafeNormalize(Vector2.UnitY);
        Vector2 normal = direction.RotatedBy(MathHelper.PiOver2);
        float oscillation = MathF.Sin(phase * GoldenAngle);
        Color color = phase % 5 == 0 ? colors.Core : Color.Lerp(colors.Body, colors.Edge, 0.5f + oscillation * 0.22f);
        Vector2 offset = normal * oscillation * 7f * intensity;
        Vector2 drift = -direction * (0.28f + intensity * 0.16f) + normal * oscillation * 0.36f;
        if ((phase & 1) == 0)
            new PassengerAfterArcParticle(center + offset, drift,
                direction.RotatedBy(oscillation * 0.22f), color,
                (48f + 18f * MathF.Abs(oscillation)) * intensity,
                (30f + 8f * MathF.Abs(oscillation)) * intensity, 10 + Math.Abs(phase % 4)).Spawn();
        if (phase % 5 == 0)
            new DefaultParticle(center, drift * 1.2f, 6, 0.35f * intensity,
                Color.Lerp(color, colors.Core, 0.38f), true).Spawn();
    }
}

public sealed class PassengerFractureParticle : Particle
{
    private readonly PassengerFissionArc arc;
    private readonly float growth;
    // 该类自己绘制几何；使用已有纹理满足粒子注册入口，不需要额外资源。
    public override string TexturePath => "ArknightsMod/Common/Particle/DefaultParticle";
    public override BlendState DrawBlendState => BlendState.Additive;

    public PassengerFractureParticle(Vector2 start, Vector2 end, Color color, float width,
        int seed, int lifetime, float growthFrames)
    {
        Position = start;
        Color = color;
        Lifetime = lifetime;
        growth = growthFrames;
        arc = new PassengerFissionArc(end - start, seed, width);
    }

    public override void Draw() => arc.Draw(Main.spriteBatch, Position, Color, Time, Lifetime, growth);
}

public sealed class PassengerAfterArcParticle : Particle
{
    private readonly Vector2 pixels;
    private readonly SpriteEffects mirror;
    public override string TexturePath => "ArknightsMod/Content/Textures/PassengerArc";
    public override BlendState DrawBlendState => BlendState.Additive;
    public override int FrameCount => 1;

    public PassengerAfterArcParticle(Vector2 position, Vector2 velocity, Vector2 direction,
        Color color, float length, float width, int lifetime)
    {
        Position = position;
        Velocity = velocity;
        Color = color;
        Lifetime = Math.Clamp(lifetime, 5, 22);
        Rotation = direction.SafeNormalize(Vector2.UnitY).ToRotation() - MathHelper.PiOver2;
        pixels = new Vector2(MathHelper.Clamp(width, 8f, 90f), MathHelper.Clamp(length, 12f, 180f));
        mirror = (Main.rand.NextBool() ? SpriteEffects.FlipHorizontally : SpriteEffects.None)
            | (Main.rand.NextBool() ? SpriteEffects.FlipVertically : SpriteEffects.None);
    }

    public override void Update() => Velocity *= 0.86f;

    public override void Draw()
    {
        float fade = MathF.Pow(1f - LifetimeRatio, 1.45f);
        Vector2 size = pixels * new Vector2(1f - LifetimeRatio * 0.30f, 1f - LifetimeRatio * 0.08f);
        Vector2 scale = size / Texture.Size();
        Vector2 origin = Texture.Size() * 0.5f;
        Main.spriteBatch.Draw(Texture, Position - Main.screenPosition, null, Color * (fade * 0.88f),
            Rotation, origin, scale, mirror, 0f);
        Main.spriteBatch.Draw(Texture, Position - Main.screenPosition, null,
            Color.Lerp(Color, PassengerLightningVisuals.Core, 0.82f) * (fade * 0.28f), Rotation, origin, scale, mirror, 0f);
    }
}
