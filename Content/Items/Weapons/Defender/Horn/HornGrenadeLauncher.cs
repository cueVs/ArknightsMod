using System;
using ArknightsMod.Content.Projectiles.Defender;
using ArknightsMod.Content.Projectiles.Defender.Horn;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Weapons.Defender.Horn;

// By T
public sealed class HornGrenadeLauncher : ExpansionWeaponBase, IShieldGuardWeapon
{
    protected override int[] EliteDamage => [388, 462, 554]; // 原面板 × 850 / 230，取整。
    public override string Texture => "Terraria/Images/Item_" + ItemID.GrenadeLauncher;
    public override void SetDefaults()
    {
        Item.width = 48;
        Item.height = 60;
        Item.damage = EliteDamage[0];
        Item.DamageType = DamageClass.Ranged;
        Item.useStyle = ItemUseStyleID.Shoot;
        Item.useTime = Item.useAnimation = 42;
        Item.noMelee = Item.noUseGraphic = Item.autoReuse = true;
        Item.knockBack = 7f;
        Item.shootSpeed = 17f;
        Item.shoot = ModContent.ProjectileType<HornGrenade>();
        Item.rare = ItemRarityID.Pink;
        Item.value = Item.sellPrice(gold: 6);
    }
    public override bool AltFunctionUse(Player player) => false;
    public override bool CanUseItem(Player player)
    {
        var state = player.GetModPlayer<HornLauncherPlayer>();
        if (ArknightsKeybinds.SkillActivatePressed(player)) { state.TryActivate(); return false; }
        return !state.Dumping && base.CanUseItem(player);
    }
    public override float UseSpeedMultiplier(Player player) => player.GetModPlayer<HornLauncherPlayer>().Mode == 3 ? 2.8f / 2.2f : 1f;
    public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
    {
        if (player.whoAmI == Main.myPlayer)
            player.GetModPlayer<HornLauncherPlayer>().Fire(source, damage, knockback);
        return false;
    }
    public void OnGuardSuccess(Player player) => player.GetModPlayer<HornLauncherPlayer>().CounterReady();
    public override void AddRecipes() => CreateRecipe().AddIngredient(ItemID.ObsidianShield)
        .AddIngredient(ItemID.GrenadeLauncher).AddIngredient(ItemID.ChlorophyteBar, 15)
        .AddIngredient(ItemID.LavaBucket, 2)
        .AddIngredient(ItemID.IllegalGunParts).AddIngredient<global::ArknightsMod.Content.Items.Material.Orirock>(12)
        .AddIngredient<global::ArknightsMod.Content.Items.Material.D32Steel>(4)
        .AddTile(ModContent.TileType<global::ArknightsMod.Content.Tiles.Infrastructure.FactoryTile>()).Register();
}
