using System;
using ArknightsMod.Content.Projectiles.Medic.Shining;
using ArknightsMod.Players;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Weapons.Medic.Shining
{
	// By T
	/// <summary>
	/// 闪灵的法杖。黑白剑形源石技艺命中敌人即可吸血；屏障与防护领域由技能提供。
	/// </summary>
	public sealed class ShiningStaff : ExpansionWeaponBase
	{
		// 原面板 106 提高 30%，四舍五入为 138。
		protected override int[] EliteDamage => [138, 138, 138];

		private static SoundStyle skillSound;

		// Sky Fracture (Item_3787) 没有注册物品动画，是确认过的单帧占位贴图。
		public override string Texture => "Terraria/Images/Item_" + ItemID.SkyFracture;

		public override void SetStaticDefaults()
		{
			// 贴图是按法杖的 45° 持握姿势绘制的；没有这项注册时，Terraria 会把它当普通射击武器横着拿。
			Item.staff[Type] = true;
		}

		public override void Load()
		{
			skillSound = new SoundStyle("ArknightsMod/Sounds/SkillActive1")
			{
				Volume = 0.58f,
				Pitch = -0.08f,
				MaxInstances = 2
			};
		}

		public override void Unload() => skillSound = default;

		public override void SetDefaults()
		{
			Item.damage = EliteDamage[0];
			Item.DamageType = DamageClass.Magic;
			Item.width = 42;
			Item.height = 48;
			Item.mana = 9;
			Item.useTime = 25;
			Item.useAnimation = 25;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.knockBack = 4.5f;
			Item.value = Item.sellPrice(gold: 1, silver: 20);
			Item.rare = ItemRarityID.LightRed;
			Item.autoReuse = true;
			Item.noMelee = true;
			Item.shoot = ModContent.ProjectileType<ShiningBladeProjectile>();
			Item.shootSpeed = 15f;
			Item.crit = 4;
			Item.UseSound = SoundID.Item43 with { Volume = 0.58f, Pitch = -0.18f };
		}

		public override bool AltFunctionUse(Player player) => false;

		public override bool CanUseItem(Player player)
		{
			if (!ArknightsKeybinds.SkillActivatePressed(player))
				return base.CanUseItem(player);

			WeaponPlayer weaponPlayer = player.GetModPlayer<WeaponPlayer>();
			// 自动掩护会在技力就绪时自行套给持有者，不响应手动重复开启。
			if (weaponPlayer.Skill != 1)
				TryActivateSkill(player, weaponPlayer);
			return false;
		}

		public override void HoldItem(Player player)
		{
			base.HoldItem(player);
			WeaponPlayer weaponPlayer = player.GetModPlayer<WeaponPlayer>();

			if (player.whoAmI == Main.myPlayer && weaponPlayer.Skill == 1
				&& weaponPlayer.CurrentSkill?.AutoTrigger == true
				&& weaponPlayer.StockCount > 0 && !weaponPlayer.SkillActive)
				TryActivateSkill(player, weaponPlayer);

			if (weaponPlayer.SkillActive && weaponPlayer.Skill == 2)
			{
				player.statDefense += 30;
				player.endurance = Math.Min(0.38f, player.endurance + 0.18f);
				EnsureSanctuary(player, weaponPlayer);
			}
		}

		public override float UseSpeedMultiplier(Player player)
		{
			WeaponPlayer weaponPlayer = player.GetModPlayer<WeaponPlayer>();
			// 基础攻速提高 40%，保留信条原有的独立攻速增益。
			return 1.4f * (weaponPlayer.SkillActive && weaponPlayer.Skill == 0 ? 1.55f : 1f);
		}

		public override void ModifyWeaponDamage(Player player, ref StatModifier damage)
		{
			base.ModifyWeaponDamage(player, ref damage);
			WeaponPlayer weaponPlayer = player.GetModPlayer<WeaponPlayer>();
			if (!weaponPlayer.SkillActive)
				return;
			if (weaponPlayer.Skill == 0)
				damage *= 1.35f;
			else if (weaponPlayer.Skill == 2)
				damage *= 1.50f;
		}

		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source,
			Vector2 position, Vector2 velocity, int type, int damage, float knockback)
		{
			if (player.whoAmI != Main.myPlayer)
				return false;

			WeaponPlayer weaponPlayer = player.GetModPlayer<WeaponPlayer>();
			int mode = weaponPlayer.SkillActive ? weaponPlayer.Skill + 1 : 0;
			Vector2 direction = velocity.SafeNormalize(new Vector2(player.direction, 0f));
			SpawnBlade(source, player, position, direction * Item.shootSpeed, damage, knockback, mode);

			if (mode == 3)
			{
				SpawnBlade(source, player, position, direction.RotatedBy(-0.075f) * Item.shootSpeed,
					(int)(damage * 0.68f), knockback * 0.75f, mode);
				SpawnBlade(source, player, position, direction.RotatedBy(0.075f) * Item.shootSpeed,
					(int)(damage * 0.68f), knockback * 0.75f, mode);
			}
			return false;
		}

		private static void SpawnBlade(IEntitySource source, Player player, Vector2 position,
			Vector2 velocity, int damage, float knockback, int mode)
		{
			int index = Projectile.NewProjectile(source, position, velocity,
				ModContent.ProjectileType<ShiningBladeProjectile>(), damage, knockback,
				player.whoAmI, mode);
			if (Main.projectile.IndexInRange(index))
				Main.projectile[index].CritChance = player.GetWeaponCrit(player.HeldItem);
		}

		private static bool TryActivateSkill(Player player, WeaponPlayer weaponPlayer)
		{
			if (player.whoAmI != Main.myPlayer || weaponPlayer.CurrentSkill == null
				|| weaponPlayer.StockCount <= 0 || weaponPlayer.SkillActive)
				return false;

			weaponPlayer.SkillActive = true;
			weaponPlayer.SkillTimer = 0;
			weaponPlayer.DelStockCount();
			if (!Main.dedServ)
				SoundEngine.PlaySound(skillSound, player.Center);

			int duration = Math.Max(1,
				(int)(weaponPlayer.CurrentSkill.CurrentLevelData.ActiveTime * 60f
					* WeaponPlayer.ActiveDurationMultiplier));
			if (weaponPlayer.Skill == 1)
				player.GetModPlayer<ShiningStaffPlayer>().GrantShield(0.15f, duration);
			else if (weaponPlayer.Skill == 2)
				SpawnSanctuary(player, duration);
			return true;
		}

		private static void EnsureSanctuary(Player player, WeaponPlayer weaponPlayer)
		{
			int type = ModContent.ProjectileType<ShiningSanctuaryProjectile>();
			if (player.whoAmI == Main.myPlayer && player.ownedProjectileCounts[type] <= 0)
			{
				int duration = Math.Max(2,
					(int)((weaponPlayer.CurrentSkill?.CurrentLevelData.ActiveTime ?? 20f) * 60f
						* WeaponPlayer.ActiveDurationMultiplier) - weaponPlayer.SkillTimer);
				SpawnSanctuary(player, duration);
			}
		}

		private static void SpawnSanctuary(Player player, int duration)
		{
			int type = ModContent.ProjectileType<ShiningSanctuaryProjectile>();
			if (player.ownedProjectileCounts[type] > 0)
				return;
			Projectile.NewProjectile(player.GetSource_Misc("ShiningSanctuary"), player.MountedCenter,
				Vector2.Zero, type, 0, 0f,
				player.whoAmI, duration);
		}

		public override void AddRecipes()
		{
			CreateRecipe()
				.AddIngredient(ItemID.SpectreStaff)
				.AddIngredient(ItemID.SpectreBar, 16)
				.AddIngredient(ItemID.CrystalShard, 20)
				.AddIngredient<global::ArknightsMod.Content.Items.Material.Polyketon>(6)
				.AddIngredient<global::ArknightsMod.Content.Items.Material.BipolarNanoflake>(4)
				.AddTile(ModContent.TileType<global::ArknightsMod.Content.Tiles.Infrastructure.FactoryTile>())
				.Register();
		}
	}
}
