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

namespace ArknightsMod.Content.Items.Weapons
{
    public class KroosCrossbow : ModItem
    {
		public override void SetStaticDefaults() {
			// DisplayName.SetDefault("Yato's Katana");
			// Tooltip.SetDefault("Yato has joined the team.");
		}

		//you should use local variable for batch change.
		private const int defaultDamage = 12;

		public override void SetDefaults()
        {
            Item.damage = defaultDamage;
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


			Item.rare = ItemRarityID.White;
			Item.value = Item.sellPrice(0, 0, 3, 20);

		}

		public override bool CanUseItem(Player player) {
			var modPlayer = Main.LocalPlayer.GetModPlayer<WeaponPlayer>();

			if (player.altFunctionUse != 2) {
				Item.damage = defaultDamage;
				Item.useTime = 8;
				Item.reuseDelay = 10;

				// S1
				if (modPlayer.Skill == 0 && modPlayer.StockCount == 0) {
					modPlayer.OffensiveRecovery();
				}
				else if (modPlayer.Skill == 0 && modPlayer.StockCount > 0) {
					Item.useTime = 4;
					modPlayer.DelStockCount();
				}
				Item.UseSound = new SoundStyle("ArknightsMod/Sounds/KroosCrossbowS1") {
					Volume = 0.8f,
					MaxInstances = 4, //This dicatates how many instances of a sound can be playing at the same time. The default is 1. Adjust this to allow overlapping sounds.
				};
			}

			return true;
		}

		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			var modPlayer = Main.LocalPlayer.GetModPlayer<WeaponPlayer>();

			if (modPlayer.Skill == 0 && modPlayer.SP == 0) {
				int newdamage = (int)Math.Round(defaultDamage * 1.4);
				Projectile.NewProjectile(source, position, velocity, type, newdamage, knockback, player.whoAmI);
			}
			else {
				Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);
			}

			return false;
		}

		public override void HoldItem(Player player) {
			var modPlayer = Main.LocalPlayer.GetModPlayer<WeaponPlayer>();
			if (!modPlayer.HoldKroosCrossbow) {
				modPlayer.SkillInitialize = true;
				modPlayer.Skill = 0;
			}

			// S1
			if (modPlayer.Skill == 0) {
				modPlayer.SetSkillData(0, 4, 1, 1, 0, true); // If you don't want to draw skill acitive icon (yellow one above operator's head), stockmax = 1 and stockskill = true.
				player.AddBuff(ModContent.BuffType<KroosCrossbowS1>(), 10);
			}

			modPlayer.HoldKroosCrossbow = true;
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
