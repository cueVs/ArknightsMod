using System;
using System.Collections.Generic;
using ArknightsMod.Content;
using ArknightsMod.Content.Buffs.Supporter.Deepcolor;
using ArknightsMod.Content.Items.Weapons;
using ArknightsMod.Content.Projectiles.Supporter.Deepcolor;
using ArknightsMod.Players;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Weapons.Supporter.Deepcolor
{
	public class DeepcolorSketch : UpgradeWeaponBase
	{
		private static SoundStyle SkillActiveSound;

		public override void Load() {
			SkillActiveSound = new SoundStyle("ArknightsMod/Sounds/SkillActive1") {
				Volume = 0.4f,
				MaxInstances = 4,
			};
		}

		public override void SetDefaults() {
			Item.maxStack = 1;
			Item.damage = 46;
			Item.DamageType = DamageClass.Summon;
			Item.mana = 10;
			Item.width = 32;
			Item.height = 32;
			Item.useTime = 30;
			Item.useAnimation = 30;
			Item.useStyle = ItemUseStyleID.HoldUp;
			Item.noUseGraphic = true;
			Item.knockBack = 2f;
			Item.value = Item.sellPrice(silver: 50);
			Item.rare = ItemRarityID.Green;
			Item.noMelee = true;
			Item.autoReuse = true;
			Item.UseSound = SoundID.Item44;
			Item.shoot = ModContent.ProjectileType<DeepcolorMinion>();
			Item.shootSpeed = 0f;
			Item.buffType = ModContent.BuffType<DeepcolorMinionBuff>();
		}

		// 左右键功能对调：左键 LOGO 攻击（弹幕另按魔法伤害单独结算），右键部署触手（原来是反过来）。
		public override bool AltFunctionUse(Player player) => true;

		public override string GetSkillActivateKeyHint()
			=> Language.GetTextValue("Mods.ArknightsMod.Items.DeepcolorSketch.SkillActivateKey", ArknightsKeybinds.SkillActivateKeyDisplay);

		public override void ModifyTooltips(List<TooltipLine> tooltips) {
			tooltips.Add(new TooltipLine(Mod, "DeepcolorSketchSkillKey",
				Language.GetTextValue("Mods.ArknightsMod.Items.DeepcolorSketch.SkillActivateKey", ArknightsKeybinds.SkillActivateKeyDisplay)));
		}

		public override bool CanUseItem(Player player) {
			if (player.altFunctionUse == 2)
				return player.GetModPlayer<DeepcolorSketchPlayer>().CanRedeploy;

			// 左键 LOGO 攻击：弹幕还在场（IsLogoChargeActive）时不允许打出下一次
			return !player.GetModPlayer<DeepcolorSketchPlayer>().IsLogoChargeActive && base.CanUseItem(player);
		}

		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			var sketchPlayer = player.GetModPlayer<DeepcolorSketchPlayer>();

			if (player.altFunctionUse == 2) {
				// 右键：部署触手
				if (DeepcolorMinion.CountActiveForPlayer(player) >= DeepcolorMinion.MaxTentacles)
					DeepcolorMinion.TryDespawnOldestForPlayer(player);

				player.AddBuff(Item.buffType, 2);
				Vector2 spawnPos = DeepcolorMinion.FindGroundSpawnPosition(Main.MouseWorld, DeepcolorMinion.FrameWidth, DeepcolorMinion.FrameHeight);
				Projectile.NewProjectile(source, spawnPos, Vector2.Zero, type, damage, knockback, player.whoAmI);
				sketchPlayer.StartRedeployCooldown();
				return false;
			}

			// 左键：LOGO 攻击。伤害按魔法伤害类型单独结算（弹幕本身是 DamageClass.Magic），
			// 不能直接用传入的 damage——那是按 Item.DamageType（Summon）算出来的召唤伤害。
			sketchPlayer.LogoStrikeWorld = Main.MouseWorld;
			int logoDamage = (int)Math.Round(player.GetDamage(DamageClass.Magic).ApplyTo(Item.damage));
			Projectile.NewProjectile(source, player.Center, Vector2.Zero,
				ModContent.ProjectileType<DeepcolorSketchLogoAttack>(), logoDamage, player.GetWeaponKnockback(Item), player.whoAmI);
			return false;
		}

		internal static bool TryActivateSkill(Player player) {
			if (Main.myPlayer != player.whoAmI)
				return false;

			var modPlayer = player.GetModPlayer<WeaponPlayer>();
			if (modPlayer.StockCount <= 0 || modPlayer.SkillActive)
				return false;

			modPlayer.SkillActive = true;
			modPlayer.SkillTimer = 0;
			modPlayer.DelStockCount();
			SoundEngine.PlaySound(SkillActiveSound, player.Center);
			return true;
		}
	}
}
