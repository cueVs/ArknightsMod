using System;
using ArknightsMod.Content;
using ArknightsMod.Content.Projectiles.Sniper.Fiammetta;
using ArknightsMod.Players;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Weapons.Sniper.Fiammetta
{
	/// <summary>
	/// 菲亚梅塔的手炮。普通攻击是真正的曲射迫击炮；三个技能分别承担射程强化、
	/// 沿弹道连续爆破、固定落点无限炮击，绝不是同一颗炮弹的三种换色。
	/// </summary>
	public sealed class FiammettaHandCannon : ExpansionWeaponBase
	{
		internal const float NormalMaximumRange = 650f;
		internal const float ProvocateMaximumRange = 790f;
		internal const float PaeniteteMaximumRange = 590f;
		internal const float ReponiteMaximumRange = 720f;
		// 基础攻速提高 20%：原 60 帧间隔 / 1.2 = 50，物品和实际炮击共用此节拍。
		internal const int AttackIntervalTicks = 50;

		// 基础伤害翻倍，所有精英档位统一为 464。
		protected override int[] EliteDamage => [464, 464, 464];

		private static SoundStyle SkillActiveSound;

		public override string Texture =>
			"ArknightsMod/Content/Items/Weapons/Sniper/Fiammetta/FiammettaHandCannon";

		public override void Load()
		{
			SkillActiveSound = new SoundStyle("ArknightsMod/Sounds/SkillActive1")
			{
				Volume = 0.62f,
				Pitch = 0.08f,
				MaxInstances = 2
			};
		}

		public override void Unload() => SkillActiveSound = default;

		public override void SetDefaults()
		{
			Item.damage = EliteDamage[0];
			Item.DamageType = DamageClass.Ranged;
			Item.width = 72;
			Item.height = 24;
			Item.useTime = AttackIntervalTicks;
			Item.useAnimation = AttackIntervalTicks;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.knockBack = 7.5f;
			Item.value = Item.sellPrice(gold: 1, silver: 20);
			Item.rare = ItemRarityID.LightRed;
			Item.autoReuse = true;
			Item.channel = true;
			Item.noMelee = true;
			Item.noUseGraphic = true;
			Item.shoot = ModContent.ProjectileType<FiammettaCannonHoldout>();
			Item.shootSpeed = 1f;
			Item.crit = 6;
			Item.UseSound = null;
		}

		public override bool AltFunctionUse(Player player) => false;

		public override bool CanUseItem(Player player)
		{
			WeaponPlayer weaponPlayer = player.GetModPlayer<WeaponPlayer>();
			int holdoutType = ModContent.ProjectileType<FiammettaCannonHoldout>();

			if (ArknightsKeybinds.SkillActivatePressed(player))
			{
				if (player.whoAmI != Main.myPlayer)
					return false;

				switch (weaponPlayer.Skill)
				{
					case 0:
						TryActivateProvocate(player, weaponPlayer);
						break;
					case 1:
						if (weaponPlayer.StockCount > 0 && !weaponPlayer.SkillActive &&
							player.ownedProjectileCounts[holdoutType] < 1)
						{
							Vector2 target = ClampTarget(player, Main.MouseWorld, PaeniteteMaximumRange);
							SpawnHoldout(player.GetSource_ItemUse(Item), player, target,
								player.GetWeaponDamage(Item), player.GetWeaponKnockback(Item), 2);
							weaponPlayer.DelStockCount();
							PlaySkillSound(player);
						}
						break;
					case 2:
						ToggleReponite(player, weaponPlayer);
						break;
				}
				return false;
			}

			// S3 开启后由控制器按照固定节奏自动炮击选定落点；左键不再偷偷追加另一套炮击。
			if (weaponPlayer.Skill == 2 && weaponPlayer.SkillActive)
				return false;

			return player.ownedProjectileCounts[holdoutType] < 1 && base.CanUseItem(player);
		}

		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source,
			Vector2 position, Vector2 velocity, int type, int damage, float knockback)
		{
			WeaponPlayer weaponPlayer = player.GetModPlayer<WeaponPlayer>();
			bool provoke = weaponPlayer.Skill == 0 && weaponPlayer.SkillActive;
			float maximumRange = provoke ? ProvocateMaximumRange : NormalMaximumRange;
			Vector2 target = ClampTarget(player, Main.MouseWorld, maximumRange);

			SpawnHoldout(source, player, target, damage, knockback, provoke ? 1 : 0);
			return false;
		}

		public override void ModifyWeaponDamage(Player player, ref StatModifier damage)
		{
			base.ModifyWeaponDamage(player, ref damage);
			WeaponPlayer weaponPlayer = player.GetModPlayer<WeaponPlayer>();
			if (weaponPlayer.Skill == 0 && weaponPlayer.SkillActive)
				damage *= 2f; // “你须直面”：专三攻击力 +100%。
		}

		public override void HoldItem(Player player)
		{
			base.HoldItem(player);
			WeaponPlayer weaponPlayer = player.GetModPlayer<WeaponPlayer>();
			if (player.whoAmI != Main.myPlayer || weaponPlayer.Skill != 2 || !weaponPlayer.SkillActive)
				return;

			int controllerType = ModContent.ProjectileType<FiammettaReponiteController>();
			if (player.ownedProjectileCounts[controllerType] < 1)
			{
				// 断线重进或弹幕池异常清理后仍能恢复永久炮击；新的落点取当前鼠标并再次限距。
				Vector2 target = ClampTarget(player, Main.MouseWorld, ReponiteMaximumRange);
				SpawnReponiteController(player, target);
			}
		}

		private static void TryActivateProvocate(Player player, WeaponPlayer weaponPlayer)
		{
			if (weaponPlayer.StockCount <= 0 || weaponPlayer.SkillActive)
				return;

			weaponPlayer.SkillActive = true;
			weaponPlayer.SkillTimer = 0;
			weaponPlayer.DelStockCount();
			PlaySkillSound(player);
		}

		private static void ToggleReponite(Player player, WeaponPlayer weaponPlayer)
		{
			if (weaponPlayer.SkillActive)
			{
				weaponPlayer.SkillActive = false;
				weaponPlayer.SkillTimer = 0;
				SoundEngine.PlaySound(SoundID.Item8 with { Volume = 0.42f, Pitch = -0.20f }, player.Center);
				return;
			}

			if (weaponPlayer.StockCount <= 0)
				return;

			Vector2 target = ClampTarget(player, Main.MouseWorld, ReponiteMaximumRange);
			weaponPlayer.SkillActive = true;
			weaponPlayer.SkillTimer = 0;
			weaponPlayer.DelStockCount();
			SpawnReponiteController(player, target);
			PlaySkillSound(player);
		}

		private static void SpawnReponiteController(Player player, Vector2 target)
		{
			Projectile.NewProjectile(player.GetSource_ItemUse(player.HeldItem), target, Vector2.Zero,
				ModContent.ProjectileType<FiammettaReponiteController>(), 0, 0f, player.whoAmI,
				target.X, target.Y);
		}

		internal static int SpawnHoldout(IEntitySource source, Player player, Vector2 target,
			int damage, float knockback, int mode)
		{
			return Projectile.NewProjectile(source, player.MountedCenter, Vector2.Zero,
				ModContent.ProjectileType<FiammettaCannonHoldout>(), damage, knockback, player.whoAmI,
				target.X, target.Y, mode);
		}

		internal static Vector2 ClampTarget(Player player, Vector2 requestedTarget, float maximumRange)
		{
			Vector2 offset = requestedTarget - player.MountedCenter;
			if (offset.LengthSquared() < 16f)
				offset = new Vector2(player.direction, 0f) * 160f;
			if (offset.Length() > maximumRange)
				offset = offset.SafeNormalize(new Vector2(player.direction, 0f)) * maximumRange;
			return player.MountedCenter + offset;
		}

		private static void PlaySkillSound(Player player)
		{
			if (!Main.dedServ)
				SoundEngine.PlaySound(SkillActiveSound, player.Center);
		}

		public override void AddRecipes()
		{
			CreateRecipe()
				.AddIngredient(ItemID.GrenadeLauncher)
				.AddIngredient(ItemID.ShroomiteBar, 16)
				.AddIngredient(ItemID.RocketI, 50)
				.AddTile(TileID.MythrilAnvil)
				.Register();
		}

	}
}
