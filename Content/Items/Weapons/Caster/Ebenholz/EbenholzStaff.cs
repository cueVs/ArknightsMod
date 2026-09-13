using System;
using ArknightsMod.Content.Items.Weapons.Caster.Harmonie;
using ArknightsMod.Content.Projectiles.Caster.Ebenholz;
using ArknightsMod.Players;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Weapons.Caster.Ebenholz;

// By T
public sealed class EbenholzStaff : ExpansionWeaponBase
{
    protected override int[] EliteDamage => [97, 109, 124]; // 原面板提高 5%，取整。
    public override string Texture => "Terraria/Images/Item_" + ItemID.ShadowbeamStaff;
    public override void SetStaticDefaults() => Item.staff[Type] = true;
    public override void SetDefaults()
    {
        Item.width = Item.height = 44;
        Item.damage = EliteDamage[0];
        Item.DamageType = DamageClass.Magic;
        Item.useStyle = ItemUseStyleID.Shoot;
        // 一轮三枚：0、5、10 帧；第 30 帧开始下一轮。
        Item.useTime = 5;
        Item.useAnimation = 30;
        Item.useLimitPerAnimation = 3;
        Item.mana = 6;
        Item.noMelee = Item.autoReuse = true;
        Item.knockBack = 2f;
        Item.rare = ItemRarityID.Yellow;
        Item.value = Item.sellPrice(gold: 7);
        Item.shoot = ModContent.ProjectileType<EbenholzCube>();
        Item.shootSpeed = 14f;
    }
    public override bool CanUseItem(Player player)
    {
        if (ArknightsKeybinds.SkillActivatePressed(player))
        {
            player.GetModPlayer<EbenholzStaffPlayer>().TryActivate();
            return false;
        }
        return base.CanUseItem(player);
    }
    public override float UseSpeedMultiplier(Player player)
        => player.GetModPlayer<EbenholzStaffPlayer>().Mode switch { 1 => 2.5f, 3 => 1.6f, _ => 1f };

    public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position,
        Vector2 velocity, int type, int damage, float knockback)
    {
        if (player.whoAmI != Main.myPlayer)
            return false;
        var state = player.GetModPlayer<EbenholzStaffPlayer>();
        Vector2 aim = (Main.MouseWorld - player.MountedCenter).SafeNormalize(new Vector2(player.direction, 0));
        position = player.MountedCenter;
        if (EbenholzCombat.Clear(position, position + aim * 34f))
            position += aim * 34f;
        int target = EbenholzCombat.FindTarget(position, 900f, Main.MouseWorld, state.Mode == 3);
        float multiplier = state.Mode == 1 ? .5f + state.Rank / 90f : state.Mode == 3 ? 1.35f + state.Rank / 30f : 1f;
        damage = Math.Max(1, (int)MathF.Round(damage * multiplier));
        Spawn(position, aim.RotatedByRandom(.035f) * 14f, damage, 0, 0);
        int stored = state.ReleaseCharges();
        for (int i = 0; i < stored; i++)
            Spawn(player.MountedCenter, aim.RotatedBy((i - (stored - 1) * .5f) * .22f) * 11f,
                (int)MathF.Round(damage * (state.Mode == 3 ? 1.4f : 1.15f)), 1, 1 + i * 4);
        SoundEngine.PlaySound(SoundID.Item103 with { Volume = .32f, Pitch = -.45f, MaxInstances = 3 }, position);
        return false;

        void Spawn(Vector2 origin, Vector2 speed, int hit, int mode, int delay)
        {
            int index = Projectile.NewProjectile(source, origin, speed, type, hit, knockback,
                player.whoAmI, mode, target + 1, delay);
            if (Main.projectile.IndexInRange(index))
                Main.projectile[index].CritChance = player.GetWeaponCrit(Item);
        }
    }
    public override void AddRecipes() => CreateRecipe().AddIngredient<HarmonieStaff>().AddIngredient(ItemID.ShadowbeamStaff)
        .AddIngredient(ItemID.SpectreBar, 5).AddIngredient(ItemID.SoulofNight, 5)
        .AddIngredient(ItemID.BrownDye).AddIngredient(ItemID.SilverDye)
        .AddRecipeGroup(OperatorWeaponRecipeGroups.AnyVanillaPiano)
        .AddIngredient<global::ArknightsMod.Content.Items.Material.Polyketon>(7)
        .AddIngredient<global::ArknightsMod.Content.Items.Material.D32Steel>(4)
        .AddTile(ModContent.TileType<global::ArknightsMod.Content.Tiles.Infrastructure.FactoryTile>()).Register();
}
