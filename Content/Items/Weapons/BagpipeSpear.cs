using ArknightsMod.Content.Projectiles;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent.Creative;
using Terraria.Audio;
using ArknightsMod.Common.Players;
using ArknightsMod.Content.Buffs;
using System;

namespace ArknightsMod.Content.Items.Weapons
{
	public class BagpipeSpear : ModItem
	{
		public override void SetStaticDefaults() {
			//ItemID.Sets.SkipsInitialUseSound[Item.type] = true; // This skips use animation-tied sound playback, so that we're able to make it be tied to use time instead in the UseItem() hook.
			ItemID.Sets.Spears[Item.type] = true; // This allows the game to recognize our new item as a spear.
			CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
		}

		//you should use local variable for batch change.
		private const int defaultDamage = 76;

		public override void SetDefaults() {
			// Common Properties
			Item.rare = ItemRarityID.Yellow; // Assign this item a rarity level of Yellow
			Item.value = Item.sellPrice(silver: 10); // The number and type of coins item can be sold for to an NPC
			Item.consumable = false;

			// Use Properties
			Item.useStyle = ItemUseStyleID.Shoot; // How you use the item (swinging, holding out, etc.)
			Item.useAnimation = 30; // The length of the item's use animation in ticks (60 ticks == 1 second.)
			Item.useTime = 30; // The length of the item's use time in ticks (60 ticks == 1 second.) And if you want to attack triple hit, useTime = useAnimation/3
			Item.autoReuse = false; // Allows the player to hold click to automatically use the item again. Most spears don't autoReuse, but it's possible when used in conjunction with CanUseItem()

			// Weapon Properties
			Item.damage = defaultDamage;
			Item.knockBack = 2.5f;
			Item.noUseGraphic = true; // When true, the item's sprite will not be visible while the item is in use. This is true because the spear projectile is what's shown so we do not want to show the spear sprite as well.
			Item.DamageType = DamageClass.Melee;
			Item.noMelee = true; // Allows the item's animation to do damage. This is important because the spear is actually a projectile instead of an item. This prevents the melee hitbox of this item.
			Item.crit = 21; // The percent chance at hitting an enemy with a crit, plus the default amount of 4.

			// Projectile Properties
			Item.shootSpeed = 3.3f; // The speed of the projectile measured in pixels per frame.
			Item.shoot = ModContent.ProjectileType<BagpipeSpearProjectileS0>(); // The projectile that is fired from this weapon

			// The sound that this item plays when used. Need "using Terraria.Audio;"
			//Item.UseSound = new SoundStyle("ArknightsMod/Sounds/BagpipeSpearS0") {
			//	Volume = 0.2f,
			//	MaxInstances = 4, //This dicatates how many instances of a sound can be playing at the same time. The default is 1. Adjust this to allow overlapping sounds.
			//};

		}

		public override bool AltFunctionUse(Player player) => true;

		public override bool ConsumeItem(Player player) => false;
		public override bool CanRightClick() => true;

		public override void RightClick(Player player) {
			var modPlayer = Main.LocalPlayer.GetModPlayer<WeaponPlayer>();
			modPlayer.Skill++;
			modPlayer.Skill = modPlayer.Skill % 3;

			// S1
			if (modPlayer.Skill == 0) {
				modPlayer.SkillInitialize = true;
			}

			// S2
			if (modPlayer.Skill == 1) {
				modPlayer.SkillInitialize = true;
			}

			// S3
			if (modPlayer.Skill == 2) {
				modPlayer.SkillInitialize = true;
			}
		}

		public override bool CanUseItem(Player player) {
			var modPlayer = Main.LocalPlayer.GetModPlayer<WeaponPlayer>();

			if (player.altFunctionUse == 2) {
				// S1
				if (modPlayer.Skill == 0 && modPlayer.StockCount > 0 && !modPlayer.SkillActive) {
					modPlayer.SkillActive = true;
					modPlayer.SkillTimer = 0;

					modPlayer.DelStockCount();

					Item.UseSound = new SoundStyle("ArknightsMod/Sounds/SkillActive1") {
						Volume = 0.6f,
						MaxInstances = 4, //This dicatates how many instances of a sound can be playing at the same time. The default is 1. Adjust this to allow overlapping sounds.
					};
					SoundEngine.PlaySound(Item.UseSound.Value, player.Center);
				}
				// S3
				if (modPlayer.Skill == 2 && modPlayer.StockCount > 0 && !modPlayer.SkillActive) {
					modPlayer.SkillActive = true;
					modPlayer.SkillTimer = 0;

					modPlayer.DelStockCount();

					Item.UseSound = new SoundStyle("ArknightsMod/Sounds/SkillActive2") {
						Volume = 0.4f,
						MaxInstances = 4, //This dicatates how many instances of a sound can be playing at the same time. The default is 1. Adjust this to allow overlapping sounds.
					};
					SoundEngine.PlaySound(Item.UseSound.Value, player.Center);
				}

				else
					return false;
			}
			else {
				Item.useAnimation = 30;
				Item.useTime = 30; // If you want to attack triple hit, useTime = useAnimation/3
				Item.damage = defaultDamage;
				Item.UseSound = new SoundStyle("ArknightsMod/Sounds/BagpipeSpearS0") {
					Volume = 0.4f,
					MaxInstances = 4, //This dicatates how many instances of a sound can be playing at the same time. The default is 1. Adjust this to allow overlapping sounds.
				};

				// S1
				if (modPlayer.Skill == 0 && modPlayer.SkillActive) {
					Item.damage = (int)Math.Round(defaultDamage * 1.45);
					Item.useAnimation = 22;
					Item.useTime = 22;
					Item.UseSound = new SoundStyle("ArknightsMod/Sounds/BagpipeSpearS0") {
						Volume = 0.4f,
						MaxInstances = 4, //This dicatates how many instances of a sound can be playing at the same time. The default is 1. Adjust this to allow overlapping sounds.
					};
				}
				// S2
				if (modPlayer.Skill == 1 && modPlayer.StockCount > 0) {
					Item.useTime = 15;
					Item.damage = defaultDamage * 2;
					Item.UseSound = new SoundStyle("ArknightsMod/Sounds/BagpipeSpearS2") {
						Volume = 0.4f,
						MaxInstances = 4, //This dicatates how many instances of a sound can be playing at the same time. The default is 1. Adjust this to allow overlapping sounds.
					};
					modPlayer.DelStockCount();
				}
				// S3
				if (modPlayer.Skill == 2 && modPlayer.SkillActive) {
					Item.damage = (int)Math.Round(defaultDamage * 2.2);
					Item.useAnimation = 48;
					Item.useTime = 16;
					Item.UseSound = new SoundStyle("ArknightsMod/Sounds/BagpipeSpearS3") {
						Volume = 0.4f,
						MaxInstances = 4, //This dicatates how many instances of a sound can be playing at the same time. The default is 1. Adjust this to allow overlapping sounds.
					};
				}
			}

			// Ensures no more than one spear can be thrown out, use this when using autoReuse
			return player.ownedProjectileCounts[Item.shoot] < 1;
		}

		public override void HoldItem(Player player) {
			var modPlayer = Main.LocalPlayer.GetModPlayer<WeaponPlayer>();
			if (!modPlayer.HoldBagpipeSpear) {
				modPlayer.SkillInitialize = true;
				modPlayer.Skill = 0;
			}

			// S1
			if (modPlayer.Skill == 0) {
				modPlayer.SetSkillData(15, 35, 60, 1, 35, false); 
				modPlayer.AutoCharge();
				player.AddBuff(ModContent.BuffType<BagpipeSpearS1>(), 10);
				modPlayer.SkillActiveTimer();
			}

			// S2
			if (modPlayer.Skill == 1) {
				modPlayer.SetSkillData(0, 4, 60, 3, 0, true);
				modPlayer.AutoCharge();
				player.AddBuff(ModContent.BuffType<BagpipeSpearS2>(), 10);
			}

			// S3
			if (modPlayer.Skill == 2) {
				modPlayer.SetSkillData(25, 40, 60, 1, 20, false);
				modPlayer.AutoCharge();
				player.AddBuff(ModContent.BuffType<BagpipeSpearS3>(), 10);
				modPlayer.SkillActiveTimer();
			}

			modPlayer.HoldBagpipeSpear = true; // you have to write this line HERE!
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