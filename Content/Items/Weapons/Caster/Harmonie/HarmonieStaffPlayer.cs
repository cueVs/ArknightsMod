using System;
using ArknightsMod.Content.Projectiles.Caster.Ebenholz;
using ArknightsMod.Content.Projectiles.Caster.Harmonie;
using ArknightsMod.Players;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Weapons.Caster.Harmonie;

public sealed class HarmonieStaffPlayer : ModPlayer
{
    private int charges, chargeClock, activeMode;
    private bool keyConsumed;
    internal bool Holding => Player.HeldItem.ModItem is HarmonieStaff && !Player.dead && !Player.CCed && !Player.noItems;
    internal int Mode => Holding && Player.GetModPlayer<WeaponPlayer>().SkillActive
        && Player.GetModPlayer<WeaponPlayer>().CurrentSkill?.Key.Item == nameof(HarmonieStaff)
        && Player.GetModPlayer<WeaponPlayer>().Skill == activeMode - 1 ? activeMode : 0;

    internal int ReleaseCharges()
    {
        int result = charges;
        charges = chargeClock = 0;
        return result;
    }

    internal void TryActivate()
    {
        if (Player.whoAmI != Main.myPlayer || !Holding || keyConsumed) return;
        keyConsumed = true;
        WeaponPlayer skills = Player.GetModPlayer<WeaponPlayer>();
        if (skills.CurrentSkill?.Key.Item != nameof(HarmonieStaff) || skills.Skill is < 0 or > 1
            || skills.SkillActive || skills.StockCount <= 0) return;
        int mode = skills.Skill + 1;
        if (mode == 2)
        {
            int type = ModContent.ProjectileType<HarmonieResonance>();
            if (Player.ownedProjectileCounts[type] > 0) return;
            Vector2 offset = Main.MouseWorld - Player.MountedCenter;
            if (offset.LengthSquared() > 600f * 600f) offset = offset.SafeNormalize(Vector2.UnitX) * 600f;
            Vector2 center = Player.MountedCenter;
            // 沿鼠标方向推进到最后一个可见的空位，水域不能隔墙生成。
            for (float distance = 16f; distance <= offset.Length(); distance += 16f)
            {
                Vector2 candidate = Player.MountedCenter + offset.SafeNormalize(Vector2.UnitX) * distance;
                if (!EbenholzCombat.Clear(Player.MountedCenter, candidate)
                    || Collision.SolidCollision(candidate - new Vector2(8f), 16, 16)) break;
                center = candidate;
            }
            int duration = Math.Max(1, (int)MathF.Ceiling(skills.CurrentSkill.CurrentLevelData.ActiveTime
                * 60f * WeaponPlayer.ActiveDurationMultiplier));
            int index = Projectile.NewProjectile(Player.GetSource_ItemUse(Player.HeldItem), center, Vector2.Zero,
                type, Math.Max(1, (int)MathF.Round(Player.GetWeaponDamage(Player.HeldItem) * .35f)),
                0f, Player.whoAmI, duration);
            if (!Main.projectile.IndexInRange(index)) return;
            Main.projectile[index].CritChance = 0;
        }
        activeMode = mode;
        skills.DelStockCount();
        skills.SkillActive = true;
        skills.SkillTimer = 0;
        SoundEngine.PlaySound(SoundID.Item26 with { Volume = .45f, Pitch = mode == 1 ? .35f : -.35f }, Player.Center);
        HarmonieVisuals.Scatter(Player.Center, 18, 3f);
    }

    public override void PostUpdate()
    {
        if (!ArknightsKeybinds.SkillActivatePressed(Player)) keyConsumed = false;
        if (Player.whoAmI != Main.myPlayer) return;
        if (!Holding) { charges = chargeClock = 0; return; }
        if (ArknightsKeybinds.SkillActivatePressed(Player)) TryActivate();
        // 与黑键相同：停火累计60帧存一枚，最多三枚；一次开火取走全部储能。
        if (Player.itemAnimation <= 0 && charges < 3 && ++chargeClock >= 60)
        {
            chargeClock = 0;
            charges++;
            SoundEngine.PlaySound(SoundID.Item26 with { Volume = .17f, Pitch = -.25f + charges * .2f }, Player.Center);
        }
        int type = ModContent.ProjectileType<HarmonieOrbit>();
        if (Player.ownedProjectileCounts[type] == 0)
            Projectile.NewProjectile(Player.GetSource_ItemUse(Player.HeldItem), Player.Center, Vector2.Zero,
                type, 0, 0f, Player.whoAmI, charges, Mode);
        foreach (Projectile p in Main.ActiveProjectiles)
        {
            if (p.owner != Player.whoAmI || p.type != type) continue;
            if ((int)p.ai[0] == charges && (int)p.ai[1] == Mode) continue;
            p.ai[0] = charges;
            p.ai[1] = Mode;
            p.netUpdate = true;
        }
    }
    public override void UpdateDead() => charges = chargeClock = activeMode = 0;
}
