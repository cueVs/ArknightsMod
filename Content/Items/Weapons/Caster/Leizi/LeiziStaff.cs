using System;
using ArknightsMod.Content.Projectiles.Caster.Leizi;
using ArknightsMod.Content.Projectiles.Caster.Passenger;
using ArknightsMod.Players;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Weapons.Caster.Leizi;

/// <summary>五星惊蛰：机械 Boss 后的链电法杖，之后用于合成异客。</summary>
public sealed class LeiziStaff : ExpansionWeaponBase
{
    // 伤害与攻速各分担约 sqrt(2.5) 倍；按整数帧取 25 帧，再取伤害 144。
    // (144 / 92) × (40 / 25) ≈ 2.504，接近目标 2.5 倍。
    protected override int[] EliteDamage => [144, 144, 144];
    private static readonly float[] Skill1Bonus = [0.30f, 0.35f, 0.40f, 0.45f, 0.50f, 0.55f, 0.60f, 0.75f, 0.90f, 1f];
    private static readonly float[] Skill2Bonus = [0.60f, 0.65f, 0.70f, 0.80f, 0.85f, 0.90f, 1f, 1.15f, 1.30f, 1.50f];
    public override string Texture => "Terraria/Images/Item_" + ItemID.ThunderStaff;
    public override void SetStaticDefaults() => Item.staff[Type] = true;

    public override void SetDefaults()
    {
        Item.damage = EliteDamage[0];
        Item.DamageType = DamageClass.Magic;
        Item.width = Item.height = 40;
        Item.mana = 10;
        Item.useTime = Item.useAnimation = 25;
        Item.useStyle = ItemUseStyleID.Shoot;
        Item.noMelee = true;
        Item.autoReuse = true;
        Item.knockBack = 2f;
        Item.crit = 4;
        Item.rare = ItemRarityID.LightRed;
        Item.value = Item.sellPrice(gold: 3);
        Item.shoot = ModContent.ProjectileType<LeiziFlyingDischarge>();
        Item.shootSpeed = LeiziFlyingDischarge.SpeedPerUpdate;
        Item.UseSound = SoundID.Item93 with { Volume = 0.32f, Pitch = 0.18f };
    }

    public override bool AltFunctionUse(Player player) => false;

    public override bool CanUseItem(Player player)
    {
        if (!ArknightsKeybinds.SkillActivatePressed(player))
            return base.CanUseItem(player);
        WeaponPlayer skill = player.GetModPlayer<WeaponPlayer>();
        if (player.whoAmI == Main.myPlayer && skill.CurrentSkill != null
            && skill.Skill is 0 or 1 && !skill.SkillActive && skill.StockCount > 0)
        {
            skill.SkillActive = true;
            skill.SkillTimer = 0;
            skill.DelStockCount();
            SoundEngine.PlaySound(SoundID.Item122 with { Volume = 0.4f, Pitch = 0.2f }, player.Center);
        }
        return false;
    }

    public override void ModifyWeaponDamage(Player player, ref StatModifier damage)
    {
        base.ModifyWeaponDamage(player, ref damage);
        WeaponPlayer skill = player.GetModPlayer<WeaponPlayer>();
        if (!skill.SkillActive || skill.Skill is < 0 or > 1)
            return;
        int rank = Math.Clamp((skill.CurrentSkill?.ForceReplaceLevel ?? skill.CurrentSkill?.Level ?? 1) - 1, 0, 9);
        damage *= 1f + (skill.Skill == 0 ? Skill1Bonus[rank] : Skill2Bonus[rank]);
    }

    public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source,
        Vector2 position, Vector2 velocity, int type, int damage, float knockback)
    {
        if (player.whoAmI != Main.myPlayer)
            return false;
        WeaponPlayer skill = player.GetModPlayer<WeaponPlayer>();
        // 生成时锁存技能状态，飞行中切技能不会改变这发链电的伤害衰减规则。
        int mode = skill.SkillActive && skill.Skill is 0 or 1 ? skill.Skill + 1 : 0;
        Vector2 direction = (Main.MouseWorld - position).SafeNormalize(new Vector2(player.direction, 0f));
        int index = Projectile.NewProjectile(source, position, direction * LeiziFlyingDischarge.SpeedPerUpdate,
            type, damage, knockback, player.whoAmI, mode);
        if (Main.projectile.IndexInRange(index))
            Main.projectile[index].CritChance = player.GetWeaponCrit(Item);
        return false;
    }

    public override void AddRecipes()
    {
        CreateRecipe()
            .AddIngredient(ItemID.ThunderStaff)
            .AddIngredient(ItemID.HallowedBar, 12)
            .AddIngredient(ItemID.SoulofLight, 8)
            .AddIngredient(ItemID.CrystalShard, 15)
            .AddIngredient(ItemID.Wire, 20)
            .AddTile(TileID.MythrilAnvil)
            .Register();
    }
}
