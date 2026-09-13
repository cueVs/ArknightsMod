using System;
using System.IO;
using ArknightsMod.Content.Projectiles.Supporter.Deepcolor;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace ArknightsMod.Common.GlobalProjectiles
{
	public enum DeepcolorRelocatePhase : byte
	{
		Stable,
		Despawning,
		Spawning
	}

	public class DeepcolorMinionRelocateGlobalProj : GlobalProjectile
	{
		public const int RelocateMovePixelsPerTick = 2;
		public const int RelocateCooldownTicks = 360;
		public const int PlayerFarDelayTicks = 240;
		public const float PlayerFarDistancePx = 16f * 16f;
		public const float PlayerNearComfortPx = 11f * 16f;
		public const float PlayerSpawnMinDistancePx = 3f * 16f;
		public const float PlayerSpawnMaxDistancePx = 9f * 16f;
		public const float EnemyVisionRadiusPx = 52f * 16f;
		public const float EnemySpawnSearchRadiusPx = 8f * 16f;
		public const float MinRelocateDistancePx = 7f * 16f;
		public const float MinEnemyRelocateImprovePx = 4f * 16f;

		public override bool InstancePerEntity => true;

		public DeepcolorRelocatePhase Phase = DeepcolorRelocatePhase.Stable;
		public float AnchorGroundY;
		public int MoveProgress;
		public int MoveDistance;
		public Vector2 PendingPosition;
		public int FarFromPlayerTimer;
		public int RelocateCooldown;
		public bool SpawnedTransitionDust;
		public int LastDustProgress;

		public bool IsTransitioning => Phase != DeepcolorRelocatePhase.Stable;

		public int ComputeMoveDistance() =>
			DeepcolorMinion.FrameHeight + DeepcolorMinion.GroundOverlapPixels + DeepcolorMinion.VisualFootSinkPixels + 28;

		public float RelocateVisualOffsetY => Phase switch {
			DeepcolorRelocatePhase.Despawning => MoveProgress,
			DeepcolorRelocatePhase.Spawning => MoveDistance - MoveProgress,
			_ => 0f
		};

		public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
			=> entity.type == ModContent.ProjectileType<DeepcolorMinion>();

		public void BeginStable(Projectile projectile) {
			Phase = DeepcolorRelocatePhase.Stable;
			MoveProgress = 0;
			SpawnedTransitionDust = false;
			LastDustProgress = 0;
			FarFromPlayerTimer = 0;
			AnchorGroundY = DeepcolorMinion.GetGroundSurfaceY(projectile.Center.X, projectile.Bottom.Y);
		}

		public void BeginDespawn(Projectile projectile, Vector2 targetPosition) {
			Phase = DeepcolorRelocatePhase.Despawning;
			MoveProgress = 0;
			MoveDistance = ComputeMoveDistance();
			PendingPosition = targetPosition;
			SpawnedTransitionDust = false;
			LastDustProgress = 0;
			AnchorGroundY = DeepcolorMinion.GetGroundSurfaceY(projectile.Center.X, projectile.Bottom.Y);
			RelocateCooldown = RelocateCooldownTicks;
			DeepcolorMinion.SpawnTransitionDustSurface(projectile.Center.X, AnchorGroundY);
		}

		public void BeginSpawn(Projectile projectile) {
			Phase = DeepcolorRelocatePhase.Spawning;
			MoveDistance = ComputeMoveDistance();
			MoveProgress = MoveDistance / 8;
			SpawnedTransitionDust = false;
			LastDustProgress = MoveProgress;
			projectile.position = PendingPosition - new Vector2(projectile.width * 0.5f, 0f);
			AnchorGroundY = DeepcolorMinion.GetGroundSurfaceY(projectile.Center.X, projectile.Bottom.Y);
			DeepcolorMinion.SpawnTransitionDustSurface(projectile.Center.X, AnchorGroundY);
		}

		public override void SendExtraAI(Projectile projectile, BitWriter bitWriter, BinaryWriter binaryWriter) {
			binaryWriter.Write((byte)Phase);
			binaryWriter.Write(AnchorGroundY);
			binaryWriter.Write(MoveProgress);
			binaryWriter.Write(MoveDistance);
			binaryWriter.WriteVector2(PendingPosition);
			binaryWriter.Write(FarFromPlayerTimer);
			binaryWriter.Write(RelocateCooldown);
			binaryWriter.Write(SpawnedTransitionDust);
			binaryWriter.Write(LastDustProgress);
		}

		public override void ReceiveExtraAI(Projectile projectile, BitReader bitReader, BinaryReader binaryReader) {
			Phase = (DeepcolorRelocatePhase)binaryReader.ReadByte();
			AnchorGroundY = binaryReader.ReadSingle();
			MoveProgress = binaryReader.ReadInt32();
			MoveDistance = binaryReader.ReadInt32();
			PendingPosition = binaryReader.ReadVector2();
			FarFromPlayerTimer = binaryReader.ReadInt32();
			RelocateCooldown = binaryReader.ReadInt32();
			SpawnedTransitionDust = binaryReader.ReadBoolean();
			LastDustProgress = binaryReader.ReadInt32();
		}
	}

	public static class DeepcolorMinionRelocateExtensions
	{
		public static DeepcolorMinionRelocateGlobalProj Relocate(this Projectile projectile)
			=> projectile.GetGlobalProjectile<DeepcolorMinionRelocateGlobalProj>();
	}
}
