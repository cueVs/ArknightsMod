using ArknightsMod.Content.Projectiles.Caster.Indigo;
using ArknightsMod.Players;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Weapons.Caster.Indigo;

public sealed class IndigoStaffPlayer : ModPlayer
{
    private int charges, chargeClock, activeMode;
    private bool keyConsumed;
    internal bool Holding => Player.HeldItem.ModItem is IndigoStaff && !Player.dead && !Player.CCed && !Player.noItems;
    internal int Mode => Holding && Player.GetModPlayer<WeaponPlayer>().SkillActive
        && Player.GetModPlayer<WeaponPlayer>().CurrentSkill?.Key.Item == nameof(IndigoStaff)
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
        if (skills.CurrentSkill?.Key.Item != nameof(IndigoStaff) || skills.Skill is < 0 or > 1
            || skills.SkillActive || skills.StockCount <= 0) return;
        activeMode = skills.Skill + 1;
        skills.DelStockCount();
        skills.SkillActive = true;
        skills.SkillTimer = 0;
        SoundEngine.PlaySound(SoundID.Item29 with { Volume = .4f, Pitch = .35f }, Player.Center);
        IndigoVisuals.Scatter(Player.Center, 18, 3f);
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
            SoundEngine.PlaySound(SoundID.Item4 with { Volume = .17f, Pitch = .15f + charges * .12f }, Player.Center);
        }
        int type = ModContent.ProjectileType<IndigoOrbit>();
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
