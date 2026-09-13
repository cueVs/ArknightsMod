using ArknightsMod.Content.Projectiles.Specialist.TexasAlter;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Weapons.Specialist.TexasAlter
{
	public class Blueberry : ModItem
	{

        public override void SetDefaults()
		{
			Item.damage = 86;
            Item.DamageType = DamageClass.Melee;
            Item.width = 66;
			Item.height = 70;
			Item.useTime = 28;
			Item.useAnimation = 28;
			Item.channel = true;
			Item.noUseGraphic = true;
			Item.noMelee = true;
			Item.useStyle = 5;
			Item.knockBack = 6;
			Item.value = Item.buyPrice(0, 22, 50, 0);
			Item.rare = 4;
			Item.UseSound = OmertosaAttackSound;
			Item.autoReuse = true;
			Item.shoot = ModContent.ProjectileType<BlueberryProjectile>();
			//Item.shoot = Mod.ProjectileType("BlueberryDarkChocolateProjectile");
			Item.shootSpeed = 2f;
		}

		SoundStyle OmertosaAttackSound = new SoundStyle("ArknightsMod/Content/Projectiles/Specialist/TexasAlter/OmertosaAttackSound") with
		{
			Volume = 0.5f,
			PitchVariance = 0.3f,
			MaxInstances = 0,
			//SoundLimitBehavior = SoundLimitBehavior.ReplaceOldest
		};

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.TitaniumBar, 20);
            recipe.AddIngredient(ItemID.FrostCore, 1);
            recipe.AddTile(TileID.MythrilAnvil);
            //recipe.AddTile(TileID.OrichalcumAnvil);
            recipe.Register();

            Recipe recipe2 = CreateRecipe();
            recipe2.AddIngredient(ItemID.AdamantiteBar, 20);
            recipe2.AddIngredient(ItemID.FrostCore, 1);
            recipe2.AddTile(TileID.MythrilAnvil);
            //recipe.AddTile(TileID.OrichalcumAnvil);
            recipe2.Register();
        }

		
	
			

    }
}

//using Terraria;
//using Terraria.ID;
//using Terraria.ModLoader;

//namespace ArknightsMod.Content.Items.Weapons
//{
//    public class BlueberryDarkChocolate : ModItem
//    {
       
//        public override void SetDefaults()
//        {
//            Item.width = 20;
//            Item.height = 20;
//            Item.maxStack = 999;
//            Item.value = 100;
//            Item.rare = ItemRarityID.Blue;
//            // Set other Item.X values here
//        }

//        public override void AddRecipes()
//        {
//            // Recipes here. See Basic Recipe Guide
//        }
//    }
//}