using ArknightsMod.Content.Buffs;
using ArknightsMod.Content.Items.Material;
using ArknightsMod.Content.Projectiles.Guard.Laevatain;
using ArknightsMod.Content.Tiles.Infrastructure;
using ArknightsMod.Players;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Weapons.Guard.Surtr
{
	public class SurtrLaevatain : UpgradeWeaponBase
	{
		private static SoundStyle SkillActiveSound;

		public override void SetDefaults()
		{
			Item.damage = 134;
			Item.DamageType = DamageClass.Melee;
			Item.width = 64;
			Item.height = 70;
			Item.useTime = 37;
			Item.useAnimation = 37;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.knockBack = 10;
			Item.value = Item.sellPrice(silver: 3000);
			Item.noUseGraphic = true;
			Item.noMelee = true;
			Item.shoot = ModContent.ProjectileType<LaevatainProjectile_normal>();
			Item.shootSpeed = 1f;
			Item.rare = ItemRarityID.Red;
			Item.UseSound = SoundID.Item1;
			Item.autoReuse = true;
		}

		public override void Load()
		{
			SkillActiveSound = new SoundStyle("ArknightsMod/Sounds/SkillActive1")
			{
				Volume = 0.4f,
				MaxInstances = 4,
			};
		}

		public override void AddRecipes()
		{
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient<PolymerizationPreparation>(4);
			recipe.AddIngredient<KetonColloid>(5);
			recipe.AddTile(ModContent.TileType<FactoryTile>());
			recipe.Register();
		}

		public override bool AltFunctionUse(Player player) => true;

		public override bool Shoot(
			Player player,
			EntitySource_ItemUse_WithAmmo source,
			Vector2 position,
			Vector2 velocity,
			int type,
			int damage,
			float knockback
		)
		{
			var modPlayer = player.GetModPlayer<WeaponPlayer>();
			if (modPlayer.Skill == 0 && !modPlayer.SkillActive)
			{
				Projectile.NewProjectile(
					source,
					position,
					velocity,
					ModContent.ProjectileType<LaevatainProjectile_normal>(),
					damage,
					knockback,
					player.whoAmI,
					0f,
					0f,
					0f
				);
				modPlayer.OffensiveRecovery();
				return false;
			}
			else if (modPlayer.Skill == 0 && modPlayer.SkillActive)
			{
				var proj = Projectile.NewProjectile(
					source,
					position,
					velocity,
					ModContent.ProjectileType<LaevatainProjectile_1_plan2>(),
					damage,
					knockback,
					player.whoAmI,
					0f,
					0f,
					2f
				);
				return false;
			}
			if (modPlayer.Skill == 1 && modPlayer.SkillActive)
			{
				// 刺击 + 特效A/B + 白光判定，全部合并在 LaevatainProjectile_2 一个类里
				Projectile.NewProjectile(
					source,
					position,
					velocity,
					ModContent.ProjectileType<LaevatainProjectile_2>(),
					damage,
					knockback,
					player.whoAmI
				);
				return false;
			}
			else if (modPlayer.Skill == 2 && modPlayer.SkillActive)
			{
				Projectile.NewProjectile(
					source,
					position,
					velocity,
					ModContent.ProjectileType<LaevatainProjectile_3>(),
					damage,
					knockback,
					player.whoAmI
				);
				return false;
			}
			Projectile.NewProjectile(
				source,
				position,
				velocity,
				ModContent.ProjectileType<LaevatainProjectile_normal>(),
				damage,
				knockback,
				player.whoAmI,
				0f,
				0f,
				0f
			);
			return false;
		}

		public override bool CanUseItem(Player player)
		{
			if (Main.myPlayer != player.whoAmI)
				return base.CanUseItem(player);

			var modPlayer = player.GetModPlayer<WeaponPlayer>();

			if (player.altFunctionUse == 2)
			{
				//其实好像写成一个就行了
				if (modPlayer.Skill == 1 && modPlayer.StockCount > 0 && !modPlayer.SkillActive)
				{
					modPlayer.SkillActive = true;
					modPlayer.SkillTimer = 0;
					modPlayer.DelStockCount();
					SoundEngine.PlaySound(SkillActiveSound, player.Center);
					return false;
				}
				else if (modPlayer.Skill == 2 && modPlayer.StockCount > 0 && !modPlayer.SkillActive)
				{
					modPlayer.SkillActive = true;
					modPlayer.SkillTimer = 0;
					modPlayer.DelStockCount();
					SoundEngine.PlaySound(SkillActiveSound, player.Center);
					// 挂成真正的 buff，即使切武器把 Skill/SkillActive 重置掉，这个效果也不会消失
					player.AddBuff(ModContent.BuffType<SurtrLaevatainS3Buff>(), SurtrLaevatainS3Buff.InitialDuration);
					return false;
				}
				return false;
			}
			if (modPlayer.Skill == 0 && modPlayer.StockCount > 0 && !modPlayer.SkillActive)
			{
				modPlayer.SkillActive = true;
				modPlayer.SkillTimer = 0;
				modPlayer.DelStockCount();
				SoundEngine.PlaySound(SkillActiveSound, player.Center);
				return false;
			}
			return base.CanUseItem(player);
		}

		public override void ModifyWeaponDamage(Player player, ref StatModifier damage)
		{
			if (Main.myPlayer != player.whoAmI)
				return;
			var modPlayer = player.GetModPlayer<WeaponPlayer>();
			if (modPlayer.Skill == 0 && modPlayer.SkillActive)
				damage *= 3.1f;
			if (modPlayer.Skill == 1 && modPlayer.SkillActive)
				damage *= 2.2f; // 只命中一个敌人时 ×1.5 的加成在 LaevatainProjectile_2 里实现
			if (modPlayer.Skill == 2 && modPlayer.SkillActive)
				damage *= 4.3f;
		}
	}
}
