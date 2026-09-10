using ArknightsMod.Content.Projectiles.Specialist.Ethan;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Weapons.Specialist.Ethan;

// By T
public sealed class EthanYoyo : ExpansionWeaponBase
{
    protected override int[] EliteDamage => [52, 62, 74];
    public override string Texture => "Terraria/Images/Item_" + ItemID.Chik;
    public override void SetStaticDefaults()
    {
        ItemID.Sets.Yoyo[Type] = true;
        ItemID.Sets.GamepadExtraRange[Type] = 15;
        ItemID.Sets.GamepadSmartQuickReach[Type] = true;
    }
    public override void SetDefaults()
    {
        Item.width = Item.height = 28;
        Item.damage = EliteDamage[0];
        Item.DamageType = DamageClass.MeleeNoSpeed;
        Item.useStyle = ItemUseStyleID.Shoot;
        Item.useTime = Item.useAnimation = 22;
        Item.channel = Item.noMelee = Item.noUseGraphic = true;
        Item.knockBack = 1f;
        Item.shoot = ModContent.ProjectileType<EthanSpinningYoyo>();
        Item.shootSpeed = 24f;
        Item.rare = ItemRarityID.Pink;
        Item.value = Item.sellPrice(gold: 4);
        Item.UseSound = SoundID.Item1;
    }
    public override bool CanUseItem(Player player)
    {
        if (ArknightsKeybinds.SkillActivatePressed(player))
        {
            player.GetModPlayer<EthanYoyoPlayer>().TryActivate();
            return false;
        }
        return base.CanUseItem(player);
    }
    public override void AddRecipes() => CreateRecipe().AddIngredient(ItemID.Chik)
        .AddIngredient(ItemID.JungleSpores, 12).AddIngredient(ItemID.SoulofNight, 6)
        .AddIngredient(ItemID.GreenDye).AddTile(TileID.MythrilAnvil).Register();
}
