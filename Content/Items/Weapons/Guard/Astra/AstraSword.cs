using ArknightsMod.Content;
using ArknightsMod.Content.Items.Weapons;
using ArknightsMod.Content.Projectiles.Guard.Astra;
using ArknightsMod.Players;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Weapons.Guard.Astra
{
	/// <summary>
	/// 星辉剑（AstraSword）：常态表现移植自 ASTES1A（星极）的示范武器 Taurus ——
	/// 剑身不直接造成伤害，每次挥砍发射"Excalibur 式蓝色能量弧光"（AstraSwordSlash），
	/// 视觉效果与 Taurus 完全一致。
	///
	/// 技能：
	///   一技能「星座守护」（Skill 0）：手动开启，技力 10/30，持续 30 秒。
	///       开启期间攻击力 +50%、防御力 +80%。
	///   二技能「星辉剑」（Skill 1）：手动开启，技力 10/20，持续 15 秒。
	///       开启期间攻击力 +80%、防御力 +80%，武器与弧光弹幕放大至 130%，
	///       并在玩家身后召唤一个跟随移动的星辉光环（AstraSwordAura，64×64），
	///       技能结束或切换武器时消失。
	/// </summary>
	public class AstraSword : UpgradeWeaponBase
	{
		private static SoundStyle skillSound;

		public override void Load() {
			skillSound = new SoundStyle("ArknightsMod/Sounds/SkillActive1") {
				Volume = 0.5f,
				MaxInstances = 2
			};
		}

		public override void Unload() => skillSound = default;

		public override void SetDefaults() {
			Item.useStyle = ItemUseStyleID.Swing;   // 挥砍动作（同 Taurus）
			Item.useAnimation = 20;
			Item.useTime = 20;
			Item.damage = 72;
			Item.DamageType = DamageClass.Melee;
			Item.knockBack = 4.5f;
			Item.width = 40;
			Item.height = 40;
			Item.scale = 1f;
			Item.UseSound = SoundID.Item1;
			Item.rare = ItemRarityID.Lime;
			Item.value = Item.buyPrice(gold: 23);
			Item.shoot = ModContent.ProjectileType<AstraSwordSlash>();
			Item.noMelee = true;            // 剑身本身不造成伤害，只由弧光弹幕承担（同 Taurus）
			Item.shootsEveryUse = true;     // 每次挥砍都生成弹幕（自动连发按住时同样生效）
			Item.autoReuse = true;
		}

		public override bool AltFunctionUse(Player player) => false;

		public override bool CanUseItem(Player player) {
			if (Main.myPlayer != player.whoAmI)
				return base.CanUseItem(player);

			var wp = player.GetModPlayer<WeaponPlayer>();

			// 技能键按下 → 激活当前选中的技能（一/二技能共用激活流程）
			if (ArknightsKeybinds.SkillActivatePressed(player)) {
				if (wp.StockCount > 0 && !wp.SkillActive) {
					wp.SkillActive = true;
					wp.SkillTimer = 0;
					wp.DelStockCount();
					if (!Main.dedServ)
						SoundEngine.PlaySound(skillSound, player.Center);
				}
				return false;
			}

			return base.CanUseItem(player);
		}

		// 二技能「星辉剑」激活期间：
		//  · 武器模型放大到 130%（Item.scale 抬高后，Shoot 里 GetAdjustedItemScale(Item)
		//    传给弹幕的 ai[2] 缩放同步增大 → 弧光视觉与扇形判定一并放大）；
		//  · 本地玩家召唤背后跟随的星辉光环（光环的消失由弹幕自行逐帧判定）。
		public override void HoldItem(Player player) {
			var wp = player.GetModPlayer<WeaponPlayer>();
			Item.scale = (wp.SkillActive && wp.Skill == 1) ? 1.3f : 1f;

			if (player.whoAmI == Main.myPlayer && wp.SkillActive && wp.Skill == 1)
				EnsureAura(player);
		}

		private static void EnsureAura(Player player) {
			int type = ModContent.ProjectileType<AstraSwordAura>();
			if (player.ownedProjectileCounts[type] <= 0)
				Projectile.NewProjectile(player.GetSource_FromThis(), player.Center,
					Vector2.Zero, type, 0, 0f, player.whoAmI);
		}

		// 攻击力加成：星座守护（Skill 0）+50%；星辉剑（Skill 1）+80%
		public override void ModifyWeaponDamage(Player player, ref StatModifier damage) {
			var wp = player.GetModPlayer<WeaponPlayer>();
			if (!wp.SkillActive)
				return;

			if (wp.Skill == 0)
				damage *= 1.5f;
			else if (wp.Skill == 1)
				damage *= 1.8f;
		}

		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			var wp = player.GetModPlayer<WeaponPlayer>();

			// ── 技能攻击分派点 ─────────────────────────────────────────────
			// 一技能「星座守护」（Skill==0）：仅数值加成，攻击形态不变。
			// 二技能「星辉剑」（Skill==1）：伤害加成见 ModifyWeaponDamage；
			//       武器/弹幕 130% 缩放见 HoldItem（经 ai[2] 传导）。
			// 如需让某技能改变弹幕形态，可在此按 wp.Skill 分支或传 mode 进弹幕。
			// ──────────────────────────────────────────────────────────────

			// 与 Taurus.Shoot 相同的发射参数协议：
			// ai[0] = 方向（含重力翻转），ai[1] = 挥砍总时长，ai[2] = 物品缩放
			float adjustedItemScale = player.GetAdjustedItemScale(Item);
			Projectile.NewProjectile(source, player.MountedCenter, new Vector2(player.direction, 0f),
				ModContent.ProjectileType<AstraSwordSlash>(), damage, knockback, player.whoAmI,
				player.direction * player.gravDir, player.itemAnimationMax, adjustedItemScale);

			// 多人模式同步使用状态（同 Taurus）
			NetMessage.SendData(MessageID.PlayerControls, number: player.whoAmI);

			return false;
		}

		public override void AddRecipes() {
			CreateRecipe()
				.AddIngredient(ItemID.HallowedBar, 12)
				.AddIngredient(ItemID.SoulofLight, 5)
				.AddTile(TileID.MythrilAnvil)
				.Register();
		}
	}

	/// <summary>
	/// 星辉剑专属玩家效果：技能开启期间防御力 +80%
	/// （星座守护与星辉剑两个技能的防御加成相同，共用此判断）。
	/// PostUpdateEquips 在所有装备的防御累加之后执行，此处乘算加成最可靠。
	/// </summary>
	public class AstraSwordPlayer : ModPlayer
	{
		public override void PostUpdateEquips() {
			var wp = Player.GetModPlayer<WeaponPlayer>();
			if (Player.HeldItem?.ModItem is AstraSword && wp.SkillActive
				&& (wp.Skill == 0 || wp.Skill == 1))
				Player.statDefense += (int)(Player.statDefense * 0.8f); // 防御力 +80%
		}
	}
}
