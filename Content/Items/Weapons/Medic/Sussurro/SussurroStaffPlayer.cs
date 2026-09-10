using System;
using ArknightsMod.Content.Projectiles.Medic.Sussurro;
using ArknightsMod.Players;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Weapons.Medic.Sussurro;

public sealed class SussurroStaffPlayer : ModPlayer
{
    private int castGrace, healingClock, shotCount, recipient, mode, rank;
    private int skillLockout;
    private bool keyWasDown;
    internal bool Holding => Player.HeldItem.ModItem is SussurroStaff;
    internal bool TreatmentActive => Holding && !Player.dead && mode > 0
        && Player.GetModPlayer<WeaponPlayer>().SkillActive
        && Player.GetModPlayer<WeaponPlayer>().Skill == mode - 1
        && Player.GetModPlayer<WeaponPlayer>().CurrentSkill?.Key.Item == nameof(SussurroStaff);
    internal bool DeepTreatment => TreatmentActive && mode == 2;
    internal bool Casting => Holding && castGrace > 0 && !Player.dead && !Player.CCed && !Player.noItems;
    internal static int HealInterval(bool deep) => deep ? 30 : 60;
    internal static int HealAmount(int activeMode, int skillRank) => activeMode switch
    {
        1 => 6 + Math.Clamp(skillRank, 0, 9) / 3,
        2 => 9 + Math.Clamp(skillRank, 0, 9) / 3,
        _ => 5
    };

    internal void TryActivate()
    {
        if (Player.whoAmI != Main.myPlayer || !Holding || Player.dead || Player.CCed || Player.noItems || keyWasDown)
            return;
        keyWasDown = true;
        WeaponPlayer skills = Player.GetModPlayer<WeaponPlayer>();
        if (skills.CurrentSkill?.Key.Item != nameof(SussurroStaff) || skills.Skill is < 0 or > 1
            || skills.SkillActive || skills.StockCount <= 0 || skillLockout > 0)
            return;
        mode = skills.Skill + 1;
        rank = Math.Clamp((skills.CurrentSkill.ForceReplaceLevel ?? skills.CurrentSkill.Level) - 1, 0, 9);
        healingClock = 0;
        skills.DelStockCount();
        skills.SkillActive = true;
        skills.SkillTimer = 0;
        // 适配沙盒战斗，用冷却替代每场两次；切换技能/复制法杖不刷新这个玩家级下限。
        skillLockout = (int)MathF.Ceiling(skills.CurrentSkill.CurrentLevelData.ActiveTime * 60f
            * WeaponPlayer.ActiveDurationMultiplier) + (mode == 2 ? 45 : 20) * 60;
        Projectile.NewProjectile(Player.GetSource_ItemUse(Player.HeldItem), Player.MountedCenter, Vector2.Zero,
            ModContent.ProjectileType<SussurroHealingBloom>(), 0, 0f, Player.whoAmI, mode == 2 ? 1.5f : 1f, Player.whoAmI);
        SoundEngine.PlaySound(SoundID.Item29 with { Volume = .45f, Pitch = .2f }, Player.Center);
    }

    internal void RegisterCast(bool allyMode)
    {
        if (Player.whoAmI != Main.myPlayer)
            return;
        // 仅成功扣魔力并进入 Shoot 的施法延长窗口；断蓝、松手、收起都会停止积累治疗。
        castGrace = Math.Max(2, Player.itemAnimationMax) + 2;
        recipient = Player.whoAmI;
        if (allyMode)
        {
            float best = 180f * 180f;
            foreach (Player other in Main.ActivePlayers)
            {
                if (other.whoAmI == Player.whoAmI || !CanTreat(Player, other))
                    continue;
                float distance = other.DistanceSQ(Main.MouseWorld);
                if (distance < best)
                {
                    best = distance;
                    recipient = other.whoAmI;
                }
            }
        }
        int auraType = ModContent.ProjectileType<SussurroMedicalAura>();
        if (Player.ownedProjectileCounts[auraType] == 0)
            Projectile.NewProjectile(Player.GetSource_ItemUse(Player.HeldItem), Player.MountedCenter, Vector2.Zero,
                auraType, 0, 0f, Player.whoAmI, DeepTreatment ? 1f : 0f);
    }

    internal bool NextCompressedShot()
    {
        shotCount++;
        if (shotCount < (DeepTreatment ? 3 : 5))
            return false;
        shotCount = 0;
        return true;
    }

    internal static bool CanTreat(Player healer, Player target) => target.active && !target.dead
        && (target.whoAmI == healer.whoAmI || (!healer.hostile && !target.hostile)
            || (healer.team != 0 && healer.team == target.team))
        && target.DistanceSQ(healer.Center) <= 600f * 600f;

    public override void PostUpdate()
    {
        if (skillLockout > 0)
            skillLockout--;
        if (!ArknightsKeybinds.SkillActivatePressed(Player))
            keyWasDown = false;
        if (Player.whoAmI != Main.myPlayer)
            return;
        if (!Casting)
        {
            healingClock = 0;
            castGrace = 0;
            shotCount = 0;
            return;
        }
        castGrace--;
        if (++healingClock < HealInterval(DeepTreatment))
            return;
        healingClock = 0;
        Player target = Main.player[recipient];
        if (!CanTreat(Player, target))
            target = Player;
        if (target.statLife >= target.statLifeMax2)
            return;
        int heal = HealAmount(TreatmentActive ? mode : 0, rank);
        Vector2 start = Player.MountedCenter + new Vector2(Player.direction * 26f, -22f);
        Projectile.NewProjectile(Player.GetSource_ItemUse(Player.HeldItem), start,
            new Vector2(Player.direction * 2.4f, -2.8f), ModContent.ProjectileType<SussurroHealingButterfly>(),
            0, 0f, Player.whoAmI, target.whoAmI, heal, DeepTreatment ? 1f : 0f);
    }

    public override void UpdateDead()
    {
        castGrace = healingClock = shotCount = mode = 0;
    }
}
