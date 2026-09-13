using System;
using ArknightsMod.Content.Items.Weapons.Caster.Indigo;
using ArknightsMod.Content.Projectiles.Caster.Ebenholz;
using ArknightsMod.Content.Projectiles.Caster.Harmonie;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Weapons.Caster.Harmonie;

// By T
public sealed class HarmonieStaff : ExpansionWeaponBase
{
    protected override int[] EliteDamage => [52, 60, 70];
    // 杖身用翡翠法杖占位，技能图标复用黑键；音符取自模组已有的 Saki 资源。
    public override string Texture => "Terraria/Images/Item_" + ItemID.EmeraldStaff;
    public override void SetStaticDefaults() => Item.staff[Type] = true;
    public override void SetDefaults()
    {
        Item.width = Item.height = 40;
        Item.damage = EliteDamage[0];
        Item.DamageType = DamageClass.Magic;
        Item.useStyle = ItemUseStyleID.Shoot;
        Item.useTime = 5;
        Item.useAnimation = 30;
        Item.useLimitPerAnimation = 3;
        Item.mana = 6;
        Item.noMelee = Item.autoReuse = true;
        Item.knockBack = 2f;
        Item.rare = ItemRarityID.LightRed;
        Item.value = Item.sellPrice(gold: 3);
        Item.shoot = ModContent.ProjectileType<HarmonieNote>();
        Item.shootSpeed = 14f;
    }

    public override bool AltFunctionUse(Player player) => false;
    public override bool CanUseItem(Player player)
    {
        if (ArknightsKeybinds.SkillActivatePressed(player))
        {
            player.GetModPlayer<HarmonieStaffPlayer>().TryActivate();
            return false;
        }
        return base.CanUseItem(player);
    }
    public override float UseSpeedMultiplier(Player player)
        => player.GetModPlayer<HarmonieStaffPlayer>().Mode switch { 1 => 5f, 2 => 1.25f, _ => 1f };

    public override void ModifyManaCost(Player player, ref float reduce, ref float mult)
    {
        // 五倍射速时每发降至20%魔力消耗，维持原先每秒魔力开销。
        if (player.GetModPlayer<HarmonieStaffPlayer>().Mode == 1) mult *= .2f;
    }

    public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position,
        Vector2 velocity, int type, int damage, float knockback)
    {
        if (player.whoAmI != Main.myPlayer) return false;
        var state = player.GetModPlayer<HarmonieStaffPlayer>();
        Vector2 aim = (Main.MouseWorld - player.MountedCenter).SafeNormalize(new Vector2(player.direction, 0));
        position = player.MountedCenter;
        if (EbenholzCombat.Clear(position, position + aim * 34f)) position += aim * 34f;
        int target = EbenholzCombat.FindTarget(position, 900f, Main.MouseWorld, false);
        if (state.Mode == 1) damage = Math.Max(1, (int)MathF.Round(damage * .5f));
        Spawn(position, aim.RotatedByRandom(.035f) * 14f, damage, 0, 0);
        int stored = state.ReleaseCharges();
        for (int i = 0; i < stored; i++)
            Spawn(player.MountedCenter, aim.RotatedBy((i - (stored - 1) * .5f) * .22f) * 11f,
                (int)MathF.Round(damage * 1.15f), 1, 1 + i * 4);
        HarmonieVisuals.Muzzle(position, aim, stored > 0);
        SoundEngine.PlaySound(SoundID.Item26 with { Volume = .22f, Pitch = -.2f + Main.rand.NextFloat(.3f), MaxInstances = 3 }, position);
        return false;

        void Spawn(Vector2 origin, Vector2 speed, int hit, int mode, int delay)
        {
            int index = Projectile.NewProjectile(source, origin, speed, type, hit, knockback,
                player.whoAmI, mode, target + 1, delay);
            if (Main.projectile.IndexInRange(index)) Main.projectile[index].CritChance = player.GetWeaponCrit(Item);
        }
    }

    public override void AddRecipes()
    {
        // 两条等价配方，钴蓝/钯金世界都能在困难模式初期制作，不要求机械首领材料。
        foreach (int bar in new[] { ItemID.CobaltBar, ItemID.PalladiumBar })
            CreateRecipe().AddIngredient<IndigoStaff>().AddIngredient(bar, 12)
                .AddIngredient(ItemID.CrystalShard, 8).AddIngredient(ItemID.SoulofLight, 6)
                .AddIngredient<global::ArknightsMod.Content.Items.Material.Device>(3)
                .AddIngredient<global::ArknightsMod.Content.Items.Material.RMA7024>(6)
                .AddTile(ModContent.TileType<global::ArknightsMod.Content.Tiles.Infrastructure.FactoryTile>()).Register();
    }
}
