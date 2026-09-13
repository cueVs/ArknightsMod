using ArknightsMod.Players;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using ArknightsMod.Content;

namespace ArknightsMod.Content.Items.Weapons.Guard.Skadi
{
	public class SkadisGreatsword : ExpansionWeaponBase
	{
		protected override int[] EliteDamage => [78, 96, 113];

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
			Item.width = 94;
			Item.height = 92;
			Item.useTime = 40;
			Item.useAnimation = 40;
			Item.knockBack = 8f;
			Item.value = Item.sellPrice(gold: 1);
			Item.rare = ItemRarityID.Green;
			Item.autoReuse = true;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.crit = 4;
		}

		public override bool AltFunctionUse(Player player) => false;

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
			return base.CanUseItem(player);
		}

		// S1 激活时攻速提升约45%（对应原作动画时间 ×100/145）
		public override float UseSpeedMultiplier(Player player) {
			var mp = player.GetModPlayer<WeaponPlayer>();
			if (mp.SkillActive && mp.Skill == 0)
				return 1.45f;
			return 1f;
		}

		// 技能伤害倍率：在基类精英化加成之上以乘区叠加，避免与基础伤害重复计算
		public override void ModifyWeaponDamage(Player player, ref StatModifier damage) {
			base.ModifyWeaponDamage(player, ref damage);
			var mp = player.GetModPlayer<WeaponPlayer>();
			if (mp.SkillActive) {
				if (mp.Skill == 0)
					damage *= 1.45f;   // S1 攻击力+45%
				else if (mp.Skill == 1)
					damage *= 2.7f;    // S2 涌潮悲歌 攻击力+170%
				else if (mp.Skill == 2)
					damage *= 2.3f;    // S3 攻击力+130%
			}
		}

		// S3 激活时：防御力×2.3，最大生命值+130%
		public override void HoldItem(Player player) {
			base.HoldItem(player);
			var mp = player.GetModPlayer<WeaponPlayer>();
			if (mp.SkillActive && mp.Skill == 2) {
				player.statDefense *= 2.3f;
				player.statLifeMax2 += (int)(player.statLifeMax * 1.3f);
				player.statLife = player.statLifeMax2;
			}
		}
	}
}
