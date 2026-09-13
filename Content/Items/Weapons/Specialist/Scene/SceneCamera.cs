using System;
using System.Collections.Generic;
using ArknightsMod.Content;
using ArknightsMod.Content.Buffs.Specialist.Scene;
using ArknightsMod.Content.Items.Weapons;
using ArknightsMod.Content.Projectiles.Specialist.Scene;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Weapons.Specialist.Scene
{
	// 稀音的摄像机：
	// 左键释放魔法攻击（消耗魔法）；右键召唤「移动摄影车」（5 秒冷却、最多 5 辆、占用仆从位）。
	// 接入技力/技能系统：技能栏可选 技能一/二，用 Down(S)+右键 释放选中技能。
	public class SceneCamera : UpgradeWeaponBase
	{
		// 摄影车召唤伤害基数（召唤系加成基于此）。
		public const int TruckBaseDamage = 24;

		public override void SetDefaults() {
			Item.width = 34;
			Item.height = 26;
			Item.useTime = 30;
			Item.useAnimation = 30;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.noMelee = true;
			Item.noUseGraphic = true;
			Item.knockBack = 0f;
			Item.value = Item.sellPrice(silver: 50);
			Item.rare = ItemRarityID.Green;
			Item.UseSound = SoundID.Item44;
			Item.autoReuse = true;
			Item.DamageType = DamageClass.Magic; // 左键普通攻击为魔法伤害
			Item.damage = 24;
			Item.mana = 8;                        // 左键消耗魔法值（右键部署经 ModifyManaCost 免除）
			Item.shoot = ModContent.ProjectileType<SceneCameraBullet>();
			Item.shootSpeed = 11f;
			Item.buffType = ModContent.BuffType<CameraTruckBuff>();
		}

		// 升级预览伤害用固定值，避免 weaponData 未初始化。
		protected override int GetDamage(int level) => Item.damage;

		public override string GetSkillActivateKeyHint()
			=> Language.GetTextValue("Mods.ArknightsMod.Items.SceneCamera.SkillActivateKey", ArknightsKeybinds.SkillActivateKeyDisplay);

		// 左键：魔法攻击；右键：召唤摄影车；技能开启键：释放选中的技能。
		public override bool AltFunctionUse(Player player) => true;

		public override bool CanUseItem(Player player) {
			// 技能开启键：只释放选中技能，绝不召唤。原来是"Down(S)+右键"的组合，
			// 现在统一挪到独立热键，不再占用右键——右键单独按下改为纯粹的"召唤摄影车"。
			if (ArknightsKeybinds.SkillActivatePressed(player)) {
				player.GetModPlayer<SceneCameraPlayer>().TryActivateSelectedSkillFromInput();
				return false;
			}
			if (player.altFunctionUse == 2) {
				return player.GetModPlayer<SceneCameraPlayer>().CanRedeploy; // 右键受 5 秒冷却
			}
			return true;
		}

		// 仅左键自动连发，右键部署需逐次点击。
		public override bool? CanAutoReuseItem(Player player) => player.altFunctionUse != 2;

		// 右键部署不消耗魔法，仅左键消耗。
		public override void ModifyManaCost(Player player, ref float reduce, ref float mult) {
			if (player.altFunctionUse == 2)
				mult = 0f;
		}

		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			if (player.altFunctionUse == 2)
				return DeployTruck(player, source, knockback);

			// 左键：朝光标方向开火。
			Vector2 dir = (Main.MouseWorld - player.Center).SafeNormalize(Vector2.UnitX);

			// 圆环快门特效（纯视觉）。
			const float ringForwardOffset = 22f;
			Vector2 ringCenter = player.Center + dir * ringForwardOffset;
			Projectile.NewProjectile(source, ringCenter, dir * 4f,
				ModContent.ProjectileType<SceneCameraShot>(), 0, 0f, player.whoAmI);

			// 抛物线魔法弹丸：求解初速度，使其在重力下恰好命中光标位置。
			Vector2 muzzle = player.Center + dir * 16f;
			Vector2 launchVel = SceneCameraBullet.ComputeLaunchVelocity(muzzle, Main.MouseWorld);
			Projectile.NewProjectile(source, muzzle, launchVel,
				ModContent.ProjectileType<SceneCameraBullet>(), damage, knockback, player.whoAmI);
			return false;
		}

		private bool DeployTruck(Player player, EntitySource_ItemUse_WithAmmo source, float knockback) {
			GetDeployStats(player, out int current, out int effectiveMax, out _, out _);
			if (effectiveMax <= 0)
				return false; // 没有可用仆从位

			if (current >= effectiveMax)
				CameraTruck.TryRemoveOldestForPlayer(player); // 满员：消除最早的一辆，再在新位置部署

			SummonTruck(player, source, free: false);
			player.GetModPlayer<SceneCameraPlayer>().StartRedeployCooldown();
			return false;
		}

		// 在鼠标正下方地面召唤一辆摄影车。free=true 时不占仆从位（技能二额外召唤）。
		public static int SummonTruck(Player player, IEntitySource source, bool free) {
			player.AddBuff(ModContent.BuffType<CameraTruckBuff>(), 2);
			int summonDamage = (int)Math.Round(player.GetDamage(DamageClass.Summon).ApplyTo(TruckBaseDamage));
			Vector2 spawnPos = CameraTruck.FindGroundSpawnPosition(Main.MouseWorld, CameraTruck.Width, CameraTruck.Height);
			return Projectile.NewProjectile(source, spawnPos, Vector2.Zero,
				ModContent.ProjectileType<CameraTruck>(), summonDamage, 0f, player.whoAmI, free ? 1f : 0f);
		}

		// 当前可部署情况：受 5 辆硬上限与剩余仆从位共同约束（免位车计入数量但不占配额）。
		// 摄影车 minionSlots=0 不进 vanilla slotsMinions，这里手动把占位车计入 maxMinions 配额。
		private static void GetDeployStats(Player player, out int current, out int effectiveMax, out int remaining, out float otherSlots) {
			CameraTruck.CountForPlayer(player, out int total, out int slotUsing);
			current = total;
			otherSlots = player.slotsMinions;            // 其他（非摄影车）召唤物占用的仆从位
			int maxSlots = player.maxMinions;
			// 占位摄影车(slotUsing)各占 1 配额；免位车不占。剩余可再部署的配额：
			float freeForTrucks = Math.Max(0f, maxSlots - otherSlots - slotUsing);
			int slotCapForTrucks = total + (int)Math.Floor(freeForTrucks);
			effectiveMax = Math.Clamp(Math.Min(CameraTruck.MaxTrucks, slotCapForTrucks), 0, CameraTruck.MaxTrucks);
			remaining = Math.Max(0, effectiveMax - total);
		}

		public override void ModifyTooltips(List<TooltipLine> tooltips) {
			Player player = Main.LocalPlayer;
			GetDeployStats(player, out int current, out int effectiveMax, out int remaining, out float otherSlots);

			tooltips.Add(new TooltipLine(Mod, "SceneCameraDeploy",
				Language.GetTextValue("Mods.ArknightsMod.Items.SceneCamera.DeployInfo", current, effectiveMax, remaining)) {
				OverrideColor = new Color(130, 230, 120)
			});

			int otherInt = (int)Math.Round(otherSlots);
			if (otherInt > 0) {
				tooltips.Add(new TooltipLine(Mod, "SceneCameraSlotNote",
					Language.GetTextValue("Mods.ArknightsMod.Items.SceneCamera.DeploySlotNote", otherInt)) {
					OverrideColor = new Color(210, 190, 120)
				});
			}
		}
	}
}
