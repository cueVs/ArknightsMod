using System;
using System.Collections.Generic;
using System.IO;
using ArknightsMod.Content.Buffs.Specialist.Scene;
using ArknightsMod.Content.Items.Weapons.Specialist.Scene;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI.Chat;

namespace ArknightsMod.Content.Projectiles.Specialist.Scene
{
	// 移动摄影车：召唤物。
	// 地面模式：手动物理（重力 + Collision.TileCollision），跟随玩家走路，遇墙跳跃，
	//           停在玩家身后，附近有敌人时原地小跳攻击。
	// 飞行模式：速度驱动，追踪玩家身后目标点；车头朝运动方向，车身拖后，摄像头晃动。
	// 拥有独立血条。
	public class CameraTruck : ModProjectile
	{
		public const int BodyWidth  = 38;
		public const int BodyHeight = 20;
		public const int HeadWidth  = 38;
		public const int HeadHeight = 18;

		public const int Width  = BodyWidth;
		public const int Height = BodyHeight + HeadHeight;

		public const int MaxTrucks = 5;

		private const float DrawScale = 1.25f;

		// 地面待机振动
		private const float BodyBobSpeed     = 0.085f;
		private const float HeadBobSpeed     = 0.115f;
		private const float BodyBobAmplitude = 2f;
		private const float HeadBodyMaxGap   = 0.5f;

		// 地面物理
		private const float Gravity      = 0.38f;
		private const float MaxFallSpeed = 12f;
		private const float GroundSpeed  = 5f;
		private const float GroundAccel  = 0.55f;
		private const float GroundDecel  = 0.80f;
		private const float JumpSpeed    = 9f;
		private const float WalkStopDist = 32f;   // 离目标点此范围内停下

		// 地面攻击（小跳）
		private const float SearchRadius     = 280f;  // 圆形索敌半径（像素）：发现目标后会主动靠近，
		                                               // 但不代表能打到——是否起跳攻击看下面 AttackRangeMargin 算出的近战距离。
		private const int   HopDuration      = 22;
		private const int   HopCooldownMax   = 180;   // 低频攻击：约3秒
		private const float HopForward       = 26f;
		private const float HopHeight        = 15f;
		private const float LeanMax          = 0.22f;
		private const float AttackRangeMargin = 8f;   // 近战可攻击距离 = 车身半宽 + 起跳位移 + 目标半宽 + 这个余量

		// 地面头部弹簧
		private const float HeadSpringK    = 0.20f;
		private const float HeadDamp       = 0.22f;
		private const float HopLandJolt    = 2.6f;
		private const float HopTakeoffJolt = -0.8f;

		// 受伤
		private const int ReceiveDamageCooldownMax = 30;

		// 迷彩 / 眩晕 / 侦查圈
		private const float CamouflageForgetRange = 320f;
		private const float StunHeadAngle         = 0.6f;
		private const int   ReconRingSegments      = 60;
		private const float ReconRingBandWidth     = 10f;

		// 飞行跟随
		private const float FlyTriggerVertDist  = 160f;
		private const float FlyTriggerHorizDist = 550f;
		private const float LandTriggerDist     = 72f;
		private const float FlyMaxSpeed         = 13f;
		private const float FlyAccel            = 0.55f;
		private const float FlyDamp             = 0.94f;
		private const float FlyFollowOffset     = 55f;   // 在玩家身后悬停横向偏移

		// 多车排队 / 互斥
		private const float SlotSpacing    = 46f;   // 排队时相邻车之间的目标间距
		private const float SeparationDist = 40f;   // 低于此距离触发互斥推力
		private const float SeparationPush = 0.45f;
		private const float FlyRotSmooth        = 0.09f;
		private const float FlyBodyLagFactor    = 1.8f;
		private const float FlyBodyLagMax       = 11f;
		private const float FlyBodyLagSmooth    = 0.18f;
		private const float FlyHeadSpringK      = 0.12f;
		private const float FlyHeadDamp         = 0.20f;

		private ref float AnimTimer     => ref Projectile.ai[0];
		private ref float HopTimer      => ref Projectile.ai[1];
		private ref float CooldownTimer => ref Projectile.ai[2];

		private int   hopDir = 1;
		private int   facingDir = 1;
		private float hopStartCenterX;
		private float hopGroundBottomY;
		private float headJoltOffset;
		private float headJoltVel;

		// 地面物理状态
		private bool wasGrounded;   // 上一帧是否在地面（供跳跃检测用）

		// 飞行显示
		private bool    isFlying;
		private float   flyRotation;
		private float   prevFlyRotation;
		private Vector2 flyBodyLag;
		private float   flyHeadAngle;
		private float   flyHeadAngleVel;

		// 独立生命
		internal int  life = -1;
		internal int  lifeMax = 100;
		internal int  defense;
		internal int  spawnOrder;
		private  int  receiveCooldown;
		private  bool lifeInitialized;

		// 技能相关
		internal bool freeSummon;
		private  int  immuneNpcId = -1;
		internal int  stunTimer;

		private bool IsHopping => HopTimer > 0f;

		private static readonly Dictionary<int, int> NextSpawnOrderByPlayer = new();

		public override string Texture =>
			"ArknightsMod/Content/Projectiles/Specialist/Scene/CameraTruck";

		public override void SetStaticDefaults() {
			ProjectileID.Sets.MinionSacrificable[Type] = true;
		}

		public override void SetDefaults() {
			Projectile.width       = Width;
			Projectile.height      = Height;
			Projectile.friendly    = true;
			Projectile.minion      = true;
			Projectile.DamageType  = DamageClass.Summon;
			Projectile.minionSlots = 0f;
			Projectile.penetrate   = -1;
			Projectile.tileCollide = false;  // 始终 false；地面物理由 Collision.TileCollision 手动模拟
			Projectile.ignoreWater = true;
			Projectile.aiStyle     = -1;
			Projectile.timeLeft    = 2;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown  = HopDuration;
		}

		public override void OnSpawn(IEntitySource source) {
			freeSummon = Projectile.ai[0] != 0f;

			SnapToGroundInit();
			AnimTimer = Main.rand.Next(360);

			Player owner = Main.player[Projectile.owner];
			lifeMax = Math.Max(1, owner.statLifeMax2);
			life    = lifeMax;
			defense = owner.statDefense;
			lifeInitialized = true;
			wasGrounded = true;

			if (!NextSpawnOrderByPlayer.TryGetValue(Projectile.owner, out int next))
				next = 0;
			spawnOrder = next;
			NextSpawnOrderByPlayer[Projectile.owner] = next + 1;
		}

		public override bool? CanCutTiles() => false;

		public override bool MinionContactDamage() => IsHopping && stunTimer <= 0;

		public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) {
			float m = SceneCameraSkills.AttackMult(Main.player[Projectile.owner]);
			if (m != 1f)
				modifiers.SourceDamage *= m;
		}

		// ========================= AI =========================

		public override void AI() {
			Player owner = Main.player[Projectile.owner];
			if (owner.dead || !owner.active || !owner.HasBuff<CameraTruckBuff>()) {
				Projectile.Kill();
				return;
			}

			Projectile.timeLeft = 2;

			if (!lifeInitialized && life < 0) {
				lifeMax = Math.Max(1, owner.statLifeMax2);
				life    = lifeMax;
				defense = owner.statDefense;
				lifeInitialized = true;
			}

			// 飞行模式切换
			if (!isFlying && stunTimer <= 0 && !IsHopping) {
				float horizDist = Math.Abs(owner.Center.X - Projectile.Center.X);
				float vertDiff  = Projectile.Center.Y - owner.Center.Y; // 正值=玩家更高
				if (horizDist > FlyTriggerHorizDist || (vertDiff > FlyTriggerVertDist && horizDist > 80f))
					EnterFly();
			}

			if (isFlying) {
				UpdateHeadJoltSpring();
				UpdateFlyFollow(owner);
			} else if (stunTimer > 0) {
				UpdateHeadJoltSpring();
				UpdateStun();
			} else if (IsHopping) {
				UpdateHeadJoltSpring();
				UpdateHop();
			} else {
				UpdateHeadJoltSpring();
				UpdateGroundFollow(owner);
			}

			if (TakeIncomingDamage(owner))
				return;

			AnimTimer++;
		}

		// -------- 飞行 --------

		private void EnterFly() {
			isFlying        = true;
			flyBodyLag      = Vector2.Zero;
			flyHeadAngle    = 0f;
			flyHeadAngleVel = 0f;
			flyRotation     = 0f;
			prevFlyRotation = 0f;
			HopTimer        = 0f;
			CooldownTimer   = 0f;
		}

		private void UpdateFlyFollow(Player owner) {
			prevFlyRotation = flyRotation;

			// 目标：玩家身后排队站位，略偏下
			int     slotIndex = GetSlotIndex(owner);
			float   targetX   = owner.Center.X - facingDir * (FlyFollowOffset + slotIndex * SlotSpacing);
			float   targetY   = owner.Center.Y + 12f;
			Vector2 toTarget  = new Vector2(targetX, targetY) - Projectile.Center;
			float   dist      = toTarget.Length();

			if (dist > 24f) {
				Projectile.velocity += toTarget / dist * FlyAccel;
				float speed = Projectile.velocity.Length();
				if (speed > FlyMaxSpeed)
					Projectile.velocity *= FlyMaxSpeed / speed;
			}

			Projectile.velocity *= FlyDamp;
			ApplySeparation(owner);

			// 朝向
			if (Math.Abs(Projectile.velocity.X) > 0.4f)
				facingDir = Projectile.velocity.X > 0 ? 1 : -1;
			else if (dist > 1f)
				facingDir = toTarget.X > 0 ? 1 : -1;

			// 平滑旋转（车头朝运动方向）
			float flySpeed = Projectile.velocity.Length();
			if (flySpeed > 0.5f) {
				float targetRot = (float)Math.Atan2(
					Projectile.velocity.Y,
					facingDir > 0 ? Projectile.velocity.X : -Projectile.velocity.X);
				float delta = MathHelper.WrapAngle(targetRot - flyRotation);
				flyRotation += delta * FlyRotSmooth;
			} else {
				flyRotation *= 1f - FlyRotSmooth;
			}

			// 车身拖后偏移
			Vector2 lagTarget = flySpeed > 0.3f
				? -Projectile.velocity.SafeNormalize(Vector2.Zero)
				  * Math.Min(flySpeed * FlyBodyLagFactor, FlyBodyLagMax)
				: Vector2.Zero;
			flyBodyLag = Vector2.Lerp(flyBodyLag, lagTarget, FlyBodyLagSmooth);

			// 摄像头晃动弹簧
			float rotChange = flyRotation - prevFlyRotation;
			flyHeadAngleVel += rotChange * 3f;
			flyHeadAngleVel += -FlyHeadSpringK * flyHeadAngle - FlyHeadDamp * flyHeadAngleVel;
			flyHeadAngle    += flyHeadAngleVel;

			// 降落判断
			if (dist < LandTriggerDist && IsNearGround(48f)) {
				isFlying            = false;
				Projectile.velocity = Vector2.Zero;
				flyRotation         = 0f;
				flyBodyLag          = Vector2.Zero;
				flyHeadAngle        = 0f;
				flyHeadAngleVel     = 0f;
				wasGrounded         = false;
			}
		}

		// -------- 地面跟随（手动物理） --------

		private void UpdateGroundFollow(Player owner) {
			// 重力
			Projectile.velocity.Y = Math.Min(Projectile.velocity.Y + Gravity, MaxFallSpeed);

			// 索敌：SearchRadius 范围内发现目标后就主动朝目标移动，而不是仍旧原地跟着玩家走；
			// 只有真正贴近到"近战距离"（车身半宽 + 起跳位移 + 目标半宽 + 余量）才会触发起跳攻击——
			// 之前是一发现目标（哪怕还隔着一大截 SearchRadius）就在原地跳，位置完全不往怪物那边挪。
			bool hasTarget    = TryFindTarget(owner, out NPC target);
			bool inAttackRange = false;
			if (hasTarget) {
				float attackRange = Projectile.width / 2f + HopForward + target.width / 2f + AttackRangeMargin;
				inAttackRange = Vector2.Distance(target.Center, Projectile.Center) <= attackRange;
			}

			// 跟随目标 X：有目标就一直朝目标贴过去（哪怕已经进入攻击距离也继续以目标为跟随点，
			// 这样冷却转好能立刻再打一下，不会先跑回玩家身后再折返）；没有目标才回到玩家身后排队站位。
			float targetX;
			if (hasTarget) {
				targetX = target.Center.X;
			}
			else {
				int slotIndex = GetSlotIndex(owner);
				targetX = owner.Center.X - owner.direction * (60f + slotIndex * SlotSpacing);
			}
			float dx = targetX - Projectile.Center.X;

			if (Math.Abs(dx) > WalkStopDist) {
				int moveDir = dx > 0 ? 1 : -1;
				facingDir = moveDir;
				Projectile.velocity.X += moveDir * GroundAccel;
				Projectile.velocity.X  = MathHelper.Clamp(Projectile.velocity.X, -GroundSpeed, GroundSpeed);
			} else {
				Projectile.velocity.X *= GroundDecel;
				if (Math.Abs(dx) > 2f)
					facingDir = dx > 0 ? 1 : -1;
			}

			ApplySeparation(owner);

			// 手动 tile 碰撞（corrVel = 实际可走距离）
			Vector2 corrVel  = Collision.TileCollision(Projectile.position, Projectile.velocity,
				Projectile.width, Projectile.height, false, false);

			bool grounded = Projectile.velocity.Y > 0 && corrVel.Y != Projectile.velocity.Y; // 撞到地面
			bool hitWall  = Math.Abs(corrVel.X) < Math.Abs(Projectile.velocity.X) - 0.01f;  // 撞到侧壁

			// 遇墙且上帧在地面 → 跳跃
			if (hitWall && wasGrounded)
				corrVel.Y = -JumpSpeed;

			wasGrounded = grounded;
			Projectile.velocity = corrVel;

			// 攻击：面朝目标，但只有真正进入近战范围才起跳
			if (hasTarget)
				facingDir = target.Center.X >= Projectile.Center.X ? 1 : -1;
			if (CooldownTimer > 0f)
				CooldownTimer--;
			else if (inAttackRange && wasGrounded)
				StartHop(facingDir);
		}

		private void UpdateStun() {
			stunTimer--;
			Projectile.velocity.Y = Math.Min(Projectile.velocity.Y + Gravity, MaxFallSpeed);
			Projectile.velocity.X *= 0.85f;

			Vector2 corrVel = Collision.TileCollision(Projectile.position, Projectile.velocity,
				Projectile.width, Projectile.height, false, false);
			wasGrounded         = Projectile.velocity.Y > 0 && corrVel.Y != Projectile.velocity.Y;
			Projectile.velocity = corrVel;
		}

		// -------- 小跳攻击（手动位移，velocity 置零） --------

		private void StartHop(int dir) {
			HopTimer         = 1f;
			hopDir           = dir;
			hopStartCenterX  = Projectile.Center.X;
			hopGroundBottomY = Projectile.Bottom.Y;
			headJoltVel     += HopTakeoffJolt;
			Projectile.velocity = Vector2.Zero;
		}

		private void UpdateHop() {
			float p     = MathHelper.Clamp(HopTimer / HopDuration, 0f, 1f);
			float swing = (float)Math.Sin(MathHelper.Pi * p);

			Projectile.position.X = hopStartCenterX + hopDir * HopForward * swing - Projectile.width  / 2f;
			Projectile.position.Y = hopGroundBottomY - HopHeight * swing           - Projectile.height;
			Projectile.velocity   = Vector2.Zero; // 手动控制位置，velocity 始终为零

			HopTimer++;
			if (HopTimer > HopDuration) {
				HopTimer      = 0f;
				CooldownTimer = HopCooldownMax;
				Projectile.position.X = hopStartCenterX - Projectile.width / 2f;
				Projectile.ResetLocalNPCHitImmunity();
				headJoltVel += HopLandJolt;
				wasGrounded  = true;
			}
		}

		// -------- 头部弹簧 --------

		private void UpdateHeadJoltSpring() {
			headJoltVel    += -HeadSpringK * headJoltOffset - HeadDamp * headJoltVel;
			headJoltOffset += headJoltVel;
		}

		// -------- 多车排队 / 互斥 --------

		// 按召唤顺序在同伴中排队，返回从 0 开始的序号（同伴越多，越靠后的车站位越靠后）
		private int GetSlotIndex(Player owner) {
			int type = ModContent.ProjectileType<CameraTruck>();
			int rank = 0;
			for (int i = 0; i < Main.maxProjectiles; i++) {
				Projectile p = Main.projectile[i];
				if (!p.active || p.owner != owner.whoAmI || p.type != type) continue;
				if (p.ModProjectile is not CameraTruck t) continue;
				if (t.spawnOrder < spawnOrder) rank++;
			}
			return rank;
		}

		// 与同伴距离过近时施加水平推力，避免重叠
		private void ApplySeparation(Player owner) {
			int type = ModContent.ProjectileType<CameraTruck>();
			for (int i = 0; i < Main.maxProjectiles; i++) {
				Projectile p = Main.projectile[i];
				if (!p.active || p.whoAmI == Projectile.whoAmI || p.owner != owner.whoAmI || p.type != type) continue;

				float dx = Projectile.Center.X - p.Center.X;
				float adx = Math.Abs(dx);
				if (adx >= SeparationDist || adx < 0.01f) continue;

				float push = (1f - adx / SeparationDist) * SeparationPush;
				Projectile.velocity.X += Math.Sign(dx) * push;
			}
		}

		// -------- 目标搜索 --------

		// 记录本 tick 内，每个玩家已经被"某一辆车"选中的怪物，避免多辆车不约而同扎堆打
		// 同一只、放着旁边其它怪不管。key 是 Player.whoAmI；tick 号变了就自动清空重建，
		// 不需要额外的清理时机。
		private static readonly Dictionary<int, (ulong Tick, HashSet<int> Claimed)> ClaimedTargetsByOwner = new();

		private static HashSet<int> GetClaimedTargetsThisTick(int ownerWhoAmI) {
			ulong tick = Main.GameUpdateCount;
			if (!ClaimedTargetsByOwner.TryGetValue(ownerWhoAmI, out var entry) || entry.Tick != tick) {
				entry = (tick, new HashSet<int>());
				ClaimedTargetsByOwner[ownerWhoAmI] = entry;
			}
			return entry.Claimed;
		}

		// 只负责"在 SearchRadius 范围内有没有可打的目标"，不代表已经近到能攻击——
		// 是否真的进入近战范围由调用方（UpdateGroundFollow）自己拿返回的 target 算距离判断。
		private bool TryFindTarget(Player owner, out NPC target) {
			target = null;
			Vector2 center = Projectile.Center;

			bool InRange(NPC npc) {
				if (npc == null || !npc.active || !npc.CanBeChasedBy(Projectile)) return false;
				return Vector2.Distance(npc.Center, center) <= SearchRadius;
			}

			// 玩家手动标记的目标优先级最高，所有车统一围攻它——这是原版召唤物"指定目标"
			// 的通用规则，属于玩家的主动选择，不受下面"分散打不同目标"的限制。
			if (owner.HasMinionAttackTargetNPC) {
				NPC marked = Main.npc[owner.MinionAttackTargetNPC];
				if (InRange(marked)) { target = marked; return true; }
			}

			// 没有手动标记：各车各自找最近的怪，但优先避开"本 tick 内已经被其它车认领"的目标，
			// 这样多辆车碰上多只怪物时会自然分散攻击；如果避开之后就没有可打的目标了
			// （比如附近就剩这一只怪），才允许退回去和别的车扎堆打同一只，而不是干脆不打。
			HashSet<int> claimed = GetClaimedTargetsThisTick(owner.whoAmI);

			NPC   bestUnclaimed = null; float bestUnclaimedDist = float.MaxValue;
			NPC   bestAny       = null; float bestAnyDist       = float.MaxValue;

			for (int i = 0; i < Main.maxNPCs; i++) {
				NPC npc = Main.npc[i];
				if (!InRange(npc)) continue;

				float dist = Vector2.Distance(npc.Center, center);
				if (dist < bestAnyDist) { bestAnyDist = dist; bestAny = npc; }
				if (!claimed.Contains(npc.whoAmI) && dist < bestUnclaimedDist) { bestUnclaimedDist = dist; bestUnclaimed = npc; }
			}

			target = bestUnclaimed ?? bestAny;
			if (target == null) return false;

			claimed.Add(target.whoAmI);
			return true;
		}

		// -------- 近地判断（供降落判断用） --------

		private bool IsNearGround(float maxPixelDist) {
			int tx = Utils.Clamp((int)(Projectile.Center.X / 16f), 1, Main.maxTilesX - 2);
			int sy = Utils.Clamp((int)((Projectile.Bottom.Y + 2f) / 16f), 1, Main.maxTilesY - 2);
			int ey = Math.Min(sy + (int)(maxPixelDist / 16f) + 1, Main.maxTilesY - 1);
			for (int ty = sy; ty < ey; ty++) {
				if (WorldGen.SolidTile(tx, ty)) return true;
			}
			return false;
		}

		// -------- 受伤 --------

		private bool TakeIncomingDamage(Player owner) {
			if (Main.myPlayer != Projectile.owner) return false;
			if (receiveCooldown > 0) { receiveCooldown--; return false; }

			bool camo = stunTimer <= 0 && SceneCameraSkills.Skill1Active(owner);
			if (camo && immuneNpcId >= 0) {
				NPC im = Main.npc[immuneNpcId];
				if (!im.active || im.friendly || Vector2.Distance(im.Center, Projectile.Center) > CamouflageForgetRange)
					immuneNpcId = -1;
			}
			if (!camo) immuneNpcId = -1;

			foreach (NPC npc in Main.ActiveNPCs) {
				if (!npc.active || npc.friendly || npc.damage <= 0 || npc.dontTakeDamage) continue;
				if (!Projectile.Hitbox.Intersects(npc.Hitbox)) continue;
				if (camo) {
					if (immuneNpcId < 0) { immuneNpcId = npc.whoAmI; continue; }
					if (npc.whoAmI == immuneNpcId) continue;
				}
				return ApplyDamage(npc.damage);
			}
			foreach (Projectile other in Main.ActiveProjectiles) {
				if (!other.active || !other.hostile || other.owner == Projectile.owner) continue;
				if (!Projectile.Hitbox.Intersects(other.Hitbox)) continue;
				return ApplyDamage(other.damage);
			}
			return false;
		}

		private bool ApplyDamage(int rawDamage) {
			int dealt = Math.Max(1, rawDamage - (int)(defense * SceneCameraSkills.DefenseMult(Main.player[Projectile.owner])));
			life -= dealt;
			receiveCooldown      = ReceiveDamageCooldownMax;
			Projectile.netUpdate = true;

			if (!Main.dedServ) {
				CombatText.NewText(Projectile.getRect(), Color.OrangeRed, dealt);
				SoundEngine.PlaySound(SoundID.NPCHit1, Projectile.Center);
			}
			if (life <= 0) { DeathEffect(); Projectile.Kill(); return true; }
			return false;
		}

		private void DeathEffect() {
			if (Main.dedServ) return;
			SoundEngine.PlaySound(SoundID.NPCDeath4, Projectile.Center);
			for (int i = 0; i < 22; i++) {
				Dust d = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(14f, 12f),
					DustID.GreenTorch, Main.rand.NextVector2Circular(3.5f, 3.5f), 0, default, Main.rand.NextFloat(1.1f, 1.8f));
				d.noGravity = true;
			}
			for (int i = 0; i < 8; i++) {
				Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.Smoke,
					Main.rand.NextVector2Circular(2.2f, 2.2f), 120, default, 1.1f);
				d.noGravity = true;
			}
		}

		// ========================= 绘制 =========================

		public override bool PreDraw(ref Color lightColor) {
			Player owner   = Main.player[Projectile.owner];
			bool   stunned = stunTimer > 0;

			if (!stunned && SceneCameraSkills.Skill2Active(owner))
				DrawReconRing();

			Texture2D bodyTex = TextureAssets.Projectile[Type].Value;
			Texture2D headTex = ModContent.Request<Texture2D>(
				"ArknightsMod/Content/Projectiles/Specialist/Scene/CameraTruck_Head").Value;

			SpriteEffects fx = facingDir < 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

			if (isFlying)
				DrawFlying(bodyTex, headTex, lightColor, fx);
			else
				DrawGrounded(bodyTex, headTex, lightColor, fx, stunned);

			return false;
		}

		private void DrawFlying(Texture2D bodyTex, Texture2D headTex, Color lightColor, SpriteEffects fx) {
			Vector2 bodyCenterScreen = Projectile.Center + flyBodyLag - Main.screenPosition
				+ new Vector2(0f, Projectile.gfxOffY);

			var bodyOrigin = new Vector2(bodyTex.Width / 2f, bodyTex.Height / 2f);
			Main.EntitySpriteDraw(bodyTex, bodyCenterScreen, null, lightColor, flyRotation, bodyOrigin, DrawScale, fx, 0);

			Vector2 truckUp      = new Vector2(0f, -1f).RotatedBy(flyRotation);
			Vector2 headCenter   = bodyCenterScreen + truckUp * ((bodyTex.Height + headTex.Height) * DrawScale * 0.5f);
			var     headOrigin   = new Vector2(headTex.Width / 2f, headTex.Height / 2f);
			Main.EntitySpriteDraw(headTex, headCenter, null, lightColor, flyRotation + flyHeadAngle, headOrigin, DrawScale, fx, 0);
		}

		private void DrawGrounded(Texture2D bodyTex, Texture2D headTex, Color lightColor, SpriteEffects fx, bool stunned) {
			float bodyBob  = (IsHopping || stunned) ? 0f
				: (1f - (float)Math.Cos(AnimTimer * BodyBobSpeed)) * 0.5f * BodyBobAmplitude;
			float lean     = (!stunned && IsHopping)
				? LeanMax * (float)Math.Sin(MathHelper.Pi * MathHelper.Clamp(HopTimer / HopDuration, 0f, 1f)) * hopDir
				: 0f;
			float idleSep  = ((float)Math.Sin(AnimTimer * HeadBobSpeed) * 0.5f + 0.5f) * HeadBodyMaxGap;
			float headLean = stunned ? StunHeadAngle * facingDir : lean;

			Vector2 footScreen = new Vector2(Projectile.Center.X, Projectile.Bottom.Y) - Main.screenPosition
				+ new Vector2(0f, Projectile.gfxOffY);
			Vector2 pivot      = footScreen + new Vector2(0f, bodyBob);

			var bodyOrigin = new Vector2(bodyTex.Width / 2f, bodyTex.Height);
			Main.EntitySpriteDraw(bodyTex, pivot, null, lightColor, lean, bodyOrigin, DrawScale, fx, 0);

			float   effectiveGap = Math.Min(idleSep - headJoltOffset, HeadBodyMaxGap);
			Vector2 headPos      = pivot + new Vector2(0f, -(bodyTex.Height * DrawScale) - effectiveGap).RotatedBy(lean);
			var     headOrigin   = new Vector2(headTex.Width / 2f, headTex.Height);
			Main.EntitySpriteDraw(headTex, headPos, null, lightColor, headLean, headOrigin, DrawScale, fx, 0);
		}

		private void DrawReconRing() {
			float   rOut = SceneCameraSkills.ReconRadiusPx;
			float   rIn  = Math.Max(2f, rOut - ReconRingBandWidth);
			Vector2 c    = Projectile.Center;
			Color   col  = new Color(60, 220, 90) * 0.35f;

			var verts = new List<VertexPositionColor>(ReconRingSegments * 6);
			for (int i = 0; i < ReconRingSegments; i++) {
				float   a0 = MathHelper.TwoPi * i       / ReconRingSegments;
				float   a1 = MathHelper.TwoPi * (i + 1) / ReconRingSegments;
				Vector2 d0 = a0.ToRotationVector2(), d1 = a1.ToRotationVector2();
				Vector2 o0 = c + d0 * rOut, o1 = c + d1 * rOut;
				Vector2 i0 = c + d0 * rIn,  i1 = c + d1 * rIn;
				verts.Add(ScenePrimitiveRenderer.Vert(i0, col)); verts.Add(ScenePrimitiveRenderer.Vert(o0, col)); verts.Add(ScenePrimitiveRenderer.Vert(o1, col));
				verts.Add(ScenePrimitiveRenderer.Vert(i0, col)); verts.Add(ScenePrimitiveRenderer.Vert(o1, col)); verts.Add(ScenePrimitiveRenderer.Vert(i1, col));
			}
			ScenePrimitiveRenderer.DrawTriangles(verts);
		}

		public override void PostDraw(Color lightColor) {
			if (Main.dedServ || life < 0 || lifeMax <= 0) return;

			float barWorldY = isFlying
				? Projectile.Center.Y - Height * DrawScale * 0.5f + Projectile.gfxOffY - 8f
				: Projectile.Bottom.Y - (BodyHeight + HeadHeight) * DrawScale + Projectile.gfxOffY - 8f;

			float alpha = Lighting.Brightness(
				(int)(Projectile.Center.X / 16f),
				(int)((Projectile.Center.Y + Projectile.gfxOffY) / 16f));
			Main.instance.DrawHealthBar(Projectile.Center.X, barWorldY, life, lifeMax, alpha, 1f);

			Vector2 mouse = Main.MouseWorld;
			if (new Rectangle((int)mouse.X - 8, (int)mouse.Y - 8, 16, 16).Intersects(Projectile.getRect())) {
				Vector2 textPos = new Vector2(Projectile.Center.X, barWorldY - 16f) - Main.screenPosition;
				string  text    = $"{Math.Max(0, life)}/{lifeMax}";
				Vector2 origin  = FontAssets.MouseText.Value.MeasureString(text) * 0.5f;
				ChatManager.DrawColorCodedStringWithShadow(Main.spriteBatch, FontAssets.MouseText.Value,
					text, textPos, Color.White, 0f, origin, Vector2.One * 0.85f);
			}
		}

		// ========================= 网络同步 =========================

		public override void SendExtraAI(BinaryWriter writer) {
			writer.Write(life);
			writer.Write(lifeMax);
			writer.Write(defense);
			writer.Write(spawnOrder);
			writer.Write(freeSummon);
			writer.Write(stunTimer);
			writer.Write(isFlying);
			writer.Write(flyRotation);
		}

		public override void ReceiveExtraAI(BinaryReader reader) {
			life            = reader.ReadInt32();
			lifeMax         = reader.ReadInt32();
			defense         = reader.ReadInt32();
			spawnOrder      = reader.ReadInt32();
			freeSummon      = reader.ReadBoolean();
			stunTimer       = reader.ReadInt32();
			lifeInitialized = true;
			isFlying        = reader.ReadBoolean();
			flyRotation     = reader.ReadSingle();
		}

		// ========================= 静态管理 =========================

		public static int CountActiveForPlayer(Player player) {
			int type = ModContent.ProjectileType<CameraTruck>(); int n = 0;
			for (int i = 0; i < Main.maxProjectiles; i++) {
				Projectile p = Main.projectile[i];
				if (p.active && p.owner == player.whoAmI && p.type == type) n++;
			}
			return n;
		}

		public static void CountForPlayer(Player player, out int total, out int slotUsing) {
			int type = ModContent.ProjectileType<CameraTruck>();
			total = slotUsing = 0;
			for (int i = 0; i < Main.maxProjectiles; i++) {
				Projectile p = Main.projectile[i];
				if (!p.active || p.owner != player.whoAmI || p.type != type) continue;
				total++;
				if (p.ModProjectile is CameraTruck t && !t.freeSummon) slotUsing++;
			}
		}

		public static void StunAllForPlayer(Player player, int ticks) {
			int type = ModContent.ProjectileType<CameraTruck>();
			for (int i = 0; i < Main.maxProjectiles; i++) {
				Projectile p = Main.projectile[i];
				if (!p.active || p.owner != player.whoAmI || p.type != type) continue;
				if (p.ModProjectile is not CameraTruck t) continue;
				if (t.IsHopping) { p.position.X = t.hopStartCenterX - p.width / 2f; t.HopTimer = 0f; }
				t.stunTimer     = ticks;
				t.isFlying      = false;
				t.flyRotation   = 0f;
				t.flyBodyLag    = Vector2.Zero;
				p.velocity      = Vector2.Zero;
				p.netUpdate     = true;
			}
		}

		public static bool TryRemoveOldestForPlayer(Player player) {
			int type = ModContent.ProjectileType<CameraTruck>();
			Projectile oldest = null; int oldestOrder = int.MaxValue;
			for (int i = 0; i < Main.maxProjectiles; i++) {
				Projectile p = Main.projectile[i];
				if (!p.active || p.owner != player.whoAmI || p.type != type) continue;
				if (p.ModProjectile is not CameraTruck truck) continue;
				if (truck.spawnOrder < oldestOrder) { oldestOrder = truck.spawnOrder; oldest = p; }
			}
			if (oldest == null) return false;
			(oldest.ModProjectile as CameraTruck)?.DeathEffect();
			oldest.Kill();
			return true;
		}

		public static Vector2 FindGroundSpawnPosition(Vector2 worldPosition, int width, int height) {
			int tileX  = Utils.Clamp((int)(worldPosition.X / 16f), 1, Main.maxTilesX - 2);
			int startY = Utils.Clamp((int)(worldPosition.Y / 16f), 1, Main.maxTilesY - 2);
			for (int tileY = startY; tileY < Main.maxTilesY - 1; tileY++) {
				if (!WorldGen.SolidTile(tileX, tileY)) continue;
				return new Vector2(tileX * 16f + 8f - width / 2f, tileY * 16f - height);
			}
			return worldPosition;
		}

		private void SnapToGroundInit() {
			int tileX  = Utils.Clamp((int)(Projectile.Center.X / 16f), 1, Main.maxTilesX - 2);
			int startY = Utils.Clamp((int)((Projectile.Bottom.Y + 2f) / 16f), 1, Main.maxTilesY - 2);
			for (int tileY = startY; tileY < Main.maxTilesY; tileY++) {
				if (!WorldGen.SolidTile(tileX, tileY)) continue;
				Projectile.position.Y = tileY * 16f - Projectile.height;
				return;
			}
		}
	}
}
