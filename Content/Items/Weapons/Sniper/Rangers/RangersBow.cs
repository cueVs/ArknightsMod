using Terraria;
using Terraria.ID;
using Terraria.ModLoader;


// 感觉有点太素了，，
namespace ArknightsMod.Content.Items.Weapons.Sniper.Rangers
{
	public class RangersBow : UpgradeWeaponBase
	{
		public override void SetDefaults() {
			Item.damage = 16;
			Item.DamageType = DamageClass.Ranged;
			Item.width = 14;
			Item.height = 14;
			Item.useTime = 30;
			Item.useAnimation = 30;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.noMelee = true;
			Item.channel = true; //Channel so that you can held the weapon [Important]
			Item.knockBack = 4;
			Item.value = Item.sellPrice(0, 0, 0, 25);
			Item.rare = ItemRarityID.White;
			Item.shoot = ProjectileID.WoodenArrowFriendly;
            Item.useAmmo = AmmoID.Arrow;
			Item.crit = 2; // The percent chance at hitting an enemy with a crit, plus the default amount of 4.
			Item.shootSpeed = 160f;
			Item.autoReuse = true;

			Item.UseSound = SoundID.Item5; 
		}

		public override void AddRecipes() {
			Recipe recipe = CreateRecipe();
			recipe.AddRecipeGroup(RecipeGroupID.Wood, 12);
            recipe.AddIngredient(ItemID.Silk, 3); 
			recipe.AddTile(TileID.WorkBenches);
			recipe.Register();
		}
	}
}