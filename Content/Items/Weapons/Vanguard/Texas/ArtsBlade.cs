
using Terraria.Audio;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using ArknightsMod.Content.Tiles.Infrastructure;

namespace ArknightsMod.Content.Items.Weapons.Vanguard.Texas
{
	public class ArtsBlade : ModItem
	{
		public override void SetDefaults()
		{
			Item.damage = 72;
			Item.DamageType = DamageClass.Melee;
			Item.width = 40;
			Item.height = 40;
			Item.useTime = 33;
			Item.useAnimation = 33;
			Item.useStyle = 1;
			Item.knockBack = 3;
			Item.value = 10000;
			Item.rare = 3;
            Item.UseSound = ArtsBladeSound;
            Item.autoReuse = true;
		}

        SoundStyle ArtsBladeSound = new SoundStyle("ArknightsMod/Content/Projectiles/Vanguard/Texas/ArtsBladeSound") with
        {
            Volume = 0.5f,
            PitchVariance = 0.3f,
            MaxInstances = 0,
            //SoundLimitBehavior = SoundLimitBehavior.ReplaceOldest
        };

        public override void AddRecipes()
		{
            Recipe recipe1 = CreateRecipe();
            recipe1.AddIngredient(ItemID.HellstoneBar, 20);
            recipe1.AddIngredient(ItemID.BluePhaseblade, 1);
            recipe1.AddTile(ModContent.TileType<FactoryTile>());
            recipe1.Register();

            Recipe recipe2 = CreateRecipe();
			recipe2.AddIngredient(ItemID.HellstoneBar, 20);
            recipe2.AddIngredient(ItemID.RedPhaseblade, 1);
            recipe2.AddTile(ModContent.TileType<FactoryTile>());
            recipe2.Register();

            Recipe recipe3 = CreateRecipe();
            recipe3.AddIngredient(ItemID.HellstoneBar, 20);
            recipe3.AddIngredient(ItemID.GreenPhaseblade, 1);
            recipe3.AddTile(ModContent.TileType<FactoryTile>());
            recipe3.Register();

            Recipe recipe4 = CreateRecipe();
            recipe4.AddIngredient(ItemID.HellstoneBar, 20);
            recipe4.AddIngredient(ItemID.PurplePhaseblade, 1);
            recipe4.AddTile(ModContent.TileType<FactoryTile>());
            recipe4.Register();

            Recipe recipe5 = CreateRecipe();
            recipe5.AddIngredient(ItemID.HellstoneBar, 20);
            recipe5.AddIngredient(ItemID.WhitePhaseblade, 1);
            recipe5.AddTile(ModContent.TileType<FactoryTile>());
            recipe5.Register();

            Recipe recipe6 = CreateRecipe();
            recipe6.AddIngredient(ItemID.HellstoneBar, 20);
            recipe6.AddIngredient(ItemID.YellowPhaseblade, 1);
            recipe6.AddTile(ModContent.TileType<FactoryTile>());
            recipe6.Register();

            Recipe recipe7 = CreateRecipe();
            recipe7.AddIngredient(ItemID.HellstoneBar, 20);
            recipe7.AddIngredient(ItemID.OrangePhaseblade, 1);
            recipe7.AddTile(ModContent.TileType<FactoryTile>());
            recipe7.Register();

        }

        public override void MeleeEffects(Player player, Rectangle hitbox)
        {

            Lighting.AddLight(player.Center, 1f, 0.9f, 0.8f);
            int dust = Dust.NewDust(new Vector2(hitbox.X, hitbox.Y), hitbox.Width, hitbox.Height, 270, 0f, 0f, 0, default(Color), 0.9f);
            Main.dust[dust].noGravity = true;

        }

    }
}