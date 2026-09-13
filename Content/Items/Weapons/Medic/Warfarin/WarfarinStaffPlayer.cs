using System;
using ArknightsMod.Content.Projectiles.Medic.Warfarin;
using ArknightsMod.Players;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Weapons.Medic.Warfarin;

public sealed class WarfarinStaffPlayer : ModPlayer
{
    internal const int ProcCooldownTicks = 15;
    private int procCooldown;
    private bool keyWasDown;

    // 同步的控制弹幕承载技能生效状态；远端无需读取本机技能键或未同步的 WeaponPlayer 字段。
    internal bool PlasmaActive => !Player.dead && Player.HeldItem.ModItem is WarfarinStaff
        && Player.ownedProjectileCounts[ModContent.ProjectileType<WarfarinPlasmaAura>()] > 0;

    internal static bool IsSummonAttack(Projectile proj)
        => proj.type != ModContent.ProjectileType<WarfarinBloodBolt>()
        && (proj.minion || proj.sentry || ProjectileID.Sets.MinionShot[proj.type] || ProjectileID.Sets.SentryShot[proj.type]);

    internal void TryActivate()
    {
        if (Player.whoAmI != Main.myPlayer || Player.HeldItem.ModItem is not WarfarinStaff
            || Player.dead || Player.CCed || Player.noItems || keyWasDown)
            return;
        keyWasDown = true;
        WeaponPlayer skills = Player.GetModPlayer<WeaponPlayer>();
        if (skills.CurrentSkill?.Key.Item != nameof(WarfarinStaff) || skills.Skill is < 0 or > 1
            || skills.SkillActive || skills.StockCount <= 0)
            return;

        if (skills.Skill == 0)
        {
            ActivateEmergencyDressing(skills);
            return;
        }
        if (PlasmaActive)
            return;
        int duration = Math.Max(1, (int)MathF.Ceiling(skills.CurrentSkill.CurrentLevelData.ActiveTime
            * 60f * WeaponPlayer.ActiveDurationMultiplier));
        int index = Projectile.NewProjectile(Player.GetSource_ItemUse(Player.HeldItem), Player.Center,
            Vector2.Zero, ModContent.ProjectileType<WarfarinPlasmaAura>(), 0, 0f, Player.whoAmI, duration);
        if (!Main.projectile.IndexInRange(index))
            return;
        skills.DelStockCount();
        skills.SkillActive = true;
        skills.SkillTimer = 0;
        procCooldown = 0;
        SoundEngine.PlaySound(SoundID.Item29 with { Volume = .5f, Pitch = -.35f }, Player.Center);
    }

    private void ActivateEmergencyDressing(WeaponPlayer skills)
    {
        int rank = Math.Clamp((skills.CurrentSkill.ForceReplaceLevel ?? skills.CurrentSkill.Level) - 1, 0, 9);
        int target = FindEmergencyPatient();
        Vector2 start = Player.MountedCenter + new Vector2(Player.direction * 22f, -18f);
        Vector2 aim = (Main.MouseWorld - start).SafeNormalize(new Vector2(Player.direction, -.3f));
        int index = Projectile.NewProjectile(Player.GetSource_ItemUse(Player.HeldItem), start, aim * 8f,
            ModContent.ProjectileType<WarfarinEmergencyDressing>(), 0, 0f, Player.whoAmI,
            target, 30 + rank * 2);
        if (!Main.projectile.IndexInRange(index))
            return;
        skills.DelStockCount();
        skills.SkillActive = true;
        skills.SkillTimer = 0;
        SoundEngine.PlaySound(SoundID.Item29 with { Volume = .45f, Pitch = -.05f }, Player.Center);
    }

    private int FindEmergencyPatient()
    {
        int selected = Player.whoAmI;
        float best = 180f * 180f;
        foreach (Player other in Main.ActivePlayers)
        {
            if (!other.active || other.dead || other.statLife >= other.statLifeMax2
                || (other.whoAmI != Player.whoAmI && other.hostile && Player.hostile && other.team != Player.team))
                continue;
            float distance = other.DistanceSQ(Main.MouseWorld);
            if (distance < best)
            {
                best = distance;
                selected = other.whoAmI;
            }
        }
        return selected;
    }

    public override void PostUpdate()
    {
        if (procCooldown > 0)
            procCooldown--;
        if (!ArknightsKeybinds.SkillActivatePressed(Player))
            keyWasDown = false;
    }

    public override void UpdateDead()
    {
        procCooldown = 0;
        keyWasDown = false;
    }

    public override void ModifyHitNPCWithProj(Projectile proj, NPC target, ref NPC.HitModifiers modifiers)
    {
        // 在命中时增加基础伤害，已经召出的随从及哨兵也立即生效，且不会污染其快照伤害。
        if (proj.owner == Player.whoAmI && PlasmaActive && IsSummonAttack(proj))
            modifiers.SourceDamage *= 1.3f;
    }

    public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
    {
        if (Player.whoAmI != Main.myPlayer || proj.owner != Player.whoAmI || !PlasmaActive
            || procCooldown > 0 || damageDone <= 0 || target.friendly || target.lifeMax <= 5 || !IsSummonAttack(proj))
            return;
        // 所有召唤物共享 15 帧冷却；血弹不标记为 MinionShot，不能递归触发自身。
        procCooldown = ProcCooldownTicks;
        Vector2 aim = (target.Center - proj.Center).SafeNormalize(new Vector2(Player.direction, -.35f));
        Projectile.NewProjectile(proj.GetSource_OnHit(target), proj.Center, aim * 15f,
            ModContent.ProjectileType<WarfarinBloodBolt>(), Player.GetWeaponDamage(Player.HeldItem),
            Player.GetWeaponKnockback(Player.HeldItem), Player.whoAmI);
    }
}
