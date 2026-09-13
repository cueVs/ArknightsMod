using ArknightsMod.Content;
using ArknightsMod.Content.Projectiles.Sniper.Exusiai;
using ArknightsMod.Content.Tiles.Infrastructure;
using ArknightsMod.Players;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Weapons.Sniper.Exusiai
{

    public class ExusiaiVector : UpgradeWeaponBase
	{
		public override void AddRecipes() {
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient<Material.PolymerizationPreparation>(4);
			recipe.AddIngredient<Material.SugarLump>(5);
			recipe.AddTile(ModContent.TileType<FactoryTile>());
			recipe.Register();
		}
		private static SoundStyle SkillActive1;
		private static SoundStyle ExusiaiVectorA;
		private static SoundStyle ExusiaiVectorS;
		public override void Load() {
			SkillActive1 = new SoundStyle("ArknightsMod/Sounds/SkillActive1") {
				Volume = 0.4f,
				MaxInstances = 4,
			};
			ExusiaiVectorA = new SoundStyle("ArknightsMod/Sounds/ExusiaiVectorA") {
				Volume = 1f,
				MaxInstances = 10,
			};
			ExusiaiVectorS = new SoundStyle("ArknightsMod/Sounds/ExusiaiVectorS") {
				Volume = 1f,
				MaxInstances = 10,
			};
		}
		public override void SetDefaults()
        {
            Item.width = 54;
            Item.height = 28;
            Item.useTime = 15;
            Item.useAnimation = 15;
            Item.shootSpeed = 8f;
            Item.damage = 108;
            Item.knockBack = 5f;
            Item.shoot = ModContent.ProjectileType<ExusiaiVector_Bullet>();
            Item.DamageType = DamageClass.Ranged;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.rare = ItemRarityID.Green;
			Item.UseSound = ExusiaiVectorA;
			Item.useAmmo = AmmoID.Bullet;
			Item.reuseDelay = 15;
            Item.value = Item.sellPrice(0);
            Item.noMelee = true;
            Item.autoReuse = true;
        }
		public override bool AltFunctionUse(Player player) => false;
		public override bool CanUseItem(Player player) {
			var modPlayer = Main.LocalPlayer.GetModPlayer<WeaponPlayer>();
			if (Main.myPlayer == player.whoAmI) {
				if (ArknightsKeybinds.SkillActivatePressed(player)) {
					if (!modPlayer.SummonMode) {
						// S2
						if (modPlayer.Skill == 1 && modPlayer.StockCount > 0 && !modPlayer.SkillActive) {
							modPlayer.SkillActive = true;
							modPlayer.SkillTimer = 0;

							modPlayer.DelStockCount();

							Item.UseSound = SkillActive1;
							SoundEngine.PlaySound(Item.UseSound.Value, player.Center);
						}
						//S3
						if (modPlayer.Skill == 2 && modPlayer.StockCount > 0 && !modPlayer.SkillActive) {
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
						Item.useAnimation = 15;
						Item.useTime = 15;
						// S1
						if (modPlayer.Skill == 0) {
							if (modPlayer.StockCount == 0) {
								modPlayer.OffensiveRecovery();
								Item.UseSound = ExusiaiVectorA;
							}
							else if (modPlayer.StockCount > 0) {
								Item.useTime =5;
								modPlayer.SkillActive = true;
								modPlayer.SkillTimer = 0;
								modPlayer.DelStockCount();
								Item.UseSound = ExusiaiVectorS;
							}
						}
						// S2
						if (modPlayer.Skill == 1 && modPlayer.SkillActive) {
							Item.useTime = 4;
							Item.UseSound = ExusiaiVectorS;
							SoundEngine.PlaySound(Item.UseSound.Value, player.Center);
						}
						else if (modPlayer.Skill == 1 && !modPlayer.SkillActive) {
							Item.UseSound = ExusiaiVectorA;
						}
						// S3
						if (modPlayer.Skill == 2 && modPlayer.SkillActive) {
							Item.useAnimation = 8;
							Item.useTime = 2;
							Item.UseSound = ExusiaiVectorS;
							SoundEngine.PlaySound(Item.UseSound.Value, player.Center);
						}
						else if (modPlayer.Skill == 2 && !modPlayer.SkillActive) {
							Item.UseSound = ExusiaiVectorA;
						}
					}
				}
			}
			return base.CanUseItem(player);
		}
		public override void ModifyWeaponDamage(Player player, ref StatModifier damage) {
			var modPlayer = Main.LocalPlayer.GetModPlayer<WeaponPlayer>();
			if (Main.myPlayer == player.whoAmI) {
				if (modPlayer.Skill == 0 && (modPlayer.StockCount > 0 || modPlayer.SkillActive == true)) {
					damage *= 1.45f;
				}
				if (modPlayer.Skill == 1 && modPlayer.SkillActive == true) {
					damage *= 1.25f;
				}
				if (modPlayer.Skill == 2 && modPlayer.SkillActive == true) {
					damage *= 1.1f;
				}
			}
		}
		public override void HoldItem(Player player) {
			var modPlayer = Main.LocalPlayer.GetModPlayer<WeaponPlayer>();
			if (Main.myPlayer == player.whoAmI)
			{ 
				if (modPlayer.Skill == 2 && modPlayer.StockCount > 0 && !modPlayer.SkillActive)
				{
					// SP 满自动触发 S3：直接在这里完成技能激活（设置 SkillActive/计时/扣充能/音效），
					// 不再需要伪造"右键状态"去复用 CanUseItem 里的触发分支。
					modPlayer.SkillActive = true;
					modPlayer.SkillTimer = 0;

					modPlayer.DelStockCount();

					Item.UseSound = SkillActive1;
					SoundEngine.PlaySound(Item.UseSound.Value, player.Center);
				}
			}
		}

		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {

            Projectile.NewProjectileDirect(source, position, velocity.RotatedByRandom(MathHelper.ToRadians(1)), type, damage, knockback, player.whoAmI);

            for (int j = 0; j < 2; j++)
            {
                float[] rand = {
                        Main.rand.NextFloat(-30f, 0f),
                        Main.rand.NextFloat(-15f, 15f),
                        Main.rand.NextFloat(0f, 30f)
                    };
                for (int i = 0; i < 3; i++)
                {
                    float angleMagnitude = Math.Abs(rand[i]);

                    Vector2 vel = velocity.RotatedBy(MathHelper.ToRadians(rand[i])) * (1f - angleMagnitude / 30f * 0.3f) * Main.rand.NextFloat(0.5f, 1.5f);

                    Projectile.NewProjectileDirect(
                        source,
                        position + velocity.SafeNormalize(Vector2.Zero) * 40,
                        vel,
                        ModContent.ProjectileType<Exusiai_Gun_Effect>(),
                        0, 0, player.whoAmI
                    );
                }
            }

            return false;
        }

        public override Vector2? HoldoutOffset()
        {
            return new Vector2(-16f, 2f);
        }
    }
}
