using System.Collections.Generic;
using ArknightsMod.Content.Items.Weapons;
using ArknightsMod.Content.Projectiles.Supporter.Pramanix;
using ArknightsMod.Players;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using ArknightsMod.Content;

namespace ArknightsMod.Content.Items.Weapons.Supporter.Pramanix
{
	public class SaintBell : UpgradeWeaponBase
	{
		private static SoundStyle SkillActiveSound;
		private static SoundStyle NoSound;

		public override void Load() {
			SkillActiveSound = new SoundStyle("ArknightsMod/Sounds/SkillActive1") {
				Volume = 0.45f,
				MaxInstances = 3,
			};
			NoSound = new SoundStyle("ArknightsMod/Sounds/NoSound") {
				Volume = 0f,
				MaxInstances = 4,
			};
			PramanixSoundHelper.Load();
		}

		public override void SetDefaults() {
			Item.damage = 48;
			Item.DamageType = DamageClass.Magic;
			Item.mana = 12;
			Item.width = 32;
			Item.height = 32;
			Item.useTime = 32;
			Item.useAnimation = 32;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.knockBack = 0f;
			Item.value = Item.sellPrice(gold: 2);
			Item.rare = ItemRarityID.LightPurple;
			Item.noMelee = true;
			Item.autoReuse = true;
			Item.noUseGraphic = true;
			Item.UseSound = SoundID.Item28;
			Item.shoot = ModContent.ProjectileType<PramanixShockwaveRing>();
			Item.shootSpeed = 0f;
		}

		public override void HoldStyle(Player player, Rectangle heldItemFrame) { }

		public override void UseStyle(Player player, Rectangle heldItemFrame) { }

		public override void ModifyTooltips(List<TooltipLine> tooltips) {
			tooltips.RemoveAll(line => line.Name == "CritChance");
		}

		public override bool AltFunctionUse(Player player) => false;

		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			if (ArknightsKeybinds.SkillActivatePressed(player))
				return false;

			var sb = player.GetModPlayer<SaintBellPlayer>();
			var wp = player.GetModPlayer<WeaponPlayer>();

			if (wp.Skill == 1 && sb.Skill2Active) {
				sb.SpawnSkill2Shockwave();
				return false;
			}

			if (wp.Skill == 2 && sb.Skill3Active) {
				sb.SpawnSkill3Volley();
				return false;
			}

			return false;
		}

		public override bool CanUseItem(Player player) {
			if (Main.myPlayer != player.whoAmI)
				return base.CanUseItem(player);

			var wp = player.GetModPlayer<WeaponPlayer>();
			var sb = player.GetModPlayer<SaintBellPlayer>();

			if (ArknightsKeybinds.SkillActivatePressed(player)) {
				if (wp.Skill == 1 && sb.Skill2Active) {
					sb.DeactivateSkill2();
					return false;
				}
				if (wp.Skill == 2 && sb.Skill3Active)
					return false;

				if (wp.Skill == 1) {
					if (wp.StockCount <= 0 || wp.SkillActive)
						return false;
					sb.ActivateSkill2();
					PramanixSoundHelper.PlaySkill2Activate(player.Center);
					return false;
				}

				if (wp.Skill == 2) {
					if (wp.StockCount <= 0 || wp.SkillActive)
						return false;
					sb.ActivateSkill3();
					PramanixSoundHelper.PlaySkill3Activate(player.Center);
					return false;
				}

				// 一技能（wp.Skill == 0）
				if (wp.StockCount <= 0 || wp.SkillActive)
					return false;

				SoundEngine.PlaySound(SkillActiveSound, player.Center);
				sb.ActivateSkill1();
				return false;
			}

			bool skillVolleyReady = sb.SkillVolleyTimer <= 0;
			if (wp.Skill == 1 && sb.Skill2Active && skillVolleyReady) {
				Item.UseSound = NoSound;
				return base.CanUseItem(player);
			}

			if (wp.Skill == 2 && sb.Skill3Active && skillVolleyReady) {
				Item.UseSound = NoSound;
				return base.CanUseItem(player);
			}

			return false;
		}
	}
}
