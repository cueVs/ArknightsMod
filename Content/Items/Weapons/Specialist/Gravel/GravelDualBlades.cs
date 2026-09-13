using ArknightsMod.Content.Items.Weapons;
using ArknightsMod.Content.Projectiles.Specialist.Gravel;
using ArknightsMod.Players;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using ArknightsMod.Content;

namespace ArknightsMod.Content.Items.Weapons.Specialist.Gravel
{
	public class GravelDualBlades : ExpansionWeaponBase
	{
		protected override int[] EliteDamage => [25, 30, 36];

		private static SoundStyle SkillActiveSfx;

		public override void Load() {
			SkillActiveSfx = new SoundStyle("ArknightsMod/Sounds/SkillActive1") { Volume = 0.5f, MaxInstances = 2 };
		}

		public override void SetDefaults() {
			Item.damage = EliteDamage[0];
			Item.DamageType = DamageClass.Melee;
			Item.width = 46;
			Item.height = 46;
			Item.useTime = 22;
			Item.useAnimation = 22;
			Item.knockBack = 3f;
			Item.value = Item.sellPrice(silver: 25);
			Item.rare = ItemRarityID.Green;
			Item.autoReuse = true;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.noMelee = true;
			Item.noUseGraphic = true;
			Item.shoot = ModContent.ProjectileType<GravelSlash>();
			Item.shootSpeed = 1f;
			Item.crit = 4;
		}

		public override bool AltFunctionUse(Player player) => false;

		public override bool CanUseItem(Player player) {
			var mp = player.GetModPlayer<WeaponPlayer>();

			if (ArknightsKeybinds.SkillActivatePressed(player)) {
				if (mp.StockCount > 0 && !mp.SkillActive) {
					mp.SkillActive = true;
					mp.SkillTimer = 0;
					mp.DelStockCount();
					SoundEngine.PlaySound(SkillActiveSfx, player.Center);

					if (mp.Skill == 1) {
						// S2 激活：初始化护盾值
						var sp = player.GetModPlayer<GravelShieldPlayer>();
						sp.ShieldMax = (int)(player.statLifeMax * 1.5f);
						sp.ShieldHp = sp.ShieldMax;

						// 生成护盾 VFX
						if (player.ownedProjectileCounts[ModContent.ProjectileType<GravelShieldVfx>()] <= 0)
							Projectile.NewProjectile(player.GetSource_FromThis(), player.Center, Vector2.Zero,
								ModContent.ProjectileType<GravelShieldVfx>(), 0, 0f, player.whoAmI);
					}
				}
				return false;
			}

			return player.ownedProjectileCounts[ModContent.ProjectileType<GravelSlash>()] <= 0
				&& base.CanUseItem(player);
		}

		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			Projectile.NewProjectile(source, player.Center, velocity,
				ModContent.ProjectileType<GravelSlash>(), damage, knockback, player.whoAmI);
			return false;
		}

		public override void HoldItem(Player player) {
			base.HoldItem(player);
			var mp = player.GetModPlayer<WeaponPlayer>();
			if (!mp.SkillActive) return;

			if (mp.Skill == 0) {
				// S1 影袭：防御+400% 随时间衰减（AutoUpdateActive=1 负责到时关闭）
				float duration = (mp.CurrentSkill?.CurrentLevelData.ActiveTime ?? 12f) * 60f;
				float progress = duration > 0f ? mp.SkillTimer / duration : 0f;
				float bonus = 4.0f * (1f - System.Math.Min(progress, 1f));
				player.statDefense *= (1f + bonus);
			} else if (mp.Skill == 1) {
				// S2 鼠群：维持护盾上限随时间锐减
				var sp = player.GetModPlayer<GravelShieldPlayer>();
				float duration = (mp.CurrentSkill?.CurrentLevelData.ActiveTime ?? 10f) * 60f;
				float remaining = 1f - System.Math.Min(mp.SkillTimer / duration, 1f);
				int shieldCap = (int)(sp.ShieldMax * remaining);

				// 护盾值只能随时间或伤害减少，不回升
				if (sp.ShieldHp > shieldCap) sp.ShieldHp = shieldCap;

				// 让 ResetEffects 不会清零（每帧写入 ShieldMax）
				sp.ShieldMax = (int)(player.statLifeMax * 1.5f);
			}
		}
	}
}
