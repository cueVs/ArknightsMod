using System;
using ArknightsMod.Content.Projectiles.Medic.Sussurro;
using ArknightsMod.Players;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Weapons.Medic.Sussurro;

// By T
/// <summary>困难模式初期的攻击医疗法杖：射击与治疗各自计时，不以敌人数量放大回血。</summary>
public sealed class SussurroStaff : ExpansionWeaponBase
{
    protected override int[] EliteDamage => [54, 62, 70];
    public override string Texture => "Terraria/Images/Item_" + ItemID.EmeraldStaff;
    public override void SetStaticDefaults()
    {
        Item.staff[Type] = true;
        ItemID.Sets.ItemsThatAllowRepeatedRightClick[Type] = true;
    }
    public override void SetDefaults()
    {
        Item.width = Item.height = 40;
        Item.damage = EliteDamage[0];
        Item.DamageType = DamageClass.Magic;
        // 沿用异客的原生连发计数：0、4、8、12 帧射击，间隔 15 帧后在第 27 帧开始下一轮。
        Item.useTime = 4;
        Item.useAnimation = 27;
        Item.useLimitPerAnimation = 4;
        Item.mana = 8;
        Item.noMelee = true;
        Item.autoReuse = true;
        Item.useStyle = ItemUseStyleID.Shoot;
        Item.knockBack = 1.5f;
        Item.rare = ItemRarityID.LightRed;
        Item.value = Item.sellPrice(gold: 3);
        Item.shoot = ModContent.ProjectileType<SussurroNeedle>();
        Item.shootSpeed = 12f;
    }

    public override bool AltFunctionUse(Player player) => true;
    public override bool CanUseItem(Player player)
    {
        if (ArknightsKeybinds.SkillActivatePressed(player))
        {
            player.GetModPlayer<SussurroStaffPlayer>().TryActivate();
            return false;
        }
        return base.CanUseItem(player);
    }
    public override float UseSpeedMultiplier(Player player)
        => player.GetModPlayer<SussurroStaffPlayer>().DeepTreatment ? 2f : 1f;

    public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source,
        Vector2 position, Vector2 velocity, int type, int damage, float knockback)
    {
        if (player.whoAmI != Main.myPlayer)
            return false;
        SussurroStaffPlayer treatment = player.GetModPlayer<SussurroStaffPlayer>();
        Vector2 direction = (Main.MouseWorld - player.MountedCenter).SafeNormalize(new Vector2(player.direction, 0f));
        position = player.MountedCenter + direction * 36f;
        treatment.RegisterCast(player.altFunctionUse == 2);
        if (player.altFunctionUse == 2)
            return false; // 右键专注治疗光标附近的队友，不向队友射出伤害激光。

        direction = direction.RotatedByRandom(MathHelper.ToRadians(2f));
        bool compressed = treatment.NextCompressedShot();
        int projectileType = compressed ? ModContent.ProjectileType<SussurroCompressionRay>() : type;
        int shotDamage = compressed ? (int)MathF.Round(damage * 1.6f) : damage;
        int index = Projectile.NewProjectile(source, position, direction * 12f,
            projectileType, shotDamage, knockback, player.whoAmI, treatment.DeepTreatment ? 1f : 0f);
        if (Main.projectile.IndexInRange(index))
            Main.projectile[index].CritChance = player.GetWeaponCrit(Item);
        SoundEngine.PlaySound((compressed ? SoundID.Item158 : SoundID.Item91)
            with { Volume = compressed ? .32f : .22f, Pitch = compressed ? .15f : .42f, MaxInstances = 3 }, position);
        return false;
    }

    public override void AddRecipes()
    {
        // 钴/钯两种世界都能制作，不要求机械首领材料。
        foreach (int bar in new[] { ItemID.CobaltBar, ItemID.PalladiumBar })
            CreateRecipe().AddIngredient(ItemID.EmeraldStaff).AddIngredient(bar, 12)
                .AddIngredient(ItemID.CrystalShard, 15).AddIngredient(ItemID.PixieDust, 15)
                .AddIngredient(ItemID.HealingPotion, 10).AddTile(TileID.Anvils).Register();
    }
}
