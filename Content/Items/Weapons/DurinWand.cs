using ArknightsMod.Content.Projectiles;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;

namespace ArknightsMod.Content.Items.Weapons
{
	public class DurinWand : ModItem
	{
		public override void SetStaticDefaults()
		{
			// DisplayName.SetDefault("Durin's wand");
			// Tooltip.SetDefault("She looks quite sleepy.");
		}

		public override void SetDefaults()
		{
			Item.damage = 5;
			Item.DamageType = DamageClass.Magic;
			Item.mana = 2;
			Item.width = 14;
			Item.height = 14;
			Item.useTime = 40;
			Item.useAnimation = 40;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.noMelee = true;
			Item.channel = true; //Channel so that you can held the weapon [Important]
			Item.knockBack = 6;
			Item.value = Item.sellPrice(0, 0, 0, 20);
			Item.rare = ItemRarityID.White;
			Item.shoot = ModContent.ProjectileType<DurinWandProjectile>();
			Item.crit = 2; // The percent chance at hitting an enemy with a crit, plus the default amount of 4.
			Item.shootSpeed = 8f;

			Item.UseSound = SoundID.Item69; // The sound when the weapon is being used.

			// The sound that this item plays when used. Need "using Terraria.Audio;"
			//Item.UseSound = new SoundStyle("ArknightsMod/Assets/Sounds/Items/Weapons/DurinWand")
			//{
			//    Volume = 0.2f,
			//    MaxInstances = 3, //This dicatates how many instances of a sound can be playing at the same time. The default is 1. Adjust this to allow overlapping sounds.
			//};
		}

		//     public override void AddRecipes()
		//     {
		//         Recipe recipe = CreateRecipe();
		//recipe.AddRecipeGroup(RecipeGroupID.Wood, 10);
		//         recipe.AddTile(TileID.WorkBenches);
		//         recipe.Register();
		//     }
	}
}