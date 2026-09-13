using ArknightsMod.Content;
using ArknightsMod.Content.Items.Material;
using ArknightsMod.Content.Items.Weapons;
using ArknightsMod.Content.Projectiles.Guard.Entelechia;
using ArknightsMod.Content.Tiles.Infrastructure;
using ArknightsMod.Players;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Weapons.Guard.Entelechia
{
	// 隐德来希的镰刀：迁移自 EntelechiaScytheS2 模组。
	// 普通攻击朝光标方向甩出月牙形「刀光」（EntelechiaScytheBladeWaveProjectile，shader 程序化绘制）。
	// 三个技能位分别对应：
	// S1 玫影觅迹：次数型瞬发技能，技力满后自动引爆（不接受技能键手动开启）——
	// 下一次普通攻击自动变为 175% 伤害、连续两段刀光的强化攻击。
	// S2 绯红壁合：持续型技能，激活后 12 秒内按住左键可挥出循环斩（沿用原模组的攻击特效与判定方式），替代普通攻击。
	// S3 灵与欲的惜别：持续型技能，效果暂留空。
	public class EntelechiaScythe : ExpansionWeaponBase
	{
		protected override int[] EliteDamage => [60, 75, 90];

		private static SoundStyle SkillActiveSfx;

		public override void Load() {
			SkillActiveSfx = new SoundStyle("ArknightsMod/Sounds/SkillActive1") { Volume = 0.5f, MaxInstances = 2 };
		}

		public override void AddRecipes() {
			CreateRecipe()
				.AddIngredient(ModContent.ItemType<PolymerizationPreparation>(), 4)
				.AddIngredient(ModContent.ItemType<RMA7024>(), 4)
				.AddIngredient(ModContent.ItemType<D32Steel>(), 2)
				.AddTile(ModContent.TileType<FactoryTile>())
				.Register();
		}

		public override void SetDefaults() {
			Item.damage = EliteDamage[0];
			Item.DamageType = DamageClass.Melee;
			Item.width = 64;
			Item.height = 64;
			Item.useTime = 30;        // 降低普攻攻击频率
			Item.useAnimation = 30;
			Item.knockBack = 4f;
			Item.value = Item.sellPrice(gold: 2);
			Item.rare = 5;
			Item.crit = 4;
			Item.noUseGraphic = true;
			Item.noMelee = true;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.useTurn = true;
			Item.channel = true;
			Item.autoReuse = true;
			Item.shootSpeed = 14f;    // 刀光飞行速度（配合存活时间保持相同飞行距离）
			Item.shoot = ModContent.ProjectileType<EntelechiaScytheBladeWaveProjectile>();
		}

		public override bool AltFunctionUse(Player player) => false;

		// S3「灵与欲的惜别」激活期间：攻击力 +135%
		public override void ModifyWeaponDamage(Player player, ref StatModifier damage) {
			base.ModifyWeaponDamage(player, ref damage); // 保留精英化阶段加成
			var mp = player.GetModPlayer<WeaponPlayer>();
			if (mp.Skill == 2 && mp.SkillActive)
				damage += 1.35f;
		}

		// S3 激活期间：攻击速度提升
		public override float UseSpeedMultiplier(Player player) {
			var mp = player.GetModPlayer<WeaponPlayer>();
			return (mp.Skill == 2 && mp.SkillActive) ? 1.4f : 1f;
		}

		// S1「玫影觅迹」技力满后自动引爆，不接受手动开启：满技力后的下一次普攻自动变为强化攻击。
		// 具体的自动触发放在 HoldItem 里（见下方），这里只需要在技能键按下、且当前选中的是 S1 时
		// 直接吞掉这次按键、什么都不做——避免误触发 base.CanUseItem 走到普通攻击判定之外的逻辑。
		public override void HoldItem(Player player) {
			if (Main.myPlayer != player.whoAmI)
				return;

			var mp = player.GetModPlayer<WeaponPlayer>();
			// EntelechiaEmpoweredNext 本身就是"已引爆待发"的标记，天然防止连续多帧重复扣充能：
			// 一旦置为 true，要等下一次 Shoot 把它消费掉（置回 false）才会再次触发这里的判断。
			if (mp.Skill == 0 && mp.StockCount > 0 && !mp.EntelechiaEmpoweredNext) {
				mp.EntelechiaEmpoweredNext = true;
				mp.DelStockCount();
				SoundEngine.PlaySound(SkillActiveSfx, player.Center);
			}
		}

		public override bool CanUseItem(Player player) {
			var mp = player.GetModPlayer<WeaponPlayer>();

			if (ArknightsKeybinds.SkillActivatePressed(player)) {
				// S1 自动触发，技能键对它不生效；只处理 S2/S3 的手动开启。
				if (mp.Skill != 0 && mp.StockCount > 0 && !mp.SkillActive) {
					// S2/S3：进入持续技能状态，由技力系统按持续时间自动结束
					mp.SkillActive = true;
					mp.SkillTimer = 0;
					mp.DelStockCount();
					SoundEngine.PlaySound(SkillActiveSfx, player.Center);

					// S3「灵与欲的惜别」：在玩家身上召唤血雾光环
					if (mp.Skill == 2 && player.whoAmI == Main.myPlayer &&
						player.ownedProjectileCounts[ModContent.ProjectileType<EntelechiaScytheS3Aura>()] <= 0) {
						Projectile.NewProjectile(player.GetSource_ItemUse(player.HeldItem),
							player.Bottom, Vector2.Zero,
							ModContent.ProjectileType<EntelechiaScytheS3Aura>(), 0, 0, player.whoAmI);
					}
				}
				return false;
			}

			// 二技能「绯红壁合」激活期间，左键改为挥出循环斩，替代普通攻击
			if (mp.Skill == 1 && mp.SkillActive)
				return player.ownedProjectileCounts[ModContent.ProjectileType<EntelechiaScytheCircleSlashProjectile>()] <= 0;

			return base.CanUseItem(player);
		}

		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			if (Main.myPlayer != player.whoAmI)
				return false;

			var mp = player.GetModPlayer<WeaponPlayer>();
			if (mp.Skill == 1 && mp.SkillActive) {
				// S2「绯红壁合」：循环斩每次命中造成 195% 攻击力
				int projId = Projectile.NewProjectile(source, player.Center, Vector2.Zero, ModContent.ProjectileType<EntelechiaScytheCircleSlashProjectile>(), (int)(damage * 1.95f), knockback, player.whoAmI);
				if (projId >= 0 && projId < Main.maxProjectiles)
					Main.projectile[projId].scale *= Item.scale;
			}
			else if (mp.Skill == 2 && mp.SkillActive) {
				// S3「灵与欲的惜别」：攻击变为放大版第二段刀光，飞行更快但距离很短、范围更大
				// （+135% 攻击力由 ModifyWeaponDamage 提供，已含在 damage 内）
				// ai2=2 → 单次伤害判定（每个目标只命中一次，不因停留而重复打）
				SpawnWave(source, player.Center, velocity * 1.6f, type, damage, knockback, player,
					scale: 1.6f, flatten: 1f, lifeMul: 0.35f, ai2: 2f);
			}
			else if (mp.EntelechiaEmpoweredNext) {
				// S1 强化普攻：175% 伤害、一前一后两段放大刀光
				mp.EntelechiaEmpoweredNext = false;
				int empDamage = (int)(damage * 1.75f);

				// 第一下立即发射：在第二下基础上左右收缩变“扁”，飞行略慢、距离更远（ai2=1 → 双重伤害）
				SpawnWave(source, player.Center, velocity * 0.82f, type, empDamage, knockback, player,
					scale: 1.5f, flatten: 0.55f, lifeMul: 1.6f, ai2: 1f);

				// 第二下延迟发射：更大、正常形状与速度（由 WeaponPlayer 倒计时触发）
				mp.EntelechiaPendingVel    = velocity;
				mp.EntelechiaPendingDamage = empDamage;
				mp.EntelechiaPendingKb     = knockback;
				mp.EntelechiaSecondShotDelay = 10;
			}
			else {
				Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);
			}

			return false;
		}

		// 生成一段刀光：ai[0]=扁平度（垂直收缩），ai[1]=寿命/距离倍率，
		// ai[2]=命中模式（0 普通、1 双重伤害(S1)、2 单次伤害(S3)）；scale 控制整体大小
		private static void SpawnWave(EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity,
			int type, int damage, float knockback, Player player, float scale, float flatten, float lifeMul,
			float ai2 = 0f) {
			int id = Projectile.NewProjectile(source, position, velocity, type, damage, knockback,
				player.whoAmI, flatten, lifeMul, ai2);
			if (id >= 0 && id < Main.maxProjectiles)
				Main.projectile[id].scale = scale;
		}
	}
}
