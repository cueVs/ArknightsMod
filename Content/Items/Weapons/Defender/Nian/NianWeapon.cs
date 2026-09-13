using ArknightsMod.Content.Projectiles.Defender.Nian;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent.Creative;
using Terraria.Audio;
using ArknightsMod.Players;
using ArknightsMod.Content.Tiles.Infrastructure;
using Microsoft.Xna.Framework;
using ArknightsMod.Content;


// 阻挡数+1，沉默和抵抗未实现

namespace ArknightsMod.Content.Items.Weapons.Defender.Nian
{

	public class NianWeapon : UpgradeWeaponBase
	{
		private static SoundStyle SkillActive1;
		private static SoundStyle NoSound;

		public override void SetStaticDefaults() {
			ItemID.Sets.Spears[Item.type] = true;
			CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
		}

		public override void AddRecipes() {
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient<Material.PolymerizationPreparation>(4);
			recipe.AddIngredient<Material.IncandescentAlloyBlock>(7);
			recipe.AddTile(ModContent.TileType<FactoryTile>());
			recipe.Register();
		}

		public override void SetDefaults() {
			Item.rare = ItemRarityID.Orange; 
			Item.value = Item.sellPrice(0, 40, 0, 0);
			Item.consumable = false;

			Item.useStyle = ItemUseStyleID.Swing; 
			Item.useAnimation = 45;
			Item.useTime = 45;
			Item.autoReuse = true; 

			// Weapon Properties
			Item.damage = 124;
			Item.knockBack = 2.5f;
			Item.noUseGraphic = true; 
			Item.DamageType = DamageClass.Melee;
			Item.noMelee = true;
			Item.channel = true;
			Item.crit = 21; 
		}

		public override bool AltFunctionUse(Player player) => false;

		public override void Load() {
			SkillActive1 = new SoundStyle("ArknightsMod/Sounds/SkillActive1") {
				Volume = 0.4f,
				MaxInstances = 4,
			};
			NoSound = new SoundStyle("ArknightsMod/Sounds/NoSound") {
				Volume = 0f,
				MaxInstances = 4,
			};
		}

		public override bool CanUseItem(Player player) {
			var modPlayer = Main.LocalPlayer.GetModPlayer<WeaponPlayer>();
			if (Main.myPlayer == player.whoAmI) {
				if (ArknightsKeybinds.SkillActivatePressed(player)) {
					// S1
					if (modPlayer.Skill == 0 && modPlayer.StockCount > 0 && !modPlayer.SkillActive) {
						modPlayer.SkillActive = true;
						modPlayer.SkillTimer = 0;

						modPlayer.DelStockCount();

						Item.UseSound = new SoundStyle("ArknightsMod/Sounds/SkillActive1") {
							Volume = 0.6f,
							MaxInstances = 4, 
						};
						SoundEngine.PlaySound(Item.UseSound.Value, player.Center);
					}
					else if (modPlayer.Skill == 1 && modPlayer.StockCount > 0 && !modPlayer.SkillActive) {
						modPlayer.SkillActive = true;
						modPlayer.SkillTimer = 0;

						modPlayer.DelStockCount();

						Item.UseSound = new SoundStyle("ArknightsMod/Sounds/SkillActive1") {
							Volume = 0.6f,
							MaxInstances = 4,
						};
						SoundEngine.PlaySound(Item.UseSound.Value, player.Center);
					}
					// S3
					else if (modPlayer.Skill == 2 && modPlayer.StockCount > 0 && !modPlayer.SkillActive) {
						modPlayer.SkillActive = true;
						modPlayer.SkillTimer = 0;

						modPlayer.DelStockCount();

						Item.UseSound = new SoundStyle("ArknightsMod/Sounds/SkillActive1") {
							Volume = 0.4f,
							MaxInstances = 4, 
						};
						SoundEngine.PlaySound(Item.UseSound.Value, player.Center);
					}

					else
						return false;
				}
				else {
					Item.UseSound = NoSound;
				}
			}
			if (modPlayer.Skill == 1 && modPlayer.SkillActive)
			{
				return false; // 2技能期间禁止使用武器
			}
			return base.CanUseItem(player);
		}

		public override void ModifyWeaponDamage(Player player, ref StatModifier damage) {
			var modPlayer = Main.LocalPlayer.GetModPlayer<WeaponPlayer>();
			if (Main.myPlayer == player.whoAmI) {
				if (modPlayer.Skill == 0 && modPlayer.SkillActive == true) {
					damage *= 1.45f;
					Item.DamageType = DamageClass.Magic;
				}
				else if (modPlayer.Skill == 2 && modPlayer.SkillActive == true && Item.type == ModContent.ItemType<NianWeapon>()) {
					damage *= 2.2f;
				}
			}
		}

		public override void HoldItem(Player player) {
			var modPlayer = Main.LocalPlayer.GetModPlayer<WeaponPlayer>();
			if (Main.myPlayer == player.whoAmI) {
				
				
				modPlayer.HoldBagpipeSpear = true; // you have to write this line HERE!
			}
			base.HoldItem(player);
		}

		public class Nianplayer : ModPlayer
		{
			public bool hasNianplayer = false;
			public override void ResetEffects() {
				if (Main.myPlayer != Player.whoAmI)
					return; 
				bool isHoldingTargetWeapon = Player.HeldItem.type == ModContent.ItemType<NianWeapon>();
				if (!isHoldingTargetWeapon) {
					Player.GetModPlayer<Nianplayer>().hasNianplayer = false;
				}

			}

            public override void PostUpdate() {
                var it = Player.HeldItem;
                if (it.type == ModContent.ItemType<NianWeapon>() && Player.controlUseItem) {
                    if (Player.ownedProjectileCounts[ModContent.ProjectileType<NianSword>()] == 0) {
                        Projectile.NewProjectile(Player.GetSource_FromThis(), Player.Center, Vector2.Zero, ModContent.ProjectileType<NianSword>(), it.damage, it.knockBack, Player.whoAmI);
                    }
					if (Player.ownedProjectileCounts[ModContent.ProjectileType<NianShield>()] == 0) {
                        Projectile.NewProjectile(Player.GetSource_FromThis(), Player.Center, Vector2.Zero, ModContent.ProjectileType<NianShield>(), it.damage, it.knockBack, Player.whoAmI);
                    }
                }

                base.PostUpdate();
            }

            public override void UpdateEquips() {
				var modPlayer = Main.LocalPlayer.GetModPlayer<WeaponPlayer>();
				if (Main.myPlayer == Player.whoAmI) {
					if (modPlayer.Skill == 0 && modPlayer.SkillActive == true) {
						Player.statDefense *= 1.7f;
					}
					if (modPlayer.Skill == 1 && modPlayer.SkillActive == true && Player.HeldItem.type == ModContent.ItemType<NianWeapon>()) {
						Player.statDefense *= 2.3f;
					}
					else if (modPlayer.Skill == 1 && !modPlayer.SkillActive == true && Player.HeldItem.type == ModContent.ItemType<NianWeapon>()) {
						Player.statDefense *= 1.8f;
					}
				}
				base.UpdateEquips();
			}
		}
		


	}
}