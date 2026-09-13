using ArknightsMod.Content.Items.Weapons;
using ArknightsMod.Content.Projectiles.Guard.Mousse;
using ArknightsMod.Players;
using ArknightsMod.Systems.Gameplay.Skill;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using ArknightsMod.Content;

namespace ArknightsMod.Content.Items.Weapons.Guard.Mousse
{
	public class MousseGlove : ExpansionWeaponBase
	{
		protected override int[] EliteDamage => [43, 52, 62];

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
			Item.knockBack = 4f;
			Item.value = Item.sellPrice(silver: 35);
			Item.rare = ItemRarityID.Green;
			Item.autoReuse = true;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.noMelee = true;
			Item.noUseGraphic = true;
			Item.shoot = ModContent.ProjectileType<MousseGlovePunch>();
			Item.shootSpeed = 16f;
			Item.crit = 4;
		}

		public override bool AltFunctionUse(Player player) => false;

		public override void ModifyHitNPC(Player player, NPC target, ref NPC.HitModifiers modifiers) {
			var mp = player.GetModPlayer<WeaponPlayer>();
			if (mp.SkillActive && mp.Skill == 0) {
				// S1 挠伤：命中目标施加虚弱，技能立即结束（一次性）
				target.AddBuff(BuffID.Weak, 5 * 60);
				mp.SkillActive = false;
			}
		}

		public override bool CanUseItem(Player player) {
			var mp = player.GetModPlayer<WeaponPlayer>();

			if (ArknightsKeybinds.SkillActivatePressed(player)) {
				// S1 已改为自动释放，技能键对它不生效；此处只保留 S2 的手动开启
				if (mp.Skill == 1 && mp.StockCount > 0 && !mp.SkillActive) {
					mp.SkillActive = true;
					mp.SkillTimer = 0;
					mp.DelStockCount();
					SoundEngine.PlaySound(SkillActiveSfx, player.Center);
				}
				return false;
			}

			// S1 挠伤：攻击充能
			if (mp.CurrentSkill?.ChargeType == SkillChargeType.Attack && mp.Skill == 0)
				mp.OffensiveRecovery();

			return base.CanUseItem(player);
		}

		public override void ModifyWeaponDamage(Player player, ref StatModifier damage) {
			base.ModifyWeaponDamage(player, ref damage);
			var mp = player.GetModPlayer<WeaponPlayer>();
			if (mp.SkillActive)
				damage *= 1.75f; // S1/S2 攻击力 +75%
		}

		public override void HoldItem(Player player) {
			base.HoldItem(player);
			var mp = player.GetModPlayer<WeaponPlayer>();
			// S1「挠伤」改为自动释放：蓄满后下一次攻击自动变为强化（×1.75 并施加虚弱），无需手动按键。
			// !mp.SkillActive 充当"已上膛"标记，命中后拳击弹幕 OnHitNPC 会置 false，待重新蓄满才会再次自动上膛，
			// 天然防止每帧重复触发。
			if (mp.Skill == 0 && mp.StockCount > 0 && !mp.SkillActive) {
				mp.SkillActive = true;
				mp.SkillTimer = 0;
				mp.DelStockCount();
				SoundEngine.PlaySound(SkillActiveSfx, player.Center);
			}
			// S2 炸毛：防御 +75%
			if (mp.SkillActive && mp.Skill == 1)
				player.statDefense += (int)((int)player.statDefense * 0.75f);
		}
	}
}
