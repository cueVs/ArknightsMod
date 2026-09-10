using System;
using ArknightsMod.Content.Items.Weapons.Guard.Hellagur;
using ArknightsMod.Content.Projectiles.BasePROJ;
using ArknightsMod.Content.Projectiles.Guard.Utage;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Weapons.Guard.Utage;

// By T
// 四星暂按精二一级基础攻击 575 × 15% = 86；不计信赖、潜能、模组。
public sealed class UtageKatana : ExpansionWeaponBase
{
    protected override int[] EliteDamage => [86, 86, 86];
    internal const string WeaponTexturePath = "ArknightsMod/Content/Items/Weapons/Guard/Utage/UtageKatana";
    public override string Texture => WeaponTexturePath;

    public override void SetDefaults()
    {
        Item.damage = EliteDamage[0];
        Item.DamageType = DamageClass.Melee;
        Item.width = 60;
        Item.height = 56;
        Item.useTime = Item.useAnimation = 2;
        Item.useStyle = ItemUseStyleID.Shoot;
        Item.noMelee = Item.noUseGraphic = Item.autoReuse = true;
        Item.knockBack = 5f;
        Item.rare = ItemRarityID.LightRed;
        Item.value = Item.sellPrice(gold: 2);
        Item.shoot = ModContent.ProjectileType<UtageKatanaSwing>();
        Item.shootSpeed = 1f;
    }

    public override bool AltFunctionUse(Player player) => false;
    public override bool CanUseItem(Player player)
    {
        if (ArknightsKeybinds.SkillActivatePressed(player))
        {
            player.GetModPlayer<UtageCombatPlayer>().TryActivate();
            return false;
        }
        return !player.GetModPlayer<UtageCombatPlayer>().Resting
            && player.ownedProjectileCounts[Item.shoot] < 1 && base.CanUseItem(player);
    }

    public override void ModifyWeaponDamage(Player player, ref StatModifier damage)
    {
        base.ModifyWeaponDamage(player, ref damage);
        var combat = player.GetModPlayer<UtageCombatPlayer>();
        if (combat.ArtsActive)
            damage *= UtageCombatPlayer.ArtsMultiplier(combat.Rank);
    }

    public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source,
        Vector2 position, Vector2 velocity, int type, int damage, float knockback)
    {
        if (player.whoAmI != Main.myPlayer)
            return false;
        var combat = player.GetModPlayer<UtageCombatPlayer>();
        if (combat.Resting)
            return false;
        // 4 是宴的法术附刃快照，绕过赫拉格的三个专属技能动作。
        int mode = combat.ArtsActive ? 4 : 0;
        int combo = BaseHeldMeleeSupport.NextCombo(player, 3);
        Vector2 aim = (Main.MouseWorld - player.MountedCenter).SafeNormalize(new Vector2(player.direction, 0));
        int index = Projectile.NewProjectile(source, player.MountedCenter, aim, type, damage, knockback,
            player.whoAmI, combo, mode, HellagurSkillBridge.GetLowHealthActionSpeed(player));
        if (Main.projectile.IndexInRange(index))
            Main.projectile[index].CritChance = player.GetWeaponCrit(Item);
        return false;
    }

    public override void AddRecipes()
    {
        foreach (int sword in new[] { ItemID.Katana, ItemID.Muramasa })
        {
            CreateRecipe().AddIngredient(sword).AddIngredient(ItemID.SoulofNight, 4)
                .AddIngredient(ItemID.DarkShard).AddTile(TileID.MythrilAnvil).Register();
        }
    }
}
