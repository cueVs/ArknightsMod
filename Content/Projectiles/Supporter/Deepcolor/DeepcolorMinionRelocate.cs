using System;
using ArknightsMod.Common.GlobalProjectiles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Projectiles.Supporter.Deepcolor
{
	public static class DeepcolorMinionRelocate
	{
		public static void UpdateRelocate(Projectile projectile, Player owner, bool isAttacking) {
			var relocate = projectile.Relocate();
			if (relocate.RelocateCooldown > 0)
				relocate.RelocateCooldown--;

			switch (relocate.Phase) {
				case DeepcolorRelocatePhase.Despawning:
					UpdateDespawning(projectile, relocate);
					return;
				case DeepcolorRelocatePhase.Spawning:
					UpdateSpawning(projectile, relocate);
					return;
			}

			if (isAttacking)
				return;

			TryStartRelocate(projectile, owner, relocate);
		}

		private static void UpdateDespawning(Projectile projectile, DeepcolorMinionRelocateGlobalProj relocate) {
			relocate.MoveProgress += DeepcolorMinionRelocateGlobalProj.RelocateMovePixelsPerTick;

			if (relocate.MoveProgress < relocate.MoveDistance)
				return;

			relocate.BeginSpawn(projectile);
			projectile.velocity = Vector2.Zero;
		}

		private static void UpdateSpawning(Projectile projectile, DeepcolorMinionRelocateGlobalProj relocate) {
			relocate.MoveProgress += DeepcolorMinionRelocateGlobalProj.RelocateMovePixelsPerTick;
			if (relocate.MoveProgress < relocate.MoveDistance)
				return;

			projectile.position = relocate.PendingPosition - new Vector2(projectile.width * 0.5f, 0f);
			projectile.velocity = Vector2.Zero;
			relocate.BeginStable(projectile);
		}

		private static void TryStartRelocate(Projectile projectile, Player owner, DeepcolorMinionRelocateGlobalProj relocate) {
			if (relocate.RelocateCooldown > 0)
				return;

			float distToPlayer = Vector2.Distance(projectile.Center, owner.Center);
			if (distToPlayer <= DeepcolorMinionRelocateGlobalProj.PlayerNearComfortPx) {
				relocate.FarFromPlayerTimer = 0;
				return;
			}

			Vector2? target = null;
			if (HasEnemyInPlayerVision(owner)) {
				relocate.FarFromPlayerTimer = 0;
				if (!HasEnemyInAttackRange(projectile, owner))
					target = FindSpawnNearPriorityEnemy(owner, projectile);
			}
			else {
				if (distToPlayer > DeepcolorMinionRelocateGlobalProj.PlayerFarDistancePx)
					relocate.FarFromPlayerTimer++;
				else
					relocate.FarFromPlayerTimer = 0;

				if (relocate.FarFromPlayerTimer >= DeepcolorMinionRelocateGlobalProj.PlayerFarDelayTicks)
					target = FindSpawnNearPlayer(owner, projectile, distToPlayer);
			}

			if (!target.HasValue)
				return;

			if (Vector2.Distance(projectile.Center, target.Value) < DeepcolorMinionRelocateGlobalProj.MinRelocateDistancePx)
				return;

			relocate.BeginDespawn(projectile, target.Value);
		}

		private static bool HasEnemyInPlayerVision(Player owner) {
			for (int i = 0; i < Main.maxNPCs; i++) {
				NPC npc = Main.npc[i];
				if (!npc.active || !npc.CanBeChasedBy(owner))
					continue;
				if (npc.Distance(owner.Center) <= DeepcolorMinionRelocateGlobalProj.EnemyVisionRadiusPx)
					return true;
			}
			return false;
		}

		private static bool HasEnemyInAttackRange(Projectile self, Player owner) {
			if (self.ModProjectile is not DeepcolorMinion minion)
				return false;

			for (int i = 0; i < Main.maxNPCs; i++) {
				NPC npc = Main.npc[i];
				if (npc.active && minion.IsInAttackRange(npc))
					return true;
			}
			return false;
		}

		private static Vector2? FindSpawnNearPriorityEnemy(Player owner, Projectile self) {
			NPC best = null;
			float bestDist = float.MaxValue;

			for (int i = 0; i < Main.maxNPCs; i++) {
				NPC npc = Main.npc[i];
				if (!npc.active || !npc.CanBeChasedBy(owner))
					continue;
				if (npc.Distance(owner.Center) > DeepcolorMinionRelocateGlobalProj.EnemyVisionRadiusPx)
					continue;

				float dist = npc.Distance(owner.Center);
				if (dist >= bestDist)
					continue;

				bestDist = dist;
				best = npc;
			}

			if (best == null)
				return null;

			Vector2? spawn = FindValidGroundNear(owner, self, best.Center, DeepcolorMinionRelocateGlobalProj.EnemySpawnSearchRadiusPx);
			if (!spawn.HasValue)
				return null;

			float currentDist = Vector2.Distance(
				new Vector2(self.Center.X, self.Center.Y),
				best.Center);
			float newDist = Vector2.Distance(
				new Vector2(spawn.Value.X, spawn.Value.Y + DeepcolorMinion.FrameHeight * 0.5f),
				best.Center);
			if (currentDist - newDist < DeepcolorMinionRelocateGlobalProj.MinEnemyRelocateImprovePx)
				return null;

			return spawn;
		}

		private static Vector2? FindSpawnNearPlayer(Player owner, Projectile self, float currentDistToPlayer) {
			Vector2? spawn = FindValidGroundNear(owner, self, owner.Center, DeepcolorMinionRelocateGlobalProj.PlayerSpawnMaxDistancePx);
			if (!spawn.HasValue)
				return null;

			float newDist = Vector2.Distance(
				new Vector2(spawn.Value.X, spawn.Value.Y + DeepcolorMinion.FrameHeight * 0.5f),
				owner.Center);
			if (newDist >= currentDistToPlayer - 32f)
				return null;

			return spawn;
		}

		private static Vector2? FindValidGroundNear(Player owner, Projectile self, Vector2 focus, float searchRadiusPx) {
			for (int i = 0; i < 36; i++) {
				float angle = Main.rand.NextFloat(MathHelper.TwoPi);
				float dist = Main.rand.NextFloat(
					DeepcolorMinionRelocateGlobalProj.PlayerSpawnMinDistancePx,
					searchRadiusPx);

				Vector2 probe = focus + angle.ToRotationVector2() * dist;
				Vector2 spawn = DeepcolorMinion.FindGroundSpawnPosition(probe, DeepcolorMinion.FrameWidth, DeepcolorMinion.FrameHeight);

				if (!IsValidSpawn(owner, self, spawn))
					continue;

				return spawn;
			}

			return null;
		}

		private static bool IsValidSpawn(Player owner, Projectile self, Vector2 spawnTopLeft) {
			Vector2 center = spawnTopLeft + new Vector2(DeepcolorMinion.FrameWidth * 0.5f, DeepcolorMinion.FrameHeight * 0.5f);

			Rectangle hitbox = new(
				(int)spawnTopLeft.X,
				(int)spawnTopLeft.Y,
				DeepcolorMinion.FrameWidth,
				DeepcolorMinion.FrameHeight);

			Rectangle playerBox = owner.getRect();
			playerBox.Inflate(12, 12);
			if (hitbox.Intersects(playerBox))
				return false;

			if (Vector2.Distance(center, owner.Center) < DeepcolorMinionRelocateGlobalProj.PlayerSpawnMinDistancePx)
				return false;

			int type = ModContent.ProjectileType<DeepcolorMinion>();
			for (int i = 0; i < Main.maxProjectiles; i++) {
				Projectile other = Main.projectile[i];
				if (!other.active || other.whoAmI == self.whoAmI || other.type != type || other.owner != owner.whoAmI)
					continue;

				if (other.Relocate().Phase == DeepcolorRelocatePhase.Despawning)
					continue;

				if (hitbox.Intersects(other.getRect()))
					return false;
			}

			return true;
		}
	}
}
