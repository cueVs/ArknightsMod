using ArknightsMod.Content;
using ArknightsMod.Content.Items.Weapons;
using ArknightsMod.Content.Projectiles.Caster.Goldenglow;
using ArknightsMod.Players;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Weapons.Caster.Goldenglow
{
	public class GoldenglowWand : ExpansionWeaponBase
	{
		protected override int[] EliteDamage => [30, 36, 41];

		private static SoundStyle SkillActiveSfx;

		public override void Load() {
			SkillActiveSfx = new SoundStyle("ArknightsMod/Sounds/SkillActive1") { Volume = 0.5f, MaxInstances = 2 };
		}

		public override void SetDefaults() {
			Item.damage = EliteDamage[0];
			Item.DamageType = DamageClass.Magic;
			Item.width = 54;
			Item.height = 54;
			Item.useTime = 23;
			Item.useAnimation = 23;
			Item.knockBack = 2f;
			Item.value = Item.sellPrice(gold: 1);
			Item.rare = ItemRarityID.Green;
			Item.autoReuse = true;
			Item.noMelee = true;
			Item.shoot = ProjectileID.MagicMissile;  // 原版可引导导弹特效
			Item.mana = 8;
			Item.crit = 4;
			Item.shootSpeed = 10f;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.channel = true;
			Item.staff[Item.type] = true;
		}

		public override bool AltFunctionUse(Player player) => true;

		public override bool CanUseItem(Player player) {
			var mp = player.GetModPlayer<WeaponPlayer>();

			// 技能开启键：激活当前选中技能。原来是"下+右键"的组合，现在统一挪到独立热键，
			// 不再占用右键——右键单独按下改为纯粹的"部署浮游信标"，见下面的分支。
			if (ArknightsKeybinds.SkillActivatePressed(player)) {
				if (mp.StockCount > 0 && !mp.SkillActive) {
					mp.SkillActive = true;
					mp.SkillTimer = 0;
					mp.DelStockCount();
					SoundEngine.PlaySound(SkillActiveSfx, player.Center);
				}
				return false;
			}

			if (player.altFunctionUse == 2) {
				// 右键：在光标位置部署浮游信标，消耗魔法值；超过上限时移除最早召唤的一个
				if (player.CheckMana(Item.mana, pay: true)) {
					int beaconType = ModContent.ProjectileType<GoldenglowBeacon>();
					if (player.ownedProjectileCounts[beaconType] >= GoldenglowBeacon.GetMaxBeacons(player)) {
						Projectile oldest = null;
						float oldestTick = float.MaxValue;
						foreach (Projectile proj in Main.ActiveProjectiles) {
							if (proj.type == beaconType && proj.owner == player.whoAmI
								&& proj.ModProjectile is GoldenglowBeacon beacon && beacon.SpawnTick < oldestTick) {
								oldest = proj;
								oldestTick = beacon.SpawnTick;
							}
						}
						oldest?.Kill();
					}

					Projectile.NewProjectile(
						player.GetSource_ItemUse(Item),
						Main.MouseWorld,
						Vector2.Zero,
						beaconType,
						0, 0f, player.whoAmI);
				}
				return false;
			}

			// 左键弹幕已达堆叠上限时直接阻止本次使用，避免持续扣魔力却打不出新弹幕
			var beaconPlayer = player.GetModPlayer<GoldenglowBeaconPlayer>();
			if (beaconPlayer.BoltCount >= GoldenglowBeaconPlayer.MaxBolts)
				return false;

			return base.CanUseItem(player);
		}

		public override void ModifyTooltips(List<TooltipLine> tooltips) {
			Player player = Main.LocalPlayer;
			int count = player.ownedProjectileCounts[ModContent.ProjectileType<GoldenglowBeacon>()];
			int max = GoldenglowBeacon.GetMaxBeacons(player);
			tooltips.Add(new TooltipLine(Mod, "GoldenglowBeaconCount",
				Language.GetTextValue("Mods.ArknightsMod.Items.GoldenglowWand.BeaconCount", count, max)));
		}

		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			var beaconPlayer = player.GetModPlayer<GoldenglowBeaconPlayer>();
			if (beaconPlayer.BoltCount >= GoldenglowBeaconPlayer.MaxBolts)
				return false;

			int boltIndex = Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);
			if (boltIndex >= 0 && boltIndex < Main.maxProjectiles) {
				Main.projectile[boltIndex].GetGlobalProjectile<GoldenglowBoltMarker>().IsGoldenglowBolt = true;
			}
			return false;
		}

		public override void ModifyWeaponDamage(Player player, ref StatModifier damage) {
			base.ModifyWeaponDamage(player, ref damage);
			var mp = player.GetModPlayer<WeaponPlayer>();
			if (mp.SkillActive) {
				damage *= mp.Skill switch {
					0 => 1.4f,
					1 => 1.6f,
					2 => 1.8f,
					_ => 1f
				};
			}
		}
	}
}
