using ArknightsMod.Content.Projectiles.Specialist.Red;
using ArknightsMod.Players;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Weapons.Specialist.Red;

public sealed class RedDagger : ExpansionWeaponBase
{
    // 原基础攻击 72 × 1.2 = 86.4，按整数面板取 86。
    protected override int[] EliteDamage => [86, 86, 86];
    internal const float AttackSpeedBonus = 1.1f;
    public override string Texture => "Terraria/Images/Item_" + ItemID.PsychoKnife;
    public override void SetDefaults()
    {
        Item.damage = EliteDamage[0];
        Item.DamageType = DamageClass.Melee;
        Item.width = 24;
        Item.height = 30;
        Item.useTime = Item.useAnimation = 6;
        Item.useStyle = ItemUseStyleID.Shoot;
        Item.channel = Item.autoReuse = Item.noUseGraphic = Item.noMelee = true;
        Item.knockBack = 1.5f;
        Item.rare = ItemRarityID.LightRed;
        Item.value = Item.sellPrice(gold: 3);
        Item.shoot = ModContent.ProjectileType<RedDaggerHoldout>();
        Item.shootSpeed = 1f;
    }
    public override bool AltFunctionUse(Player player) => false;
    public override float UseSpeedMultiplier(Player player) => AttackSpeedBonus;
    public override bool CanUseItem(Player player)
    {
        if (ArknightsKeybinds.SkillActivatePressed(player))
        {
            return false;
        }
        return player.ownedProjectileCounts[Item.shoot] < 1 && base.CanUseItem(player);
    }
    public override void ModifyWeaponDamage(Player player, ref StatModifier damage)
    {
        base.ModifyWeaponDamage(player, ref damage);
        var combat = player.GetModPlayer<RedDaggerPlayer>();
        if (combat.ExecutionActive)
            damage *= RedDaggerPlayer.ExecutionMultiplier(combat.Rank);
    }
    public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source,
        Vector2 position, Vector2 velocity, int type, int damage, float knockback)
    {
        if (player.whoAmI == Main.myPlayer)
            Projectile.NewProjectile(source, player.MountedCenter,
                (Main.MouseWorld - player.MountedCenter).SafeNormalize(new Vector2(player.direction, 0)),
                type, damage, knockback, player.whoAmI);
        return false;
    }
    public override void AddRecipes()
    {
        // 两条等价配方：钴锭或钯金锭任选一种，不要求同时提供。
        foreach (int bar in new[] { ItemID.CobaltBar, ItemID.PalladiumBar })
        {
            CreateRecipe().AddIngredient(ItemID.ThrowingKnife, 50).AddIngredient(bar, 10)
                .AddIngredient(ItemID.SoulofNight, 8).AddIngredient(ItemID.Silk, 5)
                .AddIngredient(ItemID.RedDye)
                .AddTile(TileID.MythrilAnvil).Register();
        }
    }
}
