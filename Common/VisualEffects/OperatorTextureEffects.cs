using System;
using ArknightsMod.Common.Particle;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace ArknightsMod.Common.VisualEffects;

internal static class OperatorTextureEffects
{
    internal const string GlowPath = "ArknightsMod/Common/Particle/DefaultParticle";
    internal const string SmokePath = "ArknightsMod/Assets/GrayScaleTexture/Smoke2";
    internal static void Glow(Vector2 center, float diameter, Color color, float opacity)
    {
        if (Main.dedServ || !float.IsFinite(diameter) || diameter <= 0 || diameter > 240f) return;
        Texture2D texture = ModContent.Request<Texture2D>(GlowPath).Value;
        Color light = new Color(color.R, color.G, color.B, 0) * MathHelper.Clamp(opacity, 0f, 1f);
        Main.EntitySpriteDraw(texture, center - Main.screenPosition, null, light, 0f, texture.Size() * .5f,
            new Vector2(diameter) / texture.Size(), SpriteEffects.None);
    }
    internal static void Cloud(Vector2 center, float diameter, Color color, float opacity, float rotation)
    {
        if (Main.dedServ || !float.IsFinite(diameter) || diameter <= 0 || diameter > 180f) return;
        Texture2D texture = ModContent.Request<Texture2D>(SmokePath).Value;
        Main.EntitySpriteDraw(texture, center - Main.screenPosition, null, color * MathHelper.Clamp(opacity, 0f, 1f),
            rotation, texture.Size() * .5f, new Vector2(diameter) / texture.Size(), SpriteEffects.None);
    }
    internal static void Mote(Vector2 center, Vector2 velocity, Color color, float diameter, int lifetime = 24)
    {
        if (Main.dedServ) return;
        new DefaultParticle(center, velocity, Math.Clamp(lifetime, 8, 45), MathHelper.Clamp(diameter, 3f, 24f) / 64f,
            color, true) { Deformation = Vector2.One }.Spawn();
    }
    internal static void Puff(Vector2 center, Vector2 velocity, Color color, float diameter, int lifetime = 30)
    {
        if (!Main.dedServ) new OperatorSmokeParticle(center, velocity, color, diameter, lifetime).Spawn();
    }
}

public sealed class OperatorSmokeParticle : Particle.Particle
{
    private readonly float diameter;
    private readonly Color tint;
    public override string TexturePath => OperatorTextureEffects.SmokePath;
    public OperatorSmokeParticle(Vector2 position, Vector2 velocity, Color color, float size, int lifetime)
    {
        Position = position;
        Velocity = velocity;
        tint = color;
        diameter = MathHelper.Clamp(size, 6f, 56f);
        Lifetime = Math.Clamp(lifetime, 12, 45);
        Rotation = Main.rand.NextFloat(MathHelper.TwoPi);
    }
    public override void Update()
    {
        Velocity *= .92f;
        Velocity.Y -= .012f;
        Rotation += .018f;
    }
    public override void Draw()
    {
        float t = MathHelper.Clamp(LifetimeRatio, 0f, 1f);
        float fade = MathHelper.Clamp(t * 8f, 0f, 1f) * (1f - t) * .65f;
        OperatorTextureEffects.Cloud(Position, diameter * (1f + t * .8f), tint, fade, Rotation);
    }
}
