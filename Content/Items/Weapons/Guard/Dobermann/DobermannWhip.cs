using ArknightsMod.Content.Items.Weapons;
using ArknightsMod.Content.Projectiles.Guard.Dobermann;
using ArknightsMod.Players;
using ArknightsMod.Systems.Gameplay.Skill;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using ArknightsMod.Content;

namespace ArknightsMod.Content.Items.Weapons.Guard.Dobermann
{
	public class DobermannWhip : ExpansionWeaponBase
	{
		protected override int[] EliteDamage => [41, 52, 62];

		private static SoundStyle SkillActiveSfx;

		public override void Load() {
			SkillActiveSfx = new SoundStyle("ArknightsMod/Sounds/SkillActive1") { Volume = 0.5f, MaxInstances = 2 };
		}

		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			ItemID.Sets.SkipsInitialUseSound[Item.type] = true;
		}

		public override void SetDefaults() {
			Item.damage = EliteDamage[0];
			Item.DamageType = DamageClass.Melee;
			Item.width = 48;
			Item.height = 48;
			Item.useTime = 38;
			Item.useAnimation = 38;
			Item.knockBack = 4f;
			Item.value = Item.sellPrice(silver: 35);
			Item.rare = ItemRarityID.Green;
			Item.autoReuse = true;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.noMelee = true;
			Item.noUseGraphic = true;
			Item.shoot = ModContent.ProjectileType<DobermannWhipProjectile>();
			Item.shootSpeed = 6f;
			Item.crit = 4;
		}

		public override bool AltFunctionUse(Player player) => false;

		public override bool CanUseItem(Player player) {
			var mp = player.GetModPlayer<WeaponPlayer>();

			if (ArknightsKeybinds.SkillActivatePressed(player)) {
				// S2：右键激活持续技能
				if (mp.Skill == 1 && mp.StockCount > 0 && !mp.SkillActive) {
					mp.SkillActive = true;
					mp.SkillTimer = 0;
					mp.DelStockCount();
					SoundEngine.PlaySound(SkillActiveSfx, player.Center);
				}
				return false;
			}

			// 同时只允许一根鞭子；base 检查精英阶段等条件
			if (player.ownedProjectileCounts[Item.shoot] >= 1 || !base.CanUseItem(player))
				return false;

			// S1 强力击·B：确认本次攻击能发出后才充能，避免挥鞭中途重复点击多次充能
			if (mp.Skill == 0 && mp.CurrentSkill?.ChargeType == SkillChargeType.Attack && !mp.SkillActive)
				mp.OffensiveRecovery();

			return true;
		}

		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			var mp = player.GetModPlayer<WeaponPlayer>();
			// S1 强力击·B：消耗库存，本次鞭击伤害 ×2.30
			if (mp.Skill == 0 && mp.StockCount > 0) {
				mp.DelStockCount();
				damage = (int)(damage * 2.30f);
			}
			Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);
			return false;
		}

		public override void ModifyWeaponDamage(Player player, ref StatModifier damage) {
			base.ModifyWeaponDamage(player, ref damage);
			var mp = player.GetModPlayer<WeaponPlayer>();
			// S2 大幅提升攻击力
			if (mp.SkillActive && mp.Skill == 1)
				damage *= 2.0f;
		}
	}
}
