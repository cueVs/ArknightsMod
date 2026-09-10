using System;
using ArknightsMod.Content.Projectiles.Caster.Ebenholz;
using ArknightsMod.Players;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Weapons.Caster.Ebenholz;

public sealed class EbenholzStaffPlayer : ModPlayer
{
    private int charges, chargeClock, activeMode, activeRank;
    private bool keyConsumed;
    internal bool Holding => Player.HeldItem.ModItem is EbenholzStaff && !Player.dead && !Player.CCed && !Player.noItems;
    internal int Mode => Holding && Player.GetModPlayer<WeaponPlayer>().SkillActive
        && Player.GetModPlayer<WeaponPlayer>().CurrentSkill?.Key.Item == nameof(EbenholzStaff)
        && Player.GetModPlayer<WeaponPlayer>().Skill == activeMode - 1 ? activeMode : 0;
    internal int Rank => activeRank;
    internal int ReleaseCharges()
    {
        int result = charges;
        charges = chargeClock = 0;
        return result;
    }
    internal void TryActivate()
    {
        if (Player.whoAmI != Main.myPlayer || !Holding || keyConsumed)
            return;
        keyConsumed = true;
        WeaponPlayer skills = Player.GetModPlayer<WeaponPlayer>();
        if (skills.CurrentSkill?.Key.Item != nameof(EbenholzStaff) || skills.Skill is < 0 or > 2 || skills.SkillActive)
            return;
        if (skills.Skill == 1)
        {
            TryRemnants(skills);
            return;
        }
        if (skills.StockCount <= 0)
            return;
        activeMode = skills.Skill + 1;
        activeRank = Math.Clamp((skills.CurrentSkill.ForceReplaceLevel ?? skills.CurrentSkill.Level) - 1, 0, 9);
        skills.DelStockCount();
        skills.SkillActive = true;
        skills.SkillTimer = 0;
        SoundEngine.PlaySound(SoundID.Item119 with { Volume = .5f, Pitch = -.5f }, Player.Center);
        EbenholzVisuals.Scatter(Player.Center, 24, 5f);
    }
    private void TryRemnants(WeaponPlayer skills)
    {
        if (skills.StockCount <= 0)
            return;
        int targetIndex = EbenholzCombat.FindTarget(Player.Center, 800f, Player.Center, false);
        if (targetIndex < 0)
            return;
        int rank = Math.Clamp((skills.CurrentSkill.ForceReplaceLevel ?? skills.CurrentSkill.Level) - 1, 0, 9);
        int count = charges + 1;
        int type = ModContent.ProjectileType<EbenholzRemnant>();
        int existing = Player.ownedProjectileCounts[type];
        // 同一玩家最多五个；替换旧核心不会引发额外爆炸。
        while (existing + count > 5)
        {
            Projectile oldest = null;
            foreach (Projectile p in Main.ActiveProjectiles)
                if (p.owner == Player.whoAmI && p.type == type && (oldest == null || p.timeLeft < oldest.timeLeft))
                    oldest = p;
            if (oldest == null) break;
            oldest.Kill();
            existing--;
        }
        ReleaseCharges();
        skills.DelStockCount();
        int damage = (int)MathF.Round(Player.GetWeaponDamage(Player.HeldItem) * (1.4f + rank * 1.05f / 9f));
        NPC target = Main.npc[targetIndex];
        for (int i = 0; i < count; i++)
        {
            Vector2 origin = target.Center + (MathHelper.TwoPi * i / count - MathHelper.PiOver2).ToRotationVector2() * 90f;
            if (!EbenholzCombat.Clear(Player.Center, origin) || Collision.SolidCollision(origin - new Vector2(12), 24, 24))
                origin = Player.MountedCenter;
            int index = Projectile.NewProjectile(Player.GetSource_ItemUse(Player.HeldItem), origin, Vector2.Zero,
                type, damage, 4f, Player.whoAmI);
            if (Main.projectile.IndexInRange(index)) Main.projectile[index].CritChance = Player.GetWeaponCrit(Player.HeldItem);
        }
        SoundEngine.PlaySound(SoundID.Item8 with { Volume = .5f, Pitch = -.65f }, Player.Center);
    }
    public override void PostUpdate()
    {
        if (!ArknightsKeybinds.SkillActivatePressed(Player)) keyConsumed = false;
        if (Player.whoAmI != Main.myPlayer) return;
        if (!Holding)
        {
            charges = chargeClock = 0;
            return;
        }
        if (Player.itemAnimation <= 0 && charges < 3 && ++chargeClock >= 60)
        {
            chargeClock = 0;
            charges++;
            SoundEngine.PlaySound(SoundID.Item4 with { Volume = .15f, Pitch = -.5f + charges * .12f }, Player.Center);
        }
        WeaponPlayer skills = Player.GetModPlayer<WeaponPlayer>();
        if (skills.CurrentSkill?.Key.Item == nameof(EbenholzStaff) && skills.Skill == 1 && !skills.SkillActive
            && Main.GameUpdateCount % 15 == 0)
            TryRemnants(skills);
        int haloType = ModContent.ProjectileType<EbenholzOrbit>();
        if (Player.ownedProjectileCounts[haloType] == 0)
            Projectile.NewProjectile(Player.GetSource_ItemUse(Player.HeldItem), Player.Center, Vector2.Zero,
                haloType, 0, 0f, Player.whoAmI, charges, Mode);
        foreach (Projectile p in Main.ActiveProjectiles)
        {
            if (p.owner != Player.whoAmI || p.type != haloType) continue;
            if ((int)p.ai[0] != charges || (int)p.ai[1] != Mode)
            {
                p.ai[0] = charges;
                p.ai[1] = Mode;
                p.netUpdate = true;
            }
        }
    }
    public override void UpdateDead() => charges = chargeClock = activeMode = 0;
}
