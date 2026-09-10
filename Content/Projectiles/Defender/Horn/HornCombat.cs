using System;
using Microsoft.Xna.Framework;
using Terraria;

namespace ArknightsMod.Content.Projectiles.Defender.Horn;

internal static class HornCombat
{
    private static readonly float[] Barrage = [1.3f,1.4f,1.5f,1.6f,1.7f,1.8f,1.9f,2f,2.2f,2.4f];
    private static readonly float[] FinalLine = [.1f,.15f,.2f,.25f,.3f,.35f,.4f,.5f,.6f,.7f];
    private static readonly float[] Flares = [1.6f,1.65f,1.7f,1.8f,1.85f,1.9f,2f,2.2f,2.4f,2.8f];
    internal static float BarrageMultiplier(int rank) => Barrage[Math.Clamp(rank,0,9)];
    internal static float ArtsMultiplier(int rank) => rank < 3 ? .3f : rank < 6 ? .35f : rank < 7 ? .4f : rank < 9 ? .5f : .6f;
    internal static float FinalLineBonus(int rank) => FinalLine[Math.Clamp(rank,0,9)];
    internal static float FlareMultiplier(int rank) => Flares[Math.Clamp(rank,0,9)];
    internal static bool Clear(Vector2 a, Vector2 b) => Collision.CanHitLine(a, 1, 1, b, 1, 1);
    internal static bool InCircle(Vector2 center, float radius, Rectangle target)
    {
        Vector2 closest = new(Math.Clamp(center.X, target.Left, target.Right), Math.Clamp(center.Y, target.Top, target.Bottom));
        return Vector2.DistanceSquared(center, closest) <= radius * radius;
    }
    internal static bool NearEnemy(Player player, Vector2 aim)
    {
        foreach (NPC npc in Main.ActiveNPCs)
            if (npc.CanBeChasedBy() && InCircle(player.MountedCenter, 85f, npc.Hitbox)
                && Vector2.Dot((npc.Center - player.MountedCenter).SafeNormalize(aim), aim) > .4f
                && Clear(player.MountedCenter, npc.Center)) return true;
        return false;
    }
}
