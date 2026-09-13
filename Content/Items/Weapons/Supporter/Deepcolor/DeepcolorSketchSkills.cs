using System;
using ArknightsMod.Content.Projectiles.Supporter.Deepcolor;
using ArknightsMod.Players;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Weapons.Supporter.Deepcolor
{
	public static class DeepcolorSketchSkills
	{
		public const float ShadowTentacleDamageMult = 1.06f;
		public const float ShadowTentacleDefenseMult = 1.06f;
		public const int ShadowTentacleRegenPerSecond = 7;
		// 二技能额外攻击半径（格）
		public const float VisualTrapRangeBonusTiles = 5f;
		public const float VisualTrapDodgeChance = 0.5f;
		public const float VisualTrapDrawScale = 1.5f;

		public static bool IsHoldingDeepcolorSketch(Player player)
			=> player.HeldItem.ModItem is DeepcolorSketch;

		public static bool IsSkillActive(Player player, int skillIndex) {
			if (!IsHoldingDeepcolorSketch(player))
				return false;
			var wp = player.GetModPlayer<WeaponPlayer>();
			return wp.Skill == skillIndex && wp.SkillActive;
		}

		public static bool ShadowTentacleActive(Player player) => IsSkillActive(player, 0);
		public static bool VisualTrapActive(Player player) => IsSkillActive(player, 1);

		public static float GetAttackRadiusPx(Player owner) {
			float radius = DeepcolorMinion.BaseAttackRangeRadiusPx;
			if (VisualTrapActive(owner))
				radius += VisualTrapRangeBonusTiles * 16f;
			return radius;
		}

		// 以触手中心为圆心：左→上→右半圆弧；脚底以下不索；正右及同平台略偏下可索
		public static bool IsInAttackRangeAt(Projectile tentacle, Vector2 targetCenter, Player owner) {
			Vector2 offset = targetCenter - tentacle.Center;
			float dist = offset.Length();
			float radius = GetAttackRadiusPx(owner);

			if (dist > radius)
				return false;

			if (targetCenter.Y > tentacle.Bottom.Y + 8f)
				return false;

			if (dist <= 4f)
				return true;

			float angle = MathF.Atan2(offset.Y, offset.X);
			// 上半弧：angle∈(-π,0]；正左：angle≈π
			if (angle <= 0f)
				return true;
			if (angle >= MathF.PI - 0.35f)
				return true;
			// 右前方：怪物中心常略低于触手中心，原先 angle∈(0,π) 会漏掉正右
			return offset.X > 0f && angle <= 0.85f;
		}

		public static bool IsOwnerInAnyTentacleAttackRange(Player owner) {
			if (!VisualTrapActive(owner))
				return false;

			int type = ModContent.ProjectileType<DeepcolorMinion>();
			for (int i = 0; i < Main.maxProjectiles; i++) {
				Projectile proj = Main.projectile[i];
				if (!proj.active || proj.owner != owner.whoAmI || proj.type != type)
					continue;

				if (IsInAttackRangeAt(proj, owner.Center, owner))
					return true;
			}

			return false;
		}
	}
}
