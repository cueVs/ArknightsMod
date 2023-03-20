using ArknightsMod.Content.Projectiles;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent.Creative;
using Terraria.Audio;
using ArknightsMod.Common.Players;

namespace ArknightsMod.Content.Items.Weapons
{
	public class BagpipeSpear : ModItem
	{

		public override void SetStaticDefaults() {
			// ItemID.Sets.SkipsInitialUseSound[Item.type] = true; // This skips use animation-tied sound playback, so that we're able to make it be tied to use time instead in the UseItem() hook.
			ItemID.Sets.Spears[Item.type] = true; // This allows the game to recognize our new item as a spear.
			CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
		}

		public override void SetDefaults() {
			// Common Properties
			Item.rare = ItemRarityID.Yellow; // Assign this item a rarity level of Yellow
			Item.value = Item.sellPrice(silver: 10); // The number and type of coins item can be sold for to an NPC

			// Use Properties
			Item.useStyle = ItemUseStyleID.Shoot; // How you use the item (swinging, holding out, etc.)
			Item.useAnimation = 28; // The length of the item's use animation in ticks (60 ticks == 1 second.)
			Item.useTime = 30; // The length of the item's use time in ticks (60 ticks == 1 second.) And if you want to attack triple hit, useTime = useAnimation/3
			Item.autoReuse = false; // Allows the player to hold click to automatically use the item again. Most spears don't autoReuse, but it's possible when used in conjunction with CanUseItem()

			// Weapon Properties
			Item.damage = 76;
			Item.knockBack = 2.5f;
			Item.noUseGraphic = true; // When true, the item's sprite will not be visible while the item is in use. This is true because the spear projectile is what's shown so we do not want to show the spear sprite as well.
			Item.DamageType = DamageClass.Melee;
			Item.noMelee = true; // Allows the item's animation to do damage. This is important because the spear is actually a projectile instead of an item. This prevents the melee hitbox of this item.
			Item.crit = 21; // The percent chance at hitting an enemy with a crit, plus the default amount of 4.

			// Projectile Properties
			Item.shootSpeed = 3.3f; // The speed of the projectile measured in pixels per frame.
			Item.shoot = ModContent.ProjectileType<BagpipeSpearProjectileS0>(); // The projectile that is fired from this weapon

			// The sound that this item plays when used. Need "using Terraria.Audio;"
			Item.UseSound = new SoundStyle("ArknightsMod/Sounds/BagpipeSpearS0") {
				Volume = 0.2f,
				MaxInstances = 4, //This dicatates how many instances of a sound can be playing at the same time. The default is 1. Adjust this to allow overlapping sounds.
			};
		}

		public override bool CanUseItem(Player player) {
			// Ensures no more than one spear can be thrown out, use this when using autoReuse
			return player.ownedProjectileCounts[Item.shoot] < 1;
		}

		public override void HoldItem(Player player) {
			var modPlayer = Main.LocalPlayer.GetModPlayer<WeaponPlayer>();

			modPlayer.MaxSP = 12;
			modPlayer.StockMax = 1; //How many charges can the Operator store up to?
			modPlayer.SkillChargeMax = modPlayer.MaxSP * 60; // in case of Auto Recovery Skill, you need x60 

			if (modPlayer.StockCount < modPlayer.StockMax) {
				modPlayer.SkillCharge += 1;

				if (modPlayer.SkillCharge != 0 && modPlayer.SkillCharge % 60 == 0) {
					modPlayer.SP += 1;
				}

				if (modPlayer.SkillCharge == modPlayer.SkillChargeMax) {
					modPlayer.SkillCharge = 0;
					modPlayer.StockCount += 1;
					if (modPlayer.StockCount == modPlayer.StockMax) {
						modPlayer.SP = modPlayer.MaxSP;
					}
					else modPlayer.SP = 0;
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

		// Please see Content/ExampleRecipes.cs for a detailed explanation of recipe creation.
		public override void AddRecipes() {
			CreateRecipe()
				.AddIngredient<Material.OrirockConcentration>(9)
				.AddIngredient<Material.KetonColloid>(4)
				.AddIngredient<Material.OrironBlock>(4)
				.AddTile(TileID.Anvils)
				.Register();
		}
	}
}