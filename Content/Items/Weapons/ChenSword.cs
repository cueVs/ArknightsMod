﻿using ArknightsMod.Common.Items;
using ArknightsMod.Common.Players;
using ArknightsMod.Content.Buffs;
using ArknightsMod.Content.Projectiles;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace ArknightsMod.Content.Items.Weapons
{
	public class ChenSword : UpgradeWeaponBase
	{
		private static SoundStyle SkillActive1;
		private static SoundStyle ChenSwordS0;
		private static SoundStyle ChenSwordS3;
		private static SoundStyle ChenSwordS3Last;	
		public override void Load() {
			SkillActive1 = new SoundStyle("ArknightsMod/Sounds/SkillActive1") {
				Volume = 0.4f,
				MaxInstances = 4,
			};
			ChenSwordS0 = new SoundStyle("ArknightsMod/Sounds/ChenSwordS0") {
				Volume = 0.4f,
				MaxInstances = 4,
			};
			ChenSwordS3 = new SoundStyle("ArknightsMod/Sounds/ChenSwordS3") {
				Volume = 0.4f,
				MaxInstances = 4,
			};
			ChenSwordS3Last = new SoundStyle("ArknightsMod/Sounds/ChenSwordS3Last") {
				Volume = 0.4f,
				MaxInstances = 4,
			};
		}
		public override void SetStaticDefaults() {
			//ItemID.Sets.SkipsInitialUseSound[Item.type] = true; // This skips use animation-tied sound playback, so that we're able to make it be tied to use time instead in the UseItem() hook.
			ItemID.Sets.Spears[Type] = true; // This allows the game to recognize our new item as a spear.
			Item.ResearchUnlockCount = 1;
		}

		public override void SetDefaults() {
			// Common Properties
			Item.rare = ItemRarityID.Yellow; // Assign this item a rarity level of Yellow
			Item.value = Item.sellPrice(silver: 10); // The number and type of coins item can be sold for to an NPC
			Item.consumable = false;

			// Use Properties
			Item.useStyle = ItemUseStyleID.Swing; // How you use the item (swinging, holding out, etc.)
			Item.useAnimation = 15; // The length of the item's use animation in ticks (60 ticks == 1 second.)
			Item.useTime = 15; // The length of the item's use time in ticks (60 ticks == 1 second.) And if you want to attack triple hit, useTime = useAnimation/3
			Item.autoReuse = false; // Allows the player to hold click to automatically use the item again. Most spears don't autoReuse, but it's possible when used in conjunction with CanUseItem()
			Item.scale = 0.8f;

			// Weapon Properties
			Item.damage = 71;
			Item.knockBack = 2.5f;
			// Item.noUseGraphic = true; // When true, the item's sprite will not be visible while the item is in use. This is true because the spear projectile is what's shown so we do not want to show the spear sprite as well.
			Item.DamageType = DamageClass.Melee;
			// Item.noMelee = true; // Allows the item's animation to do damage. This is important because the spear is actually a projectile instead of an item. This prevents the melee hitbox of this item.
			Item.crit = -4; // The percent chance at hitting an enemy with a crit, plus the default amount of 4.

			// Projectile Properties
			Item.shootSpeed = 1f; // The speed of the projectile measured in pixels per frame.
								  // Item.shoot = ModContent.ProjectileType<ChenSwordProjectileS3>(); // The projectile that is fired from this weapon

			// The sound that this item plays when used. Need "using Terraria.Audio;"
			//Item.UseSound = new SoundStyle("ArknightsMod/Sounds/BagpipeSpearS0") {
			//	Volume = 0.2f,
			//	MaxInstances = 4, //This dicatates how many instances of a sound can be playing at the same time. The default is 1. Adjust this to allow overlapping sounds.
			//};

		}
		public override bool AltFunctionUse(Player player) => true;
		public override bool CanUseItem(Player player) {
			var modPlayer = Main.LocalPlayer.GetModPlayer<WeaponPlayer>();
			if (Main.myPlayer == player.whoAmI) {
				if (player.altFunctionUse == 2) {
					// S3 (but now it sets S1)
					if (modPlayer.Skill == 0 && modPlayer.StockCount > 0 && !modPlayer.SkillActive && Vector2.Distance(Main.MouseWorld, player.Center) <= 600f) {
						modPlayer.SkillActive = true;
						modPlayer.SkillTimer = 0;
						modPlayer.mousePositionX = Main.MouseWorld.X;
						modPlayer.mousePositionY = Main.MouseWorld.Y - 10f;
						modPlayer.playerPositionX = player.Center.X;
						modPlayer.playerPositionY = player.Center.Y - 20f;

						modPlayer.DelStockCount();

						Item.UseSound = SkillActive1;
						SoundEngine.PlaySound(Item.UseSound.Value, player.Center);
					}

					else
						return false;
				}
				else {
					Item.useAnimation = 15;
					Item.useTime = 15; // If you want to attack triple hit, useTime = useAnimation/3
					Item.UseSound = ChenSwordS0;

					// S3 (but now it sets S1)
					if (modPlayer.Skill == 0 && modPlayer.StockCount == 0) {
						modPlayer.OffensiveRecovery();
					}

				}
			}
			// Ensures no more than one spear can be thrown out, use this when using autoReuse
			return player.ownedProjectileCounts[Item.shoot] < 3;
		}

		public override void HoldItem(Player player) {
			var mp = Main.LocalPlayer.GetModPlayer<WeaponPlayer>();
			if (Main.myPlayer == player.whoAmI) {
				// S3 (but now S1)
				if (mp.Skill == 0) {

					if (mp.StockCount > 0 && !mp.SkillActive) {
						for (int i = 0; i < 30; i++) {//Circle
							Vector2 offset = new Vector2();
							double angle = Main.rand.NextDouble() * 2d * Math.PI;
							offset.X += (float)(Math.Sin(angle) * 500);
							offset.Y += (float)(Math.Cos(angle) * 500);

							Dust circle_dust = Dust.NewDustPerfect(player.MountedCenter + offset, 235, player.velocity, 200, default(Color), 0.5f);
							circle_dust.fadeIn = 0.1f;
							circle_dust.noGravity = true;
						}
					}

					if (mp.SkillActive) {
						mp.SkillTimer++;
						player.immune = true;
						player.immuneTime = 5;
						player.immuneAlpha = 255;
						float activeTime = mp.CurrentSkill.CurrentLevelData.ActiveTime * 60;
						if (mp.SkillTimer == 1) {
							Projectile.NewProjectile(player.GetSource_ItemUse(player.HeldItem), player.Center.X - 50, player.Center.Y - 100, 0, 0, ProjectileType<ChenSwordProjectileS3Dragon>(), 0, 0, player.whoAmI, 0f);
						}
						if (mp.SkillTimer <= activeTime) {
							player.velocity = Vector2.Zero;
						}
						if (mp.SkillTimer == 10) {
							player.Teleport(new Vector2(mp.mousePositionX, mp.mousePositionY), -1, 0);
							NetMessage.SendData(MessageID.TeleportEntity, -1, -1, null, 0, player.whoAmI, mp.mousePositionX, mp.mousePositionY, 1, 0, 0);
						}

						if (mp.SkillTimer > 10 && mp.SkillTimer <= activeTime && mp.SkillTimer % 5 == 0) {
							Projectile.NewProjectile(player.GetSource_ItemUse(player.HeldItem), player.Center.X, player.Center.Y, 0, 0, ProjectileType<ChenSwordProjectileS3>(), player.GetWeaponDamage(Item) * 3, 2.5f, player.whoAmI, 0f);
							player.immuneAlpha = 0;
							if (mp.SkillTimer == 60) {
								Item.UseSound = ChenSwordS3Last;
								SoundEngine.PlaySound(Item.UseSound.Value, player.Center);
							}
							else {
								Item.UseSound = ChenSwordS3;
								SoundEngine.PlaySound(Item.UseSound.Value, player.Center);
							}

						}

						if (mp.SkillTimer == activeTime + 10) {
							player.Teleport(new Vector2(mp.playerPositionX - 10, mp.playerPositionY - 10), -1, 0);
							NetMessage.SendData(MessageID.TeleportEntity, -1, -1, null, 0, player.whoAmI, mp.mousePositionX, mp.mousePositionY, 1, 0, 0);

							mp.SkillActive = false;
							mp.SkillTimer = 0;
						}
					}
					else {
						player.immune = false;
					}

				}


			}
			base.HoldItem(player);
		}

		//public override bool? UseItem(Player player)
		//{
		//    // Because we're skipping sound playback on use animation start, we have to play it ourselves whenever the item is actually used.
		//    if (!Main.dedServ && Item.UseSound.HasValue)
		//    {
		//        SoundEngine.PlaySound(Item.UseSound.Value, player.Center);
		//    }

		//    return null;
		//}

		public override void AddRecipes() {
			CreateRecipe()
				.AddIngredient<Material.OrirockConcentration>(9)
				.AddIngredient<Material.KetonColloid>(4)
				.AddIngredient<Material.OrironBlock>(4)
				.AddTile(TileID.Anvils)
				.Register();
		}

		//public override void AddRecipes() {
		//	CreateRecipe()
		//		.AddIngredient(ItemID.DirtBlock, 1)
		//		.AddTile(TileID.WorkBenches)
		//		.Register();
		//}
	}
}