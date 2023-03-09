using ArknightsMod.Content.Projectiles;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent.Creative;
using Terraria.Audio;

namespace ArknightsMod.Content.Items.Weapons
{
	public class Shinginzan : ModItem
	{

		public override void SetStaticDefaults()
		{
			// DisplayName.SetDefault("Yato's Katana");
			// Tooltip.SetDefault("Yato has joined the team.");
			CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
		}

		public override void SetDefaults()
		{
			Item.damage = 150;
			Item.knockBack = 6.5f;
			Item.useStyle = ItemUseStyleID.Swing; // Makes the player do the proper arm motion
			Item.useAnimation = 30;
			Item.useTime = 30;
			Item.width = 36;
			Item.height = 36;
			Item.DamageType = DamageClass.MeleeNoSpeed;
			Item.channel = true; //Channel so that you can held the weapon [Important]
			Item.crit = 10; // The percent chance at hitting an enemy with a crit, plus the default amount of 4.
			Item.autoReuse = true;

			Item.rare = ItemRarityID.Yellow;
			Item.value = Item.sellPrice(0, 0, 4, 0);

			Item.shoot = ModContent.ProjectileType<ShinginzanProjectile>(); // The projectile is what makes a shortsword work
			Item.shootSpeed = 4f; // This value bleeds into the behavior of the projectile as velocity, keep that in mind when tweaking values

			Item.UseSound = SoundID.Item69; // The sound when the weapon is being used.

			// The sound that this item plays when used. Need "using Terraria.Audio;"
			//Item.UseSound = new SoundStyle("ArknightsMod/Assets/Sounds/Items/Weapons/Shinginzan")
			//{
			//    Volume = 0.2f,
			//    MaxInstances = 3, //This dicatates how many instances of a sound can be playing at the same time. The default is 1. Adjust this to allow overlapping sounds.
			//};
		}

		//public override void AddRecipes()
		//{
		//	Recipe recipe = CreateRecipe();
		//          recipe.AddRecipeGroup(RecipeGroupID.IronBar, 5); //Please check here: https://github.com/tModLoader/tModLoader/wiki/Intermediate-Recipes#recipegroups
		//          recipe.AddTile(TileID.Anvils);
		//	recipe.Register();
		//}
	}
}