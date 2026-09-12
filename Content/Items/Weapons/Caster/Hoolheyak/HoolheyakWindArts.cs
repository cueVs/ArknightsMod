using System;
using System.Collections.Generic;
using ArknightsMod.Content.Projectiles.Caster.Hoolheyak;
using ArknightsMod.Players;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Weapons.Caster.Hoolheyak;

// By T
/// <summary>原版神秘物品作为图标占位；沿用投掷武器的空手动作，实际释放魔法风弹。</summary>
public sealed class HoolheyakWindArts : ExpansionWeaponBase
{
    public override string Texture => "Terraria/Images/Item_" + ItemID.SpiritFlame;
    protected override int[] EliteDamage => [116, 131, 148]; // 原面板提高 7%，取整。
    private int barrageShot;
    private bool skillKeyConsumed;
    internal static readonly float[] SeekingDamage = [1.6f, 1.7f, 1.8f, 2f, 2.1f, 2.2f, 2.4f, 2.6f, 2.8f, 3f];
    internal static readonly float[] BarrageDamage = [.20f, .22f, .24f, .27f, .29f, .31f, .35f, .38f, .41f, .45f];
    internal static readonly float[] BarrageLiftChance = [.08f, .08f, .08f, .10f, .10f, .10f, .12f, .13f, .14f, .15f];
    internal static readonly float[] CycloneDamage = [3.2f, 3.3f, 3.4f, 3.5f, 3.6f, 3.7f, 3.8f, 4f, 4.1f, 4.2f];
    internal static readonly int[] SeekingLift = [120, 120, 120, 150, 150, 150, 180, 210, 210, 240];

    public override void SetDefaults()
    {
        Item.damage = EliteDamage[0];
        Item.DamageType = DamageClass.Magic;
        Item.width = Item.height = 40;
        Item.useStyle = ItemUseStyleID.Swing;
        Item.noUseGraphic = true;
        Item.noMelee = true;
        Item.autoReuse = true;
        Item.useTime = Item.useAnimation = 26;
        Item.mana = 10;
        Item.knockBack = 0f;
        Item.rare = ItemRarityID.Lime;
        Item.value = Item.sellPrice(gold: 5);
        Item.shoot = ModContent.ProjectileType<HoolheyakWindBolt>();
        Item.shootSpeed = 18f;
        Item.UseSound = SoundID.Item7 with { Volume = .38f, Pitch = .15f };
    }

    public override bool AltFunctionUse(Player player) => false;
    internal static int Rank(WeaponPlayer skills) => Math.Clamp(
        (skills.CurrentSkill?.ForceReplaceLevel ?? skills.CurrentSkill?.Level ?? 1) - 1, 0, 9);

    public override void HoldItem(Player player)
    {
        base.HoldItem(player);
        if (!ArknightsKeybinds.SkillActivatePressed(player))
            skillKeyConsumed = false;
    }

    public override bool CanUseItem(Player player)
    {
        WeaponPlayer skills = player.GetModPlayer<WeaponPlayer>();
        if (ArknightsKeybinds.SkillActivatePressed(player))
        {
            if (!skillKeyConsumed && skills.CurrentSkill != null && skills.Skill is 1 or 2
                && !skills.SkillActive && skills.StockCount > 0)
            {
                skillKeyConsumed = true;
                skills.SkillActive = true;
                skills.SkillTimer = 0;
                skills.DelStockCount();
                SoundEngine.PlaySound(SoundID.Item60 with { Volume = .45f, Pitch = .35f }, player.Center);
            }
            return false;
        }

        if (player.itemAnimation <= 0)
        {
            bool barrage = skills.SkillActive && skills.Skill == 1;
            bool cyclone = skills.SkillActive && skills.Skill == 2;
            // 九连发只收九次小额魔力；一轮结束后才改节奏，避免中途缩短动画多发弹。
            Item.useTime = barrage ? 4 : cyclone ? 65 : 26;
            Item.useAnimation = barrage ? 52 : Item.useTime;
            Item.useLimitPerAnimation = barrage ? 9 : 1;
            Item.mana = barrage ? 2 : cyclone ? 18 : 10;
            barrageShot = 0;
        }
        return base.CanUseItem(player);
    }

    public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source,
        Vector2 position, Vector2 velocity, int type, int damage, float knockback)
    {
        if (player.whoAmI != Main.myPlayer)
            return false;
        WeaponPlayer skills = player.GetModPlayer<WeaponPlayer>();
        int rank = Rank(skills);
        // 技能结束或切换后，未完成的九连发不得回退成低魔耗的高伤普攻。
        if (Item.useLimitPerAnimation == 9 && (!skills.SkillActive || skills.Skill != 1))
            return false;
        Vector2 direction = (Main.MouseWorld - player.MountedCenter).SafeNormalize(new Vector2(player.direction, 0));
        position = player.MountedCenter;
        // 起点只能沿空旷方向探出手掌，不能穿过紧贴玩家的墙。
        if (Collision.CanHitLine(position, 1, 1, position + direction * 22f, 1, 1))
            position += direction * 22f;
        if (skills.SkillActive && skills.Skill == 2)
        {
            Vector2 side = direction.RotatedBy(MathHelper.PiOver2);
            for (int i = -1; i <= 1; i++)
            {
                Vector2 start = position + side * i * 46f;
                if (!HoolheyakWindCombat.ClearPath(position, start))
                    start = position;
                Spawn(start, direction * 7.5f, ModContent.ProjectileType<HoolheyakTravelingCyclone>(), damage, rank);
            }
            return false;
        }

        if (skills.SkillActive && skills.Skill == 1)
        {
            List<NPC> targets = HoolheyakWindCombat.Targets(position, 900f);
            NPC target = targets.Count > 0 ? targets[Main.rand.Next(targets.Count)] : null;
            Vector2 aim = target == null ? direction.RotatedByRandom(.13f) : (target.Center - position).SafeNormalize(direction);
            // 每轮末发可以补充风场，其余八发只打击/浮空，控制弹幕与风场总量。
            int mode = ++barrageShot == 9 ? HoolheyakWindBolt.BarrageFinisher : HoolheyakWindBolt.Barrage;
            Spawn(position, aim * 18f, type, Math.Max(1, (int)MathF.Round(damage * BarrageDamage[rank])),
                mode, target?.whoAmI + 1 ?? 0, rank);
            return false;
        }

        if (skills.Skill == 0 && skills.StockCount > 0)
        {
            List<NPC> targets = HoolheyakWindCombat.Targets(position, 900f);
            targets.Sort((a, b) => a.DistanceSQ(Main.MouseWorld).CompareTo(b.DistanceSQ(Main.MouseWorld)));
            if (targets.Count > 0)
            {
                int count = Math.Min(2, targets.Count);
                skills.DelStockCount();
                for (int i = 0; i < count; i++)
                {
                    NPC target = targets[i];
                    Spawn(position, (target.Center - position).SafeNormalize(direction) * 18f, type,
                        (int)MathF.Round(damage * SeekingDamage[rank]),
                        count == 1 ? HoolheyakWindBolt.SeekingSolo : HoolheyakWindBolt.SeekingPair,
                        target.whoAmI + 1, rank);
                }
                return false;
            }
        }
        Spawn(position, direction * 18f, type, damage);
        return false;

        void Spawn(Vector2 origin, Vector2 speed, int projectileType, int hitDamage, float ai0 = 0, float ai1 = 0, float ai2 = 0)
        {
            int index = Projectile.NewProjectile(source, origin, speed, projectileType, hitDamage, 0f, player.whoAmI, ai0, ai1, ai2);
            if (Main.projectile.IndexInRange(index))
                Main.projectile[index].CritChance = player.GetWeaponCrit(Item);
        }
    }

    public override void AddRecipes() => CreateRecipe()
        .AddIngredient(ItemID.CrystalStorm)
        .AddIngredient(ItemID.CloudinaBottle)
        .AddIngredient(ItemID.Feather, 15)
        .AddIngredient(ItemID.SoulofFlight, 15)
        .AddTile(TileID.Bookcases)
        .Register();
}
