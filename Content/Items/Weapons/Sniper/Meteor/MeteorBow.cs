using ArknightsMod.Content.Items.Weapons;
using ArknightsMod.Content.Projectiles.Sniper.Meteor;
using ArknightsMod.Players;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using ArknightsMod.Content;

namespace ArknightsMod.Content.Items.Weapons.Sniper.Meteor
{
	public class MeteorBow : ExpansionWeaponBase
	{
		protected override int[] EliteDamage => [35, 42, 51];

		private static SoundStyle SkillActiveSfx;

		public override void Load() {
			SkillActiveSfx = new SoundStyle("ArknightsMod/Sounds/SkillActive1") { Volume = 0.5f, MaxInstances = 2 };
		}

		public override void SetDefaults() {
			Item.damage = EliteDamage[0];
			Item.DamageType = DamageClass.Ranged;
			Item.width = 24;
			Item.height = 56;
			Item.useTime = 30;
			Item.useAnimation = 30;
			Item.knockBack = 2f;
			Item.value = Item.sellPrice(silver: 30);
			Item.rare = ItemRarityID.Green;
			Item.autoReuse = true;
			Item.noMelee = true;
			Item.shoot = ProjectileID.WoodenArrowFriendly;
			Item.useAmmo = AmmoID.Arrow;
			Item.crit = 4;
			Item.shootSpeed = 12f;
			Item.useStyle = ItemUseStyleID.Shoot;
		}

		public override bool AltFunctionUse(Player player) => false;

		public override bool CanUseItem(Player player) {
			var mp = player.GetModPlayer<WeaponPlayer>();

			if (ArknightsKeybinds.SkillActivatePressed(player)) {
				// S1 已改为自动释放，技能键对其不生效；此处仅保留 S2 的手动开启
				if (mp.Skill == 1 && mp.StockCount > 0 && !mp.SkillActive) {
					mp.SkillActive = true;
					mp.SkillTimer = 0;
					mp.DelStockCount();
					SoundEngine.PlaySound(SkillActiveSfx, player.Center);

					// S2 碎甲击·扩散：立即对最近5个敌人造成伤害并碎甲
					int dmg = (int)(player.GetWeaponDamage(Item) * 2.0f);
					int hits = 0;
					// 按距离排序取最近5个
					foreach (NPC npc in Main.npc) {
						if (hits >= 5) break;
						if (!npc.active || npc.friendly || npc.dontTakeDamage) continue;
						if (Vector2.Distance(npc.Center, player.Center) > 700f) continue;
						int dir = npc.Center.X >= player.Center.X ? 1 : -1;
						npc.StrikeNPC(npc.CalculateHitInfo(dmg, dir, false, 2f, DamageClass.Ranged));
						npc.AddBuff(BuffID.BrokenArmor, 5 * 60);
						hits++;
					}
					mp.SkillActive = false;
				}
				return false;
			}

			mp.OffensiveRecovery();
			return base.CanUseItem(player);
		}

		public override void HoldItem(Player player) {
			base.HoldItem(player);
			var mp = player.GetModPlayer<WeaponPlayer>();
			// S1「箭矢强化」改为自动释放：蓄满后下一击自动射出强化箭（×1.8），无需手动按键。
			// !mp.SkillActive 充当"已上膛"标记，射出后 Shoot 会置 false，待重新蓄满才会再次自动上膛。
			if (mp.Skill == 0 && mp.StockCount > 0 && !mp.SkillActive) {
				mp.SkillActive = true;
				mp.SkillTimer = 0;
				mp.DelStockCount();
				SoundEngine.PlaySound(SkillActiveSfx, player.Center);
			}
		}

		public override void ModifyWeaponDamage(Player player, ref StatModifier damage) {
			base.ModifyWeaponDamage(player, ref damage);
			var mp = player.GetModPlayer<WeaponPlayer>();
			// S1：箭矢射出时造成 180% 伤害
			if (mp.SkillActive && mp.Skill == 0)
				damage *= 1.8f;
		}

		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source,
				Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			var mp = player.GetModPlayer<WeaponPlayer>();

			if (mp.SkillActive && mp.Skill == 0) {
				// 发射穿墙穿透箭，弹幕内部处理5目标上限和视野销毁
				Projectile.NewProjectile(source, position, velocity,
					ModContent.ProjectileType<MeteorArrow>(), damage, knockback, player.whoAmI);
				mp.SkillActive = false;
				return false; // 不再发射普通箭矢
			}

			return base.Shoot(player, source, position, velocity, type, damage, knockback);
		}
	}
}
