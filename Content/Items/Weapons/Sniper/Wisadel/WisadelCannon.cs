using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;
using Terraria.Audio;
using ArknightsMod.Content.Projectiles.Sniper.Wisadel;
using ArknightsMod.Content.Items.Material;
using ArknightsMod.Content.Tiles.Infrastructure;

namespace ArknightsMod.Content.Items.Weapons.Sniper.Wisadel
{
    public class WisadelCannon : ModItem
    {
        public override void SetDefaults()
        {
            Item.damage = 138;
            Item.Size = new(90, 32);
            Item.knockBack = 15;
            Item.rare = ItemRarityID.Red;
            Item.DamageType = DamageClass.Ranged;
            Item.value = Item.sellPrice(300);
            Item.crit = 20;
            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.UseSound = null;
            Item.useAmmo = AmmoID.Bullet;
            Item.shoot = ProjectileID.Bullet;
            Item.shootSpeed = 16f;
            Item.noUseGraphic = true;
            Item.noMelee = true;
            Item.channel = true;
        }
        public override void HoldItem(Player player)
        {
            if (player.ownedProjectileCounts[ModContent.ProjectileType<Wisdel_Probe>()] <= 0)
            {
				for (int i = 3; i >= 0; i--) {
					Projectile.NewProjectile(player.GetSource_ItemUse(Item), player.Center,
						Vector2.Zero, ModContent.ProjectileType<Wisdel_Probe>(),
						player.GetWeaponDamage(Item), player.GetWeaponKnockback(Item), player.whoAmI,
						ai0: i);
				}
				SoundEngine.PlaySound(Wisdel_Probe.Summon);
            }
        }
        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            type = ProjectileID.Bullet;
        }
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) => false;
		public override void AddRecipes() {
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient<CrystallineElectronicUnit>(3);
			recipe.AddIngredient<OptimizedDevice>(6);
			recipe.AddTile(ModContent.TileType<FactoryTile>());
			recipe.Register();
		}
	}
}
