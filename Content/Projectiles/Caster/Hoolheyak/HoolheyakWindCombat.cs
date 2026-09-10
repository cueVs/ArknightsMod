using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Microsoft.Xna.Framework;

namespace ArknightsMod.Content.Projectiles.Caster.Hoolheyak;

internal static class HoolheyakWindCombat
{
    internal static bool ClearPath(Vector2 start, Vector2 end) => Collision.CanHitLine(start, 1, 1, end, 1, 1);

    // 首领标记、首领的身体/手臂以及多节生物都不接受位移；不临时改写 NPC 的重力或碰撞属性。
    internal static bool CanLift(NPC npc) => npc.CanBeChasedBy() && !npc.boss
        && !NPCID.Sets.ShouldBeCountedAsBoss[npc.type] && npc.realLife < 0
        && npc.aiStyle is not (6 or 37) && npc.knockBackResist > 0f
        && npc.width <= 120 && npc.height <= 120 && npc.type != NPCID.TargetDummy;

    internal static int EnemyIdentity(NPC npc) => npc.realLife >= 0 ? npc.realLife : npc.whoAmI;

    internal static List<NPC> Targets(Vector2 origin, float radius)
    {
        List<NPC> result = new();
        HashSet<int> seen = new();
        foreach (NPC npc in Main.ActiveNPCs)
            if (npc.CanBeChasedBy() && npc.DistanceSQ(origin) <= radius * radius
                && ClearPath(origin, npc.Center) && seen.Add(EnemyIdentity(npc)))
                result.Add(npc);
        return result;
    }

    internal static NPC Closest(Vector2 origin, float radius)
    {
        NPC target = null;
        float best = radius * radius;
        foreach (NPC npc in Main.ActiveNPCs)
        {
            float distance = npc.DistanceSQ(origin);
            if (npc.CanBeChasedBy() && distance < best && ClearPath(origin, npc.Center))
            {
                best = distance;
                target = npc;
            }
        }
        return target;
    }

    // DarkPlasma 的 25:1 惯性与 1.8 像素/帧的缓慢追踪；不采用其全场吸附范围。
    internal static Vector2 Drift(Vector2 velocity, Vector2 center, Vector2 target)
        => (velocity * 25f + (target - center).SafeNormalize(Vector2.Zero) * 1.8f) / 26f;

    internal static float DistanceDamage(float distance) => MathHelper.Lerp(2f / 3f, 1f, MathHelper.Clamp(distance / 480f, 0f, 1f));
}
