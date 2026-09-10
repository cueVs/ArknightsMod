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
        if (skills.CurrentSkill?.Key.Item != nameof(WarfarinStaff) || skills.SkillActive
            || skills.StockCount <= 0 || PlasmaActive)
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
