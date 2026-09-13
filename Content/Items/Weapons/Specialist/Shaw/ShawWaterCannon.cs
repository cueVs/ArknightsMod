using ArknightsMod.Content.Projectiles.Specialist.Shaw;
using ArknightsMod.Content.Tiles.Infrastructure;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Weapons.Specialist.Shaw
{
    public class ShawWaterCannon : ModItem
    {
        public override void SetStaticDefaults() { }

        private Vector2 offset = new Vector2(0f, 10f);

        public override void SetDefaults()
        {
            Item.DefaultToRangedWeapon(ModContent.ProjectileType<ShawWaterCannon_Projectile>(), AmmoID.Gel, singleShotTime: 30, shotVelocity: 6f, hasAutoReuse: true);
            Item.width = 44;
            Item.height = 28;
            Item.damage = 53 * (int)1.8f;//
            Item.knockBack = 20f;
            Item.UseSound = SoundID.Item85;
            //Item.value = Item.buyPrice(gold: 1);
            Item.rare = ItemRarityID.Yellow;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Projectile.NewProjectile(source, position + offset, velocity, type, damage, knockback, player.whoAmI);

            Vector2 offset2 = offset + velocity.SafeNormalize(Vector2.Zero) * 32;
            for (int i = 0; i < 12; i++)
            {
                int dust = Dust.NewDust(position + offset2, 0, 0, DustID.Water_Snow);
                Main.dust[dust].noGravity = true;
                Main.dust[dust].velocity = velocity.RotatedByRandom(MathHelper.PiOver4) * Main.rand.NextFloat(0, 1);
                Main.dust[dust].scale = 2;
                Main.dust[dust].alpha = 95;
            }

            return false;
        }

        public override Vector2? HoldoutOffset()
        {
            return offset;
        }
		public override void AddRecipes() {
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient<Material.Oriron>(2);
			recipe.AddTile(ModContent.TileType<FactoryTile>());
			recipe.Register();
		}
	}
}
