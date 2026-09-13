using ArknightsMod.Content.Items.Weapons;
using ArknightsMod.Players;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using ArknightsMod.Content;

namespace ArknightsMod.Content.Items.Weapons.Supporter.Beanstalk
{
	public class BeanstalkOrb : ExpansionWeaponBase
	{
		protected override int[] EliteDamage => [57, 64, 74];

		private static SoundStyle SkillActiveSfx;

		// S2 技能结束后 10 秒禁止攻击
		private int _s2CooldownTimer = 0;
		private bool _wasS2Active = false;

		public override void Load() {
			SkillActiveSfx = new SoundStyle("ArknightsMod/Sounds/SkillActive1") { Volume = 0.5f, MaxInstances = 2 };
		}

		public override void SetDefaults() {
			Item.damage = EliteDamage[0];
			Item.DamageType = DamageClass.Magic;
			Item.width = 28;
			Item.height = 28;
			Item.useTime = 37;
			Item.useAnimation = 37;
			Item.knockBack = 2f;
			Item.value = Item.sellPrice(silver: 40);
			Item.rare = ItemRarityID.Green;
			Item.autoReuse = true;
			Item.noMelee = true;
			Item.shoot = ProjectileID.MagicMissile;
			Item.mana = 11;
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

			// S2 结束后冷却期内禁止攻击
			if (_s2CooldownTimer > 0) return false;

			mp.OffensiveRecovery();

			// S1：提升攻击速度
			if (mp.SkillActive && mp.Skill == 0) {
				Item.useTime = 21;
				Item.useAnimation = 21;
			} else {
				Item.useTime = 37;
				Item.useAnimation = 37;
			}

			return base.CanUseItem(player);
		}

		public override void ModifyWeaponDamage(Player player, ref StatModifier damage) {
			base.ModifyWeaponDamage(player, ref damage);
			var mp = player.GetModPlayer<WeaponPlayer>();
			// S2 命运：攻击力 +100%
			if (mp.SkillActive && mp.Skill == 1)
				damage *= 2.0f;
		}

		public override bool Shoot(Player player, Terraria.DataStructures.EntitySource_ItemUse_WithAmmo source,
				Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			var mp = player.GetModPlayer<WeaponPlayer>();

			if (mp.SkillActive && mp.Skill == 1) {
				// S2：同时攻击范围内所有敌人（不发射弹幕，直接伤害）
				float range = 700f;
				bool hitAny = false;
				foreach (NPC npc in Main.npc) {
					if (!npc.active || npc.friendly || npc.dontTakeDamage) continue;
					if (Vector2.Distance(npc.Center, player.Center) > range) continue;
					int dir = npc.Center.X >= player.Center.X ? 1 : -1;
					npc.StrikeNPC(npc.CalculateHitInfo(damage, dir, false, knockback, DamageClass.Magic));
					hitAny = true;
				}
				return !hitAny; // 命中任意敌人则不再生成普通弹幕
			}

			return base.Shoot(player, source, position, velocity, type, damage, knockback);
		}

		public override void HoldItem(Player player) {
			base.HoldItem(player);
			var mp = player.GetModPlayer<WeaponPlayer>();

			// 检测 S2 刚刚结束 → 触发 10 秒冷却
			bool isS2Active = mp.SkillActive && mp.Skill == 1;
			if (_wasS2Active && !isS2Active)
				_s2CooldownTimer = 10 * 60;
			_wasS2Active = isS2Active;

			if (_s2CooldownTimer > 0)
				_s2CooldownTimer--;
		}
	}
}
