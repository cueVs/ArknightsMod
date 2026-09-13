using ArknightsMod.Content.Items.Weapons;
using ArknightsMod.Content.Projectiles.Guard.Hongdou;
using ArknightsMod.Players;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using ArknightsMod.Content;

namespace ArknightsMod.Content.Items.Weapons.Guard.Hongdou
{
	public class HongdouLance : ExpansionWeaponBase
	{
		protected override int[] EliteDamage => [44, 52, 60];

		private static SoundStyle SkillActiveSfx;

		public override void Load() {
			SkillActiveSfx = new SoundStyle("ArknightsMod/Sounds/SkillActive1") { Volume = 0.5f, MaxInstances = 2 };
		}

		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			ItemID.Sets.Spears[Item.type] = true;
			ItemID.Sets.SkipsInitialUseSound[Item.type] = true;
		}

		public override void SetDefaults() {
			Item.damage = EliteDamage[0];
			Item.DamageType = DamageClass.Melee;
			Item.width = 56;
			Item.height = 58;
			Item.useTime = 22;
			Item.useAnimation = 22;
			Item.knockBack = 5f;
			Item.value = Item.sellPrice(silver: 40);
			Item.rare = ItemRarityID.Green;
			Item.autoReuse = true;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.noMelee = true;
			Item.noUseGraphic = true;
			Item.shoot = ModContent.ProjectileType<HongdouSpearStab>();
			Item.shootSpeed = 3f;
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
				}
				return false;
			}

			// S2 槌音：攻击间隔增大
			if (mp.SkillActive && mp.Skill == 1) {
				Item.useTime = 33;
				Item.useAnimation = 33;
			} else {
				Item.useTime = 22;
				Item.useAnimation = 22;
			}

			return player.ownedProjectileCounts[Item.shoot] < 1 && base.CanUseItem(player);
		}

		public override void ModifyWeaponDamage(Player player, ref StatModifier damage) {
			base.ModifyWeaponDamage(player, ref damage);
			var mp = player.GetModPlayer<WeaponPlayer>();
			if (mp.SkillActive) {
				if (mp.Skill == 0)
					damage *= 1.8f;
				else if (mp.Skill == 1)
					damage *= 3.0f;
			}
		}
	}
}
