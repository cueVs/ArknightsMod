using System;
using ArknightsMod.Content.Projectiles.Specialist.Ethan;
using ArknightsMod.Players;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Weapons.Specialist.Ethan;

public sealed class EthanYoyoPlayer : ModPlayer
{
    private bool keyConsumed;
    internal bool Holding => Player.HeldItem.ModItem is EthanYoyo && !Player.dead && !Player.CCed && !Player.noItems;
    private WeaponPlayer Skills => Player.GetModPlayer<WeaponPlayer>();
    internal bool Selected => Holding && Skills.CurrentSkill?.Key.Item == nameof(EthanYoyo);
    internal int Rank => Selected ? Math.Clamp((Skills.CurrentSkill.ForceReplaceLevel ?? Skills.CurrentSkill.Level) - 1, 0, 9) : 0;
    internal bool CrossSuspension => Selected && Skills.Skill == 1 && Skills.SkillActive;
    internal bool FancySpins => Selected && Skills.Skill == 0;
    internal float AttackMultiplier => CrossSuspension ? 1f + EthanCombat.AttackBonus(Rank) : 1f;
    internal float BindChance => .25f * (CrossSuspension ? EthanCombat.BindMultiplier(Rank) : 1f);
    internal int BindDuration => Player.HeldItem.ModItem is EthanYoyo { EliteStage: >= 2 } ? 180 : 120;

    internal void TryActivate()
    {
        if (Player.whoAmI != Main.myPlayer || !Selected || keyConsumed) return;
        keyConsumed = true;
        if (Skills.Skill != 1 || Skills.SkillActive || Skills.StockCount <= 0) return;
        Skills.DelStockCount();
        Skills.SkillTimer = 0;
        Skills.SkillActive = true;
        EthanVisuals.Scatter(Player.Center, 28, 5f);
        SoundEngine.PlaySound(SoundID.Item8 with { Pitch = .3f, Volume = .45f }, Player.Center);
    }
    public override void PostUpdate()
    {
        if (!ArknightsKeybinds.SkillActivatePressed(Player)) keyConsumed = false;
        if (!Holding || Player.whoAmI != Main.myPlayer) return;
        // Channelled yoyos do not call CanUseItem again when the skill key is pressed.
        if (ArknightsKeybinds.SkillActivatePressed(Player)) TryActivate();
        if (Selected && Skills.Skill == 0) Skills.SkillActive = true;
        if (Player.ownedProjectileCounts[ModContent.ProjectileType<EthanSpinningYoyo>()] > 0)
            Player.aggro -= 400;
    }
    public override bool FreeDodge(Player.HurtInfo info)
    {
        if (Player.whoAmI != Main.myPlayer || !Holding
            || Player.ownedProjectileCounts[ModContent.ProjectileType<EthanSpinningYoyo>()] == 0
            || (info.DamageSource.SourceNPCIndex < 0 && info.DamageSource.SourceProjectileLocalIndex < 0)
            || Main.rand.NextFloat() >= .5f) return false;
        Player.immune = true;
        Player.immuneTime = Math.Max(Player.immuneTime, 20);
        EthanVisuals.Scatter(Player.Center, 20, 4f);
        return true;
    }
}
