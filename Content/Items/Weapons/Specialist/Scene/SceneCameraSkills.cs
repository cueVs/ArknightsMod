using ArknightsMod.Content.Projectiles.Specialist.Scene;
using ArknightsMod.Players;
using ArknightsMod.Systems.Gameplay.Skill;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Weapons.Specialist.Scene
{
	// 稀音的摄像机技能效果查询助手（仿 DeepcolorSketchSkills）。
	// 技能效果统一以「手持稀音的摄像机」为生效门槛；具体激活状态 latch 在 SceneCameraPlayer。
	public static class SceneCameraSkills
	{
		public const float Skill1AttackBonus = 0.6f;    // 技能一：攻击 +60%
		public const float Skill2AttackBonus = 1.3f;    // 技能二：攻击 +130%
		public const float Skill2DefenseBonus = 1.3f;   // 技能二：防御 +130%
		public const float ReconRadiusPx = 160f;        // 侦查圈半径
		public const float ReconDamageBonus = 1.5f;     // 圈内敌人受到玩家伤害 +150%

		public static bool IsHolding(Player player) => player.HeldItem.ModItem is SceneCamera;

		public static bool Skill1Active(Player player)
			=> IsHolding(player) && player.GetModPlayer<SceneCameraPlayer>().Skill1Active;

		public static bool Skill2Active(Player player)
			=> IsHolding(player) && player.GetModPlayer<SceneCameraPlayer>().Skill2Timer > 0;

		public static bool BlocksSkillCharge(Player player) {
			if (!IsHolding(player))
				return false;
			var scene = player.GetModPlayer<SceneCameraPlayer>();
			var wp = player.GetModPlayer<WeaponPlayer>();
			if (wp.Skill == 0 && scene.Skill1Active)
				return true; // 一技能 latch
			if (wp.Skill == 1 && scene.Skill2Timer > 0)
				return true; // 二技能倒计时
			return false;
		}

		public static bool BlocksSkill1Charge(Player player) => BlocksSkillCharge(player);

		public static void MaintainSkill1ChargeState(Player player) {
			var wp = player.GetModPlayer<WeaponPlayer>();
			if (wp.Skill != 0 || wp.CurrentSkill == null)
				return;

			SkillLevelData data = wp.CurrentSkill.CurrentLevelData;
			wp.StockCount = data.MaxStack;
			wp.SkillCharge = 0;
			wp.SP = data.MaxSP;
			wp.SkillActive = true;
		}

		// 摄影车攻击倍率（技能一/二可叠加，乘法）。
		public static float AttackMult(Player player) {
			float m = 1f;
			if (Skill1Active(player)) m *= 1f + Skill1AttackBonus;
			if (Skill2Active(player)) m *= 1f + Skill2AttackBonus;
			return m;
		}

		// 摄影车防御倍率（技能二）。
		public static float DefenseMult(Player player) => Skill2Active(player) ? 1f + Skill2DefenseBonus : 1f;

		// 敌人是否处于该玩家任一摄影车的侦查圈内（仅技能二期间）。
		public static bool IsEnemyInReconZone(Player player, NPC npc) {
			if (!Skill2Active(player))
				return false;

			int type = ModContent.ProjectileType<CameraTruck>();
			float rSq = ReconRadiusPx * ReconRadiusPx;
			for (int i = 0; i < Main.maxProjectiles; i++) {
				Projectile p = Main.projectile[i];
				if (!p.active || p.owner != player.whoAmI || p.type != type)
					continue;
				if (Vector2.DistanceSquared(p.Center, npc.Center) <= rSq)
					return true;
			}
			return false;
		}
	}
}
