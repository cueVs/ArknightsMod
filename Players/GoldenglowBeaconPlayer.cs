using ArknightsMod.Content.Buffs;
using ArknightsMod.Content.Projectiles.Caster.Goldenglow;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;

namespace ArknightsMod.Players
{
	// 维护浮游信标 BUFF 与场上浮游单元数量的同步，技能时效结束时清除多出的浮游单元
	public class GoldenglowBeaconPlayer : ModPlayer
	{
		// 场上由浮游单元发射、仍存在的魔法弹幕数量，用于限制弹幕堆叠上限
		public int BoltCount;

		public const int MaxBolts = 1;

		private int lastMaxBeacons = GoldenglowBeacon.BaseMaxBeacons;

		public override void ResetEffects() {
			int count = 0;
			foreach (Projectile proj in Main.ActiveProjectiles) {
				if (proj.active && proj.owner == Player.whoAmI
					&& proj.GetGlobalProjectile<GoldenglowBoltMarker>().IsGoldenglowBolt) {
					count++;
				}
			}
			BoltCount = count;
		}

		public override void PostUpdateBuffs() {
			int beaconType = ModContent.ProjectileType<GoldenglowBeacon>();
			int count = Player.ownedProjectileCounts[beaconType];

			int currentMax = GoldenglowBeacon.GetMaxBeacons(Player);
			int excess = currentMax < lastMaxBeacons ? count - currentMax : 0;
			if (excess > 0) {
				var beacons = new List<(Projectile proj, float spawnTick)>();
				foreach (Projectile proj in Main.ActiveProjectiles) {
					if (proj.type == beaconType && proj.owner == Player.whoAmI && proj.ModProjectile is GoldenglowBeacon beacon) {
						beacons.Add((proj, beacon.SpawnTick));
					}
				}
				// 按召唤时刻从新到旧排序，优先移除最新召唤的浮游单元
				beacons.Sort((a, b) => b.spawnTick.CompareTo(a.spawnTick));
				for (int i = 0; i < excess && i < beacons.Count; i++) {
					beacons[i].proj.Kill();
				}
				count = Player.ownedProjectileCounts[beaconType];
			}
			lastMaxBeacons = currentMax;

			int buffType = ModContent.BuffType<GoldenglowBeaconBuff>();
			if (count > 0) {
				Player.AddBuff(buffType, 60);
			}
			else if (Player.HasBuff(buffType)) {
				Player.DelBuff(Player.FindBuffIndex(buffType));
			}
		}
	}
}
