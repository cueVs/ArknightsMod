using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;

namespace ArknightsMod.Content.Projectiles.Caster.Ebenholz;

internal static class EbenholzCombat
{
    internal static bool Clear(Vector2 a, Vector2 b) => Collision.CanHitLine(a, 1, 1, b, 1, 1);
    internal static int Identity(NPC npc) => npc.realLife >= 0 ? npc.realLife : npc.whoAmI;
    internal static bool Priority(NPC npc) => npc.boss || NPCID.Sets.ShouldBeCountedAsBoss[npc.type] || npc.lifeMax >= 1500;
    internal static bool CanPull(NPC npc) => npc.CanBeChasedBy() && !npc.boss
        && !NPCID.Sets.ShouldBeCountedAsBoss[npc.type] && npc.realLife < 0 && npc.aiStyle is not (6 or 37)
        && npc.knockBackResist > 0 && npc.width <= 120 && npc.height <= 120 && npc.type != NPCID.TargetDummy;
    internal static int FindTarget(Vector2 origin, float range, Vector2 preference, bool priority)
    {
        int result = -1;
        float score = float.MaxValue;
        foreach (NPC npc in Main.ActiveNPCs)
        {
            if (!npc.CanBeChasedBy() || npc.DistanceSQ(origin) > range * range || !Clear(origin, npc.Center)) continue;
            float candidate = npc.DistanceSQ(preference) + npc.DistanceSQ(origin) * .15f;
            if (priority && Priority(npc)) candidate -= 10000000f;
            if (candidate >= score) continue;
            result = npc.whoAmI;
            score = candidate;
        }
        return result;
    }
    internal static Vector2 Home(Vector2 velocity, Vector2 offset, float age, int seed, bool wisp)
    {
        Vector2 forward = offset.SafeNormalize(velocity.SafeNormalize(Vector2.UnitX));
        float progress = MathHelper.Clamp((age - 8f) / 40f, 0f, 1f);
        float speed = MathHelper.Lerp(wisp ? 10f : 14f, wisp ? 21f : 23f, progress);
        float inertia = MathHelper.Lerp(20f, 9f, progress);
        Vector2 desired = forward * speed + forward.RotatedBy(MathHelper.PiOver2)
            * MathF.Sin(age * .12f + seed * .73f) * 2f * (1f - progress);
        Vector2 result = (velocity * inertia + desired) / (inertia + 1f);
        return result.LengthSquared() > 25f * 25f ? result.SafeNormalize(Vector2.UnitX) * 25f : result;
    }
    internal static bool Circle(Rectangle target, Vector2 center, float radius)
    {
        Vector2 closest = Vector2.Clamp(center, target.TopLeft(), target.BottomRight());
        return Vector2.DistanceSquared(center, closest) <= radius * radius;
    }
    internal static void Pull(Vector2 center, float strength)
    {
        if (Main.netMode == NetmodeID.MultiplayerClient) return;
        foreach (NPC npc in Main.ActiveNPCs)
        {
            Vector2 offset = center - npc.Center;
            float distance = offset.Length();
            if (!CanPull(npc) || distance < 12f || distance > 300f || !Clear(center, npc.Center)) continue;
            float fade = 1f - distance / 300f;
            Vector2 desired = offset / distance * (2f + 6f * fade) * strength;
            npc.velocity = Vector2.Lerp(npc.velocity, desired, .025f + .10f * fade);
            if (Main.netMode == NetmodeID.Server && Main.GameUpdateCount % 10 == 0) npc.netUpdate = true;
        }
    }
}
