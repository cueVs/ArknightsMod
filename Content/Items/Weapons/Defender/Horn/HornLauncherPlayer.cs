using System;
using ArknightsMod.Content.Projectiles.Defender;
using ArknightsMod.Content.Projectiles.Defender.Horn;
using ArknightsMod.Players;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Weapons.Defender.Horn;

public sealed class HornLauncherPlayer : ModPlayer
{
    private int activeMode, activeRank, ammunition, counterTime, dumpCount, dumpClock, dumpDamage;
    private bool keyConsumed;
    private float healthDebt;
    internal bool Holding => Player.HeldItem.ModItem is HornGrenadeLauncher && !Player.dead && !Player.CCed && !Player.noItems;
    private WeaponPlayer Skills => Player.GetModPlayer<WeaponPlayer>();
    private bool Selected => Holding && Skills.CurrentSkill?.Key.Item == nameof(HornGrenadeLauncher);
    internal int Mode => Selected && Skills.SkillActive && Skills.Skill == activeMode - 1 ? activeMode : 0;
    internal bool Dumping => dumpCount > 0;
    internal byte VisualState => Dumping || (Mode == 2 && ammunition <= 5) ? (byte)4
        : Mode == 2 ? (byte)2 : Overdrive ? (byte)5 : Mode == 3 ? (byte)3 : (byte)0;
    private int Rank => Math.Clamp((Skills.CurrentSkill.ForceReplaceLevel ?? Skills.CurrentSkill.Level) - 1, 0, 9);
    private bool Overdrive => Mode == 3 && Skills.SkillTimer >= 12 * 60 * WeaponPlayer.ActiveDurationMultiplier;
    internal void CounterReady()
    {
        if (!Holding || Player.whoAmI != Main.myPlayer) return;
        counterTime = 300;
        HornVisuals.Sparks(Player.Center, 26, 6f);
        SoundEngine.PlaySound(SoundID.Item37 with { Volume = .6f, Pitch = .2f }, Player.Center);
    }
    internal void TryActivate()
    {
        if (!Selected || keyConsumed || Player.whoAmI != Main.myPlayer || Dumping) return;
        keyConsumed = true;
        if (Mode != 0)
        {
            if (Mode == 2 && ammunition is > 0 and <= 5)
            {
                dumpCount = ammunition;
                dumpClock = 0;
                dumpDamage = Player.GetWeaponDamage(Player.HeldItem);
                PayHealth((int)(Player.statLife * .6f));
            }
            EndSkill();
            return;
        }
        if (Skills.Skill is not (1 or 2) || Skills.SkillActive || Skills.StockCount <= 0) return;
        activeMode = Skills.Skill + 1;
        activeRank = Rank;
        ammunition = activeMode == 2 ? 10 : 0;
        healthDebt = 0;
        Skills.DelStockCount();
        Skills.SkillTimer = 0;
        Skills.SkillActive = true;
        HornVisuals.Sparks(Player.Center, 32, 5f);
        SoundEngine.PlaySound(SoundID.Item29 with { Volume = .45f, Pitch = -.4f }, Player.Center);
    }
    private void EndSkill()
    {
        if (Selected) Skills.SkillActive = false;
        ammunition = 0;
        healthDebt = 0;
    }
    private void PayHealth(int amount)
    {
        if (amount <= 0) return;
        Player.statLife = Math.Max(1, Player.statLife - amount);
        if (Main.netMode == NetmodeID.MultiplayerClient)
            NetMessage.SendData(MessageID.PlayerLifeMana, -1, -1, null, Player.whoAmI);
    }
    internal void Fire(IEntitySource source, int baseDamage, float knockback, bool release = false)
    {
        if (!Holding || Player.whoAmI != Main.myPlayer || (Dumping && !release)) return;
        Vector2 aim = (Main.MouseWorld - Player.MountedCenter).SafeNormalize(new Vector2(Player.direction, 0));
        Player.ChangeDir(aim.X >= 0f ? 1 : -1);
        byte visualState = release ? (byte)4 : VisualState; // Snapshot before the last round ends S2.
        float physical = 1f, arts = 0f, radius = 95f;
        int flare = 0;
        if (release || Mode == 2)
        {
            physical = HornCombat.BarrageMultiplier(activeRank);
            if (release || ammunition <= 5) arts = HornCombat.ArtsMultiplier(activeRank);
            if (!release)
            {
                ammunition--;
                Skills.SkillTimer = (10 - ammunition) * 60;
                if (ammunition <= 0) EndSkill();
            }
        }
        else if (Mode == 3)
            physical += HornCombat.FinalLineBonus(activeRank) * (Overdrive ? 2f : 1f);
        else if (Selected && Skills.Skill == 0 && Skills.StockCount > 0)
        {
            Skills.DelStockCount();
            physical = HornCombat.FlareMultiplier(Rank);
            radius = 130f;
            flare = (Rank < 6 ? 5 : Rank < 9 ? 6 : 8) * 60;
        }
        float counter = counterTime > 0 ? 2f : 1f;
        counterTime = 0;
        int hit = Math.Max(1, (int)MathF.Round(baseDamage * physical * counter));
        int artsHit = (int)MathF.Round(baseDamage * arts * counter);
        Vector2 origin = Player.MountedCenter;
        if (HornCombat.Clear(origin, origin + aim * 43f)) origin += aim * 43f;
        int index = Projectile.NewProjectile(source, origin, aim * 17f,
            ModContent.ProjectileType<HornGrenade>(),
            hit, knockback, Player.whoAmI, radius, flare, artsHit);
        if (Main.projectile.IndexInRange(index))
        {
            Main.projectile[index].CritChance = Player.GetWeaponCrit(Player.HeldItem);
            ((HornGrenade)Main.projectile[index].ModProjectile).VisualState = visualState;
            Main.projectile[index].netUpdate = true;
        }
        HornVisuals.Muzzle(origin, aim, counter > 1f);
        SoundEngine.PlaySound(SoundID.Item61 with { Volume = .55f, Pitch = -.3f, MaxInstances = 4 }, origin);
        Player.velocity -= aim * .65f;
        int guardType = ModContent.ProjectileType<HornGuard>();
        foreach (Projectile p in Main.ActiveProjectiles)
            if (p.owner == Player.whoAmI && p.type == guardType) { p.ai[2] = 10; p.netUpdate = true; }
    }
    public override void PostUpdate()
    {
        if (!ArknightsKeybinds.SkillActivatePressed(Player)) keyConsumed = false;
        if (Player.whoAmI != Main.myPlayer) return;
        if (!Holding)
        {
            counterTime = dumpCount = ammunition = 0;
            healthDebt = 0;
            return;
        }
        if (counterTime > 0) counterTime--;
        if (ArknightsKeybinds.SkillActivatePressed(Player)) TryActivate();
        if (Dumping)
        {
            if (!Selected || Skills.Skill != 1) dumpCount = 0;
            else if (dumpClock-- <= 0)
            {
                Fire(Player.GetSource_ItemUse(Player.HeldItem), dumpDamage, Player.HeldItem.knockBack, true);
                dumpCount--;
                dumpClock = 3; // 4 real frames between overload rounds.
            }
        }
        if (Overdrive)
        {
            float duration = Math.Max(1f, 12 * 60 * WeaponPlayer.ActiveDurationMultiplier);
            float progress = MathHelper.Clamp((Skills.SkillTimer - duration) / duration, 0f, 1f);
            healthDebt += Player.statLifeMax2 * .12f * progress / 60f;
            if (healthDebt >= 1f)
            {
                int cost = (int)healthDebt;
                healthDebt -= cost;
                PayHealth(cost);
                if (Player.statLife <= 1) EndSkill();
            }
        }
        else healthDebt = 0f;
        int type = ModContent.ProjectileType<HornGuard>();
        if (Player.ownedProjectileCounts[type] == 0)
            Projectile.NewProjectile(Player.GetSource_ItemUse(Player.HeldItem), Player.Center, Vector2.Zero, type, 0, 0f, Player.whoAmI);
    }
    public override void UpdateDead() => counterTime = dumpCount = ammunition = activeMode = 0;
}
