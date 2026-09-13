using ArknightsMod.Content.Projectiles.Medic.Warfarin;
using ArknightsMod.Content.Items.Weapons.Medic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Weapons.Medic.Warfarin;

// By T
public sealed class WarfarinStaff : ExpansionWeaponBase
{
    protected override int[] EliteDamage => [12, 15, 18];
    public override string Texture => "ArknightsMod/Content/Items/Weapons/WarfarinWeaponTrue";

    public override void SetStaticDefaults()
    {
        Item.staff[Type] = true;
    }

    public override void SetDefaults()
    {
        Item.width = 46;
        Item.height = 46;
        Item.damage = EliteDamage[0];
        Item.DamageType = DamageClass.Summon;
        Item.useStyle = ItemUseStyleID.Shoot;
        // 五连血弹集中射出，下一次完整攻击按医疗五级规则在 5.7 秒后开始。
        Item.useTime = 12;
        Item.useAnimation = MedicalTreatment.StandardAttackIntervalTicks;
        Item.useLimitPerAnimation = 5;
        Item.noMelee = true;
        Item.autoReuse = true;
        Item.knockBack = 1.5f;
        Item.rare = ItemRarityID.Blue;
        Item.value = Item.sellPrice(silver: 40);
        Item.shoot = ModContent.ProjectileType<WarfarinBloodBolt>();
        Item.shootSpeed = 15f;
    }

    public override bool AltFunctionUse(Player player) => false;

    public override void HoldItem(Player player)
    {
        base.HoldItem(player);
        if (ArknightsKeybinds.SkillActivatePressed(player))
            player.GetModPlayer<WarfarinStaffPlayer>().TryActivate();
    }

    public override bool CanUseItem(Player player)
    {
        if (ArknightsKeybinds.SkillActivatePressed(player))
        {
            player.GetModPlayer<WarfarinStaffPlayer>().TryActivate();
            return false;
        }
        return base.CanUseItem(player);
    }

    public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source,
        Vector2 position, Vector2 velocity, int type, int damage, float knockback)
    {
        if (player.whoAmI != Main.myPlayer)
            return false;
        Vector2 aim = velocity.SafeNormalize(Vector2.UnitX * player.direction);
        position = player.MountedCenter + Collision.TileCollision(player.MountedCenter - new Vector2(5f),
            aim * 36f, 10, 10, true, true);
        Projectile.NewProjectile(source, position, velocity.RotatedByRandom(MathHelper.ToRadians(3f)),
            type, damage, knockback, player.whoAmI);
        SoundEngine.PlaySound(SoundID.Item17 with { Volume = .28f, Pitch = -.2f, MaxInstances = 3 }, position);
        return false;
    }

}
