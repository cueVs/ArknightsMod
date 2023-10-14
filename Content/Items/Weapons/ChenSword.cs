using ArknightsMod.Content.Projectiles;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent.Creative;
using Terraria.Audio;
using ArknightsMod.Common.Players;
using ArknightsMod.Content.Buffs;
using System;
using Microsoft.Xna.Framework;
using static Terraria.ModLoader.ModContent;

namespace ArknightsMod.Content.Items.Weapons
{
	public class ChenSword : ModItem
	{
		public override void SetStaticDefaults() {
			//ItemID.Sets.SkipsInitialUseSound[Item.type] = true; // This skips use animation-tied sound playback, so that we're able to make it be tied to use time instead in the UseItem() hook.
			ItemID.Sets.Spears[Item.type] = true; // This allows the game to recognize our new item as a spear.
			CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
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

		// Right Click in world
		public override bool AltFunctionUse(Player player) => true;
		// Right Click in inventory
		//public override bool ConsumeItem(Player player) => false;
		//public override bool CanRightClick() => true;

		//public override void RightClick(Player player) {
		//	var modPlayer = Main.LocalPlayer.GetModPlayer<WeaponPlayer>();
		//	modPlayer.Skill++;
		//	modPlayer.Skill = modPlayer.Skill % 3;

		//	// S1
		//	if (modPlayer.Skill == 0) {
		//		modPlayer.SkillInitialize = true;
		//	}

		//	// S2
		//	if (modPlayer.Skill == 1) {
		//		modPlayer.SkillInitialize = true;
		//	}

		//	// S3
		//	if (modPlayer.Skill == 2) {
		//		modPlayer.SkillInitialize = true;
		//	}
		//}

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

						Item.UseSound = new SoundStyle("ArknightsMod/Sounds/SkillActive1") {
							Volume = 0.6f,
							MaxInstances = 4, //This dicatates how many instances of a sound can be playing at the same time. The default is 1. Adjust this to allow overlapping sounds.
						};
						SoundEngine.PlaySound(Item.UseSound.Value, player.Center);
					}

					else
						return false;
				}
				else {
					Item.useAnimation = 15;
					Item.useTime = 15; // If you want to attack triple hit, useTime = useAnimation/3
					Item.UseSound = new SoundStyle("ArknightsMod/Sounds/ChenSwordS0") {
						Volume = 0.4f,
						MaxInstances = 4, //This dicatates how many instances of a sound can be playing at the same time. The default is 1. Adjust this to allow overlapping sounds.
					};

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
			var modPlayer = Main.LocalPlayer.GetModPlayer<WeaponPlayer>();
			if (Main.myPlayer == player.whoAmI) {
				if (!modPlayer.HoldChenSword) {
					modPlayer.SkillInitialize = true;
					modPlayer.Skill = 0;
				}

				if (Main.myPlayer == player.whoAmI) {
					// S3 (but now it sets S1)
					if (modPlayer.Skill == 0) {
						modPlayer.SetSkillData(20, 30, 1, 1, 1, false, false);
						player.AddBuff(BuffType<ChenSwordS3>(), 10);

						if (modPlayer.StockCount > 0 && !modPlayer.SkillActive) {
							for (int i = 0; i < 30; i++) {//Circle
								Vector2 offset = new Vector2();
								double angle = Main.rand.NextDouble() * 2d * Math.PI;
								offset.X += (float)(Math.Sin(angle) * (500));
								offset.Y += (float)(Math.Cos(angle) * (500));

								Dust circle_dust = Dust.NewDustPerfect(player.MountedCenter + offset, 235, player.velocity, 200, default(Color), 0.5f);
								circle_dust.fadeIn = 0.1f;
								circle_dust.noGravity = true;
							}
						}

						if (modPlayer.SkillActive) {
							modPlayer.SkillTimer++;
							player.immune = true;
							player.immuneTime = 5;
							player.immuneAlpha = 255;
							if (modPlayer.SkillTimer == 1) {
								Projectile.NewProjectile(player.GetSource_ItemUse(player.HeldItem), player.Center.X - 50, player.Center.Y - 100, 0, 0, ProjectileType<ChenSwordProjectileS3Dragon>(), 0, 0, player.whoAmI, 0f);
							}
							if (modPlayer.SkillTimer <= modPlayer.SkillActiveTime * 60) {
								player.velocity = Vector2.Zero;
							}
							if (modPlayer.SkillTimer == 10) {
								player.Teleport(new Vector2(modPlayer.mousePositionX, modPlayer.mousePositionY), -1, 0);
								NetMessage.SendData(MessageID.TeleportEntity, -1, -1, null, 0, (float)player.whoAmI, modPlayer.mousePositionX, modPlayer.mousePositionY, 1, 0, 0);
							}

							if (modPlayer.SkillTimer > 10 && modPlayer.SkillTimer <= modPlayer.SkillActiveTime * 60 && modPlayer.SkillTimer % 5 == 0) {
								Projectile.NewProjectile(player.GetSource_ItemUse(player.HeldItem), player.Center.X, player.Center.Y, 0, 0, ProjectileType<ChenSwordProjectileS3>(), player.GetWeaponDamage(Item) * 3, 2.5f, player.whoAmI, 0f);
								player.immuneAlpha = 0;
								if (modPlayer.SkillTimer == 60) {
									SoundStyle projSound = new SoundStyle("ArknightsMod/Sounds/ChenSwordS3Last") {
										Volume = 0.7f,
										MaxInstances = 1,
									};
									SoundEngine.PlaySound(projSound);
								}
								else {
									SoundStyle projSound = new SoundStyle("ArknightsMod/Sounds/ChenSwordS3") {
										Volume = 0.7f,
										MaxInstances = 4,
									};
									SoundEngine.PlaySound(projSound);
								}

							}

							if (modPlayer.SkillTimer == modPlayer.SkillActiveTime * 60 + 10) {
								player.Teleport(new Vector2(modPlayer.playerPositionX - 10, modPlayer.playerPositionY - 10), -1, 0);
								NetMessage.SendData(MessageID.TeleportEntity, -1, -1, null, 0, (float)player.whoAmI, modPlayer.mousePositionX, modPlayer.mousePositionY, 1, 0, 0);

								modPlayer.SkillActive = false;
								modPlayer.SkillTimer = 0;
							}
						}
						else {
							player.immune = false;
						}

					}
				}


				modPlayer.HoldChenSword = true; // you have to write this line HERE!
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