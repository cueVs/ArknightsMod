using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Systems.Gameplay.Damage
{
	/// <summary>
	/// 法术伤害/法抗系统。
	///
	/// 历史备注：这里原来是一套用 MonoMod 直接改写原版字节码（IL_NPC.GetIncomingStrikeModifiers /
	/// IL_NPC.HitModifiers.GetDamage 等）的实现，想让一次攻击同时按物理/法术/元素/真实四条轨道
	/// 分别结算。那套代码从来没有真正启用过（Load/Unload 整段被注释掉），审查后发现里面有个
	/// 把 DamageCategoryNPC 引用直接塞进 DamageClass 类型参数槽位、绕过类型检查的操作，
	/// 风险是整个战斗管线级别的崩溃，所以整段替换成了下面这套用 tModLoader 官方 Hook
	/// （ModifyHitByItem/ModifyHitByProjectile）实现的版本：只做"法术伤害吃目标法抗"这一件事，
	/// 不做多类型伤害同时结算，但足够覆盖当前的实际需求，而且完全在受支持的 API 范围内。
	/// </summary>
	public class DamageCategoryNPC : GlobalNPC
	{
		public override bool InstancePerEntity => true;

		/// <summary>法术抗性，0~1 的比例。命中来自 <see cref="ArtsWeaponRegistry"/> 的攻击时，
		/// 按 FinalDamage *= (1 - artsResistance) 直接扣减（和 WeaknessSystem 里的减益写法同一套公式）。</summary>
		public float artsResistance;

		public override void ModifyHitByItem(NPC npc, Player player, Item item, ref NPC.HitModifiers modifiers) {
			if (artsResistance > 0f && IsArtsItemHit(player, item))
				modifiers.FinalDamage *= (1f - artsResistance);
		}

		public override void ModifyHitByProjectile(NPC npc, Projectile projectile, ref NPC.HitModifiers modifiers) {
			// 弹幕算不算法伤，在它生成那一刻就判定好并记在 ArtsProjectileMarker 上了，这里只是读结果。
			if (artsResistance > 0f && projectile.GetGlobalProjectile<ArtsProjectileMarker>().IsArtsDamage)
				modifiers.FinalDamage *= (1f - artsResistance);
		}

		/// <summary>物品直接命中（近战挥砍/戳刺）算不算法伤：原版武器查固定表，模组武器走带条件的规则。</summary>
		private static bool IsArtsItemHit(Player player, Item item) {
			if (ArtsWeaponRegistry.VanillaArtsItemTypes.Contains(item.type))
				return true;

			ArtsWeaponRule rule = ArtsWeaponRegistry.GetRule(item.type);
			return rule != null && rule.DirectHit(player);
		}
	}

	/// <summary>
	/// 按地区给原版怪物赋默认法抗：邪恶群系（腐化/猩红）20，困难模式额外 +20；
	/// 其余地区（森林等）默认 0。只在生成那一刻取一次附近玩家所在地区，不做逐帧跟随——
	/// 这是"这只怪物是在什么环境里生成的"的一次性快照，不是"这只怪物当前站在哪"。
	///
	/// 只处理原版怪物（npc.ModNPC == null）。本模组自己的干员化敌人已经在各自 SetDefaults
	/// 里手动设过法抗（参见 Caster.cs/Hound.cs/Evolution.cs 等 ~20 个文件），这里不会碰它们。
	/// </summary>
	public class ArtsResistanceZoneDefaults : GlobalNPC
	{
		private const float EvilBiomeResistance = 0.20f;
		private const float EvilBiomeResistanceHardmode = 0.40f;

		// 显式指定的原版怪物，优先于地区默认值。
		private static readonly Dictionary<int, float> ExplicitOverrides = new() {
			{ NPCID.KingSlime, 0.30f },
			{ NPCID.PossessedArmor, 0.40f },
		};

		public override void OnSpawn(NPC npc, IEntitySource source) {
			if (npc.ModNPC != null)
				return;

			var cat = npc.GetGlobalNPC<DamageCategoryNPC>();

			if (ExplicitOverrides.TryGetValue(npc.type, out float explicitValue)) {
				cat.artsResistance = explicitValue;
				return;
			}

			cat.artsResistance = GetZoneDefault(npc.Center);
		}

		private static float GetZoneDefault(Vector2 position) {
			Player nearest = FindNearestPlayer(position);
			if (nearest == null)
				return 0f;

			if (nearest.ZoneCorrupt || nearest.ZoneCrimson)
				return Main.hardMode ? EvilBiomeResistanceHardmode : EvilBiomeResistance;

			return 0f;
		}

		private static Player FindNearestPlayer(Vector2 position) {
			Player nearest = null;
			float nearestDistSq = float.MaxValue;
			foreach (Player player in Main.ActivePlayers) {
				float distSq = Vector2.DistanceSquared(player.Center, position);
				if (distSq < nearestDistSq) {
					nearestDistSq = distSq;
					nearest = player;
				}
			}
			return nearest;
		}
	}
}
