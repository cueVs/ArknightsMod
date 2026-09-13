using System;
using System.IO;
using ArknightsMod.Content.Buffs.Medic.Kaltsit;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Projectiles.Medic.Kaltsit
{
	// 凯尔希套装召唤物 M3(Mon3tr)。行为参照原版星辰守卫：常态悬浮在玩家身后，
	// 周围出现敌人时主动飞过去攻击，优先打距离玩家最近的那个（不是距离 M3 自己最近的）。
	// 和星辰守卫不同的是有独立血量——受到足够伤害后不会消失，而是变成能量水晶，
	// 固定悬浮在玩家身后、暂时不参与战斗，一段时间后自动复苏。
	//
	// 生命周期由 Mon3trBuff 维护，不在这里自己判断"套装还穿不穿"，见那个文件顶部注释。
	//
	// ⚠ 下面标了"占位数值"的常量（生命、防御、伤害、索敌距离、复苏时间等）都是这一版
	// 随手定的，没有照抄任何官方资料，纯粹为了让整套流程能跑起来——数值不合适随时改，
	// 不影响状态机/动画/召唤逻辑本身。
	//
	// 技能（40~45 / 60~72 / 74~88 三段动画）目前只搭好了"能播放、播完自动回到待机"的
	// 状态机骨架，TriggerSkill1/2/3 三个 public 方法就是留给以后接判定逻辑的入口，
	// 现在没有任何地方调用它们——按约定，触发条件"稍后再说"。
	public class Mon3tr : ModProjectile
	{
		// ── 主动画表（mo3ter-完整动画_gif_sheet.png，206x15488，88 帧，每帧 206x176）──
		private const int FrameWidth  = 206;
		private const int FrameHeight = 176;

		// 帧号按用户给出的"第 N 帧"（从 1 开始数）直接写，转源矩形时统一 -1。
		private const int SpawnFrameFirst  = 16, SpawnFrameLast  = 29; // 复苏/首次登场
		private const int IdleFrameFirst   = 31, IdleFrameLast   = 38; // 待机
		private const int Skill1FrameFirst = 40, Skill1FrameLast = 45; // 一技能（未接判定）
		private const int AttackFrameFirst = 47, AttackFrameLast = 58; // 普通攻击
		private const int Skill2FrameFirst = 60, Skill2FrameLast = 72; // 二技能（未接判定）
		private const int Skill3FrameFirst = 74, Skill3FrameLast = 88; // 三技能（未接判定）

		private const int SpawnFrameCount  = SpawnFrameLast  - SpawnFrameFirst  + 1;
		private const int IdleFrameCount   = IdleFrameLast   - IdleFrameFirst   + 1;
		private const int Skill1FrameCount = Skill1FrameLast - Skill1FrameFirst + 1;
		private const int AttackFrameCount = AttackFrameLast - AttackFrameFirst + 1;
		private const int Skill2FrameCount = Skill2FrameLast - Skill2FrameFirst + 1;
		private const int Skill3FrameCount = Skill3FrameLast - Skill3FrameFirst + 1;

		// 伤害不再靠"贴身碰撞判定"，而是在攻击动画的最后几帧手动打出多段命中（见
		// UpdateAttacking/TryDealMultiHitSegment）。MultiHitSegments 段伤害平分总伤害
		// （Projectile.damage 里的余数塞进最后一段），不是每段都打一次满伤害。
		private const int MultiHitSegments   = 3;
		private const int MultiHitStartFrame = AttackFrameCount - MultiHitSegments; // 倒数第几帧开始（含）

		private const int SpawnTicksPerFrame  = 5;
		private const int IdleTicksPerFrame   = 8;
		private const int AttackTicksPerFrame = 4;
		private const int SkillTicksPerFrame  = 4;

		private const float DrawScale = 1f; // 本体贴图 1:1 显示，不做缩放

		// ── 水晶动画表（m3_水晶_gif_sheet.png，22x448，14 帧，每帧 22x32）──
		private const string CrystalTexturePath = "ArknightsMod/Content/Projectiles/Medic/Kaltsit/Mon3trCrystal";
		private const int CrystalFrameWidth  = 22;
		private const int CrystalFrameHeight = 32;
		private const int CrystalFrameCount  = 14;
		private const int CrystalTicksPerFrame = 6;
		private const float CrystalDrawScale = 1f; // 贴图 1:1 显示，不做缩放

		// ── 悬浮跟随（占位数值）──
		private const float HoverOffsetX  = 45f;
		private const float HoverOffsetY  = -40f;
		private const float BobAmplitude  = 4f;
		private const float BobSpeed      = 0.05f;
		private const float HoverAccel    = 0.6f;
		private const float HoverMaxSpeed = 9f;
		private const float HoverDamp     = 0.9f;

		// ── 索敌 / 追击（占位数值）──
		// 以玩家为圆心的索敌半径，不是以 M3 自己为圆心；追击/攻击期间也每帧用这个半径
		// 重新判断目标还在不在，天然起到"玩家把距离拉开就放弃追击"的效果，不需要单独的脱战判定。
		private const float AggroRadius     = 500f;
		private const float ChaseAccel      = 0.9f;
		private const float ChaseMaxSpeed   = 13f;
		private const float ChaseDamp       = 0.92f;

		// 攻击站位：停在目标左/右这么远的地方，不贴脸；具体站哪一侧取决于 M3 当前在
		// 目标的哪一边（就近取，不绕路）。到位的容差范围内就算"到位"，可以转入攻击。
		private const float AttackStandoffX = 60f;
		private const float AttackStandoffY = -8f;
		private const float StandoffArriveTolerance = 10f;

		// 攻击间隔＝普攻动画播放一整轮的时长：冷却从"开始出手"那一刻起算，动画播完
		// 冷却也正好转好，只要目标还在就能无缝接下一次，不会在两次攻击之间多出一段空等。
		private const int AttackCooldownMax = AttackFrameCount * AttackTicksPerFrame;

		// ── 生命 / 复苏 ──
		private const int   LifeMax          = 1086;
		private const int   Defense          = 41;
		private const int   ReceiveCooldownBase = 40;
		private const int   ReviveTicks      = 1200; // 水晶状态持续多久后自动复苏，约 20 秒

		// ── 伤害（没有对应武器，直接给固定基础伤害）──
		private const int BaseDamage = 280;

		private enum Mon3trState : byte { Spawning, Idle, Chasing, Attacking, Skill1, Skill2, Skill3, Crystal }

		private Mon3trState state = Mon3trState.Spawning;
		private int   animFrame;      // 当前状态动画序列里的第几帧（0 开始）
		private int   animTimer;      // 帧计时器
		private int   facingDir = -1;
		private int   attackCooldown;
		private int   reviveTimer;
		private int   life = -1;
		private int   receiveCooldown;
		private float bobTimer;
		private bool  lifeInitialized;

		// 当前锁定的攻击目标（NPC 下标）。Chasing/Attacking 期间每帧都用"离玩家最近"
		// 重新核对一遍，核对结果和这个不一致就说明目标该换了——即便正在攻击动画里也会
		// 立刻中断转而扑向新目标，实现"攻击目标可以半路转移，永远优先离玩家最近的那个"。
		private int   currentTargetWhoAmI = -1;
		private int   lastMultiHitFrame = -1; // 本次攻击动画里，多段伤害已经打到第几帧了（防止同一帧重复触发）

		public override string Texture => "ArknightsMod/Content/Projectiles/Medic/Kaltsit/Mon3tr";

		public override void SetDefaults() {
			Projectile.width       = 75;
			Projectile.height      = 75;
			Projectile.friendly    = true;
			Projectile.minion      = true;
			Projectile.DamageType  = DamageClass.Summon;
			Projectile.minionSlots = 0f; // 套装白送的召唤物，不占召唤栏
			Projectile.penetrate   = -1;
			Projectile.tileCollide = false; // 全程飞行，不做地形碰撞
			Projectile.ignoreWater = true;
			Projectile.aiStyle     = -1;
			Projectile.timeLeft    = 2;
			Projectile.damage      = BaseDamage;
			// 不再依赖碰撞判定造成伤害（MinionContactDamage 恒为 false，见下），
			// usesLocalNPCImmunity/localNPCHitCooldown 这两个碰撞冷却字段就没有意义了，去掉。
		}

		public override void OnSpawn(IEntitySource source) {
			Player owner = Main.player[Projectile.owner];
			Projectile.Center = owner.Center + new Vector2(-owner.direction * HoverOffsetX, HoverOffsetY);
			bobTimer = Main.rand.NextFloat(1000f);
			life = LifeMax;
			lifeInitialized = true;
			EnterState(Mon3trState.Spawning);
		}

		/// <summary>
		/// 由 KaltsitSetPlayer.PostUpdateEquips 每帧调用：套装还穿着、身上却没有 M3 时补生成一个。
		/// 只在本地玩家这边生成（其它客户端靠 Projectile 自身的多人同步看到），照抄
		/// CommandCenterPet.EnsureFor 的写法——这是本 mod 里唯一验证过可用的"buff 常驻期间
		/// 自动补生成"模式；之前把生成逻辑放进 ModBuff.Update() 里试过，没有真正生成出来。
		/// </summary>
		public static void EnsureFor(Player player) {
			if (player.whoAmI != Main.myPlayer) return;

			int type = ModContent.ProjectileType<Mon3tr>();
			if (player.ownedProjectileCounts[type] > 0) return;

			Projectile.NewProjectile(player.GetSource_Misc("KaltsitSet"), player.Center, Vector2.Zero,
				type, BaseDamage, 0f, player.whoAmI);
		}

		public override bool? CanCutTiles() => false;

		// 不再靠"贴身碰撞"造成伤害——见 UpdateAttacking 里对 TryDealMultiHitSegment 的调用，
		// 伤害改成在攻击动画最后几帧手动打出多段命中。碰撞本身不再产生任何伤害判定。
		public override bool MinionContactDamage() => false;

		// ========================= AI =========================

		public override void AI() {
			Player owner = Main.player[Projectile.owner];
			if (owner.dead || !owner.active || !owner.HasBuff<Mon3trBuff>()) {
				Projectile.Kill();
				return;
			}

			Projectile.timeLeft = 2;

			if (!lifeInitialized) {
				life = LifeMax;
				lifeInitialized = true;
			}

			bobTimer += 1f;

			switch (state) {
				case Mon3trState.Spawning:  UpdateSpawning(owner);        break;
				case Mon3trState.Idle:      UpdateIdle(owner);            break;
				case Mon3trState.Chasing:   UpdateChasing(owner);         break;
				case Mon3trState.Attacking: UpdateAttacking(owner);       break;
				case Mon3trState.Skill1:    UpdateSkillPlaceholder(owner, Skill1FrameCount); break;
				case Mon3trState.Skill2:    UpdateSkillPlaceholder(owner, Skill2FrameCount); break;
				case Mon3trState.Skill3:    UpdateSkillPlaceholder(owner, Skill3FrameCount); break;
				case Mon3trState.Crystal:   UpdateCrystal(owner);         break;
			}

			if (attackCooldown > 0)
				attackCooldown--;
		}

		private void EnterState(Mon3trState next) {
			state = next;
			animFrame = 0;
			animTimer = 0;
			if (next == Mon3trState.Attacking)
				lastMultiHitFrame = -1; // 每次重新进入攻击状态，多段命中记录都要清空重来
			Projectile.netUpdate = true;
		}

		// -------- 待机：悬浮在玩家身后，索敌 --------

		private void UpdateIdle(Player owner) {
			HoverToward(GetHoverTarget(owner), owner);
			StepAnim(IdleFrameCount, IdleTicksPerFrame, loop: true);
			TakeIncomingDamage();

			// 只要附近出现目标就切去追击，冷却是否转好交给 Chasing 自己在真正出手前判断——
			// 这样才能保证"只要目标没死、也没被更近的怪替换掉，就一直贴着打"，
			// 不会打完一下又先飞回玩家身后再折返。
			if (TryFindTarget(owner, out NPC target)) {
				currentTargetWhoAmI = target.whoAmI;
				EnterState(Mon3trState.Chasing);
			}
		}

		// -------- 追击：朝"当前离玩家最近的目标"侧面站位飞去，不贴脸 --------

		private void UpdateChasing(Player owner) {
			// 每一帧都重新找一次"离玩家最近的目标"，而不是死盯着上一次锁定的那个——
			// 目标死亡、玩家把距离拉开、或者出现了更近的新目标，都会在这里自然体现出来：
			// 找不到就回待机，找到的换了就自动转移攻击目标。
			if (!TryFindTarget(owner, out NPC target)) {
				currentTargetWhoAmI = -1;
				EnterState(Mon3trState.Idle);
				return;
			}
			currentTargetWhoAmI = target.whoAmI;

			Vector2 standoff = GetStandoffPoint(target);
			Vector2 toStandoff = standoff - Projectile.Center;
			float   dist        = toStandoff.Length();

			if (dist <= StandoffArriveTolerance) {
				// 已经到站位：不用再逼近，稳住等冷却；冷却转好立刻接下一次攻击。
				Projectile.velocity *= 0.85f;
				facingDir = target.Center.X >= Projectile.Center.X ? 1 : -1;
				if (attackCooldown <= 0) {
					attackCooldown = AttackCooldownMax; // 从出手这一刻起算，见常量注释
					EnterState(Mon3trState.Attacking);
					return;
				}
			}
			else {
				Projectile.velocity += toStandoff / dist * ChaseAccel;
				float speed = Projectile.velocity.Length();
				if (speed > ChaseMaxSpeed)
					Projectile.velocity *= ChaseMaxSpeed / speed;
				Projectile.velocity *= ChaseDamp;

				if (Math.Abs(Projectile.velocity.X) > 0.3f)
					facingDir = Projectile.velocity.X > 0 ? 1 : -1;
			}

			StepAnim(IdleFrameCount, IdleTicksPerFrame, loop: true); // 复用待机帧当作飞行/伺机姿态
			TakeIncomingDamage();
		}

		// 站位点：目标左侧或右侧 AttackStandoffX 远的地方，就近取——M3 当前在目标左边就
		// 继续站左边，在右边就继续站右边，不会为了换边绕大圈。
		private Vector2 GetStandoffPoint(NPC target) {
			float side = Projectile.Center.X <= target.Center.X ? -1f : 1f;
			return target.Center + new Vector2(side * AttackStandoffX, AttackStandoffY);
		}

		// -------- 攻击：原地播放攻击动画，目标半路变了就立刻中断转去追新目标 --------

		private void UpdateAttacking(Player owner) {
			// 每一帧都重新核对"当前离玩家最近的目标"，核对结果和正在打的这个对不上——
			// 不管是目标死了、跑远了、还是出现了更近的新目标——就立刻中断攻击动画，
			// 转入 Chasing 扑向新目标，而不是把这一下打完。
			if (!TryFindTarget(owner, out NPC nearest) || nearest.whoAmI != currentTargetWhoAmI) {
				currentTargetWhoAmI = nearest?.whoAmI ?? -1;
				EnterState(Mon3trState.Chasing);
				return;
			}

			Projectile.velocity *= 0.8f; // 攻击时基本定住，不再飞行
			facingDir = nearest.Center.X >= Projectile.Center.X ? 1 : -1;

			TryDealMultiHitSegment(nearest);
			TakeIncomingDamage();

			bool finished = StepAnim(AttackFrameCount, AttackTicksPerFrame, loop: false);

			// 播完动画交回 Chasing 去判断"还有没有目标"——目标还活着/换了更近的都会
			// 无缝接上下一次追击或攻击，不会先绕回玩家身后再折返。
			if (finished)
				EnterState(Mon3trState.Chasing);
		}

		// 攻击动画进入最后几帧后，每帧打一段（一共 MultiHitSegments 段），伤害不靠碰撞判定，
		// 直接对锁定目标结算——总伤害平分到每一段（余数塞最后一段），不是每段都打满伤害。
		private void TryDealMultiHitSegment(NPC target) {
			if (Main.myPlayer != Projectile.owner) return;
			if (animFrame < MultiHitStartFrame) return;
			if (animFrame == lastMultiHitFrame) return; // 这一帧已经打过了
			lastMultiHitFrame = animFrame;

			int segmentIndex = animFrame - MultiHitStartFrame; // 0 开始
			int segDamage = Projectile.damage / MultiHitSegments;
			if (segmentIndex == MultiHitSegments - 1)
				segDamage = Projectile.damage - segDamage * (MultiHitSegments - 1); // 余数归最后一段

			int hitDirection = target.Center.X >= Projectile.Center.X ? 1 : -1;
			target.SimpleStrikeNPC(segDamage, hitDirection, damageType: Projectile.DamageType);
		}

		// -------- 技能占位：播完动画自动回待机，命中/效果逻辑留空 --------

		private void UpdateSkillPlaceholder(Player owner, int frameCount) {
			Projectile.velocity *= 0.8f;
			TakeIncomingDamage();

			bool finished = StepAnim(frameCount, SkillTicksPerFrame, loop: false);
			if (finished)
				EnterState(Mon3trState.Idle);
		}

		/// <summary>预留给"以后再定"的一技能触发条件调用。现在没有任何地方会调用它。</summary>
		public void TriggerSkill1() {
			if (state is Mon3trState.Crystal or Mon3trState.Spawning) return;
			EnterState(Mon3trState.Skill1);
		}

		/// <summary>预留给"以后再定"的二技能触发条件调用。现在没有任何地方会调用它。</summary>
		public void TriggerSkill2() {
			if (state is Mon3trState.Crystal or Mon3trState.Spawning) return;
			EnterState(Mon3trState.Skill2);
		}

		/// <summary>预留给"以后再定"的三技能触发条件调用。现在没有任何地方会调用它。</summary>
		public void TriggerSkill3() {
			if (state is Mon3trState.Crystal or Mon3trState.Spawning) return;
			EnterState(Mon3trState.Skill3);
		}

		// -------- 复苏：播完出场动画进入待机 --------

		private void UpdateSpawning(Player owner) {
			// 出场/复苏动画播放期间不结算受伤（无敌帧），避免刚冒出来就被同一波伤害秒回水晶。
			HoverToward(GetHoverTarget(owner), owner);
			bool finished = StepAnim(SpawnFrameCount, SpawnTicksPerFrame, loop: false);
			if (finished)
				EnterState(Mon3trState.Idle);
		}

		// -------- 水晶：固定悬浮在玩家身后，不参与战斗，倒计时复苏 --------

		private void UpdateCrystal(Player owner) {
			HoverToward(GetHoverTarget(owner), owner);
			StepAnim(CrystalFrameCount, CrystalTicksPerFrame, loop: true);

			if (--reviveTimer <= 0) {
				life = LifeMax;
				EnterState(Mon3trState.Spawning);
			}
		}

		// -------- 通用：悬浮移动 / 动画推进 --------

		private Vector2 GetHoverTarget(Player owner) {
			float bob = (float)Math.Sin(bobTimer * BobSpeed) * BobAmplitude;
			return owner.Center + new Vector2(-owner.direction * HoverOffsetX, HoverOffsetY + bob);
		}

		// 朝向：移动时跟移动方向走；停下来时跟玩家当前朝向保持一致（不是背对着玩家看），
		// 这样玩家一停下 M3 不会突然扭过去面朝反方向。
		private void HoverToward(Vector2 target, Player owner) {
			Vector2 toTarget = target - Projectile.Center;
			float   dist     = toTarget.Length();
			if (dist > 4f) {
				Projectile.velocity += toTarget / dist * HoverAccel;
				float speed = Projectile.velocity.Length();
				if (speed > HoverMaxSpeed)
					Projectile.velocity *= HoverMaxSpeed / speed;
			}
			Projectile.velocity *= HoverDamp;

			if (Math.Abs(Projectile.velocity.X) > 0.3f)
				facingDir = Projectile.velocity.X > 0 ? 1 : -1;
			else
				facingDir = owner.direction;
		}

		/// <summary>推进 animFrame，返回这一帧是否"播完了"（非循环模式下到达最后一帧时为 true）。</summary>
		private bool StepAnim(int frameCount, int ticksPerFrame, bool loop) {
			animTimer++;
			if (animTimer < ticksPerFrame)
				return false;
			animTimer = 0;

			if (loop) {
				animFrame = (animFrame + 1) % frameCount;
				return false;
			}

			if (animFrame >= frameCount - 1)
				return true;
			animFrame++;
			return false;
		}

		// -------- 索敌：以玩家为圆心找最近的怪，而不是以 M3 自己为圆心 --------

		private bool TryFindTarget(Player owner, out NPC target) {
			target = null;
			float bestDist = AggroRadius;

			for (int i = 0; i < Main.maxNPCs; i++) {
				NPC npc = Main.npc[i];
				if (!npc.active || npc.friendly || npc.dontTakeDamage || npc.lifeMax <= 5) continue;
				if (!npc.CanBeChasedBy(Projectile)) continue;

				float dist = Vector2.Distance(npc.Center, owner.Center);
				if (dist < bestDist) { bestDist = dist; target = npc; }
			}

			return target != null;
		}

		// -------- 受伤：敌对 NPC 本体和敌对弹幕都能命中 M3，伤害结算到它自己的生命值 --------
		// 两种命中共用 receiveCooldown；基础无敌时间与原版玩家普通受伤相同，为 40 tick。
		// 主人具有 longInvince（十字项链一类效果）时，同样按原版规则延长到 80 tick。
		// 水晶和出场状态不会调用这里，因此保持无敌。
		private void TakeIncomingDamage() {
			if (Main.myPlayer != Projectile.owner) return;
			if (receiveCooldown > 0) {
				receiveCooldown--;
				return;
			}

			foreach (NPC npc in Main.ActiveNPCs) {
				if (!npc.active || npc.friendly || npc.damage <= 0 || npc.dontTakeDamage) continue;
				if (!Projectile.Hitbox.Intersects(npc.Hitbox)) continue;
				ApplyDamage(npc.damage);
				return;
			}

			foreach (Projectile other in Main.ActiveProjectiles) {
				if (!other.active || !other.hostile || other.owner == Projectile.owner) continue;
				if (!Projectile.Hitbox.Intersects(other.Hitbox)) continue;
				ApplyDamage(other.damage);
				return;
			}
		}

		private void ApplyDamage(int rawDamage) {
			int dealt = Math.Max(1, rawDamage - Defense);
			life -= dealt;
			Player owner = Main.player[Projectile.owner];
			receiveCooldown = owner.longInvince ? ReceiveCooldownBase * 2 : ReceiveCooldownBase;
			Projectile.netUpdate = true;

			if (!Main.dedServ) {
				CombatText.NewText(Projectile.getRect(), Color.OrangeRed, dealt);
				SoundEngine.PlaySound(SoundID.NPCHit1, Projectile.Center);
			}

			if (life <= 0) {
				life = 0;
				BecomeCrystal();
			}
		}

		private void BecomeCrystal() {
			if (!Main.dedServ) {
				SoundEngine.PlaySound(SoundID.NPCDeath4, Projectile.Center);
				for (int i = 0; i < 16; i++) {
					Dust d = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(14f, 12f),
						DustID.GemEmerald, Main.rand.NextVector2Circular(3f, 3f), 0, default, Main.rand.NextFloat(1f, 1.6f));
					d.noGravity = true;
				}
			}
			Projectile.velocity = Vector2.Zero;
			reviveTimer = ReviveTicks;
			EnterState(Mon3trState.Crystal);
		}

		// ========================= 绘制 =========================

		public override bool PreDraw(ref Color lightColor) {
			Vector2 drawCenter = Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY);
			SpriteEffects fx = facingDir < 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

			if (state == Mon3trState.Crystal) {
				Texture2D tex = ModContent.Request<Texture2D>(CrystalTexturePath).Value;
				var src = new Rectangle(0, animFrame * CrystalFrameHeight, CrystalFrameWidth, CrystalFrameHeight);
				var origin = new Vector2(CrystalFrameWidth / 2f, CrystalFrameHeight / 2f);
				Main.EntitySpriteDraw(tex, drawCenter, src, lightColor, 0f, origin, CrystalDrawScale, fx, 0);
				return false;
			}

			int rowIndex = state switch {
				Mon3trState.Spawning  => SpawnFrameFirst  - 1 + animFrame,
				Mon3trState.Idle      => IdleFrameFirst   - 1 + animFrame,
				Mon3trState.Chasing   => IdleFrameFirst   - 1 + animFrame,
				Mon3trState.Attacking => AttackFrameFirst - 1 + animFrame,
				Mon3trState.Skill1    => Skill1FrameFirst - 1 + animFrame,
				Mon3trState.Skill2    => Skill2FrameFirst - 1 + animFrame,
				Mon3trState.Skill3    => Skill3FrameFirst - 1 + animFrame,
				_ => IdleFrameFirst - 1,
			};

			Texture2D bodyTex = TextureAssets.Projectile[Type].Value;
			var bodySrc    = new Rectangle(0, rowIndex * FrameHeight, FrameWidth, FrameHeight);
			var bodyOrigin = new Vector2(FrameWidth / 2f, FrameHeight / 2f);
			Main.EntitySpriteDraw(bodyTex, drawCenter, bodySrc, lightColor, 0f, bodyOrigin, DrawScale, fx, 0);
			return false;
		}

		public override void PostDraw(Color lightColor) {
			if (Main.dedServ || state == Mon3trState.Crystal || state == Mon3trState.Spawning) return;

			float barWorldY = Projectile.Center.Y - FrameHeight * DrawScale * 0.5f + Projectile.gfxOffY - 8f;
			float alpha = Lighting.Brightness((int)(Projectile.Center.X / 16f), (int)((Projectile.Center.Y + Projectile.gfxOffY) / 16f));
			Main.instance.DrawHealthBar(Projectile.Center.X, barWorldY, life, LifeMax, alpha, 1f);
		}

		// ========================= 网络同步 =========================

		public override void SendExtraAI(BinaryWriter writer) {
			writer.Write((byte)state);
			writer.Write(life);
			writer.Write(attackCooldown);
			writer.Write(reviveTimer);
		}

		public override void ReceiveExtraAI(BinaryReader reader) {
			state           = (Mon3trState)reader.ReadByte();
			life            = reader.ReadInt32();
			attackCooldown  = reader.ReadInt32();
			reviveTimer     = reader.ReadInt32();
			lifeInitialized = true;
			animFrame = 0;
			animTimer = 0;
		}
	}
}
