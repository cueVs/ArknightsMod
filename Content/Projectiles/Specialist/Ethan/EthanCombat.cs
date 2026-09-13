using System;
using ArknightsMod.Content.Items.Weapons.Specialist.Ethan;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Projectiles.Specialist.Ethan;

internal static class EthanCombat
{
    private static readonly float[] AttackBonuses = [.2f, .25f, .3f, .35f, .4f, .45f, .5f, .55f, .6f, .7f];
    internal static float AttackBonus(int rank) => AttackBonuses[Math.Clamp(rank, 0, 9)];
    internal static float BindMultiplier(int rank) => rank < 3 ? 1.5f : rank < 6 ? 2f : rank < 9 ? 2.5f : 3f;
    internal static bool CanBind(NPC npc) => npc.active && !npc.friendly && !npc.boss && npc.realLife < 0
        && npc.knockBackResist > 0f && npc.aiStyle is not (6 or 37) && npc.width <= 180 && npc.height <= 180;
    internal static bool Clear(Vector2 a, Vector2 b) => Collision.CanHitLine(a, 1, 1, b, 1, 1);
    internal static bool InCircle(Vector2 center, float radius, Rectangle target)
    {
        Vector2 closest = new(Math.Clamp(center.X, target.Left, target.Right), Math.Clamp(center.Y, target.Top, target.Bottom));
        return Vector2.DistanceSquared(center, closest) <= radius * radius;
    }
    internal static void Hit(Projectile projectile, NPC target)
    {
        if (projectile.owner != Main.myPlayer) return;
        Player owner = Main.player[projectile.owner];
        var state = owner.GetModPlayer<EthanYoyoPlayer>();
        if (state.CrossSuspension)
            Projectile.NewProjectile(projectile.GetSource_FromThis(), target.Center, Vector2.Zero,
                ModContent.ProjectileType<EthanCrossFlash>(), Math.Max(1, (int)MathF.Round(owner.GetWeaponDamage(owner.HeldItem) * .35f)),
                0f, projectile.owner,
                Math.Clamp(Math.Max(target.width, target.height) * .7f, 32f, 85f), Main.rand.NextFloat(-.18f, .18f));
        if (!state.Holding || !target.active || target.life <= 0) return;
        var control = target.GetGlobalNPC<EthanBindingNPC>();
        // One roll per enemy/owner/attack cadence, shared by the glove's second yoyo and its aura.
        if (control.CanRoll(projectile.owner) && CanBind(target) && Main.rand.NextFloat() < state.BindChance)
            target.AddBuff(ModContent.BuffType<EthanBind>(), state.BindDuration);
        if (!state.FancySpins) return;
        int duration = (state.Rank < 3 ? 2 : state.Rank < 6 ? 3 : 4) * 60;
        int damage = Math.Max(1, (int)MathF.Round(owner.GetWeaponDamage(owner.HeldItem) * (.12f + state.Rank * .02f)));
        int type = ModContent.ProjectileType<EthanEcho>();
        foreach (Projectile p in Main.ActiveProjectiles)
            if (p.type == type && p.owner == projectile.owner && p.ai[0] == 1 && p.ai[2] == target.whoAmI + 1)
            {
                p.timeLeft = duration;
                p.damage = damage;
                p.netUpdate = true;
                return;
            }
        int index = Projectile.NewProjectile(projectile.GetSource_FromThis(), target.Center, Vector2.Zero, type,
            damage, 0f, projectile.owner, 1, duration, target.whoAmI + 1);
        if (Main.projectile.IndexInRange(index)) Main.projectile[index].timeLeft = duration;
    }
}
