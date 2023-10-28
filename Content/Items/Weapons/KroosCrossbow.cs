using ArknightsMod.Common.Players;
using ArknightsMod.Content.Buffs;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.Audio;
using Terraria.DataStructures;
using ArknightsMod.Common.UI;

namespace ArknightsMod.Content.Items.Weapons
{
    public class KroosCrossbow : ModItem
    {
		public override void SetStaticDefaults() {
			// DisplayName.SetDefault("Yato's Katana");
			// Tooltip.SetDefault("Yato has joined the team.");
		}

		public override void SetDefaults()
        {
            Item.damage = 12;
			Item.DamageType = DamageClass.Ranged;
			Item.width = 120;
            Item.height = 60;
			Item.scale = 0.5f;
            Item.useTime = 8;
            Item.useAnimation = 8;
			Item.reuseDelay = 10;
			Item.consumeAmmoOnLastShotOnly = true;

			Item.useStyle = ItemUseStyleID.Shoot;
			Item.noMelee = true;
			Item.channel = true; //Channel so that you can held the weapon [Important]
			Item.knockBack = 2;
			Item.shoot = ProjectileID.WoodenArrowFriendly;
			Item.shootSpeed = 9f;
			Item.useAmmo = AmmoID.Arrow;
			Item.crit = 16; // The percent chance at hitting an enemy with a crit, plus the default amount of 4.


			Item.rare = ItemRarityID.Blue;
			Item.value = Item.sellPrice(0, 0, 3, 20);

		}

		public override bool CanUseItem(Player player) {
			var modPlayer = Main.LocalPlayer.GetModPlayer<WeaponPlayer>();
			if (Main.myPlayer == player.whoAmI) {
				if (player.altFunctionUse != 2) {
					Item.useTime = 8;
					Item.reuseDelay = 10;

					// S1
					if (modPlayer.Skill == 0) {
						if(modPlayer.StockCount == 0) {
							modPlayer.OffensiveRecovery();
						}
						else if(modPlayer.StockCount > 0) {
							Item.useTime = 4;
							modPlayer.SkillActive = true;
							modPlayer.SkillTimer = 0;
							modPlayer.DelStockCount();
						}
					}
					Item.UseSound = new SoundStyle("ArknightsMod/Sounds/KroosCrossbowS1") {
						Volume = 0.8f,
						MaxInstances = 4, //This dicatates how many instances of a sound can be playing at the same time. The default is 1. Adjust this to allow overlapping sounds.
					};
				}
			}
			return true;
		}

		public override void ModifyWeaponDamage(Player player, ref StatModifier damage) {
			var modPlayer = Main.LocalPlayer.GetModPlayer<WeaponPlayer>();
			if (Main.myPlayer == player.whoAmI) {
				if (modPlayer.Skill == 0 && (modPlayer.StockCount > 0 || modPlayer.SkillActive == true)) {
					damage *= 1.4f;
				}
			}
		}

		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);
			return false;
		}

		public override void HoldItem(Player player) {
			var modPlayer = Main.LocalPlayer.GetModPlayer<WeaponPlayer>();
			if (Main.myPlayer == player.whoAmI) {
				modPlayer.SetAllSkillsData(1, 7, 0, 4, 0, null, null, null, null, null, null, "SampleIcon");
				if (!modPlayer.HoldKroosCrossbow) {
					modPlayer.SkillInitialize = true;
					modPlayer.Skill = 0;
				}

				// S1
				if (modPlayer.Skill == 0) {
					modPlayer.SetSkillData(0, 4, 1, 1, 0.2f, true, true); // If you don't want to draw skill acitive icon (yellow one above operator's head), stockmax = 1 and stockskill = true.
					modPlayer.SkillActiveTimer();
				}

				modPlayer.HoldKroosCrossbow = true; // you have to write this line HERE!
			}
			base.HoldItem(player);
		}

		// This method lets you adjust position of the gun in the player's hands. Play with these values until it looks good with your graphics.
		//public override Vector2? HoldoutOffset() {
		//	return new Vector2(-2f, -2f);
		//}

		public override void AddRecipes() {
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient<Placeable.RMA12>(1);
			recipe.AddIngredient<Placeable.Grind>(1);
			recipe.AddTile(TileID.WorkBenches);
			recipe.Register();
		}
	}
}
