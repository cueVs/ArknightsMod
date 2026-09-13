using ArknightsMod.Content.Projectiles.Sniper.Shirayuki;
using ArknightsMod.Content.Tiles.Infrastructure;
using ArknightsMod.Players;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using ArknightsMod.Content;

namespace ArknightsMod.Content.Items.Weapons.Sniper.Shirayuki
{
	public class Shirayuki_Shuriken : UpgradeWeaponBase
	{
		private static SoundStyle SkillActive1;
		private static SoundStyle NoSound;
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
		private float Skill = 0;
		private float Skill2Factor = 0.5f;

		public override void SetDefaults()
		{
			Item.damage = 74;
			Item.knockBack = 1f;
			Item.useAnimation = 84;
			Item.useTime = 84;
			Item.shootSpeed = 16f;

			Item.shoot = ModContent.ProjectileType<Shirayuki_Shuriken_Proj>();
			Item.DamageType = DamageClass.Ranged;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.UseSound = SoundID.Item1;
			//Item.rare = ItemRarityID.Green;
			//Item.value = Item.sellPrice(silver: 5);

			Item.noUseGraphic = true;
			Item.noMelee = true;
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
						Item.UseSound = SoundID.Item1;
						// S1
						if (modPlayer.Skill == 0 && modPlayer.SkillActive) {

						}

					}
				}
			}
			return base.CanUseItem(player);
		}

		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
		{
			var modPlayer = Main.LocalPlayer.GetModPlayer<WeaponPlayer>();
			if (modPlayer.Skill == 0 && modPlayer.SkillActive)
			{
				Projectile.NewProjectile(source, position, velocity, type, damage, knockback, Main.myPlayer, Skill);
			}
			else if (modPlayer.Skill == 1 && modPlayer.SkillActive)
			{
				Projectile.NewProjectile(source, position, velocity, type, (int)(damage * Skill2Factor), knockback, Main.myPlayer, Skill);
			}
			else
			{
				Projectile.NewProjectile(source, position, velocity, type, damage, knockback, Main.myPlayer, Skill);
			}

			return false;
		}
		public override void AddRecipes() {
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient<Material.OrirockCube>(4);
			recipe.AddTile(ModContent.TileType<FactoryTile>());
			recipe.Register();
		}

	}
}
