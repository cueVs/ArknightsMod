using ArknightsMod.Content;
using ArknightsMod.Content.Projectiles.Guard.Specter;
using ArknightsMod.Players;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Weapons.Guard.Specter
{
	// By T
	/// <summary>
	/// 幽灵鲨的圆锯。普通攻击沿用煌链锯已经验证过的持续接触群攻骨架，
	/// 角色差异则全部放在深海视觉、持续再生与「肉斩骨断」的锁血/失能闭环中。
	/// </summary>
	public sealed class SpecterBoneSaw : ExpansionWeaponBase
	{
		public const float Skill1DamageMultiplier = 2f;
		public const float Skill2DamageMultiplier = 2.6f;

		// 五星强攻手：基础咬合略低于煌，但仍以每 7 帧一次的接触群攻作为主要输出。
		// 幽灵鲨五星：精二 1 级 551 攻击 × 17.5%，四舍五入为 96。
		protected override int[] EliteDamage => [96, 96, 96];

		private static SoundStyle SkillActiveSfx;

		public override string Texture => "Terraria/Images/Item_" + ItemID.SawtoothShark;

		public override void Load() {
			SkillActiveSfx = new SoundStyle("ArknightsMod/Sounds/SkillActive1") {
				Volume = 0.66f,
				MaxInstances = 2
			};
		}

		public override void Unload() => SkillActiveSfx = default;

		public override void SetDefaults() {
			Item.damage = EliteDamage[0];
			Item.DamageType = DamageClass.Melee;
			Item.width = 46;
			Item.height = 28;
			Item.useTime = 10;
			Item.useAnimation = 10;
			Item.knockBack = 6f;
			Item.value = Item.sellPrice(silver: 50);
			Item.rare = ItemRarityID.LightRed;
			Item.autoReuse = true;
			Item.channel = true;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.noMelee = true;
			Item.noUseGraphic = true;
			Item.UseSound = SpecterVisuals.MotorSound;
			Item.shoot = ModContent.ProjectileType<SpecterBoneSawHoldout>();
			Item.shootSpeed = 1f;
			Item.crit = 4;
		}

		public override bool AltFunctionUse(Player player) => false;

		public override bool CanUseItem(Player player) {
			SpecterBoneSawPlayer specterPlayer = player.GetModPlayer<SpecterBoneSawPlayer>();

			if (specterPlayer.ExhaustionTimeLeft > 0)
				return false;

			if (ArknightsKeybinds.SkillActivatePressed(player)) {
				TryActivateSelectedSkill(player);
				return false;
			}

			return player.ownedProjectileCounts[Item.shoot] < 1 && base.CanUseItem(player);
		}

		/// <summary>
		/// CanUseItem 与 ModPlayer 的按键边沿检测共用同一入口。后者保证玩家按住链锯时
		/// itemTime 被持有弹幕维持在 2，也仍能即时开启技能。
		/// </summary>
		internal bool TryActivateSelectedSkill(Player player) {
			WeaponPlayer weaponPlayer = player.GetModPlayer<WeaponPlayer>();
			SpecterBoneSawPlayer specterPlayer = player.GetModPlayer<SpecterBoneSawPlayer>();
			if (player.whoAmI != Main.myPlayer || weaponPlayer.StockCount <= 0
				|| weaponPlayer.SkillActive || specterPlayer.ExhaustionTimeLeft > 0
				|| !specterPlayer.TryStartSkill(weaponPlayer.Skill)) {
				return false;
			}

			weaponPlayer.SkillActive = true;
			weaponPlayer.SkillTimer = 0;
			weaponPlayer.DelStockCount();

			if (!Main.dedServ) {
				SoundEngine.PlaySound(SkillActiveSfx, player.Center);
				SoundEngine.PlaySound(SpecterVisuals.MotorSound with {
					Volume = weaponPlayer.Skill == 1 ? 0.78f : 0.62f,
					Pitch = weaponPlayer.Skill == 1 ? -0.28f : 0.16f
				}, player.Center);
				SpecterVisuals.SpawnSkillActivation(player.Center, frenzy: weaponPlayer.Skill == 1);
				SpecterVisuals.AddShake(player, weaponPlayer.Skill == 1 ? 10 : 6,
					weaponPlayer.Skill == 1 ? 4.8f : 2.8f);
			}

			if (weaponPlayer.Skill == 1
				&& player.ownedProjectileCounts[ModContent.ProjectileType<SpecterFrenzyAura>()] < 1) {
				Projectile.NewProjectile(player.GetSource_ItemUse(Item), player.Center, Vector2.Zero,
					ModContent.ProjectileType<SpecterFrenzyAura>(), 0, 0f, player.whoAmI);
			}
			return true;
		}

		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source,
			Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			Vector2 aim = velocity.SafeNormalize(new Vector2(player.direction, 0f));
			Projectile.NewProjectile(source, player.MountedCenter, aim,
				ModContent.ProjectileType<SpecterBoneSawHoldout>(), damage, knockback, player.whoAmI);
			return false;
		}

		public override void ModifyWeaponDamage(Player player, ref StatModifier damage) {
			base.ModifyWeaponDamage(player, ref damage);
			SpecterBoneSawPlayer specterPlayer = player.GetModPlayer<SpecterBoneSawPlayer>();
			if (specterPlayer.Skill2Active)
				damage *= Skill2DamageMultiplier;
			else if (specterPlayer.Skill1Active)
				damage *= Skill1DamageMultiplier;
		}

		public override void AddRecipes() {
			CreateRecipe()
				.AddIngredient(ItemID.SawtoothShark)
				.AddIngredient(ItemID.HallowedBar, 5)
				.AddIngredient(ItemID.SharkFin)
				.AddIngredient(ItemID.SoulofFright, 4)
				.AddIngredient<global::ArknightsMod.Content.Items.Material.Orirock>(6)
				.AddIngredient<global::ArknightsMod.Content.Items.Material.LoxicKohl>(8)
				.AddTile(ModContent.TileType<global::ArknightsMod.Content.Tiles.Infrastructure.FactoryTile>())
				.Register();
		}
	}
}
