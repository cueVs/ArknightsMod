using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Projectiles.Guard.Blaze;

// Keep impact shake separate: the legacy global shake normalizes velocity every frame,
// so halving only its initial strength would not halve the visible displacement.
public sealed class BlazeImpactShakePlayer : ModPlayer
{
    private int remaining;
    private Vector2 offset, velocity;
    internal void Add(int duration, float strength)
    {
        remaining = Math.Max(remaining, duration);
        offset += Main.rand.NextVector2Circular(strength, strength);
        velocity = Main.rand.NextVector2Circular(1f, 1f).SafeNormalize(Vector2.UnitX) * Math.Max(2f, strength);
    }
    public override void ResetEffects()
    {
        if (remaining > 0) remaining--;
    }
    public override void PostUpdate()
    {
        if (remaining > 0)
        {
            offset += velocity;
            velocity = velocity.SafeNormalize(Vector2.Zero) * 4f;
            if (offset.Length() >= 5f)
            {
                offset = offset.SafeNormalize(Vector2.Zero) * 5f;
                velocity = -4f * offset.SafeNormalize(Vector2.Zero).RotatedByRandom(.5);
            }
        }
        else offset = offset.SafeNormalize(Vector2.Zero) * Math.Max(offset.Length() - 10f, 0f);
    }
    public override void ModifyScreenPosition()
    {
        if (Player.whoAmI == Main.myPlayer) Main.screenPosition += offset * .5f;
    }
    public override void UpdateDead() { remaining = 0; offset = velocity = Vector2.Zero; }
}
