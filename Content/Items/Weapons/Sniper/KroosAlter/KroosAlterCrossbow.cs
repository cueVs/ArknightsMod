using ArknightsMod.Content.Items.Material;
using ArknightsMod.Content.Projectiles.Sniper.KroosAlter;
using ArknightsMod.Content.Tiles.Infrastructure;
using ArknightsMod.Players;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using ArknightsMod.Content;

namespace ArknightsMod.Content.Items.Weapons.Sniper.KroosAlter
{
    public class KroosAlterCrossbow : UpgradeWeaponBase
    {
		private static SoundStyle SkillActive1;
		private static SoundStyle NoSound;
		public int Skill2HitCounter = 0;
		public override void SetDefaults()
        {
            Item.width = 52;
            Item.height = 32;
            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.shootSpeed = 8f;
            Item.damage = 72;
            Item.knockBack = 3f;
            Item.shoot = ModContent.ProjectileType<KroosAlterCrossbow_Hold>();
            Item.DamageType = DamageClass.Ranged;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.rare = ItemRarityID.Green;
            Item.useAmmo = AmmoID.Arrow;
			Item.reuseDelay = 10;
            Item.value = Item.sellPrice(0);
            Item.noMelee = true;
            Item.autoReuse = true;
            Item.noUseGraphic = true;
            Item.channel = true;
        }
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

		public override bool AltFunctionUse(Player player) => false;
		public override bool CanUseItem(Player player) {
			var modPlayer = Main.LocalPlayer.GetModPlayer<WeaponPlayer>();
			if (Main.myPlayer == player.whoAmI) {
				if (ArknightsKeybinds.SkillActivatePressed(player)) {
					if (!modPlayer.SummonMode) {
						// S1
						if (modPlayer.Skill == 0 && modPlayer.StockCount > 0 && !modPlayer.SkillActive) {
							modPlayer.SkillActive = true;
							modPlayer.SkillTimer = 0;

							modPlayer.DelStockCount();

							Item.UseSound = SkillActive1;
							SoundEngine.PlaySound(Item.UseSound.Value, player.Center);
						}
						//S2
						if (modPlayer.Skill == 1 && modPlayer.StockCount > 0 && !modPlayer.SkillActive) {
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
				else {
					if (!modPlayer.SummonMode) {
						Item.UseSound = NoSound;
					}
				}
			}
			return base.CanUseItem(player);
		}
		public override void ModifyWeaponDamage(Player player, ref StatModifier damage) {
			var modPlayer = Main.LocalPlayer.GetModPlayer<WeaponPlayer>();
			if (Main.myPlayer == player.whoAmI) {
				if (modPlayer.Skill == 0 && modPlayer.SkillActive == true) {
					damage *= 1.4f;
					player.aggro -= 1250;
				}
			}
		}
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Projectile.NewProjectileDirect(source, position, velocity,
				ModContent.ProjectileType<KroosAlterCrossbow_Hold>(), damage, knockback, player.whoAmI);
            return false;
        }

        public override Vector2? HoldoutOffset()
        {
            return new Vector2(0, 4);
        }
		public override void AddRecipes() {
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient(ModContent.ItemType<Aketon>());
			recipe.AddIngredient(ModContent.ItemType<OrironCluster>());
			recipe.AddTile(ModContent.TileType<FactoryTile>());
			recipe.Register();
		}
		public override void UpdateInventory(Player player) {
			// 技能结束清零
			var modPlayer = Main.LocalPlayer.GetModPlayer<WeaponPlayer>();
			if (modPlayer.SkillActive == false) {
				Skill2HitCounter = 0;
			}
		}
	}
}
