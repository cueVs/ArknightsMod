using Microsoft.Xna.Framework;

namespace ArknightsMod.Content.Projectiles.Caster.Passenger;

// 配色随每个粒子保存，不切换全局颜色，惊蛰和异客同时攻击时互不串色。
internal readonly record struct LightningPalette(Color Body, Color Edge, Color Core, Vector3 Light)
{
    internal static readonly LightningPalette Passenger = new(
        new(255, 145, 28), new(255, 218, 88), new(255, 247, 210), new(0.78f, 0.38f, 0.06f));
    internal static readonly LightningPalette Leizi = new(
        new(255, 75, 12), new(255, 235, 20), new(255, 252, 190), new(0.8f, 0.48f, 0.025f));
}
