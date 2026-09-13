using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using ArknightsMod.Content.Projectiles.Supporter.Orchid;
using ArknightsMod.Content.Items.Material;
using ArknightsMod.Content.Tiles.Infrastructure;

namespace ArknightsMod.Content.Items.Weapons.Supporter.Orchid
{
    public class OrchidUmbrellla : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.staff[Type] = true;
        }

        public override void SetDefaults()
        {
            Item.width = Item.height = 40;
            Item.useAnimation = 57;
            Item.useTime = 57;
            Item.useLimitPerAnimation = 1;
            Item.shootSpeed = 8;
            Item.mana = 1;
            Item.useStyle = 1;
            Item.shoot = ModContent.ProjectileType<OrchidUmbrellla_Projectile>();
            Item.DamageType = DamageClass.Magic;

            Item.UseSound = SoundID.Item1;

            Item.SetWeaponValues(24, 0);//伤害，击退

            Item.SetShopValues(ItemRarityColor.Green2, Item.sellPrice(gold: 1));

            Item.autoReuse = true;
            Item.noMelee = true;
        }
		public override void AddRecipes() {
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient<Polyester>(2);
			recipe.AddTile(ModContent.TileType<FactoryTile>());
			recipe.Register();
		}
	}
}
