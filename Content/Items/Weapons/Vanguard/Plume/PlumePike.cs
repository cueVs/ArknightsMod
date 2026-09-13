using ArknightsMod.Content.Items.Weapons;
using ArknightsMod.Content.Projectiles.Vanguard.Plume;
using ArknightsMod.Players;
using ArknightsMod.Systems.Gameplay.Skill;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using ArknightsMod.Content;

namespace ArknightsMod.Content.Items.Weapons.Vanguard.Plume
{
	// 翎羽的戈：长枪刺击。
	// S1 迅捷打击·α型（攻击充能，右键激活）：攻击力+25%，攻击速度+25。
	public class PlumePike : ExpansionWeaponBase
	{
		protected override int[] EliteDamage => [40, 52];

		private static SoundStyle SkillActiveSfx;

		private const int UseTimeNormal = 24;
		private const int UseTimeSkill  = 18; // 攻速+25 ≈ 25% 更快

		public override void Load() {
			SkillActiveSfx = new SoundStyle("ArknightsMod/Sounds/SkillActive1") { Volume = 0.5f, MaxInstances = 2 };
		}

		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			ItemID.Sets.Spears[Item.type] = true;
			ItemID.Sets.SkipsInitialUseSound[Item.type] = true;
		}

		public override void SetDefaults() {
			Item.damage       = EliteDamage[0];
			Item.DamageType   = DamageClass.Melee;
			Item.width        = 50;
			Item.height       = 52;
			Item.useTime      = UseTimeNormal;
			Item.useAnimation = UseTimeNormal;
			Item.knockBack    = 5f;
			Item.value        = Item.sellPrice(silver: 25);
			Item.rare         = ItemRarityID.Blue;
			Item.autoReuse    = true;
			Item.useStyle     = ItemUseStyleID.Shoot;
			Item.noMelee      = true;
			Item.noUseGraphic = true;
			Item.shoot        = ModContent.ProjectileType<PlumeSpearStab>();
			Item.shootSpeed   = 3f;
			Item.crit         = 4;
		}

		public override bool AltFunctionUse(Player player) => false;

		public override bool CanUseItem(Player player) {
			var mp = player.GetModPlayer<WeaponPlayer>();

			// 右键：激活技能
			if (ArknightsKeybinds.SkillActivatePressed(player)) {
				if (mp.StockCount > 0 && !mp.SkillActive) {
					mp.SkillActive = true;
					mp.SkillTimer  = 0;
					mp.DelStockCount();
					SoundEngine.PlaySound(SkillActiveSfx, player.Center);
				}
				return false;
			}

			// 攻击充能
			if (mp.CurrentSkill?.ChargeType == SkillChargeType.Attack && !mp.SkillActive)
				mp.OffensiveRecovery();

			// 技能激活期间：攻速+25
			Item.useTime      = mp.SkillActive ? UseTimeSkill : UseTimeNormal;
			Item.useAnimation = mp.SkillActive ? UseTimeSkill : UseTimeNormal;

			return player.ownedProjectileCounts[Item.shoot] < 1 && base.CanUseItem(player);
		}

		public override void ModifyWeaponDamage(Player player, ref StatModifier damage) {
			base.ModifyWeaponDamage(player, ref damage);
			var mp = player.GetModPlayer<WeaponPlayer>();
			if (mp.SkillActive)
				damage *= 1.25f; // 攻击力+25%
		}
	}
}
