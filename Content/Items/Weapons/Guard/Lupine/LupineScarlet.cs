using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using ArknightsMod.Content.Projectiles.Guard.Lupine;
using ArknightsMod.Content;

namespace ArknightsMod.Content.Items.Weapons.Guard.Lupine
{
	// 狼之绯（Lupine Scarlet）—— 移植自 EndfieldNPCs，保留三连击挥砍 + 右键碎影
	// 贴图使用新的 Rossi.png（与类同目录的 LupineScarlet.png 自动解析）
	public class LupineScarlet : ModItem
	{
		public override void SetDefaults() {
			// ===== 手感参数 =====
			const int SwingDuration = 18;
			const int SwingEndHoldFrames = 12;

			Item.width = 44;
			Item.height = 44;

			Item.damage = 60;
			Item.DamageType = DamageClass.Melee;

			Item.useTime = SwingDuration + SwingEndHoldFrames;
			Item.useAnimation = SwingDuration + SwingEndHoldFrames;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.noUseGraphic = true;
			Item.noMelee = true;
			Item.channel = false;

			Item.knockBack = 6f;
			Item.crit = 4;

			Item.value = Item.sellPrice(gold: 2);
			Item.rare = ItemRarityID.LightRed;

			Item.UseSound = SoundID.Item1;
			Item.autoReuse = true;
			Item.useTurn = true;

			Item.shoot = ModContent.ProjectileType<LupineScarletSwingProjectile>();
			Item.shootSpeed = 0f;
		}

		public override bool AltFunctionUse(Player player) => false;

		public override bool CanUseItem(Player player) {
			if (ArknightsKeybinds.SkillActivatePressed(player)) {
				Item.useStyle = ItemUseStyleID.Shoot;
				Item.useTime = 32;
				Item.useAnimation = 32;
				Item.autoReuse = false;
				Item.UseSound = SoundID.Item20;
				Item.shoot = ModContent.ProjectileType<LupineScarletRightClickController>();
				return player.ownedProjectileCounts[Item.shoot] <= 0;
			}

			Item.useStyle = ItemUseStyleID.Swing;
			Item.autoReuse = true;
			Item.UseSound = SoundID.Item1;
			Item.shoot = ModContent.ProjectileType<LupineScarletSwingProjectile>();
			return player.ownedProjectileCounts[Item.shoot] <= 0;
		}

		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source,
				Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			if (ArknightsKeybinds.SkillActivatePressed(player)) {
				Projectile.NewProjectile(source, player.MountedCenter, Vector2.Zero, Item.shoot,
					damage, knockback, player.whoAmI, Main.MouseWorld.X, Main.MouseWorld.Y);
				return false;
			}

			// 三连击循环由挥砍弹幕内部 attackType 管理
			Projectile.NewProjectile(source, player.MountedCenter, Vector2.Zero, Item.shoot,
				damage, knockback, player.whoAmI);
			return false;
		}
	}
}
