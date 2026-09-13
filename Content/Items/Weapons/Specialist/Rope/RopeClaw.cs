using System.Collections.Generic;
using System.Linq;
using ArknightsMod.Content.Items.Weapons;
using ArknightsMod.Content.Projectiles.Specialist.Rope;
using ArknightsMod.Players;
using ArknightsMod.Systems.Gameplay.Skill;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using ArknightsMod.Content;

namespace ArknightsMod.Content.Items.Weapons.Specialist.Rope
{
	// 暗索的钩爪：链鞭类武器。
	// 普通攻击挥出钩链，命中把敌人拉向玩家（拉到近处）。
	// S1 勾爪发射：攻击充能，下次鞭击 ×1.9 并把敌人强力拉至面前。
	// S2 复式勾爪：攻击充能，右键即时向 2 个最远的敌人发射抓钩 ×2.25 拖拽至面前。
	public class RopeClaw : ExpansionWeaponBase
	{
		protected override int[] EliteDamage => [44, 53, 64];

		private static SoundStyle SkillActiveSfx;

		private const float HookSpeed = 22f;      // S2 抓钩飞出速度
		private const float S2SearchRange = 900f; // S2 搜索最远敌人的范围

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
			Item.width = 46;
			Item.height = 46;
			Item.useTime = 32;
			Item.useAnimation = 32;
			Item.knockBack = 4f;
			Item.value = Item.sellPrice(silver: 35);
			Item.rare = ItemRarityID.Green;
			Item.autoReuse = true;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.noMelee = true;
			Item.noUseGraphic = true;
			Item.shoot = ModContent.ProjectileType<RopeWhipProjectile>();
			Item.shootSpeed = 6f;
			Item.crit = 4;
		}

		public override bool AltFunctionUse(Player player) => false;

		public override bool CanUseItem(Player player) {
			var mp = player.GetModPlayer<WeaponPlayer>();

			if (ArknightsKeybinds.SkillActivatePressed(player)) {
				// S2 复式勾爪：即时向 2 个最远敌人发射抓钩
				if (mp.Skill == 1 && mp.StockCount > 0 && !mp.SkillActive) {
					mp.DelStockCount();
					SoundEngine.PlaySound(SkillActiveSfx, player.Center);
					FireDoubleHooks(player, 2.25f);
				}
				return false;
			}

			// 同时只允许一根钩链；base 检查精英阶段等条件
			if (player.ownedProjectileCounts[Item.shoot] >= 1 || !base.CanUseItem(player))
				return false;

			// 当前所选技能为攻击充能型 → 本次攻击充能
			if (mp.CurrentSkill?.ChargeType == SkillChargeType.Attack && !mp.SkillActive)
				mp.OffensiveRecovery();

			return true;
		}

		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			var mp = player.GetModPlayer<WeaponPlayer>();

			bool empowered = false;
			// S1 勾爪发射：消耗库存，本次鞭击 ×1.9 并强力拉拽
			if (mp.Skill == 0 && mp.StockCount > 0) {
				mp.DelStockCount();
				damage = (int)(damage * 1.9f);
				empowered = true;
			}

			int p = Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);
			if (empowered && p >= 0 && p < Main.maxProjectiles
				&& Main.projectile[p].ModProjectile is RopeWhipProjectile whip)
				whip.Empowered = true;

			return false;
		}

		// S2：朝 2 个最远敌人各发射一枚抓钩
		private void FireDoubleHooks(Player player, float dmgMul) {
			if (Main.myPlayer != player.whoAmI) return;
			int dmg = (int)(player.GetWeaponDamage(Item) * dmgMul);

			var targets = FindFarthestTargets(player, 2);
			if (targets.Count == 0) {
				SpawnHook(player, Main.MouseWorld, dmg);
				return;
			}
			foreach (NPC npc in targets)
				SpawnHook(player, npc.Center, dmg);
		}

		private void SpawnHook(Player player, Vector2 target, int dmg) {
			Vector2 dir = (target - player.Center).SafeNormalize(new Vector2(player.direction, 0f));
			Projectile.NewProjectile(
				player.GetSource_ItemUse(Item),
				player.Center,
				dir * HookSpeed,
				ModContent.ProjectileType<RopeHookProjectile>(),
				dmg, Item.knockBack, player.whoAmI);
		}

		// 找出范围内最远的若干个敌人
		private static List<NPC> FindFarthestTargets(Player player, int count) {
			return Main.npc
				.Where(npc => npc.active && !npc.friendly && !npc.dontTakeDamage
					&& npc.CanBeChasedBy()
					&& Vector2.Distance(npc.Center, player.Center) <= S2SearchRange)
				.OrderByDescending(npc => Vector2.Distance(npc.Center, player.Center))
				.Take(count)
				.ToList();
		}
	}
}
