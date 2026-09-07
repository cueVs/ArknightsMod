using System;
using ArknightsMod.Content.Items.Weapons.Caster.Leizi;
using ArknightsMod.Content.Projectiles.Caster.Passenger;
using ArknightsMod.Players;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Weapons.Caster.Passenger;

/// <summary>异客的链式源石技艺；技能接入现有技力/升级体系，特效在本模组内独立运行。</summary>
public sealed class PassengerConductor : ExpansionWeaponBase
{
    private bool stormKeyConsumed;
    // 六星精二满级基础攻击 689 × 20% = 137.8，取 138；不计信赖、潜能和模组。
    // 资料：https://prts.wiki/w/异客 。精英化不再额外改变本轮约定的面板基准。
    protected override int[] EliteDamage => [138, 138, 138];
    internal const float NormalRange = 720f;
    internal const float FocusRange = 880f;
    internal const float JumpRange = 240f;

    // Magic Missile 是单帧斜向法杖，精细物品贴图完成前使用原版占位。
    public override string Texture => "Terraria/Images/Item_" + ItemID.MagicMissile;
    public override void SetStaticDefaults() => Item.staff[Type] = true;

    public override void SetDefaults()
    {
        Item.damage = EliteDamage[0];
        Item.DamageType = DamageClass.Magic;
        Item.width = Item.height = 40;
        Item.mana = 12;
        // MiracleMatterJav 的原生三连发：0、10、20 帧射击，45 帧开始下一组。
        Item.useTime = 10;
        Item.useAnimation = 45;
        Item.useLimitPerAnimation = 3;
        Item.useStyle = ItemUseStyleID.Shoot;
        Item.noMelee = true;
        Item.autoReuse = true;
        Item.knockBack = 2f;
        Item.crit = 4;
        Item.rare = ItemRarityID.Yellow;
        Item.value = Item.sellPrice(gold: 6);
        Item.shoot = ModContent.ProjectileType<PassengerFlyingDischarge>();
        Item.shootSpeed = PassengerFlyingDischarge.SpeedPerUpdate;
        Item.UseSound = SoundID.Item93 with { Volume = 0.38f, Pitch = -0.12f };
    }

    public override bool AltFunctionUse(Player player) => false;

    public override void HoldItem(Player player)
    {
        base.HoldItem(player);
        if (!ArknightsKeybinds.SkillActivatePressed(player))
            stormKeyConsumed = false;
    }

    internal static int SkillRank(WeaponPlayer player) => Math.Clamp(
        (player.CurrentSkill?.ForceReplaceLevel ?? player.CurrentSkill?.Level ?? 1) - 1, 0, 9);

    private static float FocusBonus(int rank) => rank switch
    {
        < 3 => 0.10f, < 6 => 0.15f, 6 => 0.20f, 7 => 0.25f, _ => 0.30f
    };

    public override float UseSpeedMultiplier(Player player)
    {
        WeaponPlayer skills = player.GetModPlayer<WeaponPlayer>();
        if (!skills.SkillActive || skills.Skill != 1)
            return 1f;
        float interval = SkillRank(skills) switch { < 3 => 0.8f, < 6 => 0.7f, < 9 => 0.6f, _ => 0.5f };
        return 1f / interval;
    }

    public override void ModifyWeaponDamage(Player player, ref StatModifier damage)
    {
        base.ModifyWeaponDamage(player, ref damage);
        WeaponPlayer skills = player.GetModPlayer<WeaponPlayer>();
        if (skills.SkillActive && skills.Skill == 1)
            damage *= 1f + FocusBonus(SkillRank(skills));
    }

    public override bool CanUseItem(Player player)
    {
        if (!ArknightsKeybinds.SkillActivatePressed(player))
        {
            // 只在新一轮使用前切换节奏，不截断已经开始的三连发。
            // 本轮只调整普攻；聚焦指令沿用原先 40 帧基准的加速单发。
            if (player.itemAnimation <= 0)
            {
                WeaponPlayer cadence = player.GetModPlayer<WeaponPlayer>();
                bool focus = cadence.SkillActive && cadence.Skill == 1;
                Item.useTime = focus ? 40 : 10;
                Item.useAnimation = focus ? 40 : 45;
                Item.useLimitPerAnimation = focus ? 1 : 3;
            }
            return base.CanUseItem(player);
        }

        WeaponPlayer skills = player.GetModPlayer<WeaponPlayer>();
        if (player.whoAmI != Main.myPlayer || skills.CurrentSkill == null || skills.StockCount <= 0
            || skills.SkillActive || skills.Skill == 0)
            return false;

        if (skills.Skill == 2)
        {
            // 每次按下技能键花一层库存，长按不会一帧内自动烧掉两层。
            if (stormKeyConsumed)
                return false;
            NPC target = PassengerTargeting.FindStormTarget(player, Main.MouseWorld);
            if (target == null || player.ownedProjectileCounts[ModContent.ProjectileType<PassengerThunderstorm>()] >= 2)
                return false;

            int index = Projectile.NewProjectile(player.GetSource_ItemUse(Item), target.Center, Vector2.Zero,
                ModContent.ProjectileType<PassengerThunderstorm>(), player.GetWeaponDamage(Item), Item.knockBack,
                player.whoAmI, SkillRank(skills));
            if (!Main.projectile.IndexInRange(index))
                return false;
            Main.projectile[index].CritChance = player.GetWeaponCrit(Item);
            stormKeyConsumed = true;
            // 雷暴是独立的瞬发部署，不锁住技能活跃态；库存可继续充能，也可叠放第二次雷暴。
        }
        else
        {
            skills.SkillActive = true;
            skills.SkillTimer = 0;
        }

        skills.DelStockCount();
        if (!Main.dedServ)
            SoundEngine.PlaySound(SoundID.Item122 with { Volume = 0.52f, Pitch = -0.25f }, player.Center);
        return false;
    }

    public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source,
        Vector2 position, Vector2 velocity, int type, int damage, float knockback)
    {
        if (player.whoAmI != Main.myPlayer)
            return false;

        WeaponPlayer skills = player.GetModPlayer<WeaponPlayer>();
        bool focus = skills.SkillActive && skills.Skill == 1;
        Vector2 direction = (Main.MouseWorld - position).SafeNormalize(new Vector2(player.direction, 0f));
        float visibleWidth = Main.screenWidth / Math.Max(0.25f, Main.GameViewMatrix.Zoom.X);
        int updates = PassengerFlyingDischarge.FlightUpdates(visibleWidth, focus);
        int rank = SkillRank(skills);
        float candidateMultiplier = skills.Skill == 0 ? (rank == 9 ? 2.5f : 1.5f + rank * 0.1f) : 1f;
        int index = Projectile.NewProjectile(source, position, direction * PassengerFlyingDischarge.SpeedPerUpdate,
            ModContent.ProjectileType<PassengerFlyingDischarge>(), damage, knockback, player.whoAmI,
            updates, candidateMultiplier, focus ? 5 : 4);
        if (Main.projectile.IndexInRange(index))
            Main.projectile[index].CritChance = player.GetWeaponCrit(Item);
        return false;
    }

    public override void AddRecipes()
    {
        CreateRecipe()
            .AddIngredient<LeiziStaff>()
            .AddIngredient(ItemID.MagnetSphere)
            .AddIngredient(ItemID.SpectreBar, 12)
            .AddIngredient(ItemID.CrystalShard, 15)
            .AddIngredient(ItemID.Wire, 40)
            .AddTile(TileID.MythrilAnvil)
            .Register();
    }
}
