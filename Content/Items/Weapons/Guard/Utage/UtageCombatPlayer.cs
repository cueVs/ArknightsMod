using System;
using ArknightsMod.Content.Projectiles.Guard.Utage;
using ArknightsMod.Players;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Weapons.Guard.Utage;

public sealed class UtageCombatPlayer : ModPlayer
{
    private int mode;
    private int healingClock;
    private int hitHealCooldown;
    private bool keyWasDown;
    private int recoveryLockout;
    public int Rank { get; private set; }
    private bool Holding => Player.HeldItem.ModItem is UtageKatana;
    private bool CurrentActivation => Holding && mode != 0 && !Player.dead
        && Player.GetModPlayer<WeaponPlayer>().CurrentSkill?.Key.Item == nameof(UtageKatana)
        && Player.GetModPlayer<WeaponPlayer>().SkillActive
        && Player.GetModPlayer<WeaponPlayer>().Skill == mode - 1;
    public bool Resting => CurrentActivation && mode == 1;
    public bool ArtsActive => CurrentActivation && mode == 2;
    internal static int LifeAfterCost(int life) => Math.Max(1, life - life / 2);
    private static readonly float[] ArtsBonuses = [.5f, .55f, .6f, .65f, .7f, .75f, .8f, .9f, 1f, 1.1f];
    internal static float ArtsMultiplier(int rank) => 1f + ArtsBonuses[Math.Clamp(rank, 0, 9)];

    internal bool TryActivate()
    {
        if (Player.whoAmI != Main.myPlayer || !Holding || Player.dead || Player.CCed || Player.noItems)
            return false;
        WeaponPlayer skill = Player.GetModPlayer<WeaponPlayer>();
        if (skill.CurrentSkill == null || skill.CurrentSkill.Key.Item != nameof(UtageKatana)
            || skill.Skill is < 0 or > 1 || skill.SkillActive || skill.StockCount <= 0 || recoveryLockout > 0)
            return false;
        Rank = Math.Clamp((skill.CurrentSkill.ForceReplaceLevel ?? skill.CurrentSkill.Level) - 1, 0, 9);
        mode = skill.Skill + 1;
        healingClock = 0;
        skill.DelStockCount();
        skill.SkillTimer = 0;
        skill.SkillActive = true;
        // 独立于公共技力的冷却下限，换武器/切技能不能刷新初始技力来反复治疗。
        recoveryLockout = (int)Math.Ceiling(skill.CurrentSkill.CurrentLevelData.ActiveTime * 60f
            * WeaponPlayer.ActiveDurationMultiplier) + (mode == 1 ? 15 : 30) * 60;
        if (mode == 2)
        {
            Player.statLife = LifeAfterCost(Player.statLife);
            SyncLife();
        }
        // 分神立即收刀，避免上一发尚未结束的挥砍继续伤害/吸血。
        foreach (Projectile projectile in Main.ActiveProjectiles)
            if (projectile.owner == Player.whoAmI && projectile.ModProjectile is UtageKatanaSwing)
                projectile.Kill();
        SoundEngine.PlaySound(SoundID.Item71 with { Volume = .5f, Pitch = .12f }, Player.Center);
        // 使用无伤害弹幕同步一次视觉；不新增全局网络包。
        Projectile.NewProjectile(Player.GetSource_ItemUse(Player.HeldItem), Player.Center, Vector2.Zero,
            ModContent.ProjectileType<UtageSkillFlash>(), 0, 0f, Player.whoAmI, mode);
        return true;
    }

    public override void PostUpdateEquips()
    {
        if (Resting)
            Player.statDefense += 10 + Rank; // 不照搬原作 +100%～200% 防御。
    }

    public override void PostUpdate()
    {
        if (hitHealCooldown > 0)
            hitHealCooldown--;
        if (recoveryLockout > 0)
            recoveryLockout--;
        if (Player.whoAmI != Main.myPlayer)
            return;
        bool keyDown = ArknightsKeybinds.SkillActivatePressed(Player);
        if (keyDown && !keyWasDown)
            TryActivate();
        keyWasDown = keyDown;
        if (!CurrentActivation)
        {
            mode = 0;
            healingClock = 0;
            return;
        }
        if (Resting && ++healingClock >= 60)
        {
            healingClock = 0;
            HealLimited(Math.Max(1, (int)MathF.Floor(Player.statLifeMax2 * (.01f + Rank / 900f))));
        }
    }

    internal void HealOnHit()
    {
        if (Player.whoAmI != Main.myPlayer || !Holding || Resting || hitHealCooldown > 0
            || Player.lifeSteal <= 0f || Player.moonLeech)
            return;
        int heal = Math.Min(2, (int)Player.lifeSteal);
        int actual = HealLimited(heal);
        if (actual > 0)
        {
            Player.lifeSteal -= actual;
            hitHealCooldown = 20; // 按玩家计时，不因多目标、多个子更新或切换实例重复吸血。
        }
    }

    private int HealLimited(int requested)
    {
        if (Player.dead || Player.statLife <= 0 || Player.moonLeech)
            return 0;
        int actual = Math.Min(requested, Math.Max(0, Player.statLifeMax2 - Player.statLife));
        if (actual <= 0)
            return 0;
        Player.Heal(actual);
        SyncLife();
        return actual;
    }

    private void SyncLife()
    {
        if (Main.netMode == NetmodeID.MultiplayerClient)
            NetMessage.SendData(MessageID.PlayerLifeMana, number: Player.whoAmI);
    }

    public override void UpdateDead()
    {
        mode = healingClock = hitHealCooldown = 0;
        keyWasDown = false;
    }
}
