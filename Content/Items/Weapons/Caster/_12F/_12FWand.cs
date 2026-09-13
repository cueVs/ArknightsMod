using ArknightsMod.Content.Projectiles.Caster._12F;
using Microsoft.Xna.Framework;

using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
namespace ArknightsMod.Content.Items.Weapons.Caster._12F
{
	public class _12FWand : UpgradeWeaponBase
	{
		private static SoundStyle NoSound;
        public override void Load()
        {
            NoSound = new SoundStyle("ArknightsMod/Sounds/NoSound")
            {
                Volume = 0f,
                MaxInstances = 4,
            };
        }

		public override void SetDefaults() {
			Item.damage = 30;
			Item.mana = 8;
			Item.knockBack = 4f;
			Item.useStyle = ItemUseStyleID.Swing; 
			Item.useAnimation = 87;
			Item.useTime = 87;
			Item.width = 20;
			Item.height = 20;
			Item.DamageType = DamageClass.Magic;  
			Item.channel = true; 
			Item.autoReuse = true;
			Item.noMelee = true; 
			Item.shoot = ModContent.ProjectileType<_12F_Projectile>();

			Item.rare = ItemRarityID.White;
			Item.value = Item.sellPrice(0, 0, 3, 50);

		}

		public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();

            recipe.AddIngredient(ItemID.GoldBar, 3);
            recipe.AddIngredient(ItemID.PalmWood, 10);
            recipe.AddTile(TileID.WorkBenches);

            recipe.Register();
        }
		public override void UseItemFrame(Player player)
		{
			float progress = 1f - player.itemAnimation / (float)player.itemAnimationMax;

			if (progress < 0.66f)
			{
				float holdRotation = -MathHelper.PiOver2 * 0.7f;
				player.itemRotation = holdRotation * player.direction;
			}
			else
			{
				float swingProgress = (progress - 0.66f) / 0.34f;

				float startRot = -MathHelper.PiOver2 * 0.7f;
				float endRot = MathHelper.PiOver2 * 0.8f;

				player.itemRotation = MathHelper.Lerp(startRot, endRot, swingProgress) * player.direction;
			}
		}


	}
}