using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Projectiles.BasePROJ
{
	// ============================================================================
	//  BaseLanceHP —— 长枪 / 长矛 / 骑枪 的基础持枪弹幕
	//
	//  职责边界（只做通用部分，角色专属放子类）：
	//   · 起手锁定瞄准方向；
	//   · 收枪(Prepare)→可选蓄势停顿(ChargeHold)→前刺(Thrust)→收枪(Recovery) 的位移曲线；
	//   · 前手持枪、枪尖位置与握点绘制、左右自适应；
	//   · 仅在前刺有效窗口内开启伤害；从手部到枪尖的胶囊线碰撞；
	//   · 每次真实前刺的本地 NPC 命中记录（一次刺同一目标只命中一次）；
	//   · 攻速影响持续帧数；生命周期、持有物品检查与联机同步。
	//
	//  只支持前后伸缩，不做旋转大挥——需要横扫请用 BaseSwordHP，避免这里变成第二套
	//  万能近战框架。
	//
	//  动作数据由子类通过 GetMotion(actionId) 提供；动作编号由物品在生成弹幕时写入 ai[0]。
	// ============================================================================
	public abstract class BaseLanceHP : ModProjectile
	{
		/// <summary>一次前刺的位移/伤害参数。全部为"通用描述"，不含任何角色专属判断。</summary>
		public struct LanceMotion
		{
			public int Prepare;      // 收枪预备时长（tick，未乘攻速）
			public int ChargeHold;   // 蓄势停顿时长（0 = 无）
			public int Thrust;       // 前刺时长（伤害只在此窗口开启）
			public int Recover;      // 收枪时长
			public float GripMin;    // 握点距手部的最近距离（收枪时）
			public float GripMax;    // 握点距手部的最远距离（前刺顶点）
			public float HitWidthMul;// 判定宽度倍率（相对 Profile.CombatWidth）
			public float DamageMul;  // 该动作的伤害倍率
			public MeleeEase ThrustEase; // 前送距离的非线性曲线
			public float AngleStart; // 伤害段起始角（相对瞄准方向，面朝左时自动镜像）
			public float AngleEnd;   // 伤害段结束角；两者不同时即为长枪挑/劈动作
			public MeleeEase AngleEase;
		}

		// ── 子类提供 ────────────────────────────────────────────────
		protected abstract WeaponSpriteProfile Profile { get; }
		protected abstract LanceMotion GetMotion(int actionId);
		/// <summary>绑定的物品类型；手持切换成别的物品时弹幕自我结束。</summary>
		protected abstract int BindingItemType();
		/// <summary>客户端绘制用贴图（专用服务器上返回值不会被使用）。</summary>
		protected abstract Texture2D GetWeaponTexture();

		// 子类可选钩子（不要让子类重写整段 AI）
		protected virtual void ModifyStrike(NPC target, ref NPC.HitModifiers modifiers) { }
		protected virtual void OnStrike(NPC target, in NPC.HitInfo hit, int damageDone) { }
		protected virtual void OnActionEvent(int eventId, Vector2 tipWorld) { }
		/// <summary>每个运动子步完成后的专属表现钩子；只允许做视觉反馈，不应改伤害或玩家位移。</summary>
		protected virtual void OnMotionStep() { }

		protected int ActionId => (int)Projectile.ai[0];
		/// <summary>技能状态：0 普通 / 1 一技能 / 2 二技能。由物品在生成时写入 ai[1]，供子类判断。</summary>
		protected int SkillMode => (int)Projectile.ai[1];

		/// <summary>整套动作的额外速度系数（&gt;1 更快、&lt;1 更慢）。子类按 SkillMode 覆写以实现技能加/减速。</summary>
		protected virtual float ActionSpeedScale => 1f;

		private Player Owner => Main.player[Projectile.owner];

		// 运行期状态（各客户端按同一攻速与动作数据独立推导，天然同步）
		private float aimAngle;
		private float currentAngle;
		private bool initialized;
		// 使用浮点动作时间，让子类可以用 extraUpdates 增加采样密度而不把动作整体加速。
		private float age;
		private int tPrepare, tHold, tThrust, tRecover, tTotal;
		private LanceMotion motion;

		// 当前帧几何，供 Colliding / 事件使用
		private Vector2 handWorld;
		private Vector2 tipWorld;
		private bool damaging;
		private float phaseProgress;
		private int lastEventFiredPhase = -1;

		// 枪尖轨迹（供子类画锐利拖尾）：[0] 为最新；仅在有效前刺窗口内累积，出窗后逐帧淡出。
		private const int TrailLen = 10;
		private readonly Vector2[] tipTrail = new Vector2[TrailLen];
		private int tipTrailCount;

		// 供子类 VFX 使用的只读状态
		protected Vector2 HandWorld => handWorld;
		protected Vector2 TipWorld => tipWorld;
		protected bool Damaging => damaging;
		protected float AimAngle => aimAngle;
		protected float CurrentAngle => currentAngle;
		protected float PhaseProgress => phaseProgress;
		protected Vector2[] TipTrail => tipTrail;
		protected int TipTrailCount => tipTrailCount;

		/// <summary>子类在此叠加"我们风格"的加法发光/拖尾；基类先画完武器再调用它。默认无。</summary>
		protected virtual void DrawWeaponVfxBehind(Color lightColor) { }
		protected virtual void DrawWeaponVfx(Color lightColor) { }

		public override void SetDefaults() {
			Projectile.width = 32;
			Projectile.height = 32;
			Projectile.friendly = true;
			Projectile.penetrate = -1;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
			Projectile.aiStyle = -1;
			Projectile.DamageType = DamageClass.Melee;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = -1; // 每个弹幕对同一 NPC 只命中一次 = 一次前刺只命中一次
			Projectile.timeLeft = 600;           // 初始化时按动作总长覆盖
		}

		public override bool ShouldUpdatePosition() => false;

		private void Init() {
			Player player = Owner;
			aimAngle = Projectile.velocity.ToRotation();
			currentAngle = aimAngle;
			motion = GetMotion(ActionId);

			float atkSpeed = player.GetTotalAttackSpeed(Projectile.DamageType) * ActionSpeedScale;
			tPrepare = BaseHeldMeleeSupport.ScaleTicks(motion.Prepare, atkSpeed);
			tHold = motion.ChargeHold > 0 ? BaseHeldMeleeSupport.ScaleTicks(motion.ChargeHold, atkSpeed) : 0;
			tThrust = BaseHeldMeleeSupport.ScaleTicks(motion.Thrust, atkSpeed);
			tRecover = BaseHeldMeleeSupport.ScaleTicks(motion.Recover, atkSpeed);
			tTotal = tPrepare + tHold + tThrust + tRecover;

			Projectile.timeLeft = (tTotal + 2) * (Projectile.extraUpdates + 1);
			initialized = true;
		}

		public override void AI() {
			Player player = Owner;

			// 手持切换/死亡：立即结束
			if (player.dead || !player.active || player.noItems || player.CCed || player.HeldItem.type != BindingItemType()) {
				Projectile.Kill();
				return;
			}

			if (!initialized)
				Init();

			// 相位与相位内进度
			int phase; // 0 prepare, 1 hold, 2 thrust, 3 recover
			float p;
			float a = age;
			if (a < tPrepare) { phase = 0; p = tPrepare > 0 ? a / (float)tPrepare : 1f; }
			else if (a < tPrepare + tHold) { phase = 1; p = tHold > 0 ? (a - tPrepare) / (float)tHold : 1f; }
			else if (a < tPrepare + tHold + tThrust) { phase = 2; p = tThrust > 0 ? (a - tPrepare - tHold) / (float)tThrust : 1f; }
			else { phase = 3; p = tRecover > 0 ? (a - tPrepare - tHold - tThrust) / (float)tRecover : 1f; }
			phaseProgress = MathHelper.Clamp(p, 0f, 1f);

			// 握点相对手部的距离曲线
			float gripDist = phase switch {
				0 => MathHelper.Lerp(motion.GripMax * 0.5f, motion.GripMin, BaseHeldMeleeSupport.Ease(MeleeEase.EaseOut, p)),
				1 => motion.GripMin,
				2 => MathHelper.Lerp(motion.GripMin, motion.GripMax, BaseHeldMeleeSupport.Ease(motion.ThrustEase, p)),
				_ => MathHelper.Lerp(motion.GripMax, motion.GripMin * 0.4f, BaseHeldMeleeSupport.Ease(MeleeEase.EaseIn, p)),
			};

			bool faceLeft = BaseHeldMeleeSupport.FaceLeft(aimAngle);
			float angleOffset = phase switch {
				0 => MathHelper.Lerp(0f, motion.AngleStart, BaseHeldMeleeSupport.Ease(MeleeEase.Smooth, p)),
				1 => motion.AngleStart,
				2 => MathHelper.Lerp(motion.AngleStart, motion.AngleEnd,
					BaseHeldMeleeSupport.Ease(motion.AngleEase, p)),
				_ => MathHelper.Lerp(motion.AngleEnd, 0f, BaseHeldMeleeSupport.Ease(MeleeEase.Smooth, p)),
			};
			// “上挑/下劈”是屏幕空间语义；面朝左时镜像相对角，避免动作上下颠倒。
			currentAngle = aimAngle + angleOffset * (faceLeft ? -1f : 1f);
			Vector2 dir = BaseHeldMeleeSupport.Dir(currentAngle);

			handWorld = player.MountedCenter + dir * gripDist;
			tipWorld = handWorld + dir * (Profile.CombatReach * Projectile.scale);

			// 进入前刺相位的第一帧：重置本地免疫（让这一次刺能重新命中），并触发前刺开始事件（此时 tipWorld 已是当前帧值）
			if (phase == 2 && lastEventFiredPhase != 2) {
				Projectile.ResetLocalNPCHitImmunity();
				lastEventFiredPhase = 2;
				OnActionEvent(1, tipWorld);
			}

			Projectile.Center = (handWorld + tipWorld) * 0.5f;
			// velocity 始终保留锁定瞄准方向，避免远端在挥到半途才收到弹幕时把当前挑/劈角误当成基准角。
			Projectile.velocity = BaseHeldMeleeSupport.Dir(aimAngle);
			Projectile.rotation = currentAngle;
			Projectile.spriteDirection = faceLeft ? -1 : 1;

			damaging = phase == 2;

			// 枪尖轨迹累积/淡出
			if (damaging) {
				for (int i = tipTrail.Length - 1; i > 0; i--)
					tipTrail[i] = tipTrail[i - 1];
				tipTrail[0] = tipWorld;
				if (tipTrailCount < tipTrail.Length)
					tipTrailCount++;
			}
			else if (tipTrailCount > 0) {
				tipTrailCount--;
			}
			OnMotionStep();

			BaseHeldMeleeSupport.HoldAndPose(player, Projectile, handWorld, faceLeft);

			if (age >= tTotal) {
				Projectile.Kill();
				return;
			}
			age += 1f / (Projectile.extraUpdates + 1f);
		}

		public override bool? CanDamage() => damaging ? null : false;

		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
			if (!damaging)
				return false;
			if (!BaseHeldMeleeSupport.CapsuleHits(handWorld, tipWorld,
					Profile.CombatWidth * motion.HitWidthMul, targetHitbox))
				return false;
			// 不隔墙伤害：弹幕本体可穿墙只是为了绘制不被墙打断，命中仍需手部到目标之间没有实心方块遮挡
			// 握点在突刺时会向前移动很远；若从握点开始查视线，墙可能落在玩家与握点之间而被跳过。
			return Collision.CanHit(Owner.MountedCenter, 1, 1,
				targetHitbox.TopLeft(), targetHitbox.Width, targetHitbox.Height);
		}

		public sealed override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) {
			modifiers.SourceDamage *= motion.DamageMul;
			ModifyStrike(target, ref modifiers);
		}

		public sealed override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			OnStrike(target, hit, damageDone);
		}

		public override bool PreDraw(ref Color lightColor) {
			if (Main.dedServ)
				return false;
			Texture2D tex = GetWeaponTexture();
			bool faceLeft = BaseHeldMeleeSupport.FaceLeft(aimAngle);
			DrawWeaponVfxBehind(lightColor);
			BaseHeldMeleeSupport.DrawHeld(tex, handWorld, currentAngle, faceLeft, Profile, Projectile.scale, lightColor);
			DrawWeaponVfx(lightColor);
			return false;
		}

		// 锁定的瞄准角作为最小同步状态；动作时序在各端由攻速+动作数据确定，无需同步。
		public override void SendExtraAI(BinaryWriter writer) {
			writer.Write(aimAngle);
		}

		public override void ReceiveExtraAI(BinaryReader reader) {
			aimAngle = reader.ReadSingle();
		}
	}
}
