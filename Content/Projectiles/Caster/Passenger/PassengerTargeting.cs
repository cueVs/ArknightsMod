using System;
using System.Collections.Generic;
using ArknightsMod.Content.Items.Weapons.Caster.Passenger;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Projectiles.Caster.Passenger;

internal static class PassengerTargeting
{
    internal static int EnemyIdentity(NPC npc) => npc.realLife >= 0 ? npc.realLife : npc.whoAmI;
    internal static bool ClearPath(Vector2 start, Vector2 end) => Collision.CanHitLine(start, 1, 1, end, 1, 1);

    internal static NPC FindInitialTarget(Player player, Vector2 mouse, float range)
    {
        NPC best = null;
        float bestDistance = 240f * 240f;
        foreach (NPC npc in Main.ActiveNPCs)
        {
            if (!npc.CanBeChasedBy(player) || npc.Distance(player.MountedCenter) > range
                || !ClearPath(player.MountedCenter, npc.Center))
                continue;
            float distance = Vector2.DistanceSquared(mouse, npc.Center);
            if (distance >= bestDistance)
                continue;
            bestDistance = distance;
            best = npc;
        }
        return best;
    }

    internal static NPC FindStormTarget(Player player, Vector2 mouse)
    {
        NPC best = null;
        foreach (NPC npc in Main.ActiveNPCs)
        {
            if (!npc.CanBeChasedBy(player) || npc.Distance(player.MountedCenter) > 960f
                || Vector2.DistanceSquared(mouse, npc.Center) > 400f * 400f
                || !ClearPath(player.MountedCenter, npc.Center))
                continue;
            // 辉煌裂片优先当前生命值最高者；多节敌人按本体生命值比较。
            int hp = Main.npc[EnemyIdentity(npc)].life;
            int bestHp = best == null ? -1 : Main.npc[EnemyIdentity(best)].life;
            if (hp > bestHp || hp == bestHp && Vector2.DistanceSquared(mouse, npc.Center) < Vector2.DistanceSquared(mouse, best.Center))
                best = npc;
        }
        return best;
    }

    internal static Vector2 ClampAim(Player player, Vector2 requested, float range)
    {
        Vector2 offset = requested - player.MountedCenter;
        if (offset.LengthSquared() > range * range)
            offset = offset.SafeNormalize(Vector2.UnitX) * range;
        return player.MountedCenter + offset;
    }

    internal static bool FireChain(IEntitySource source, Player player, Vector2 aim,
        int damage, float knockback, float intensity, int targets, int slowTicks, int crit,
        Vector2? stormCenter = null, Vector2? emissionOrigin = null, NPC firstHit = null)
    {
        if (player.whoAmI != Main.myPlayer || !stormCenter.HasValue && firstHit == null)
            return false;
        int limit = Math.Clamp(targets, 1, 5);
        Vector2 start = stormCenter.HasValue ? aim : emissionOrigin ?? player.MountedCenter;
        int index = Projectile.NewProjectile(source, start,
            stormCenter.HasValue ? stormCenter.Value - start : Vector2.Zero,
            ModContent.ProjectileType<PassengerChainDischarge>(), damage, knockback,
            player.whoAmI, MathHelper.Clamp(intensity, 1f, 1.5f), slowTicks, stormCenter.HasValue ? -limit : limit);
        if (!Main.projectile.IndexInRange(index))
            return false;
        Main.projectile[index].CritChance = crit;
        ((PassengerChainDischarge)Main.projectile[index].ModProjectile).InitializeRoute(player, aim, firstHit);
        return true;
    }
}
