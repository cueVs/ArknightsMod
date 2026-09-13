using ArknightsMod.Content.Projectiles.Sniper.Adnachiel;
using ArknightsMod.Content.Tiles.Infrastructure;
using ArknightsMod.Players;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using ArknightsMod.Content;

namespace ArknightsMod.Content.Items.Weapons.Sniper.Adnachiel
{
	public class AdnachielCrossbow : UpgradeWeaponBase
	{
		private static SoundStyle KroosCrossbowS1;
		private static SoundStyle SkillActive1;
		public override void Load() {
			KroosCrossbowS1 = new SoundStyle("ArknightsMod/Sounds/KroosCrossbowS1") {
				Volume = 0.4f,
				MaxInstances = 4,
			};
			SkillActive1 = new SoundStyle("ArknightsMod/Sounds/SkillActive1") {
				Volume = 0.4f,
				MaxInstances = 4,
			};
		}
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 1;
		}

		public override void SetDefaults() {
			Item.damage = 19;
			Item.DamageType = DamageClass.Ranged;
			Item.width = 120;
			Item.height = 60;
			Item.useTime = 30;
			Item.useAnimation = 30;
			Item.scale =0.9f;
			Item.autoReuse = true;
			Item.consumeAmmoOnLastShotOnly = true;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.noMelee = true;
			Item.channel = true; //Channel so that you can held the weapon [Important]
			Item.knockBack = 2;
			Item.shoot = ProjectileID.WoodenArrowFriendly;
			Item.shootSpeed = 15f;
			Item.useAmmo = AmmoID.Arrow;


			Item.rare = ItemRarityID.Blue;
			Item.value = Item.sellPrice(0, 0, 3, 20);
			Item.UseSound = KroosCrossbowS1;
		}
		public override bool AltFunctionUse(Player player) => false;
		public override bool CanUseItem(Player player) {
			var modPlayer = Main.LocalPlayer.GetModPlayer<WeaponPlayer>();
			if (Main.myPlayer == player.whoAmI) {
				if (!ArknightsKeybinds.SkillActivatePressed(player)) {
					Item.UseSound = KroosCrossbowS1;

				}
			}
			if (Main.myPlayer == player.whoAmI) {
				if (ArknightsKeybinds.SkillActivatePressed(player)) {
					if (!modPlayer.SummonMode) {
						// S3
						if (modPlayer.Skill == 0 && modPlayer.StockCount > 0 && !modPlayer.SkillActive) {
							modPlayer.SkillActive = true;
							modPlayer.SkillTimer = 0;

							modPlayer.DelStockCount();

							Item.UseSound = SkillActive1;
							SoundEngine.PlaySound(Item.UseSound.Value, player.Center);
						}
						else
							return false;
					}
				}
			}
			return base.CanUseItem(player);
		}
		public override void ModifyWeaponDamage(Player player, ref StatModifier damage) {
			var modPlayer = Main.LocalPlayer.GetModPlayer<WeaponPlayer>();
			if (Main.myPlayer == player.whoAmI) {
				if (modPlayer.Skill == 0 && modPlayer.SkillActive == true) {
					damage *= 1.5f;
				}
			}
		}

		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			Projectile.NewProjectile(source, position, velocity, ModContent.ProjectileType<Adnachiel_Arrow>(), damage, knockback, player.whoAmI);
			return false;
		}
		public override Vector2? HoldoutOffset() {
			return new Vector2(-3f, 0f);
		}

		// This method lets you adjust position of the gun in the player's hands. Play with these values until it looks good with your graphics.
		//public override Vector2? HoldoutOffset() {
		//	return new Vector2(-2f, -2f);
		//}

		public override void AddRecipes() {
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient<Material.Polyester>(2);
			recipe.AddTile(ModContent.TileType<FactoryTile>());
			recipe.Register();
		}
	}
}
