using ArknightsMod.Content.Projectiles.Caster.Durin;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Weapons.Caster.Durin
{
	public class DurinWand : UpgradeWeaponBase
	{
		public override void SetStaticDefaults() {
			// DisplayName.SetDefault("Durin's wand");
			// Tooltip.SetDefault("She looks quite sleepy.");
		}

		public override void SetDefaults() {
			Item.damage = 24;
			Item.DamageType = DamageClass.Magic;
			Item.mana = 5;
			Item.width = 14;
			Item.height = 14;
			Item.useTime = 40;
			Item.useAnimation = 40;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.noMelee = true;
			Item.channel = true; //Channel so that you can held the weapon [Important]
			Item.knockBack = 4;
			Item.value = Item.sellPrice(0, 0, 0, 20);
			Item.rare = ItemRarityID.White;
			Item.shoot = ModContent.ProjectileType<DurinWand_Projectile>();
			Item.crit = 2; // The percent chance at hitting an enemy with a crit, plus the default amount of 4.
			Item.shootSpeed = 16f;
			Item.autoReuse = true;

			Item.UseSound = SoundID.Item20; 
		}

		public override void AddRecipes() {
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient(ItemID.ManaCrystal,1);
			recipe.AddIngredient(ItemID.RichMahogany,10);
			recipe.AddTile(TileID.WorkBenches);
			recipe.Register();
		}
	}
}