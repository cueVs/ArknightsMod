using System;
using ArknightsMod.Content.Projectiles.Specialist.Red;
using ArknightsMod.Players;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Weapons.Specialist.Red;

// 部署由快速移动或快捷栏切入触发，不是自动定时施法；切物品不会刷新已有冷却。
public sealed class RedDaggerPlayer : ModPlayer
{
    internal readonly RedDeploymentState Deployment = new();
    private Item armedKnife;
    private int armedSlot = -1;
    private int armedTime;
    private int armedSkill;
    private int armedRank;
    private bool pendingTeleport;
    private bool motionInitialized;
    private bool wasDashing;
    private int previousHotbarSlot = -1;
    private bool previousHotbarWeapon;
    private Vector2 previousVelocity;
    private float incomingFallSpeed;
    private ulong lastClockTick = ulong.MaxValue;
    public int Rank { get; private set; }
    private int activeSkill;
    private bool Holding => Player.HeldItem.ModItem is RedDagger;
    public bool DeploymentActive => !Player.dead && Holding && Deployment.Remaining > 0;
    public bool ExecutionActive => DeploymentActive && activeSkill == 0;
    internal float AttackSpeedMultiplier => DeploymentActive ? 1.5f : 1f;
    private static readonly float[] ExecutionBonus = [.35f, .4f, .45f, .5f, .55f, .6f, .65f, .7f, .75f, .8f];
    private static readonly float[] WolfBonus = [1.4f, 1.5f, 1.6f, 1.7f, 1.8f, 1.9f, 2f, 2.1f, 2.3f, 2.5f];
    private static readonly int[] StunTimes = [60, 60, 60, 90, 90, 90, 120, 138, 156, 180];
    internal static float ExecutionMultiplier(int rank) => 1f + ExecutionBonus[Math.Clamp(rank, 0, 9)];
    internal static float WolfMultiplier(int rank) => WolfBonus[Math.Clamp(rank, 0, 9)];
    internal static int StunDuration(int rank) => StunTimes[Math.Clamp(rank, 0, 9)];

    public override void Load() => On_Player.Teleport += ObserveTeleport;
    public override void Unload() => On_Player.Teleport -= ObserveTeleport;

    private static void ObserveTeleport(On_Player.orig_Teleport orig, Player player, Vector2 newPos, int style, int extraInfo)
    {
        Vector2 before = player.position;
        orig(player, newPos, style, extraInfo);
        // 只观测成功传送：不改原版行为，不把出生、回城或坐标纠正当作快速部署。
        if (player.whoAmI != Main.myPlayer || Vector2.DistanceSquared(before, player.position) < 1f
            || !RedDeploymentState.IsCombatTeleport(style))
            return;
        var combat = player.GetModPlayer<RedDaggerPlayer>();
        if (combat.Holding)
            combat.RememberKnife();
        if (combat.HasArmedKnife && (combat.Holding || IsTeleportTool(player.HeldItem.type)))
            combat.pendingTeleport = true;
    }

    private static bool IsTeleportTool(int type) => type is ItemID.RodofDiscord or ItemID.RodOfHarmony or ItemID.PortalGun;
    private bool HasArmedKnife => armedTime > 0 && armedSlot >= 0 && armedSlot < Player.inventory.Length
        && ReferenceEquals(Player.inventory[armedSlot], armedKnife) && armedKnife.ModItem is RedDagger;

    private void RememberKnife()
    {
        var skill = Player.GetModPlayer<WeaponPlayer>();
        if (!Holding || skill.CurrentSkill?.Key.Item != nameof(RedDagger) || skill.Skill is < 0 or > 1)
        {
            armedTime = 0;
            return;
        }
        armedKnife = Player.HeldItem;
        armedSlot = Player.selectedItem;
        armedTime = 90; // 刀 -> 传送工具的 1.5 秒宽限；放背包里不能一直生效。
        armedSkill = skill.Skill;
        armedRank = Math.Clamp((skill.CurrentSkill.ForceReplaceLevel ?? skill.CurrentSkill.Level) - 1, 0, 9);
    }

    public override void PreUpdate()
    {
        TickClock();
        if (armedTime > 0)
            armedTime--;
        if (Holding)
            RememberKnife();
        else if (!IsTeleportTool(Player.HeldItem.type))
            armedTime = 0;
        incomingFallSpeed = Math.Max(0, Player.velocity.Y * Player.gravDir);
    }

    public override void PreUpdateMovement() =>
        incomingFallSpeed = Math.Max(incomingFallSpeed, Player.velocity.Y * Player.gravDir);

    public override void PostUpdate()
    {
        if (Player.whoAmI != Main.myPlayer)
            return;
        // 只认其他快捷栏武器 -> 匕首的选槽变化：不把进世界、原地持有或替换槽内物品当作切入。
        bool switchedToDagger = Holding && previousHotbarWeapon
            && Player.selectedItem is >= 0 and < 10 && Player.selectedItem != previousHotbarSlot;
        bool dashing = Player.dashDelay < 0;
        bool landing = Holding && motionInitialized
            && Math.Max(incomingFallSpeed, previousVelocity.Y * Player.gravDir) >= RedDeploymentState.LandingSpeed
            && Math.Abs(Player.velocity.Y) < .1f
            && Math.Abs(Collision.TileCollision(Player.position, new Vector2(0, 2 * Player.gravDir),
                Player.width, Player.height, false, false, (int)Player.gravDir).Y) < 1f;
        bool dash = Holding && motionInitialized && RedDeploymentState.IsDashStart(wasDashing, dashing,
            previousVelocity.X, Player.velocity.X, Player.controlLeft || Player.controlRight,
            !Player.mount.Active && Player.grappling[0] < 0 && Player.immuneTime <= 0);
        bool teleported = pendingTeleport;
        pendingTeleport = false; // 冷却内事件直接丢弃，不能缓存到就绪时自动放。
        if (Holding)
            RememberKnife();
        if (teleported || switchedToDagger || (Holding && (landing || dash)))
            TryDeploy(teleported);
        previousVelocity = Player.velocity;
        wasDashing = dashing;
        motionInitialized = !Player.dead;
        previousHotbarSlot = Player.dead ? -1 : Player.selectedItem;
        previousHotbarWeapon = !Player.dead && Player.selectedItem is >= 0 and < 10
            && !Holding && Player.HeldItem.damage > 0;
        UpdateSkillDisplay();
    }

    internal bool TryDeploy(bool teleport)
    {
        if (Player.whoAmI != Main.myPlayer || Player.dead || Player.CCed || Player.noItems
            || !HasArmedKnife || (!Holding && !(teleport && IsTeleportTool(Player.HeldItem.type)))
            || Deployment.Cooldown > 0)
            return false;
        // 先算未强化伤害，再开始新一轮状态，避免自己的增伤重复放大部署爆发。
        int damage = (int)MathF.Round(Player.GetWeaponDamage(armedKnife)
            * (armedSkill == 1 ? WolfMultiplier(armedRank) : 1f));
        int index = Projectile.NewProjectile(Player.GetSource_ItemUse(armedKnife), Player.MountedCenter,
            Vector2.Zero, ModContent.ProjectileType<RedWolfPulse>(), damage, 5f, Player.whoAmI,
            armedSkill == 1 ? StunDuration(armedRank) : 0);
        if (!Main.projectile.IndexInRange(index))
            return false;
        Main.projectile[index].CritChance = Player.GetWeaponCrit(armedKnife);
        Deployment.Start();
        Rank = armedRank;
        activeSkill = armedSkill;
        // 同步弹幕携带持续时间/技能/等级，远端只接收显示状态，不重复产生伤害弹幕。
        Projectile.NewProjectile(Player.GetSource_ItemUse(armedKnife), Player.MountedCenter, Vector2.Zero,
            ModContent.ProjectileType<RedDeploymentAura>(), 0, 0, Player.whoAmI, 0, activeSkill, Rank);
        SoundEngine.PlaySound(SoundID.Item71 with { Volume = .5f, Pitch = .25f }, Player.Center);
        return true;
    }

    internal void ReceiveDeployment(int remaining, int skill, int rank)
    {
        if (Player.whoAmI == Main.myPlayer)
            return;
        Deployment.Receive(remaining);
        activeSkill = Math.Clamp(skill, 0, 1);
        Rank = Math.Clamp(rank, 0, 9);
    }

    private void UpdateSkillDisplay()
    {
        var skill = Player.GetModPlayer<WeaponPlayer>();
        if (!Holding || skill.CurrentSkill?.Key.Item != nameof(RedDagger))
            return;
        // 使用已有自定义充能入口，既不改公共系统，也不受双倍回复/持续时间设置干扰。
        skill.SkillActive = DeploymentActive;
        skill.SkillTimer = RedDeploymentState.Duration - Deployment.Remaining;
        skill.Div = 60;
        skill.SkillChargeMax = RedDeploymentState.Duration;
        skill.SkillCharge = RedDeploymentState.Duration - Deployment.Cooldown;
        skill.SP = skill.SkillCharge / 60;
        skill.StockCount = Deployment.Cooldown == 0 ? 1 : 0;
    }

    public override bool FreeDodge(Player.HurtInfo info)
    {
        if (Player.whoAmI != Main.myPlayer || !ExecutionActive
            || (info.DamageSource.SourceNPCIndex < 0 && info.DamageSource.SourceProjectileLocalIndex < 0)
            || Main.rand.NextFloat() >= .1f + Rank / 90f)
            return false;
        Player.immune = true;
        Player.immuneTime = Math.Max(Player.immuneTime, 20);
        RedDaggerVisuals.Hit(Player.Center, true);
        return true;
    }

    public override void UpdateDead()
    {
        // 死亡不清冷却；死亡期间仍继续计时，但清除待触发事件和武器宽限。
        TickClock();
        Deployment.CancelEffect();
        armedTime = 0;
        pendingTeleport = motionInitialized = wasDashing = false;
        previousHotbarSlot = -1;
        previousHotbarWeapon = false;
    }

    private void TickClock()
    {
        if (lastClockTick == Main.GameUpdateCount) return;
        lastClockTick = Main.GameUpdateCount;
        Deployment.Tick();
    }
}
