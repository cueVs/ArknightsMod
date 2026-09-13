using ArknightsMod.Content.Projectiles.Guard.Melantha;
using ArknightsMod.Players;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using ArknightsMod.Content;

namespace ArknightsMod.Content.Items.Weapons.Guard.Melantha
{
	public class MelanthasSword : ExpansionWeaponBase
	{
		protected override int[] EliteDamage => [67, 83];

		private static SoundStyle SkillActiveSfx;

		public override void Load() {
			SkillActiveSfx = new SoundStyle("ArknightsMod/Sounds/SkillActive1") {
				Volume = 0.5f,
				MaxInstances = 2,
			};
		}

		public override void SetDefaults() {
			Item.damage = EliteDamage[0];
			Item.DamageType = DamageClass.Melee;
			Item.width = 46;
			Item.height = 48;
			Item.useTime = 22;
			Item.useAnimation = 22;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.knockBack = 5f;
			Item.value = Item.sellPrice(silver: 30);
			Item.rare = ItemRarityID.Blue;
			Item.autoReuse = true;
			Item.noMelee = true;
			Item.noUseGraphic = true;          // 挥砍特效由弹幕绘制，不显示武器贴图
			Item.shoot = ProjectileType<MelanthaSlash>();
			Item.shootSpeed = 6f;
			Item.crit = 4;
		}

		public override bool AltFunctionUse(Player player) => false;

		private int segment;   // 0=向下挥砍(白)，1=上挑挥砍(粉)，每次攻击交替

		public override bool CanUseItem(Player player) {
			var mp = player.GetModPlayer<WeaponPlayer>();

			// 右键：激活已充能的技能
			if (ArknightsKeybinds.SkillActivatePressed(player)) {
				if (mp.StockCount > 0 && !mp.SkillActive) {
					mp.SkillActive = true;
					mp.SkillTimer = 0;
					mp.DelStockCount();
					SoundEngine.PlaySound(SkillActiveSfx, player.Center);
				}
				return false;
			}

			// 左键：仅当场上没有挥砍弹幕时才能再次挥砍，确保每段动作完整不跳帧
			return player.ownedProjectileCounts[ProjectileType<MelanthaSlash>()] <= 0;
		}

		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			Projectile.NewProjectile(source, player.Center, velocity,
				ProjectileType<MelanthaSlash>(), damage, knockback, player.whoAmI, segment);
			segment ^= 1;   // 两段交替：白↔粉
			return false;
		}

		// 技能伤害用乘区叠加在精英化基础之上，避免与基类的精英化加成重复计算
		public override void ModifyWeaponDamage(Player player, ref StatModifier damage) {
			base.ModifyWeaponDamage(player, ref damage);
			var mp = player.GetModPlayer<WeaponPlayer>();
			if (mp.SkillActive && mp.Skill == 0)
				damage *= 1.5f;   // S1 技能激活：攻击力+50%
		}
	}
}
