using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using ArknightsMod.Content.Projectiles.Sniper.Exusiai;

namespace ArknightsMod.Content.Items.Weapons.Sniper.Exusiai
{
    public class ExusiaiVector_Ammo : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 99;
        }

        public override void SetDefaults()
        {
            Item.damage = 18;
            Item.DamageType = DamageClass.Ranged;
            Item.width = 8;
            Item.height = 8;
            Item.maxStack = Item.CommonMaxStack;
            Item.consumable = true;
            Item.knockBack = 1.5f;
            Item.value = 10;
            Item.rare = ItemRarityID.Green;
            Item.shoot = ModContent.ProjectileType<ExusiaiVector_Bullet>();
            Item.shootSpeed = 2f; // The speed of the projectile. This value equivalent to Silver Bullet since ExampleBullet's Projectile.extraUpdates is 1.
            Item.ammo = AmmoID.Bullet;
        }
    }
}
