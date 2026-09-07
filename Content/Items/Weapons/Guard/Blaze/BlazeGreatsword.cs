using System;
using ArknightsMod.Content;
using ArknightsMod.Content.Items.Weapons;
using ArknightsMod.Content.Projectiles.Guard.Blaze;
using ArknightsMod.Players;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Weapons.Guard.Blaze
{
	/// <summary>
	/// 煌 / Blaze 的链锯大剑。S1、S2 为自动技能，技能键只用于手动开启 S3。
	/// </summary>
	public class BlazeGreatsword : ExpansionWeaponBase
	{
		public const float PowerStrikeDamageMultiplier = 2.9f;
		public const float ExtensionAttackMultiplier = 2f;

		// 链锯靠高频咬合而不是单次大数字输出：单击伤害降至原来的约三分之二，
		// 普通持有弹幕每 7 帧刷新一次咬合判定，S3 再由独立控制器加速至每 3 帧一次。
		// 煌六星：精二满级 765 攻击 × 20% = 153。
		protected override int[] EliteDamage => [153, 153, 153];

		private static SoundStyle SkillActiveSfx;

		public override string Texture => BlazeChainsawSprite.TexturePath;

		public override void SetStaticDefaults() {
			Main.RegisterItemAnimation(Type, new DrawAnimationVertical(5, BlazeChainsawSprite.FrameCount));
		}

		public override void Load() {
			SkillActiveSfx = new SoundStyle("ArknightsMod/Sounds/SkillActive1") {
				Volume = 0.65f,
				MaxInstances = 2
			};
		}

		public override void Unload() => SkillActiveSfx = default;

		public override void SetDefaults() {
			Item.damage = EliteDamage[0];
			Item.DamageType = DamageClass.Melee;
			Item.width = 54;
			Item.height = 24;
			Item.useTime = 10;
			Item.useAnimation = 10;
			Item.knockBack = 6.5f;
			Item.value = Item.sellPrice(silver: 60);
			Item.rare = ItemRarityID.LightRed;
			Item.autoReuse = true;
			Item.channel = true;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.noMelee = true;
			Item.noUseGraphic = true;
			Item.UseSound = BlazeVisuals.ButcherMotorSound;
			Item.shoot = ModContent.ProjectileType<BlazeChainsawHoldout>();
			Item.shootSpeed = 1f;
			Item.crit = 4;
		}

		public override bool AltFunctionUse(Player player) => false;

		public override bool CanUseItem(Player player) {
			var weaponPlayer = player.GetModPlayer<WeaponPlayer>();
			var blazePlayer = player.GetModPlayer<BlazeGreatswordPlayer>();
			int burstType = ModContent.ProjectileType<BlazeBoilingBurstController>();

			if (ArknightsKeybinds.SkillActivatePressed(player)) {
				// S1 与 S2 完全自动；只有 S3 响应技能键。
				if (weaponPlayer.Skill == 2 && weaponPlayer.StockCount > 0
					&& !weaponPlayer.SkillActive && player.ownedProjectileCounts[burstType] < 1
					&& player.ownedProjectileCounts[Item.shoot] < 1
					&& player.whoAmI == Main.myPlayer) {
					Vector2 aim = player.DirectionTo(Main.MouseWorld).SafeNormalize(new Vector2(player.direction, 0f));
					int controller = Projectile.NewProjectile(player.GetSource_ItemUse(Item), player.MountedCenter, aim,
						burstType, player.GetWeaponDamage(Item), player.GetWeaponKnockback(Item), player.whoAmI,
						ai0: player.selectedItem, ai1: 3f, ai2: WeaponPlayer.ActiveDurationMultiplier);
					if (controller < 0 || controller >= Main.maxProjectiles)
						return false;

					// 只有控制器确实创建成功后才扣技力并进入状态，避免弹幕池满时永久卡技。
					weaponPlayer.SkillActive = true;
					weaponPlayer.SkillTimer = 0;
					weaponPlayer.DelStockCount();
					blazePlayer.BeginBoilingBurst();
					if (!Main.dedServ) {
						SoundEngine.PlaySound(SkillActiveSfx, player.Center);
						SoundEngine.PlaySound(BlazeVisuals.ButcherMotorSound with { Volume = 0.68f, Pitch = -0.22f }, player.Center);
					}
				}
				return false;
			}

			// 终爆后的最后一秒只是技能收尾，不再让不可见控制器锁住普通攻击。
			bool burstStillControlsWeapon = player.ownedProjectileCounts[burstType] > 0
				&& blazePlayer.BoilingBurstActive;
			return player.ownedProjectileCounts[Item.shoot] < 1
				&& !burstStillControlsWeapon
				&& base.CanUseItem(player);
		}

		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source,
			Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			var weaponPlayer = player.GetModPlayer<WeaponPlayer>();
			var blazePlayer = player.GetModPlayer<BlazeGreatswordPlayer>();
			bool extended = weaponPlayer.Skill == 1 && blazePlayer.ExtensionDeployed;
			Vector2 aim = velocity.SafeNormalize(new Vector2(player.direction, 0f));
			Projectile.NewProjectile(source, player.MountedCenter, aim,
				ModContent.ProjectileType<BlazeChainsawHoldout>(), damage, knockback, player.whoAmI,
				weaponPlayer.Skill, extended ? 1f : 0f);

			return false;
		}

		public override void ModifyWeaponDamage(Player player, ref StatModifier damage) {
			base.ModifyWeaponDamage(player, ref damage);
			var weaponPlayer = player.GetModPlayer<WeaponPlayer>();
			var blazePlayer = player.GetModPlayer<BlazeGreatswordPlayer>();
			if (weaponPlayer.Skill == 1 && blazePlayer.ExtensionDeployed)
				damage *= ExtensionAttackMultiplier;
		}

		public override void AddRecipes() {
			CreateRecipe()
				.AddIngredient(ItemID.ButchersChainsaw)
				.AddIngredient(ItemID.ShroomiteBar, 16)
				.AddIngredient(ItemID.Wire, 30)
				.AddTile(TileID.MythrilAnvil)
				.Register();
		}

	}
}
