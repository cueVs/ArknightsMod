using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Content.NPCs.Enemy.ThroughChapter4
{
	// 弑君者地面移动与脱困：贴地、嵌墙、脱困逻辑集中在此，主 AI 只调用公开入口。
	public partial class Crownslayer
	{
		private const int MoveDefaultHitboxWidth = 36;
		private const int MoveSlopeCollisionWidth = 24;
		private const int MoveStuckThreshold = 48;
		private const int MoveWallBumpUnstuck = 28;
		private const int MoveUnstuckCooldown = 90;
		private const int MoveSafeSearchRings = 24;

		private int moveStuckTicks;
		private int moveWallBumpTicks;
		private int moveWallEmbedTicks;
		private int moveUnstuckCooldown;
		private Vector2 moveStuckAnchor;
		private bool moveSlopeHitboxNarrowed;
		private const int MoveWallEmbedThreshold = 12;

		private float Move_FootY => NPC.position.Y + NPC.height;

		private Vector2 MoveBoxTopLeft(Vector2 center) => center - NPC.Size * 0.5f;

		private bool Move_IsNearTopSlope() {
			float nextX = NPC.position.X + NPC.velocity.X;
			float nextFootY = Move_FootY + NPC.velocity.Y;
			int minTileX = (int)Math.Floor((Math.Min(NPC.position.X, nextX) - 2f) / 16f);
			int maxTileX = (int)Math.Floor((Math.Max(NPC.position.X, nextX) + NPC.width + 2f) / 16f);
			int minTileY = (int)Math.Floor((Math.Min(Move_FootY, nextFootY) - 24f) / 16f);
			int maxTileY = (int)Math.Floor((Math.Max(Move_FootY, nextFootY) + 24f) / 16f);

			for (int tx = minTileX; tx <= maxTileX; tx++) {
				for (int ty = minTileY; ty <= maxTileY; ty++) {
					if (!WorldGen.InWorld(tx, ty, 1))
						continue;

					Tile tile = Main.tile[tx, ty];
					if (tile.HasUnactuatedTile && Main.tileSolid[tile.TileType] && !Main.tileSolidTop[tile.TileType]
						&& (tile.Slope == SlopeType.SlopeDownLeft || tile.Slope == SlopeType.SlopeDownRight))
						return true;
				}
			}

			return false;
		}

		// 原版用整个 NPC 矩形贴斜坡，36px 宽的矩形会被高侧提前托住。
		// 仅在引擎处理地形碰撞时改用与双脚相近的宽度，绘制和战斗判定前立即恢复。
		private void Move_PrepareSlopeCollisionHitbox() {
			if (moveSlopeHitboxNarrowed || NPC.noTileCollide || NPC.width != MoveDefaultHitboxWidth || !Move_IsNearTopSlope())
				return;

			Vector2 center = NPC.Center;
			NPC.width = MoveSlopeCollisionWidth;
			NPC.Center = center;
			moveSlopeHitboxNarrowed = true;
		}

		private void Move_RestoreCombatHitbox() {
			if (!moveSlopeHitboxNarrowed)
				return;

			Vector2 center = NPC.Center;
			NPC.width = MoveDefaultHitboxWidth;
			NPC.Center = center;
			moveSlopeHitboxNarrowed = false;
		}

		private static float Move_GetSlopeSurfaceOffset(SlopeType slope, float localX) {
			localX = MathHelper.Clamp(localX, 0f, 16f);
			return slope switch {
				SlopeType.SlopeDownLeft => localX,
				SlopeType.SlopeDownRight => 16f - localX,
				_ => 0f
			};
		}

		private static bool Move_TryGetSurfaceY(float worldX, float fromFootY, out float surfaceFootY) {
			int tx = (int)(worldX / 16f);
			int startTy = (int)(fromFootY / 16f) - 2;

			for (int ty = startTy; ty <= startTy + 6; ty++) {
				if (!WorldGen.InWorld(tx, ty, 1))
					continue;

				Tile tile = Main.tile[tx, ty];
				if (!tile.HasUnactuatedTile || !Main.tileSolid[tile.TileType] || Main.tileSolidTop[tile.TileType])
					continue;

				float surface = ty * 16f;
				if (tile.IsHalfBlock)
					surface += 8f;
				else if (tile.Slope == SlopeType.SlopeDownLeft || tile.Slope == SlopeType.SlopeDownRight)
					surface += Move_GetSlopeSurfaceOffset(tile.Slope, worldX - tx * 16f);
				else if (tile.Slope != SlopeType.Solid)
					continue;

				if (fromFootY <= surface + 12f) {
					surfaceFootY = surface;
					return true;
				}
			}

			surfaceFootY = fromFootY;
			return false;
		}

		// 半身以上嵌入墙体（不含正常站在斜坡/半砖上）。
		private bool Move_IsWallEmbedded(Vector2 center) {
			Vector2 box = MoveBoxTopLeft(center) + new Vector2(6f, 10f);
			int w = NPC.width - 12;
			// SolidCollision 会把斜砖当作完整的 16x16 包围盒。排除脚底 18px，
			// 只检查躯干，避免正常站在斜坡低端时被误判为嵌墙。
			int h = NPC.height - 28;
			return w > 8 && h > 8 && Collision.SolidCollision(box, w, h);
		}

		private bool Move_IsBlocked(Vector2 center) {
			if (!Collision.SolidCollision(MoveBoxTopLeft(center), NPC.width, NPC.height))
				return false;

			if (Move_IsWallEmbedded(center))
				return true;

			return !Move_HasGround(center);
		}

		private bool Move_HasGround(Vector2 center) {
			float footY = center.Y + NPC.height * 0.5f;
			float halfWidth = NPC.width * 0.5f;
			float footInset = Math.Min(4f, NPC.width * 0.25f);

			return Move_HasGroundAtX(center.X - halfWidth + footInset, footY)
				|| Move_HasGroundAtX(center.X, footY)
				|| Move_HasGroundAtX(center.X + halfWidth - footInset, footY);
		}

		private static bool Move_HasGroundAtX(float worldX, float footY) {
			return Move_TryGetSurfaceY(worldX, footY, out float surfaceY)
				&& surfaceY >= footY - 4f
				&& surfaceY <= footY + 6f;
		}

		private bool Move_NeedsLanding(Vector2 center) =>
			Move_IsWallEmbedded(center) || !Move_HasGround(center);

		private bool Move_TryCompareSurfaceAhead(int dir, out float hereY, out float aheadY) {
			hereY = 0f;
			aheadY = 0f;
			float footY = Move_FootY;
			if (!Move_TryGetSurfaceY(NPC.Center.X, footY, out hereY))
				return false;

			// 探针略高于脚底，确保能识别半砖与一格砖形成的 8~16px 上台阶。
			return Move_TryGetSurfaceY(NPC.Center.X + dir * 20f, footY - 4f, out aheadY);
		}

		// 斜坡/台阶方向仍有路时，不因 collideX 停住。
		private bool Move_AllowHorizontalAccel(int dir) {
			if (!NPC.collideX)
				return true;

			return Move_TryCompareSurfaceAhead(dir, out float hereY, out float aheadY)
				&& Math.Abs(hereY - aheadY) >= 2f;
		}

		private void Move_TickSlopeAssist(bool wantsMoveX, float diffX) {
			// 斜砖由原版坡面碰撞负责；这里的负 Y 速度只用于跨半砖/直角台阶。
			// 否则上坡会被误认成连续台阶，弑君者会反复起跳并悬在坡面上方。
			if (!wantsMoveX || NPC.noTileCollide || Move_IsNearTopSlope())
				return;

			int dir = diffX > 0 ? 1 : -1;
			if (Move_TryCompareSurfaceAhead(dir, out float hereSurf, out float aheadSurf)) {
				float stepUp = hereSurf - aheadSurf;
				if (stepUp > 4f && stepUp <= 22f && NPC.velocity.Y >= -1f)
					NPC.velocity.Y = -6f;
			}

		}

		private Vector2 Move_FindOpen(Vector2 from, bool requireGround) {
			if (!Move_IsBlocked(from) && (!requireGround || Move_HasGround(from)))
				return from;

			for (int ring = 1; ring <= MoveSafeSearchRings; ring++) {
				for (int x = -ring; x <= ring; x++) {
					for (int y = -ring; y <= ring; y++) {
						if (Math.Abs(x) != ring && Math.Abs(y) != ring)
							continue;

						Vector2 p = from + new Vector2(x * 16f, y * 16f);
						if (Move_IsBlocked(p) || (requireGround && !Move_HasGround(p)))
							continue;

						return p;
					}
				}
			}

			return from;
		}

		private Vector2 Move_SnapToGround(Vector2 desired) {
			for (int dy = 0; dy <= 16; dy++) {
				Vector2 lower = desired + new Vector2(0f, dy * 4f);
				if (!Move_IsBlocked(lower) && Move_HasGround(lower))
					return lower;
			}

			for (int dy = 1; dy <= 12; dy++) {
				Vector2 higher = desired - new Vector2(0f, dy * 4f);
				if (!Move_IsBlocked(higher) && Move_HasGround(higher))
					return higher;
			}

			return Move_FindOpen(desired, requireGround: false);
		}

		private Vector2 Move_FindStandableNearPlayer(Player target) {
			Vector2 best = Move_SnapToGround(target.Center + new Vector2(0f, -64f));
			float bestDist = Vector2.DistanceSquared(best, NPC.Center);

			for (int ring = 2; ring <= 16; ring++) {
				for (int x = -ring; x <= ring; x++) {
					for (int y = -2; y <= ring; y++) {
						if (Math.Abs(x) != ring && y != ring && y != -2)
							continue;

						Vector2 c = target.Center + new Vector2(x * 16f, y * 16f);
						if (Move_IsBlocked(c) || !Move_HasGround(c))
							continue;

						float d = Vector2.DistanceSquared(c, NPC.Center);
						if (d < bestDist) {
							bestDist = d;
							best = c;
						}
					}
				}
			}

			return best;
		}

		private bool Move_UsesGroundTick() {
			if (NPC.noTileCollide)
				return false;

			switch (CurrentAIState) {
				case AIState.Idle:
				case AIState.Recover:
				case AIState.Skill_1:
				case AIState.Skill_2:
				case AIState.Skill_8:
				case AIState.Skill_9:
					return true;
				default:
					return false;
			}
		}

		private void Move_ResetWatch() {
			moveStuckTicks = 0;
			moveWallBumpTicks = 0;
			moveWallEmbedTicks = 0;
			moveStuckAnchor = NPC.position;
		}

		private void Move_BeginFrame() {
			if (moveUnstuckCooldown > 0)
				moveUnstuckCooldown--;
		}

		private void Move_AfterStateMachine(Player target) {
			if (!Move_UsesGroundTick()) {
				moveWallEmbedTicks = 0;
				return;
			}

			if (!Move_IsWallEmbedded(NPC.Center)) {
				moveWallEmbedTicks = 0;
				return;
			}

			moveWallEmbedTicks++;
			if (moveWallEmbedTicks < MoveWallEmbedThreshold)
				return;

			Vector2 nudge = Move_SnapToGround(Move_FindOpen(NPC.Center, requireGround: true));
			if (!Move_IsWallEmbedded(nudge)) {
				NPC.Center = nudge;
				NPC.velocity = Vector2.Zero;
				NPC.netUpdate = true;
				Move_ResetWatch();
				return;
			}

			// 位置微调也受连续帧阈值保护；若微调失败，再执行强制脱困。
			if (moveUnstuckCooldown <= 0)
				Move_ForceUnstuck(target);
		}

		private void Move_TickIdleChase(Player target, bool wantsMoveX, float diffX) {
			Move_TickSlopeAssist(wantsMoveX, diffX);

			int moveDir = diffX > 0 ? 1 : -1;
			if (NPC.collideX && wantsMoveX && !Move_AllowHorizontalAccel(moveDir)) {
				NPC.velocity.X = 0f;
				moveWallBumpTicks++;
				float dy = target.Center.Y - NPC.Center.Y;
				if (moveWallBumpTicks >= 6 && dy > -56f && dy < 80f && NPC.velocity.Y <= 0.5f)
					NPC.velocity.Y = -6.5f;
				if (moveWallBumpTicks >= MoveWallBumpUnstuck && moveUnstuckCooldown <= 0)
					Move_ForceUnstuck(target);
			}
			else {
				moveWallBumpTicks = Math.Max(0, moveWallBumpTicks - 2);
			}

			if (!wantsMoveX || NPC.noTileCollide || moveUnstuckCooldown > 0) {
				moveStuckTicks = Math.Max(0, moveStuckTicks - 2);
				moveStuckAnchor = NPC.position;
				return;
			}

			if (Vector2.DistanceSquared(moveStuckAnchor, NPC.position) < 4f)
				moveStuckTicks++;
			else {
				moveStuckTicks = Math.Max(0, moveStuckTicks - 4);
				moveStuckAnchor = NPC.position;
			}

			if (moveStuckTicks >= MoveStuckThreshold && moveUnstuckCooldown <= 0)
				Move_ForceUnstuck(target);
		}

		private bool Move_BlockSkillPick() =>
			moveStuckTicks >= 20 || moveWallBumpTicks >= 14 || Move_IsWallEmbedded(NPC.Center);

		private void Move_ForceUnstuck(Player target) {
			if (moveUnstuckCooldown > 0)
				return;

			Vector2 escape = Move_FindStandableNearPlayer(target);
			if (Move_NeedsLanding(escape))
				escape = Move_SnapToGround(Move_FindOpen(NPC.Center, requireGround: false));

			SetPhysics(true, false);
			NPC.Center = escape;
			NPC.velocity = Vector2.Zero;
			SetPhysics(true, true);

			// 清除位置历史，防止传送后残留旧轨迹被渲染为冲刺拖尾
			if (NPC.oldPos != null)
				for (int i = 0; i < NPC.oldPos.Length; i++)
					NPC.oldPos[i] = Vector2.Zero;

			NPC.netUpdate = true;
			Move_ResetWatch();
			moveUnstuckCooldown = MoveUnstuckCooldown;
			CurrentAIState = AIState.Recover;
			StateTimer = 18;
		}

		private void Move_EndMelee(Player target) {
			Move_ClearSwordSlashes();
			NPC.velocity = Vector2.Zero;
			SetPhysics(true, true);

			if (Move_NeedsLanding(NPC.Center)) {
				Vector2 land = Move_SnapToGround(Move_FindOpen(NPC.Center, requireGround: true));
				if (Move_NeedsLanding(land))
					land = Move_FindStandableNearPlayer(target);
				NPC.Center = land;
				NPC.netUpdate = true;
			}

			CurrentAnimation = NPCState.Walk;
			ResetToIdle();
		}

		private void Move_ClearSwordSlashes() {
			if (Main.netMode == NetmodeID.MultiplayerClient)
				return;

			int slashType = ModContent.ProjectileType<SwordSlashEffect>();
			for (int i = 0; i < Main.maxProjectiles; i++) {
				Projectile p = Main.projectile[i];
				if (p.active && p.type == slashType && (int)p.ai[2] == NPC.whoAmI)
					p.Kill();
			}
		}

		private void Move_SoftenLandingOnIdle() {
			if (!Move_NeedsLanding(NPC.Center))
				return;

			Vector2 land = Move_SnapToGround(Move_FindOpen(NPC.Center, requireGround: true));
			if (!Move_IsWallEmbedded(land)) {
				NPC.Center = land;
				NPC.velocity = Vector2.Zero;
				NPC.netUpdate = true;
			}
		}
	}
}
