using ArknightsMod.Content.Items.Weapons;
using ArknightsMod.Content.Projectiles.Guard.Estelle;
using ArknightsMod.Players;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using ArknightsMod.Content;

namespace ArknightsMod.Content.Items.Weapons.Guard.Estelle
{
	public class EstelleGlove : ExpansionWeaponBase
	{
		protected override int[] EliteDamage => [39, 47, 57];

		private static SoundStyle SkillActiveSfx;

		public override void Load() {
			SkillActiveSfx = new SoundStyle("ArknightsMod/Sounds/SkillActive1") { Volume = 0.5f, MaxInstances = 2 };
		}

		public override void SetDefaults() {
			Item.damage = EliteDamage[0];
			Item.DamageType = DamageClass.Melee;
			Item.width = 44;
			Item.height = 44;
			Item.useTime = 22;
			Item.useAnimation = 22;
			Item.knockBack = 5f;
			Item.value = Item.sellPrice(silver: 35);
			Item.rare = ItemRarityID.Green;
			Item.autoReuse = true;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.noMelee = true;
			Item.noUseGraphic = true;
			Item.shoot = ModContent.ProjectileType<EstelleGlovePunch>();
			Item.shootSpeed = 16f;
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

			return base.CanUseItem(player);
		}

		public override void ModifyWeaponDamage(Player player, ref StatModifier damage) {
			base.ModifyWeaponDamage(player, ref damage);
			var mp = player.GetModPlayer<WeaponPlayer>();
			if (!mp.SkillActive) return;
			damage *= mp.Skill switch {
				0 => 1.8f,   // S1 攻击力+80%
				1 => 2.5f,   // S2 舍身突击 攻击力+150%
				_ => 1f
			};
		}

		public override void HoldItem(Player player) {
			base.HoldItem(player);
			var mp = player.GetModPlayer<WeaponPlayer>();
			// S2 舍身突击：禁止生命回复
			if (mp.SkillActive && mp.Skill == 1)
				player.lifeRegen = System.Math.Min(player.lifeRegen, 0);
		}
	}
}
