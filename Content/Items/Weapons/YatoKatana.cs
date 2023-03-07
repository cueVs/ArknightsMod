using ArknightsMod.Content.Projectiles;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;

namespace ArknightsMod.Content.Items.Weapons
{
	public class YatoKatana : ModItem
	{

        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("Yato's Katana");
            // Tooltip.SetDefault("Yato has joined the team.");
        }

        public override void SetDefaults()
		{
            Item.damage = 8;
            Item.knockBack = 4f;
            Item.useStyle = ItemUseStyleID.Rapier; // Makes the player do the proper arm motion
            Item.useAnimation = 12;
            Item.useTime = 12;
            Item.width = 40;
            Item.height = 40;
            Item.DamageType = DamageClass.MeleeNoSpeed;
            Item.channel = true; //Channel so that you can held the weapon [Important]
            Item.autoReuse = false;
            Item.noUseGraphic = true; // The sword is actually a "projectile", so the item should not be visible when used
            Item.noMelee = true; // The projectile will do the damage and not the item

            Item.rare = ItemRarityID.White;
            Item.value = Item.sellPrice(0, 0, 3, 20);

            Item.shoot = ModContent.ProjectileType<YatoKatanaProjectile>(); // The projectile is what makes a shortsword work
            Item.shootSpeed = 2.3f; // This value bleeds into the behavior of the projectile as velocity, keep that in mind when tweaking values

            // The sound that this item plays when used. Need "using Terraria.Audio;"
            Item.UseSound = new SoundStyle("ArknightsMod/Sounds/YatoKatana")
            {
                Volume = 0.2f,
                MaxInstances = 1, //This dicatates how many instances of a sound can be playing at the same time. The default is 1. Adjust this to allow overlapping sounds.
            };
        }

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient<Placeable.OrirockCube>(5); //Please check here: https://github.com/tModLoader/tModLoader/wiki/Intermediate-Recipes#recipegroups
            recipe.AddTile(TileID.WorkBenches);
            recipe.Register();
        }
    }
}