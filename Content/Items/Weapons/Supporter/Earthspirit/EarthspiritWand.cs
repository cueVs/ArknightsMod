using ArknightsMod.Content.Items.Weapons;
using ArknightsMod.Players;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using ArknightsMod.Content;

namespace ArknightsMod.Content.Items.Weapons.Supporter.Earthspirit
{
	public class EarthspiritWand : ExpansionWeaponBase
	{
		protected override int[] EliteDamage => [37, 57, 74];

		private static SoundStyle SkillActiveSfx;

		public override void Load() {
			SkillActiveSfx = new SoundStyle("ArknightsMod/Sounds/SkillActive1") { Volume = 0.5f, MaxInstances = 2 };
		}

		public override void SetDefaults() {
			Item.damage = EliteDamage[0];
			Item.DamageType = DamageClass.Magic;
			Item.width = 46;
			Item.height = 80;
			Item.useTime = 29;
			Item.useAnimation = 29;
			Item.knockBack = 2f;
			Item.value = Item.sellPrice(silver: 40);
			Item.rare = ItemRarityID.Green;
			Item.autoReuse = true;
			Item.noMelee = true;
			Item.shoot = ProjectileID.MagicMissile;
			Item.mana = 7;
			Item.crit = 4;
			Item.shootSpeed = 9f;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.staff[Item.type] = true;
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

			// S2 流沙化：停止攻击
			if (mp.SkillActive && mp.Skill == 1) return false;

			mp.OffensiveRecovery();
			return base.CanUseItem(player);
		}

		public override void ModifyWeaponDamage(Player player, ref StatModifier damage) {
			base.ModifyWeaponDamage(player, ref damage);
			var mp = player.GetModPlayer<WeaponPlayer>();
			// S1 攻击力强化·B型：攻击力 +80%
			if (mp.SkillActive && mp.Skill == 0)
				damage *= 1.8f;
		}

		public override void HoldItem(Player player) {
			base.HoldItem(player);
			var mp = player.GetModPlayer<WeaponPlayer>();
			// S2 流沙化：每 1.4 秒（84 tick）对附近敌人施加减速
			if (mp.SkillActive && mp.Skill == 1 && mp.SkillTimer % 84 == 0) {
				foreach (NPC npc in Main.npc) {
					if (!npc.active || npc.friendly) continue;
					if (Terraria.Utils.Distance(npc.Center, player.Center) < 300f)
						npc.AddBuff(BuffID.Slow, 90);
				}
			}
		}
	}
}
