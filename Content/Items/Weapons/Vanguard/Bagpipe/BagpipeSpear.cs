using ArknightsMod.Content.Items.Material;
using ArknightsMod.Content.Projectiles.Vanguard.Bagpipe;
using ArknightsMod.Content.Rarities;
using ArknightsMod.Content.Tiles.Infrastructure;
using ArknightsMod.Players;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using ArknightsMod.Content;

namespace ArknightsMod.Content.Items.Weapons.Vanguard.Bagpipe
{
	public class BagpipeSpear : UpgradeWeaponBase
	{
		#region 音频加载
		private static SoundStyle SkillActive1;
		private static SoundStyle SkillActive2;
		public override void Load() {
			SkillActive1 = new SoundStyle("ArknightsMod/Sounds/SkillActive1") {
				Volume = 0.4f,
				MaxInstances = 4,
			};
			SkillActive2 = new SoundStyle("ArknightsMod/Sounds/SkillActive2") {
				Volume = 1f,
				MaxInstances = 4,
			};
		}
		#endregion

		public override bool MeleePrefix() => true;
		public override void AddRecipes() {
			CreateRecipe()
				.AddIngredient(ItemID.JoustingLance, 1)
				.AddIngredient<PolymerizationPreparation>(4)
				.AddIngredient<OrirockConcentration>(9)
				.AddTile(ModContent.TileType<FactoryTile>())
				.Register();
		}
		public override void SetDefaults() {
			Item.damage = 118;           // 攻击力
			Item.DamageType = DamageClass.Melee;
			Item.width = 90;            // 丢出体积
			Item.height = 96;           // 丢出体积
			Item.scale = 1;             // 图片缩放
			Item.useTime = 30;          // 使用一次时间 
			Item.useAnimation = 30;     // 动画显示时间
			Item.knockBack = 2f;        // 击退
			Item.value = 200000;        // 价格 
			Item.rare = 11;
			Item.autoReuse = true;      // 是否可以连续使用
			Item.noMelee = true;        // 贴图是否造成伤害
			Item.shoot = 88;
			Item.crit = 4;              // 暴击概率
			Item.shootSpeed = 9;        // 弹幕射速
			Item.useTurn = false;
			Item.noUseGraphic = true;
			Item.useStyle = ItemUseStyleID.Rapier;
			Item.channel = true;
		}

		// 初动（初始化技能数值）
		/*public void InitializeSkillStats() {
			var modPlayer = Main.LocalPlayer.GetModPlayer<WeaponPlayer>();
			globalItem.PrimaryCharge = 15;
			globalItem.PowerStrikeCharge = 0;
			globalItem.UltimateCharge = 75;
			globalItem.PrimaryChargeMax = 35;
			globalItem.PowerStrikeChargeMax = 4;
			globalItem.UltimateChargeMax = 90;
			globalItem.Duration1 = 35;
			globalItem.Duration2 = -1;
			globalItem.Duration3 = 30;
			//if (globalItem.UpgradeLevel == 1) Item.damage = 60;
			if (globalItem.UpgradeLevel == 2) {
				// Item.damage = 70;
				globalItem.PowerStrikeStacks = 1;
				globalItem.PrimaryCharge += 6;
				globalItem.PowerStrikeCharge += 2;
				globalItem.UltimateCharge += 6;
			}
		}*/

		public override void HoldItemFrame(Player player) {
			var modPlayer = player.GetModPlayer<WeaponPlayer>();

			if (modPlayer.Skill == 2 && modPlayer.SkillActive)
				player.AddBuff(ModContent.BuffType<BagpipeDefenseBuff>(), 5);
		}
		public override bool AltFunctionUse(Player player) => false;
		public override bool CanUseItem(Player player) {
			var modPlayer = player.GetModPlayer<WeaponPlayer>();

			if (Main.myPlayer == player.whoAmI) {

				if (modPlayer.Skill == 2 && modPlayer.SkillActive)
					player.AddBuff(ModContent.BuffType<BagpipeDefenseBuff>(), 35);

				if (ArknightsKeybinds.SkillActivatePressed(player)) {
					if (!modPlayer.SummonMode) {
						//s1
						if (modPlayer.Skill == 0 && modPlayer.StockCount > 0 && !modPlayer.SkillActive) {
							player.controlUseItem = false;
							modPlayer.SkillActive = true;
							modPlayer.SkillTimer = 0;
							modPlayer.DelStockCount();
							SoundEngine.PlaySound(SkillActive1, player.position);
						}
						//s3
						else if (modPlayer.Skill == 2 && modPlayer.StockCount > 0 && !modPlayer.SkillActive) {
							modPlayer.SkillActive = true;
							modPlayer.SkillTimer = 0;
							modPlayer.DelStockCount();
							SoundEngine.PlaySound(SkillActive2, player.position);
						}
						else
							return false;
					}
				}
				else {
					if (!modPlayer.SummonMode) {
						// S1
						if (modPlayer.Skill == 0) {
							if (modPlayer.StockCount == 0) {
								modPlayer.OffensiveRecovery(); //自动回复
							}
						}
						//S2
						if (modPlayer.Skill == 1 && !modPlayer.SkillActive) {
							if (modPlayer.StockCount == 0) {
								modPlayer.OffensiveRecovery();
							}
							else if (modPlayer.StockCount > 0) //自动触发
							{
								modPlayer.SkillActive = true;
								modPlayer.SkillTimer = 0;
								modPlayer.DelStockCount();
							}
						}
						//s3
						if (modPlayer.Skill == 2 && !modPlayer.SkillActive) {
							if (modPlayer.StockCount == 0) {
								modPlayer.OffensiveRecovery();
							}
						}
					}
				}
			}
			return base.CanUseItem(player);
		}
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {

			var modPlayer = player.GetModPlayer<WeaponPlayer>();
			if (!modPlayer.SkillActive) {
				NormalAttack(player, source, position, velocity, type, damage, knockback);
			}
			else {
				switch (modPlayer.Skill) {
					case 0:	// 1技能
						Skill_1Attack(player, source, position, velocity, type, damage, knockback);
						break;

					case 1: //2技能
						Skill_2Attack(player, source, position, velocity, type, damage, knockback);
						break;

					case 2: // 3技能
						Skill_3Attack(player, source, position, velocity, type, damage, knockback);
						break;
				}
			}
			return false;
		}
		public void NormalAttack(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {

			var soundNormalAttack = new SoundStyle("ArknightsMod/Content/Items/Weapons/Vanguard/Bagpipe/BagpipeAttack1") {
				MaxInstances = 4
			};
			SoundEngine.PlaySound(soundNormalAttack, player.position);

			if (Main.rand.Next(1, 101) > 28) // 天赋
			{
				Projectile.NewProjectile(source, position,
					velocity, ModContent.ProjectileType<BagpipeSpearProj2>(), damage, knockback, Main.myPlayer, 0, 2, 0);
			}
			else {
				Projectile.NewProjectile(source, position,
					velocity, ModContent.ProjectileType<BagpipeSpearProj2>(), (int)(damage * 1.3f), knockback, Main.myPlayer, 0, 2, 0);
				Projectile.NewProjectile(source, position - new Vector2(0, -5),
					velocity / 1.3f, ModContent.ProjectileType<BagpipeSpearProj3>(), (int)(damage * 1.3f), knockback, Main.myPlayer, 0, 1, 0);
			}
		}
		public void Skill_1Attack(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {

			var soundNormalAttack = new SoundStyle("ArknightsMod/Content/Items/Weapons/Vanguard/Bagpipe/BagpipeAttack1") {
				MaxInstances = 4
			};
			SoundEngine.PlaySound(soundNormalAttack, player.position);
			player.itemTime =
			player.itemAnimation = (int)(player.itemAnimation * (100f / 145f));

			if (Main.rand.Next(1, 101) > 28) // 天赋
			{
				Projectile.NewProjectile(source, position,
					velocity * 1.45f, ModContent.ProjectileType<BagpipeSpearProj2>(), (int)(damage * 1.45f), knockback, Main.myPlayer, 0, 3, 4);
			}
			else {
				Projectile.NewProjectile(source, position,
					velocity * 1.45f, ModContent.ProjectileType<BagpipeSpearProj2>(), (int)(damage * 1.45f * 1.3f), knockback, Main.myPlayer, 0, 3, 4);
				Projectile.NewProjectile(source, position - new Vector2(0, -5),
					velocity / 1.3f, ModContent.ProjectileType<BagpipeSpearProj3>(), (int)(damage * 1.45f * 1.3f), knockback, Main.myPlayer, 0, 1, 0);
			}
		}
		public void Skill_2Attack(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {

			var soundSkill2 = new SoundStyle("ArknightsMod/Content/Items/Weapons/Vanguard/Bagpipe/BagpipeAttack2") {
				MaxInstances = 4
			};
			SoundEngine.PlaySound(soundSkill2, player.position);

			if (Main.rand.Next(1, 101) > 28) // 天赋
			{
				Projectile.NewProjectile(source, position,
					velocity, ModContent.ProjectileType<BagpipeSpearProj2>(), (int)(damage * 2f), knockback, Main.myPlayer, 0, 2, 0);
			}
			else {
				Projectile.NewProjectile(source, position,
					velocity, ModContent.ProjectileType<BagpipeSpearProj2>(), (int)(damage * 2f * 1.3f), knockback, Main.myPlayer, 0, 2, 0);
				Projectile.NewProjectile(source, position - new Vector2(0, -5),
					velocity / 1.3f, ModContent.ProjectileType<BagpipeSpearProj3>(), (int)(damage * 2f * 1.3f), knockback, Main.myPlayer, 0, 1, 0);
			}

			if (Main.rand.Next(1, 101) > 28) // 天赋
			{
				Projectile.NewProjectile(source, position - new Vector2(0, -5),
					velocity / 1.3f, ModContent.ProjectileType<BagpipeSpearProj3>(), (int)(damage * 2f), knockback, Main.myPlayer, 0, 1, 0);
			}
			else {
				Projectile.NewProjectile(source, position - new Vector2(0, -5),
					velocity / 1.3f, ModContent.ProjectileType<BagpipeSpearProj3>(), (int)(damage * 2f * 1.3f), knockback, Main.myPlayer, 0, 1, 0);
				Projectile.NewProjectile(source, position - new Vector2(0, -5),
					velocity / 1.3f, ModContent.ProjectileType<BagpipeSpearProj3>(), (int)(damage * 2f * 1.3f), knockback, Main.myPlayer, 0, 1, 0);
			}
		}
		public void Skill_3Attack(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {

			var soundSkill3 = new SoundStyle("ArknightsMod/Content/Items/Weapons/Vanguard/Bagpipe/BagpipeAttack3") {
				MaxInstances = 4
			};
			SoundEngine.PlaySound(soundSkill3, player.position);
			player.AddBuff(ModContent.BuffType<BagpipeDefenseBuff>(), 35);

			player.itemTime =
			player.itemAnimation = (int)(player.itemAnimation * 1.5f);

			if (Main.rand.Next(1, 101) > 28) // 天赋
			{
				Projectile.NewProjectile(source, position,
					velocity / 1.3f, ModContent.ProjectileType<BagpipeSpearProj2>(), (int)(damage * 2.2f), knockback, Main.myPlayer, 0, 10, 3);
			}
			else {
				Projectile.NewProjectile(source, position,
					velocity / 1.3f, ModContent.ProjectileType<BagpipeSpearProj2>(), (int)(damage * 2.2f * 1.3f), knockback, Main.myPlayer, 0, 10, 3);
				Projectile.NewProjectile(source, position - new Vector2(0, -5),
					velocity / 1.4f, ModContent.ProjectileType<BagpipeSpearProj3>(), (int)(damage * 2.2f * 1.3f), knockback, Main.myPlayer, 0, 1, 0);
			}

			if (Main.rand.Next(1, 101) > 28) // 天赋
			{
				Projectile.NewProjectile(source, position - new Vector2(0, -5),
					velocity / 1.4f, ModContent.ProjectileType<BagpipeSpearProj3>(), (int)(damage * 2.2f), knockback, Main.myPlayer, 0, 1, 0);
			}
			else {
				Projectile.NewProjectile(source, position - new Vector2(0, -5),
					velocity / 1.4f, ModContent.ProjectileType<BagpipeSpearProj3>(), (int)(damage * 2.2f * 1.3f), knockback, Main.myPlayer, 0, 1, 0);
				Projectile.NewProjectile(source, position - new Vector2(0, -5),
					velocity / 1.4f, ModContent.ProjectileType<BagpipeSpearProj3>(), (int)(damage * 2.2f * 1.3f), knockback, Main.myPlayer, 0, 1, 0);
			}

			if (Main.rand.Next(1, 101) > 28) // 天赋
			{
				Projectile.NewProjectile(source, position - new Vector2(0, -5),
					velocity / 1.3f, ModContent.ProjectileType<BagpipeSpearProj3>(), (int)(damage * 2.2f), knockback, Main.myPlayer, 0, 1, 0);
			}
			else {
				Projectile.NewProjectile(source, position - new Vector2(0, -5),
					velocity / 1.3f, ModContent.ProjectileType<BagpipeSpearProj3>(), (int)(damage * 2.2f * 1.3f), knockback, Main.myPlayer, 0, 1, 0);
				Projectile.NewProjectile(source, position - new Vector2(0, -5),
					velocity / 1.3f, ModContent.ProjectileType<BagpipeSpearProj3>(), (int)(damage * 2.2f * 1.3f), knockback, Main.myPlayer, 0, 1, 0);
			}
		}
	}
}